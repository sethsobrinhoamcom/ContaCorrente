#!/bin/bash

echo "Deploying to Kubernetes..."

# Criar namespace (se não existir)
kubectl create namespace contacorrente --dry-run=client -o yaml | kubectl apply -f -

# Aplicar ConfigMap
kubectl apply -f k8s/configmap.yaml -n contacorrente

# Aplicar Deployment
kubectl apply -f k8s/deployment.yaml -n contacorrente

# Aplicar Service
kubectl apply -f k8s/service.yaml -n contacorrente

# Aplicar HPA
kubectl apply -f k8s/hpa.yaml -n contacorrente

# Verificar status
echo "Waiting for deployment..."
kubectl rollout status deployment/contacorrente-api -n contacorrente

# Listar pods
kubectl get pods -n contacorrente

# Listar services
kubectl get services -n contacorrente

echo "Deployment completed!"