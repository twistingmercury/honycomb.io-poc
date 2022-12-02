package api

import (
	"bytes"
	"context"
	"fmt"
	"io"
	"net"
	"net/http"
	"time"
	"twistingmercury/otel-rnd/facade/tracing"

	"github.com/gorilla/mux"
	"github.com/rs/zerolog/log"
	"go.opentelemetry.io/otel"
	"go.opentelemetry.io/otel/attribute"
	"go.opentelemetry.io/otel/propagation"
)

func ListenAndServe() error {
	router := mux.NewRouter()
	router.HandleFunc("/widgets/", addWidget).Methods("POST")
	router.HandleFunc("/widgets/", getWidget).Methods("GET")
	router.HandleFunc("/widgets/{id}", getWidgetById).Methods("GET")
	router.HandleFunc("/widgets/{id}", delWidget).Methods("DELETE")
	log.Info().Msg("facade running on port 5000")
	return http.ListenAndServe(":5000", router)

}

func getWidget(writer http.ResponseWriter, request *http.Request) {
	traceCtx, span := tracing.StartSpan("api.getWidget", request.Context())
	defer span.End()

	r, err := http.NewRequest("GET", "http://localhost:3000/widgets/", nil)
	if err != nil {
		writer.WriteHeader(500)
		return
	}

	resp, err := invokeApi(r, traceCtx)

	if err != nil {
		writer.WriteHeader(500)
		return
	}

	bytes, err := io.ReadAll(resp.Body)
	if err != nil {
		writer.WriteHeader(500)
		writer.Write([]byte("failed to execute request"))
		return
	}
	writer.WriteHeader(200)
	writer.Write(bytes)
	log.Info().Msg("getWidget completed successfully")
}

func getWidgetById(writer http.ResponseWriter, request *http.Request) {
	traceCtx, span := tracing.StartSpan("api.getWidgetById", request.Context())
	defer span.End()

	vars := mux.Vars(request)
	if len(vars) == 0 {
		writer.WriteHeader(400)
		writer.Write([]byte("invalid id"))
	}

	path := fmt.Sprintf("http://localhost:3000/widgets/%s", vars["id"])
	r, err := http.NewRequest("GET", path, nil)
	if err != nil {
		writer.WriteHeader(500)
		log.Error().Msg(err.Error())
		return
	}

	resp, err := invokeApi(r, traceCtx)

	if err != nil {
		writer.WriteHeader(500)
		log.Error().Msg(err.Error())
		return
	}
	defer resp.Body.Close()

	bytes, err := io.ReadAll(resp.Body)
	if err != nil {
		writer.WriteHeader(500)
		log.Error().Msg(err.Error())
		return
	}
	writer.Write(bytes)
	log.Info().Msg("getWidgetById completed successfully")
}

func addWidget(writer http.ResponseWriter, request *http.Request) {
	traceCtx, span := tracing.StartSpan("api.addWidget", request.Context())
	defer span.End()

	body, err := io.ReadAll(request.Body)
	if err != nil {
		writer.WriteHeader(400)
	}
	defer request.Body.Close()

	r, err := http.NewRequest("POST", "http://localhost:3000/widgets/", bytes.NewBuffer(body))
	if err != nil {
		writer.WriteHeader(500)
		writer.Write([]byte("failed to create request"))
		return
	}
	r.Header.Add("Content-Type", "application/json")

	res, err := invokeApi(r, traceCtx)
	if err != nil {
		writer.WriteHeader(500)
		writer.Write([]byte("failed to execute request"))
		return
	}
	defer res.Body.Close()

	writer.WriteHeader(202)
	writer.Write([]byte("accepted"))
	log.Info().Msg("addWidget completed successfully")
}

func delWidget(writer http.ResponseWriter, request *http.Request) {
	traceCtx, span := tracing.StartSpan("api.delWidget", request.Context())
	defer span.End()

	vars := mux.Vars(request)
	if len(vars) == 0 {
		writer.WriteHeader(400)
		writer.Write([]byte("invalid id"))
	}

	path := fmt.Sprintf("http://localhost:3000/widgets/%s", vars["id"])

	r, err := http.NewRequest("DELETE", path, nil)
	if err != nil {
		writer.WriteHeader(500)
		log.Error().Msg(err.Error())
		return
	}

	resp, err := invokeApi(r, traceCtx)
	if err != nil {
		writer.WriteHeader(500)
		writer.Write([]byte("failed to execute request"))
		log.Error().Msg(err.Error())
		return
	}
	defer resp.Body.Close()

	defer resp.Body.Close()
	writer.WriteHeader(204)
	writer.Write([]byte("no content"))
	log.Info().Msg("delWidget completed successfully")
}

func newHttpClient() (c *http.Client) {
	c = &http.Client{
		Transport: &http.Transport{
			Dial: (&net.Dialer{
				Timeout: 3 * time.Second,
			}).Dial,
			TLSHandshakeTimeout:   5 * time.Second,
			ResponseHeaderTimeout: 5 * time.Second,
			ExpectContinueTimeout: 1 * time.Second,
		},
	}
	return
}

func invokeApi(request *http.Request, ctx context.Context) (*http.Response, error) {
	traceCtx, span := tracing.StartSpan("api.invokeApi", ctx)
	span.SetAttributes(attribute.String("method", request.Method))
	span.SetAttributes(attribute.String("endpoint", request.RequestURI))
	defer span.End()

	client := newHttpClient()
	request = request.WithContext(traceCtx)

	propagator := otel.GetTextMapPropagator()
	propagator.Inject(traceCtx, propagation.HeaderCarrier(request.Header))

	resp, err := client.Do(request)
	if err != nil {
		return nil, err
	}
	return resp, nil
}
