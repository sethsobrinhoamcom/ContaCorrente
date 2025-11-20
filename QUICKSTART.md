# 🚀 Quick Start Guide

## Opção 1: Executar Localmente (Mais Rápido)
```bash
# 1. Clone e navegue
git clone <repo-url>
cd contacorrente-api

# 2. Restaure e execute
dotnet restore
cd src/ContaCorrente.Api
dotnet run

# 3. Acesse
open http://localhost:5000
```

## Opção 2: Docker
```bash
# 1. Build e run
docker-compose up -d

# 2. Acesse
open http://localhost:5000

# 3. Parar
docker-compose down
```

## Opção 3: Kubernetes
```bash
# 1. Build da imagem
docker build -t contacorrente-api:latest .

# 2. Deploy
kubectl apply -f k8s/

# 3. Port forward
kubectl port-forward service/contacorrente-service 5000:80

# 4. Acesse
open http://localhost:5000
```

## Testar a API
```bash
# Criar conta
curl -X POST http://localhost:5000/api/contacorrente \
  -H "Content-Type: application/json" \
  -d '{
    "numero": 12345,
    "nome": "João Silva",
    "senha": "senha123"
  }'

# Obter conta (use o ID retornado)
curl http://localhost:5000/api/contacorrente/{id}
```