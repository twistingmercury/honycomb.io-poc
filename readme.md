# Honeycomb.io OpenTelemetry RnD

A project to figure out how to use OpenTelemetry and Honeycomb.io

## Goals (in no particular order)

1. OpenTelemetry to propagate a trace when an async message broker, in this case [Azure ServiceBus](https://azure.microsoft.com/en-us/products/service-bus/#overview)
2. Propagating traces across service boundaries using multiple lanugages.  
   a. .NET  
   b. Node.js  
   c. Go  
   d. Python  
   e. JVM-based language  
3. Explore the use of telemetry events vs. traditional logging.
4. Explore Honeycomb.io services.

---

## Prerequisites

1. You will need an Azure account.
    * Create a ServiceBus resource.
    * Add a queue named `new_widgets`
    * Create a shared access policy for the queue that can both `Listen` and `Send`.
  
2. You will need Docker/Docker Compose.
   
3. docker image [otel/opentelemetry-collector-contrib](https://hub.docker.com/r/otel/opentelemetry-collector-contrib)

### Docker volumes

This was developed and use on a Macbook. You will need to make slight adjustments to the volumes in
the [docker-compose.yaml](docker-compose.yaml).

## Docker Compose

Run `docker compose up` in the directory with the `docker-compose.yaml` file.

### OpenTelemetry Collector

1. Copy [otel-collector-config.yaml](otel-collector-config.yaml) to
   directory `~/volumes/otel-collector/otel-collector-config.yaml`
2. Update the value for `<api key>` to your API key.
3. You can verify the collector is running by navigating to [localhost:13133](http://localhost:13133).

### MongoDB

No setup is required. If you want to log into MongoDB using something
like [mongo-express](https://hub.docker.com/_/mongo-express) the credentials are:

* user: `admin`
* pass: `pass`

---

## !!! Warning !!!

THIS SAMPLE IS NOT TO BE CONSIDERED A WAY TO HOST AN APPLICATION IN SECURELY PRODUCTION!!!
