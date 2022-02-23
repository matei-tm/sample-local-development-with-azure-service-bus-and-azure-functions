- [Description](#description)
  - [Solution structure](#solution-structure)
    - [Required nuget feed](#required-nuget-feed)
    - [The dual behavior of the AzureFunction.MassTransit.Dual.Demo* csproj files](#the-dual-behavior-of-the-azurefunctionmasstransitdualdemo-csproj-files)
    - [The dual behavior of the AzureFunction.MassTransit.Dual.DemoQueue SubmitOrderFunction class](#the-dual-behavior-of-the-azurefunctionmasstransitdualdemoqueue-submitorderfunction-class)
    - [The dual behavior of the AzureFunction.MassTransit.Dual.DemoTopic SubmitOrderFunction class](#the-dual-behavior-of-the-azurefunctionmasstransitdualdemotopic-submitorderfunction-class)
- [References](#references)
  - [Nice to read](#nice-to-read)
- [Prerequisites](#prerequisites)
  - [Mandatory: An Azure subscription](#mandatory-an-azure-subscription)
  - [Mandatory: Azure CLI (az)](#mandatory-azure-cli-az)
      - [Windows setup](#windows-setup)
      - [MacOS/Linux setup](#macoslinux-setup)
    - [Notes: Azure CLI](#notes-azure-cli)
  - [Mandatory: Kubernetes CLI (kubectl)](#mandatory-kubernetes-cli-kubectl)
      - [Windows setup](#windows-setup-1)
      - [MacOS/Linux setup](#macoslinux-setup-1)
  - [Helm (optional)](#helm-optional)
      - [Windows setup](#windows-setup-2)
      - [MacOS/Linux setup](#macoslinux-setup-2)
  - [Mandatory: Docker CLI](#mandatory-docker-cli)
- [Azure Setup](#azure-setup)
  - [Create a resource group](#create-a-resource-group)
  - [Create a service bus namespace](#create-a-service-bus-namespace)
    - [Create a queue OrdersQueue](#create-a-queue-ordersqueue)
    - [Create a topic OrdersTopic](#create-a-topic-orderstopic)
    - [Create OrdersSubscription subscription for OrdersTopic](#create-orderssubscription-subscription-for-orderstopic)
    - [Get the Azure Service Bus connection string](#get-the-azure-service-bus-connection-string)
  - [Create an App Insights resource](#create-an-app-insights-resource)
    - [Get the App Insights connection](#get-the-app-insights-connection)
    - [Get the App Insights instrumentation key](#get-the-app-insights-instrumentation-key)
- [Local machine setup](#local-machine-setup)
  - [Clone the repo](#clone-the-repo)
  - [Switch to Docker context for Kubernetes](#switch-to-docker-context-for-kubernetes)
  - [Install KEDA](#install-keda)
  - [Install Dashboard](#install-dashboard)
    - [Troubleshoot. Check the process that is owner on port 8001](#troubleshoot-check-the-process-that-is-owner-on-port-8001)
    - [Start the dashboard](#start-the-dashboard)
    - [Setup access](#setup-access)
    - [Refresh the token](#refresh-the-token)
  - [Docker local registry](#docker-local-registry)
  - [RabbitMq](#rabbitmq)
    - [In Docker](#in-docker)
    - [In k8s with Helm](#in-k8s-with-helm)
      - [Deployment](#deployment)
      - [Forwarding ports to host](#forwarding-ports-to-host)
- [Review and update the configuration files](#review-and-update-the-configuration-files)
  - [Configuration keys explained](#configuration-keys-explained)
    - [File local.settings.json](#file-localsettingsjson)
    - [File appsettings*.json](#file-appsettingsjson)
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
    - [Verify the secrets](#verify-the-secrets)
      - [ASB Topic Trigger (Mandatory)](#asb-topic-trigger-mandatory)
      - [ASB Queue Trigger (Mandatory)](#asb-queue-trigger-mandatory)
      - [RabbitMq Queue Trigger (Optional)](#rabbitmq-queue-trigger-optional)
      - [RabbitMq Topic Trigger (Optional)](#rabbitmq-topic-trigger-optional)
  - [Playing and testing](#playing-and-testing)
    - [Build the solution](#build-the-solution)
    - [Create RabbitMq demo endpoints (Mandatory)](#create-rabbitmq-demo-endpoints-mandatory)
      - [Verifying the endpoints](#verifying-the-endpoints)
    - [Play with Azure Service Bus](#play-with-azure-service-bus)
      - [Send a message and verify](#send-a-message-and-verify)
      - [Publish a message and verify](#publish-a-message-and-verify)
    - [Play with RabbitMq](#play-with-rabbitmq)
      - [Send a message and verify](#send-a-message-and-verify-1)
      - [Publish a message and verify](#publish-a-message-and-verify-1)
  - [Removing k8s resources](#removing-k8s-resources)

# Description

The current repo serves as a playground to demonstrate how to switch an Azure Service Bus to RabbitMq in the context of an Azure Function and local development environment.
It is trying to solve the lack of a local emulator for Azure Service Bus. The issue is mentioned by Jimmy Bogard in this [article](https://jimmybogard.com/local-development-with-azure-service-bus/).
It provides a local, completly isolated dev environment for Azure Functions inside local Kubernetes infrastructure.

The proposed solution is using:
- a Kubernetes in Docker infrastructure
- KEDA for event driven scaling of Azure Functions instances
- HELM (optional) for deploying RabbitMq to K8s
- Azure Application Insights (optional) for monitoring

The switching procedure is based on a custom configuration (named LocalDev) that is conditionally processed in the Azure Functions csproj files.

Note: Because RabbitMq does not have an equivalent for the ASB topic, the topics will be simulated as queues (work in progress to switch to exchange and keys)

## Solution structure

```ascii
├───docker-files
├───docs
├───k8s
└───src
    ├───AzureFunction.MassTransit.Demo.Core
    │   └───Consumers
    ├───AzureFunction.MassTransit.Dual.DemoQueue
    │   └───Properties
    ├───AzureFunction.MassTransit.Dual.DemoTopic
    │   └───Properties
    ├───EndpointCreator.MassTransit.Rmq.Demo
    │   └───Properties
    ├───HeroDomain.Contracts
    ├───Publisher.MassTransit.Asb.Demo
    │   └───Properties
    ├───Publisher.MassTransit.Demo.Core
    └───Publisher.MassTransit.Rmq.Demo
        └───Properties
```
### Required nuget feed

The demo is using https://www.myget.org/F/matei-tm/api/v2 feed for the prerelease nugets of MassTransit.WebJobs.Extensions.RabbitMQ
See the [Nuget.Config](NuGet.Config) file

### The dual behavior of the AzureFunction.MassTransit.Dual.Demo* csproj files

The Azure functions project files are containing package references for both Asb and RabbitMq, but depending on the configuration value, at build time, they will generate a clean assembly.

![fig](./docs/af_proj.png)

### The dual behavior of the AzureFunction.MassTransit.Dual.DemoQueue SubmitOrderFunction class

The conditional compilation is done by using #if/#else/#endif preprocessor directives

![fig](./docs/queue_trigger_class.png)

### The dual behavior of the AzureFunction.MassTransit.Dual.DemoTopic SubmitOrderFunction class

The conditional compilation is done by using #if/#else/#endif preprocessor directives

![fig](./docs/topic_trigger_class.png)

# References

- [Jimmy Bogard - Local Development with Azure Service Bus](https://jimmybogard.com/local-development-with-azure-service-bus/)
- [Chris Patterson - Modern .NET Messaging using MassTransit](https://www.youtube.com/watch?v=jQNQDLv7QmU)
- [MassTransit Webjobs with RabbitMq](https://github.com/matei-tm/MassTransit/tree/webjobs-rabbitmq-integration)
- [MassTransit Demo app](https://github.com/matei-tm-csv/AzureFunction.Demo/tree/develop)

## Nice to read

- [Application Insights for Worker Service applications](https://docs.microsoft.com/en-us/azure/azure-monitor/app/worker-service)


# Prerequisites 

For the sake of simplicity, the installation tasks will be performed with:

- [Chocolatey](https://chocolatey.org/) for Windows OS
- [Homebrew](https://formulae.brew.sh/) for MacOS/Linux OS

## Mandatory: An [Azure subscription](https://portal.azure.com/#home)

The current demo can be completed with a minimal subscription. It can be:

- a student version with 100$ [credit](https://www.microsoftazuresponsorships.com/Balance)
- a dev essentials version with 200$ [credit](https://www.microsoftazuresponsorships.com/Balance)
- a pay-as-you-go (depending on the speed of progress, it will charge less than 20$)

## Mandatory: Azure CLI (az)

Check [here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) for installation instructions.

#### Windows setup

```powwershell
# open an administrative Power Shell console
choco install azure-cli
```

#### MacOS/Linux setup

```bash
brew install azure-cli
```

### Notes: Azure CLI

If you are using the local installation of the Azure CLI and you are managing several tenants and subscriptions, run the ```az login``` command first and add your subscription. See [here](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli) different authentication methods.

```bash
# accessing from localhost
az login
```

## Mandatory: Kubernetes CLI (kubectl)

Check [here](https://kubernetes.io/docs/tasks/tools/) for installation instructions.

#### Windows setup

```powwershell
# open an administrative Power Shell console
choco install kubernetes-cli
```

#### MacOS/Linux setup

```bash
brew install kubernetes-cli
```

## Helm (optional)

Check [here](https://krew.sigs.k8s.io/docs/user-guide/setup/install/) for installation instructions.

#### Windows setup

```powwershell
# open an administrative Power Shell console
choco install kubernetes-helm
```

#### MacOS/Linux setup

```bash
brew install kubernetes-helm
```


## Mandatory: Docker CLI

In order to be able to build the custom images containing disks with Iso files, the docker CLI is needed. Install Docker Desktop on your localbox. Depending on your OS use the proper installation guide:

- Mac https://docs.docker.com/docker-for-mac/install/
- Windows https://docs.docker.com/docker-for-windows/install/


# Azure Setup

If you want to play with Azure Service Bus, a preliminary setup is required.
The following actions are using the Azure CLI. As well you can use the Azure portal to complete them. Finally we will need:

- A service bus namespace
- A queue named OrdersQueue
- A topic named OrdersTopic
- A subscription OrdersSubscription to the OrdersTopic
- A connection string to the service bus

## Create a resource group

All the resources will be created under a single resource group named **k8s**. Having everything in one basket will permit to purge all resources in a single step and cut all the subsequent costs. The following command is using westeurope as location. If it's the case, change it according to your own needs.

```bash
az group create --location westeurope -n k8s 
```

![image](docs/az_group_create.png)

## Create a service bus namespace

Run the following command to create a Service Bus messaging namespace.
In order to avoid naming conflicts generated by another player of this demo, replace **DemoSb04** with your own name of the Service Bus namespace.

```
az servicebus namespace create --resource-group k8s --name DemoSb04 --location westeurope
```

![image](docs/az_asbnamespace_create.png.png)

### Create a queue OrdersQueue

Run the following command to create a queue in the namespace you created in the previous step.  
Replace **DemoSb04** with your own name of the Service Bus namespace.

```
az servicebus queue create --resource-group k8s --namespace-name DemoSb04 --name OrdersQueue
```

![image](docs/az_queue_create.png)

### Create a topic OrdersTopic

Run the following command to create a topic in the namespace you created in the previous step.  
Replace **DemoSb04** with your own name of the Service Bus namespace.

```
az servicebus topic create --resource-group k8s --namespace-name DemoSb04 --name OrdersTopic
```

![image](docs/az_topic_create.png)

### Create OrdersSubscription subscription for OrdersTopic

Run the following command to create a subscription to the topic you created in the previous step.  
Replace **DemoSb04** with your own name of the Service Bus namespace.

```
az servicebus topic subscription create --resource-group k8s --namespace-name DemoSb04 --topic-name OrdersTopic --name OrdersSubscription
```

![image](docs/az_subscription_create.png)

### Get the Azure Service Bus connection string

Run the following command to get the primary connection string for the namespace. You will use this connection string to connect to the queue/topic and send and receive messages.
Replace **DemoSb04** with your own name of the Service Bus namespace.

```
az servicebus namespace authorization-rule keys list --resource-group k8s --namespace-name DemoSb04 --name RootManageSharedAccessKey --query primaryConnectionString --output tsv
```

Note down the connection string and the queue name. You use them to send and receive messages.

![image](docs/az_asb_connection.png)

## Create an App Insights resource

If you want to monitor the applications create an App Insights resource and provide the connection information to the configuration files.

```bash
az extension add -n application-insights
az monitor app-insights component create --app demoApp --location westeurope --kind web --resource-group k8s --application-type web
```

![image](docs/az_create_appins.png)
### Get the App Insights connection 

```bash
# bash/zsh
az monitor app-insights component show --app demoApp --resource-group k8s | grep connection
```

![image](docs/az_appins_key.png)

### Get the App Insights instrumentation key 

```bash
# bash/zsh
az monitor app-insights component show --app demoApp --resource-group k8s | grep instrumentationKey
```

# Local machine setup

## Clone the repo

Clone the repo and switch to that folder

```
git clone https://github.com/matei-tm/AzureFunction.Demo
cd AzureFunction.Demo
```

## Switch to Docker context for Kubernetes

Assure that current kubectl context is pointing to docker desktop

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

If the port 8001 is captured by another app, get the app name, verify it and close/kill it.

```Powershell
# Powershell
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

Access to the dashboard is expiring after a period of inactivity. In order to generate a new token, execute the following command in bash terminal

```
kubectl -n kubernetes-dashboard get secret $(kubectl -n kubernetes-dashboard get sa/admin-user -o jsonpath="{.secrets[0].name}") -o go-template="{{.data.token | base64decode}}"
```

## Docker local registry

We need a local docker registry to host the images. Create a local [registry](https://docs.docker.com/registry/)

```bash
docker run -d -p 5000:5000 --name registry registry:2
```

## RabbitMq

For the RabbitMq service, two mutually exclusive options were provided:

- as a Docker container (recommended, with fewer steps)
- as k8s deployment using Helm

Use that one, that fits better your curiosity.

### In Docker

Create a RabbitMq container

```
docker run -d --hostname my-rabbit --name some-rabbit -p 8080:15672 -p 5672:5672 rabbitmq:3-management
```

Connection string for Azure Functions amqp://guest:guest@host.docker.internal:5672
Connection string for Publisher amqp://guest:guest@127.0.0.1:5672
RabbitMq Dashboard http://127.0.0.1:8080/ with username:guest and password:guest

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




# Review and update the configuration files

Replace the connection strings for ASB/RabbitMq/AppInsights in the following files. Use the values provided in previous steps

```ascii
.
├── docker-files
├── docs
├── k8s
└── src
    ├── AzureFunction.MassTransit.Demo.Core
    │   └── Consumers
    ├── AzureFunction.MassTransit.Dual.DemoQueue
    │   ├── Properties
    │   ├── host.json
    │   └── local.settings.json
    ├── AzureFunction.MassTransit.Dual.DemoTopic
    │   ├── Properties
    │   ├── host.json
    │   └── local.settings.json
    ├── EndpointCreator.MassTransit.Rmq.Demo
    │   ├── Properties
    │   ├── appsettings.Development.json
    │   ├── appsettings.json
    │   └── host.json
    ├── HeroDomain.Contracts
    ├── Publisher.MassTransit.Asb.Demo
    │   ├── Properties
    │   ├── appsettings.Development.json
    │   └── appsettings.json
    ├── Publisher.MassTransit.Demo.Core
    └── Publisher.MassTransit.Rmq.Demo
        ├── Properties
        ├── appsettings.Development.json
        ├── appsettings.json
        └── host.json
```

Optional, the secrets for publishers can be provided through development [safe storage](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows) feature.

## Configuration keys explained

### File local.settings.json

| Key         | Description     | Retrieve section |
|--------------|-----------|------------|
| APPINSIGHTS_INSTRUMENTATIONKEY | App Insights instrumentation key      | [view](#get-the-app-insights-instrumentation-key)        |
| APPLICATIONINSIGHTS_CONNECTION_STRING      | App Insights connection string  | [view](#get-the-app-insights-connection)       |
| AzureWebJobsServiceBus      | the ASB endpoint for the Azure Function trigger  | [view](#get-the-azure-service-bus-connection-string)       |
| ServiceBus      | the ASB endpoint for the consumers configured to handle the message received by the trigger. The key name is the default value requested by MassTransit  | [view](#get-the-azure-service-bus-connection-string)      |


### File appsettings*.json

 Key         | Description     | Retrieve section |
|--------------|-----------|------------|
| ApplicationInsights.InstrumentationKey | App Insights instrumentation key      | [view]
| AppConfig.ServiceBusConnectionString      | the ASB endpoint to publish/send messages  | [view](#get-the-azure-service-bus-connection-string)       |

Other keys can stay with the current values

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

![error](docs/deployerror.png)

Being a local, private registry the host.docker.internal must be added to insecure-registries section in Docker config file.

![fix](docs/registryerrorfix.png)


### Verify the secrets

The keys contained by local.settings.json are deployed as secrets into k8s.

![image](docs/secrets.png)

Check the stored values to be in sync with the collected values from az cli output. Check [File local.settings.json](#file-localsettingsjson) section for more info

#### ASB Topic Trigger (Mandatory)

Go to http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/#/secret/rabbit/af-masstransit-asb-topic?namespace=rabbit and verify the following keys:

- APPINSIGHTS_INSTRUMENTATIONKEY
- APPLICATIONINSIGHTS_CONNECTION_STRING
- AzureWebJobsServiceBus - the ASB endpoint for the Azure Function trigger
- ServiceBus - the ASB endpoint for the consumers configured by MassTransit

#### ASB Queue Trigger (Mandatory)

Go to http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/#/secret/rabbit/af-masstransit-asb-queue?namespace=rabbit and verify the following keys:

- APPINSIGHTS_INSTRUMENTATIONKEY
- APPLICATIONINSIGHTS_CONNECTION_STRING
- AzureWebJobsServiceBus - the ASB endpoint for the Azure Function trigger
- ServiceBus - the ASB endpoint for the consumers configured by MassTransit

#### RabbitMq Queue Trigger (Optional)

Go to http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/#/secret/rabbit/af-masstransit-rmq-queue?namespace=rabbit and verify the following keys:

- APPINSIGHTS_INSTRUMENTATIONKEY
- APPLICATIONINSIGHTS_CONNECTION_STRING

#### RabbitMq Topic Trigger (Optional)

Go to http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/#/secret/rabbit/af-masstransit-rmq-topic?namespace=rabbit and verify the following keys:

- APPINSIGHTS_INSTRUMENTATIONKEY
- APPLICATIONINSIGHTS_CONNECTION_STRING

## Playing and testing


### Build the solution

You can build any of Debug/Release/LocalDev configuration

```bash
dotnet build src/AzureFunction.Demo.sln --configuration Debug
```

### Create RabbitMq demo endpoints (Mandatory)

Run the following application in order to create the queues and exchanges needed by the demo.

```
./src/EndpointCreator.MassTransit.Rmq.Demo/bin/Debug/net6.0/EndpointCreator.MassTransit.Rmq.Demo.exe
```

#### Verifying the endpoints

Open http://127.0.0.1:8080/#/queues (user:guest, pwd: guest) and verify the queues to be as in the following figure

![image](docs/rmq_endpoints.png)

### Play with Azure Service Bus

In a new terminal run the following commands

```bash
cd src/Publisher.MassTransit.Asb.Demo/bin/Debug/net6.0
./Publisher.MassTransit.Asb.Demo.exe
```
#### Send a message and verify

Sending a message will use the OrdersQueue queue

![image](docs/asb_send.png)

Check in k8s dashboard on Pods section if a replica af-masstransit-asb-**queue**-* is launched (the launcher can have a 30 sec delay).
Open the pod and check the logs.

![image](docs/asb_receive_from_send.png)

#### Publish a message and verify

Publishing a message will use the OrdersTopic topic

![image](docs/asb_publish.png)

Check in k8s dashboard on Pods section if a replica af-masstransit-asb-**topic**-* is launched (the launcher can have a 30 sec delay).
Open the pod and check the logs.

![image](docs/asb_receive_from_publish.png)

### Play with RabbitMq

In a new dos_command/powershell terminal run the following commands

```bash
cd src/Publisher.MassTransit.Rmq.Demo/bin/Debug/net6.0
./Publisher.MassTransit.Rmq.Demo.exe
```

#### Send a message and verify

Sending a message will use the OrdersQueue

![image](docs/rmq_send.png)

Check in k8s dashboard on Pods section if a replica af-masstransit-rmq-queue-* is launched (the launcher can have a 30 sec delay).
Open the pod and check the logs.

![image](docs/rmq_receive_from_send.png)

#### Publish a message and verify

Publishing a message will use the OrdersTopic queue (no topic equivalent in RabbitMq)

![image](docs/rmq_publish.png)

Check in k8s dashboard on Pods section if a replica af-masstransit-rmq-**topic**p is launched (the launcher can have a 30 sec delay).
Open the pod and check the logs.

![image](docs/rmq_receive_from_publish.png)

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


