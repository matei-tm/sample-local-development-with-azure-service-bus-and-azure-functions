- [Description](#description)
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
  - [Deploy](#deploy)
    - [to registry](#to-registry)
      - [Registry test](#registry-test)
    - [to k8s](#to-k8s)
    - [Removing](#removing)

# Description

The current repo serves as a playground to demonstrate how to switch an Azure Service Bus to RabbitMq in the context of an Azure Function and local development environment.
The proposed solution is using:
- a Kubernetes in Docker infrastructure
- KEDA for event driven scaling of Azure Functions instances
- HELM (optional) for deploying RabbitMq to K8s
- Azure Application Insights (optional) for monitoring

The switching procedure is based on a custom configuration (named LocalDev) that is conditionally processed in the Azure Functions csproj files.

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
kubectl apply -f .\adminuser.yaml
kubectl apply -f .\clusterrolebinding.yaml
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


## Deploy

### to registry

```
func init --docker-only
```

#### Registry test

```
docker pull localhost:5000/af-masstransit-dual-demo/af-masstransit-dual
```

### to k8s

```
func kubernetes deploy --name af-masstransit-rmq-queue --registry localhost:5000/af-masstransit-dual-demo --namespace rabbit
```

### Removing

```
kubectl delete deploy <name-of-function-deployment>
kubectl delete ScaledObject <name-of-function-deployment>
kubectl delete secret <name-of-function-deployment>
```


