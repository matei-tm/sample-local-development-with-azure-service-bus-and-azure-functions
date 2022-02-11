- [References](#references)
- [Local machine setup](#local-machine-setup)
  - [RabbitMq](#rabbitmq)
  - [Switch to Docker context for Kubernetes](#switch-to-docker-context-for-kubernetes)
  - [Install KEDA](#install-keda)
  - [Install Dashboard](#install-dashboard)
    - [Troubleshoot. Check the process that is owner on port 8001](#troubleshoot-check-the-process-that-is-owner-on-port-8001)
    - [Start the dashboard](#start-the-dashboard)
    - [Setup access](#setup-access)
    - [Refresh the token](#refresh-the-token)
  - [Docker local registry](#docker-local-registry)
  - [Deploy](#deploy)
    - [to registry](#to-registry)
      - [Registry test](#registry-test)
    - [to k8s](#to-k8s)
    - [Removing](#removing)

# References

- [Jimmy Bogard - Local Development with Azure Service Bus](https://jimmybogard.com/local-development-with-azure-service-bus/)
- [Chris Patterson - Modern .NET Messaging using MassTransit](https://www.youtube.com/watch?v=jQNQDLv7QmU)


# Local machine setup

## RabbitMq

```
docker run -d --hostname my-rabbit --name some-rabbit -p 8080:15672 -p 5672:5672 rabbitmq:3-management
```


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
func kubernetes deploy --name af-masstransit-dual --registry localhost:5000/af-masstransit-dual-demo
```

### Removing

```
kubectl delete deploy <name-of-function-deployment>
kubectl delete ScaledObject <name-of-function-deployment>
kubectl delete secret <name-of-function-deployment>
```


