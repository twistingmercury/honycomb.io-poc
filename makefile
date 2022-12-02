default: build

build:
	docker rmi mcg/otel-client:0.0.1 --force
	docker build -t mcg/otel-client:0.0.1 .
	docker system prune -f
