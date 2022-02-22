- [Description](#description)
  - [Solution structure](#solution-structure)
- [References](#references)
  - [Nice to read](#nice-to-read)
- [Azure Setup](#azure-setup)
  - [Create a service bus](#create-a-service-bus)
    - [Create a queue OrdersQueue](#create-a-queue-ordersqueue)
    - [Create a topic OrdersTopic](#create-a-topic-orderstopic)
    - [Create OrdersSubscription subscription for OrdersTopic](#create-orderssubscription-subscription-for-orderstopic)
  - [Create an App Insights resource](#create-an-app-insights-resource)
- [Local machine setup](#local-machine-setup)
  - [Switch to Docker context for Kubernetes](#switch-to-docker-context-for-kubernetes)
  - [Install KEDA](#install-keda)
  - [Install Dashboard](#install-dashboard)
    - [Troubleshoot. Check the process that is owner on port 8001](#troubleshoot-check-the-process-that-is-owner-on-port-8001)
    - [Start the dashboard](#start-the-dashboard)
    - [Setup access](#setup-access)
    - [Refresh the token](#refresh-the-token)
  - [Docker local registry](#docker-local-registry)
  - [RabbitMq](#rabbitmq)
    - [In k8s with Helm](#in-k8s-with-helm)
      - [Deployment](#deployment)
      - [Forwarding ports to host](#forwarding-ports-to-host)
    - [In Docker](#in-docker)
- [Azure functions deployment](#azure-functions-deployment)
  - [Build Docker image](#build-docker-image)
    - [Azure function with ASB endpoint and topic trigger](#azure-function-with-asb-endpoint-and-topic-trigger)
    - [Azure function with ASB endpoint and queue trigger](#azure-function-with-asb-endpoint-and-queue-trigger)
    - [Azure function with RabbitMq endpoint and topic trigger](#azure-function-with-rabbitmq-endpoint-and-topic-trigger)
    - [Azure function with RabbitMq endpoint and queue trigger](#azure-function-with-rabbitmq-endpoint-and-queue-trigger)
    - [All in one](#all-in-one)
    - [Verify the build images](#verify-the-build-images)
  - [Push docker images to local registry](#push-docker-images-to-local-registry)
    - [Registry test](#registry-test)
  - [Deploying azure functions images to k8s](#deploying-azure-functions-images-to-k8s)
    - [Use one of the following commands to deploy](#use-one-of-the-following-commands-to-deploy)
    - [Deploying all](#deploying-all)
    - [Troubleshooting: server gave HTTP response to HTTPS client](#troubleshooting-server-gave-http-response-to-https-client)
  - [Playing and testing](#playing-and-testing)
  - [Removing k8s resources](#removing-k8s-resources)

# Description

The current repo serves as a playground to demonstrate how to switch an Azure Service Bus to RabbitMq in the context of an Azure Function and local development environment.
The proposed solution is using:
- a Kubernetes in Docker infrastructure
- KEDA for event driven scaling of Azure Functions instances
- HELM (optional) for deploying RabbitMq to K8s
- Azure Application Insights (optional) for monitoring

The switching procedure is based on a custom configuration (named LocalDev) that is conditionally processed in the Azure Functions csproj files.

## Solution structure

# References

- [Jimmy Bogard - Local Development with Azure Service Bus](https://jimmybogard.com/local-development-with-azure-service-bus/)
- [Chris Patterson - Modern .NET Messaging using MassTransit](https://www.youtube.com/watch?v=jQNQDLv7QmU)
- [MassTransit Webjobs with RabbitMq](https://github.com/matei-tm/MassTransit/tree/webjobs-rabbitmq-integration)
- [MassTransit Demo app](https://github.com/matei-tm-csv/AzureFunction.Demo/tree/develop)

## Nice to read

- [Application Insights for Worker Service applications](https://docs.microsoft.com/en-us/azure/azure-monitor/app/worker-service)

# Azure Setup

## Create a service bus

### Create a queue OrdersQueue
### Create a topic OrdersTopic
### Create OrdersSubscription subscription for OrdersTopic

## Create an App Insights resource

If you want to monitor the applications create an App Insights resource and provide the connection information to the configuration files.

# Local machine setup

## Switch to Docker context for Kubernetes

```
kubectl config use-context docker-desktop
```

## Install KEDA

Setting Kubernetes-based Event Driven Autoscaler

```
helm repo add kedacore https://kedacore.github.io/charts
helm repo update

kubectl create namespace keda
helm install keda kedacore/keda --namespace keda
```

## Install Dashboard

```
kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.4.0/aio/deploy/recommended.yaml
```


### Troubleshoot. Check the process that is owner on port 8001

```
$theCulpritPort="8001"
Get-NetTCPConnection -LocalPort $theCulpritPort `
| Select-Object -Property "OwningProcess", @{'Name' = 'ProcessName';'Expression'={(Get-Process -Id $_.OwningProcess).Name}} `
| Get-Unique
```

### Start the dashboard

```
kubectl proxy
```

### Setup access

```
kubectl apply -f k8s/adminuser.yaml
kubectl apply -f k8s/clusterrolebinding.yaml
```

### Refresh the token

```
http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/
```

```
kubectl -n kubernetes-dashboard get secret $(kubectl -n kubernetes-dashboard get sa/admin-user -o jsonpath="{.secrets[0].name}") -o go-template="{{.data.token | base64decode}}"
```

## Docker local registry

Create local registry 
https://docs.docker.com/registry/

```
docker run -d -p 5000:5000 --name registry registry:2
```


## RabbitMq

For the RabbitMq service, two options were provided:

- as k8s deployment using Helm
- as a Docker container

### In k8s with Helm

#### Deployment

```
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update 
helm install rabbit-deploy --set bitnami/rabbitmq --namespace rabbit
 ```

#### Forwarding ports to host

 ```
 kubectl port-forward --namespace rabbit svc/rabbit-deploy-rabbitmq 5672:5672
 kubectl port-forward --namespace rabbit  svc/rabbit-deploy-rabbitmq 15672:15672
```

Access the dashboard and create the user guest:guest with access on queues and topics

Connection string amqp://guest:guest@rabbit-deploy-rabbitmq.rabbit:5672

### In Docker

Create a RabbitMq container

```
docker run -d --hostname my-rabbit --name some-rabbit -p 8080:15672 -p 5672:5672 rabbitmq:3-management
```

Connection string amqp://guest:guest@host.docker.internal:5672


# Azure functions deployment

## Build Docker image

From repo root folder, execute the following commands

### Azure function with ASB endpoint and topic trigger

```bash
# Azure function with ASB endpoint and topic trigger
docker build -t af-masstransit-asb-topic -f docker-files/af-masstransit-asb-topic.Dockerfile .
```

### Azure function with ASB endpoint and queue trigger

```bash
# Azure function with ASB endpoint and queue trigger
docker build -t af-masstransit-asb-queue -f docker-files/af-masstransit-asb-queue.Dockerfile .
```

### Azure function with RabbitMq endpoint and topic trigger

```bash
# Azure function with RabbitMq endpoint and topic trigger
docker build -t af-masstransit-rmq-topic -f docker-files/af-masstransit-rmq-topic.Dockerfile .
```

### Azure function with RabbitMq endpoint and queue trigger

```bash
# Azure function with RabbitMq endpoint and queue trigger
docker build -t af-masstransit-rmq-queue -f docker-files/af-masstransit-rmq-queue.Dockerfile .
```

### All in one

```bash
torq=( topic queue )
asborrmq=( asb rmq )
for i in {0..1}; do 
  for j in {0..1}; do 
    docker build -t localhost:5000/af-masstransit-dual-demo/af-masstransit-"${asborrmq[$j]}"-"${torq[$i]}" -f docker-files/af-masstransit-"${asborrmq[$j]}"-"${torq[$i]}".Dockerfile . ; 
  done; 
done;
```
### Verify the build images

```bash
docker image ls -a | grep af-masstransit
```

## Push docker images to local registry

The builded images will be pushed to the local registry. Assuming that you built all the images

```bash
torq=( topic queue )
asborrmq=( asb rmq )
for i in {0..1}; do 
  for j in {0..1}; do 
    docker push localhost:5000/af-masstransit-dual-demo/af-masstransit-"${asborrmq[$j]}"-"${torq[$i]}"  ; 
  done; 
done;
```


### Registry test

```
docker pull localhost:5000/af-masstransit-dual-demo/af-masstransit-rmq-queue
```

## Deploying azure functions images to k8s

### Use one of the following commands to deploy 
### Deploying all

```bash
torq=( topic queue )
asborrmq=( asb rmq )
for i in {0..1}; do 
  for j in {0..1}; do 
    func kubernetes deploy --name af-masstransit-"${asborrmq[$j]}"-"${torq[$i]}"  --image-name host.docker.internal:5000/af-masstransit-dual-demo/af-masstransit-"${asborrmq[$j]}"-"${torq[$i]}" --namespace rabbit ; 
  done; 
done;
```

Depending on your needs, you can use only a single deployment. In that case use one of the following commands

```bash
# Use one command at a time
func kubernetes deploy --name af-masstransit-asb-topic --image-name host.docker.internal:5000/af-masstransit-dual-demo/af-masstransit-asb-topic --namespace rabbit
func kubernetes deploy --name af-masstransit-rmq-topic --image-name host.docker.internal:5000/af-masstransit-dual-demo/af-masstransit-rmq-topic --namespace rabbit
func kubernetes deploy --name af-masstransit-asb-queue --image-name host.docker.internal:5000/af-masstransit-dual-demo/af-masstransit-asb-queue --namespace rabbit
func kubernetes deploy --name af-masstransit-rmq-queue --image-name host.docker.internal:5000/af-masstransit-dual-demo/af-masstransit-rmq-queue --namespace rabbit
```

### Troubleshooting: server gave HTTP response to HTTPS client

[error](docs/deployerror.png)

Being a local, private registry the host.docker.internal must be added to insecure-registries section in Docker config file.

[fix](docs/registryerrorfix.png.png)


## Playing and testing

```
dotnet build
dotnet run
```

## Removing k8s resources

If you need to clean everything, use the following command

```bash
torq=( topic queue )
asborrmq=( asb rmq )
for i in {0..1}; do 
  for j in {0..1}; do 
    kubectl delete deploy af-masstransit-"${asborrmq[$j]}"-"${torq[$i]}" --namespace rabbit ; 
    kubectl delete ScaledObject af-masstransit-"${asborrmq[$j]}"-"${torq[$i]}" --namespace rabbit ; 
    kubectl delete secret af-masstransit-"${asborrmq[$j]}"-"${torq[$i]}" --namespace rabbit ; 
  done; 
done;
```


