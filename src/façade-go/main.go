package main

import (
	"log"
	"twistingmercury/otel-rnd/facade/api"
	"twistingmercury/otel-rnd/facade/tracing"
)

func main() {
	_, err := tracing.InitTraceProvider()
	if err != nil {
		log.Fatal(err)
	}

	if err := api.ListenAndServe(); err != nil {
		log.Fatalln(err)
	}
}
