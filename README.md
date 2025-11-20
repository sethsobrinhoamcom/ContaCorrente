# 🏦 Conta Corrente API

API REST para gerenciamento de contas correntes e transferências bancárias, desenvolvida com .NET 8, seguindo princípios de Clean Architecture e CQRS.

## 📋 Índice

- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Executando a Aplicação](#executando-a-aplicação)
- [Executando com Docker](#executando-com-docker)
- [Executando com Kubernetes](#executando-com-kubernetes)
- [Testes](#testes)
- [Endpoints da API](#endpoints-da-api)
- [Idempotência](#idempotência)
- [Estrutura do Projeto](#estrutura-do-projeto)

## 🚀 Tecnologias

- **.NET 8** - Framework principal
- **C# 12** - Linguagem de programação
- **SQLite** - Banco de dados
- **Dapper** - Micro ORM
- **MediatR** - Implementação do padrão CQRS
- **FluentValidation** - Validação de dados
- **FluentResults** - Tratamento de resultados
- **Swagger/OpenAPI** - Documentação da API
- **Docker** - Containerização
- **Kubernetes** - Orquestração de containers
- **xUnit** - Framework de testes
- **Moq** - Mock para testes
- **FluentAssertions** - Assertions para testes

## 🏛️ Arquitetura

O projeto segue os princípios de **Clean Architecture** e **CQRS**:
```
src/
├── ContaCorrente.Api/           # Camada de apresentação (Controllers, Middleware)
├── ContaCorrente.Application/   # Camada de aplicação (Use Cases, DTOs)
├── ContaCorrente.Domain/        # Camada de domínio (Entidades, Interfaces)
└── ContaCorrente.Infrastructure/ # Camada de infraestrutura (Repositórios, DB)
```

### Padrões Utilizados

- **CQRS** (Command Query Responsibility Segregation)
- **Repository Pattern**
- **Mediator Pattern**
- **Dependency Injection**
- **Unit of Work** (implícito via transações)

## 📦 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (opcional)
- [kubectl](https://kubernetes.io/docs/tasks/tools/) (opcional)

## 💻 Instalação

### 1. Clone o repositório
```bash
git clone https://github.com/sethsobrinhoamcom/contacorrente-api.git
cd contacorrente-api
```

### 2. Restaurar dependências
```bash
dotnet restore
```

### 3. Compilar o projeto
```bash
dotnet build
```

## ▶️ Executando a Aplicação

### Modo Development
```bash
cd src/ContaCorrente.Api
dotnet run
```

A API estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000` ou `https://localhost:5001`

### Variáveis de Ambiente
```bash
# Connection String
export ConnectionStrings__DefaultConnection="Data Source=contacorrente.db"

# Environment
export ASPNETCORE_ENVIRONMENT="Development"
```

## 🐳 Executando com Docker

### Build da imagem
```bash
docker build -t contacorrente-api:latest .
```

### Executar container
```bash
docker run -d \
  --name contacorrente-api \
  -p 5000:8080 \
  -e ConnectionStrings__DefaultConnection="Data Source=/app/data/contacorrente.db" \
  -v contacorrente-data:/app/data \
  contacorrente-api:latest
```

### Usando Docker Compose
```bash
# Iniciar
docker-compose up -d

# Ver logs
docker-compose logs -f

# Parar
docker-compose down

# Parar e remover volumes
docker-compose down -v
```

Acesse: `http://localhost:5000`

## ☸️ Executando com Kubernetes

### Pré-requisitos

Certifique-se de ter o Kubernetes rodando (Docker Desktop, Minikube, etc.)

### 1. Habilitar Kubernetes no Docker Desktop

1. Abra Docker Desktop
2. Settings > Kubernetes
3. Marque "Enable Kubernetes"
4. Apply & Restart

### 2. Build da imagem
```bash
docker build -t contacorrente-api:latest .
```

### 3. Deploy no Kubernetes
```bash
# Aplicar todos os manifestos
kubectl apply -f k8s/

# OU aplicar individualmente
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/hpa.yaml
```

### 4. Verificar o deployment
```bash
# Ver pods
kubectl get pods

# Ver services
kubectl get services

# Ver deployments
kubectl get deployments

# Ver detalhes do pod
kubectl describe pod <pod-name>

# Ver logs
kubectl logs <pod-name>
```

### 5. Acessar a aplicação
```bash
# Obter a URL do serviço
kubectl get service contacorrente-service

# Se estiver usando LoadBalancer local
# Acesse: http://localhost
```

### 6. Comandos úteis do Kubernetes
```bash
# Port forward para acessar localmente
kubectl port-forward service/contacorrente-service 5000:80

# Escalar manualmente
kubectl scale deployment contacorrente-api --replicas=5

# Ver HPA status
kubectl get hpa

# Deletar tudo
kubectl delete -f k8s/
```

## 🧪 Testes

### Executar todos os testes
```bash
dotnet test
```

### Executar com cobertura
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Executar testes específicos
```bash
dotnet test --filter "FullyQualifiedName~CriarContaCorrenteTests"
```

### Ver relatório de cobertura
```bash
# Instalar ferramenta (uma vez)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Gerar relatório
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Abrir relatório
open coveragereport/index.html  # macOS
start coveragereport/index.html # Windows
xdg-open coveragereport/index.html # Linux
```

## 📚 Endpoints da API

### Conta Corrente

#### Criar Conta Corrente
```http
POST /api/contacorrente
Content-Type: application/json

{
  "numero": 12345,
  "nome": "João Silva",
  "senha": "senha123"
}
```

**Resposta (201 Created):**
```json
{
  "idContaCorrente": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
}
```

#### Obter Conta Corrente
```http
GET /api/contacorrente/{id}
```

**Resposta (200 OK):**
```json
{
  "idContaCorrente": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "numero": 12345,
  "nome": "João Silva",
  "ativo": true,
  "saldo": 1000.50
}
```

#### Obter Extrato
```http
GET /api/contacorrente/{id}/extrato?dataInicio=2024-01-01&dataFim=2024-12-31
```

**Resposta (200 OK):**
```json
[
  {
    "idMovimento": "550e8400-e29b-41d4-a716-446655440000",
    "dataMovimento": "19/11/2024 10:30:00",
    "tipoMovimento": "C",
    "valor": 500.00
  },
  {
    "idMovimento": "660e8400-e29b-41d4-a716-446655440000",
    "dataMovimento": "19/11/2024 14:45:00",
    "tipoMovimento": "D",
    "valor": 100.00
  }
]
```

### Transferência

#### Realizar Transferência
```http
POST /api/transferencia
Content-Type: application/json
X-Idempotency-Key: 123e4567-e89b-12d3-a456-426614174000

{
  "idContaCorrenteOrigem": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "idContaCorrenteDestino": "a37bc10b-48cc-5372-b567-1e02c2c3d489",
  "valor": 250.00
}
```

**Resposta (200 OK):**
```json
{
  "idTransferencia": "770e8400-e29b-41d4-a716-446655440000",
  "mensagem": "Transferência realizada com sucesso"
}
```

### Health Check
```http
GET /health
```

**Resposta (200 OK):**
```json
{
  "status": "Healthy",
  "timestamp": "2024-11-19T15:30:00Z",
  "version": "1.0.0"
}
```

## 🔐 Idempotência

A API suporta idempotência em operações de transferência através do header `X-Idempotency-Key`.

### Como funciona

1. Cliente envia uma requisição com uma chave única no header
2. API processa e armazena o resultado
3. Requisições subsequentes com a mesma chave retornam o resultado armazenado

### Exemplo
```bash
# Primeira requisição
curl -X POST http://localhost:5000/api/transferencia \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: minha-chave-unica-123" \
  -d '{
    "idContaCorrenteOrigem": "origem-id",
    "idContaCorrenteDestino": "destino-id",
    "valor": 100.00
  }'

# Segunda requisição (retorna o mesmo resultado sem processar novamente)
curl -X POST http://localhost:5000/api/transferencia \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: minha-chave-unica-123" \
  -d '{
    "idContaCorrenteOrigem": "origem-id",
    "idContaCorrenteDestino": "destino-id",
    "valor": 100.00
  }'
```

## 📁 Estrutura do Projeto
```
ContaCorrente.Api/
├── src/
│   ├── ContaCorrente.Api/
│   │   ├── Controllers/
│   │   │   ├── ContaCorrenteController.cs
│   │   │   ├── TransferenciaController.cs
│   │   │   └── HealthController.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── ValidationBehavior.cs
|   |   ├── Models
|   |   |   ├── Request
|   |   |   |   ├── 
|   |   |   ├── Response
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── ContaCorrente.Application/
│   │   ├── DTOs/
│   │   ├── UseCases/
│   │   │   ├── ContasCorrentes/
│   │   │   │   ├── Commands/
│   │   │   │   └── Queries/
│   │   │   └── Transferencias/
│   │   │       └── Commands/
│   │
│   ├── ContaCorrente.Domain/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   └── Services/
│   │
│   └── ContaCorrente.Infrastructure/
│       ├── Data/
│       ├── Repositories/
│       └── Services/
│
├── tests/
│   └── ContaCorrente.Tests/
│       └── UseCases/
│
├── k8s/
│   ├── configmap.yaml
│   ├── deployment.yaml
│   ├── service.yaml
│   └── hpa.yaml
│
├── scripts/
│   ├── build-and-push.sh
│   ├── deploy-k8s.sh
│   └── test.sh
│
├── docker-compose.yml
├── Dockerfile
├── .dockerignore
├── .gitignore
└── README.md
```

## 🔧 Configuração

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=contacorrente.db"
  }
}
```

### Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | `Development` |
| `ASPNETCORE_URLS` | URLs de binding | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | String de conexão SQLite | `Data Source=contacorrente.db` |

## 📊 Monitoramento

### Métricas do Kubernetes
```bash
# CPU e Memória dos pods
kubectl top pods

# Status do HPA
kubectl get hpa contacorrente-hpa

# Eventos
kubectl get events --sort-by=.metadata.creationTimestamp
```

### Logs
```bash
# Docker
docker logs contacorrente-api -f

# Kubernetes
kubectl logs -f deployment/contacorrente-api

# Docker Compose
docker-compose logs -f
```

## 🐛 Troubleshooting

### Problema: Porta já em uso
```bash
# Verificar processo usando a porta
lsof -i :5000  # macOS/Linux
netstat -ano | findstr :5000  # Windows

# Matar processo
kill -9 <PID>  # macOS/Linux
taskkill /PID <PID> /F  # Windows
```

### Problema: Banco de dados locked
```bash
# Remover arquivo de lock
rm contacorrente.db-shm
rm contacorrente.db-wal
```

### Problema: Imagem Docker não atualiza
```bash
# Rebuild sem cache
docker build --no-cache -t contacorrente-api:latest .

# Limpar imagens antigas
docker image prune -a
```

### Problema: Pods não iniciam no Kubernetes
```bash
# Verificar eventos
kubectl describe pod <pod-name>

# Verificar logs
kubectl logs <pod-name>

# Forçar recriação
kubectl rollout restart deployment/contacorrente-api
```

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 👥 Autores

- **Seth Sobrinho** - [GitHub](https://github.com/sethsobrinhoamcom)

## 📞 Contato

- Email: seth.sobrinho@amcom.com.br
