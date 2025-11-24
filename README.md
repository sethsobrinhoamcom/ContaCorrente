# 🏦 BankMore - Sistema Bancário Digital

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-326CE5?logo=kubernetes)](https://kubernetes.io/)
[![Kafka](https://img.shields.io/badge/Apache%20Kafka-Integrated-231F20?logo=apache-kafka)](https://kafka.apache.org/)
[![JWT](https://img.shields.io/badge/JWT-Authentication-000000?logo=json-web-tokens)](https://jwt.io/)

Sistema bancário digital completo com operações de conta corrente, transferências, autenticação JWT e processamento assíncrono de eventos com Apache Kafka.

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Funcionalidades](#funcionalidades)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Requisitos](#requisitos)
- [Instalação Rápida](#instalação-rápida)
- [Executando](#executando)
- [Testes](#testes)
- [API Endpoints](#api-endpoints)
- [Autenticação JWT](#autenticação-jwt)
- [Kafka](#kafka)
- [Docker](#docker)
- [Kubernetes](#kubernetes)
- [Estrutura do Projeto](#estrutura-do-projeto)

## 🎯 Visão Geral

BankMore é uma fintech fictícia desenvolvida seguindo os princípios de **Clean Architecture**, **DDD** e **CQRS**. O sistema oferece funcionalidades bancárias essenciais com foco em segurança, escalabilidade e processamento assíncrono.

### Destaques

- ✅ **Autenticação JWT** em todos os endpoints protegidos
- ✅ **Validação de CPF** brasileira
- ✅ **Idempotência** para operações críticas
- ✅ **Event-Driven Architecture** com Kafka
- ✅ **Clean Architecture** + **CQRS** + **DDD**
- ✅ **Testes Automatizados** com xUnit
- ✅ **Docker** e **Kubernetes** ready
- ✅ **Sistema de Tarifas** assíncrono

## ⚡ Funcionalidades

### Operações Bancárias

| Operação | Tarifa | Limite |
|----------|--------|--------|
| **Criar Conta** | Grátis | - |
| **Depósito** | Grátis | R$ 10.000 |
| **Saque** | R$ 0,50 | R$ 5.000 |
| **Transferência** | R$ 1,00 | Ilimitado |
| **Consulta Saldo** | Grátis | - |
| **Extrato** | Grátis | - |

### Recursos Técnicos

- 🔐 **Autenticação JWT** com Bearer Token
- 🔒 **Senha criptografada** (SHA256 + Salt)
- ✔️ **Validação de CPF** completa
- 🔄 **Idempotência** via `X-Idempotency-Key`
- 📊 **Eventos assíncronos** no Kafka
- 🚫 **Contas inativas** bloqueadas
- ⚠️ **Tipos de erro** padronizados
- 📝 **Auditoria** completa de operações

## 🏛️ Arquitetura

### Clean Architecture + CQRS
```
┌─────────────────────────────────────────────────────────┐
│                    Presentation                          │
│  • Controllers (Auth, ContaCorrente, Transferencia)     │
│  • Middleware (JWT, Exception Handling)                 │
│  • Swagger/OpenAPI                                      │
└───────────────────┬─────────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────────┐
│                  Application                             │
│  • Commands (CriarConta, Deposito, Saque, Transfer)    │
│  • Queries (ObterConta, ObterExtrato, ObterSaldo)      │
│  • Validators (FluentValidation)                        │
│  • DTOs                                                  │
└───────────────────┬─────────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────────┐
│                    Domain                                │
│  • Entities (ContaCorrente, Movimento, etc)             │
│  • Events (DepositoRealizadoEvent, etc)                 │
│  • Interfaces (IRepository, IService)                   │
│  • Enums (ErrorType)                                     │
│  • Exceptions (DomainException)                          │
└───────────────────┬─────────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────────┐
│               Infrastructure                             │
│  • Repositories (Dapper + SQLite)                       │
│  • Services (JWT, Password, CPF Validator)              │
│  • Kafka (Producer/Consumer)                            │
│  • Database Initializer                                 │
└─────────────────────────────────────────────────────────┘
```

### Event-Driven Architecture
```
API → Command Handler → Repository (DB)
                ↓
         EventPublisher
                ↓
          Kafka Broker
        /      |      \
depositos  saques  transferencias
        \      |      /
         EventConsumer
                ↓
    ┌──────────┼──────────┐
Notificações  Logs  Sistema Tarifas
                           ↓
                    (tarifacoes topic)
                           ↓
                    API Consumer
                           ↓
                    Débito Automático
```

## 🚀 Tecnologias

### Backend
- **.NET 8** - Framework
- **C# 12** - Linguagem
- **ASP.NET Core** - Web API
- **SQLite** - Banco de dados
- **Dapper** - Micro ORM
- **MediatR** - CQRS/Mediator
- **FluentValidation** - Validações
- **FluentResults** - Result Pattern

### Segurança
- **JWT Bearer** - Autenticação
- **SHA256 + Salt** - Hash de senhas
- **CPF Validator** - Validação brasileira

### Mensageria
- **Apache Kafka 7.5** - Message Broker
- **Confluent.Kafka** - Cliente .NET
- **Zookeeper** - Coordenação
- **Kafka UI** - Interface Web

### DevOps
- **Docker** - Containerização
- **Docker Compose** - Orquestração local
- **Kubernetes** - Orquestração produção
- **Swagger/OpenAPI** - Documentação

### Testes
- **xUnit** - Framework de testes
- **Moq** - Mocking
- **FluentAssertions** - Assertions

## 📦 Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

**Opcional:**
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Postman](https://www.postman.com/downloads/)
- [jq](https://stedolan.github.io/jq/) (para scripts)

## 🚀 Instalação Rápida
```bash
# 1. Clonar repositório
git clone <repo-url>
cd contacorrente-api

# 2. Setup completo automatizado
make setup

# OU manualmente:
./scripts/setup-complete.sh
```

## ▶️ Executando

### Opção 1: Localmente (Development)
```bash
# Terminal 1: Iniciar Kafka
docker-compose up -d zookeeper kafka kafka-ui

# Terminal 2: Iniciar API
cd src/ContaCorrente.Api
dotnet run

# Terminal 3: Iniciar Tarifas (OPCIONAL)
./scripts/run-tarifas.sh
```

**Acesso:**
- API: http://localhost:5000
- Swagger: http://localhost:5000
- Kafka UI: http://localhost:8080

### Opção 2: Docker Compose (Recomendado)
```bash
# Iniciar tudo
docker-compose up -d

# Ver logs
docker-compose logs -f contacorrente-api

# Parar tudo
docker-compose down
```

### Opção 3: Kubernetes
```bash
# Build
docker build -t contacorrente-api:latest .

# Deploy completo
kubectl apply -f k8s/

# Port forward
kubectl port-forward service/contacorrente-service 5000:80

# Verificar
kubectl get pods
kubectl get services
```

## 🧪 Testes

### Executar testes unitários
```bash
make test

# OU
dotnet test
```

### Teste End-to-End completo
```bash
make test-api

# OU
./scripts/test-api-jwt.sh
```

Este script testa:
- ✅ Criação de 2 contas
- ✅ Login e obtenção de JWT
- ✅ Depósito de R$ 1.000
- ✅ Saque de R$ 100 (+ tarifa R$ 0,50)
- ✅ Transferência de R$ 250 (+ tarifa R$ 1,00)
- ✅ Consulta de saldo
- ✅ Inativação de conta
- ✅ Validações de segurança

## 📚 API Endpoints

### Autenticação

#### Criar Conta (Público)
```http
POST /api/contacorrente
Content-Type: application/json

{
  "numero": 12345,
  "cpf": "12345678901",
  "nome": "João Silva",
  "senha": "senha123"
}
```

#### Login (Público)
```http
POST /api/auth/login
Content-Type: application/json

{
  "cpfOrNumeroConta": "12345678901",
  "senha": "senha123"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "idContaCorrente": "...",
  "numeroConta": "12345",
  "nome": "João Silva"
}
```

### Operações (Requer JWT)

Todos os endpoints abaixo requerem o header:
```
Authorization: Bearer {token}
```

#### Consultar Saldo
```http
GET /api/contacorrente/saldo
```

#### Realizar Depósito
```http
POST /api/contacorrente/deposito
Content-Type: application/json
X-Idempotency-Key: {guid}

{
  "valor": 500.00
}
```

#### Realizar Saque
```http
POST /api/contacorrente/saque
Content-Type: application/json
X-Idempotency-Key: {guid}

{
  "valor": 100.00
}
```

#### Transferência
```http
POST /api/transferencia
Content-Type: application/json
X-Idempotency-Key: {guid}

{
  "idContaCorrenteDestino": "...",
  "valor": 250.00
}
```

#### Obter Extrato
```http
GET /api/contacorrente/extrato?dataInicio=2024-01-01&dataFim=2024-12-31
```

#### Inativar Conta
```http
POST /api/contacorrente/inativar
Content-Type: application/json

{
  "senha": "senha123"
}
```

### Tipos de Erro
```json
{
  "message": "Mensagem descritiva",
  "errorType": "INVALID_DOCUMENT | USER_UNAUTHORIZED | INVALID_ACCOUNT | INACTIVE_ACCOUNT | INVALID_VALUE | INVALID_TYPE | INSUFFICIENT_BALANCE | INVALID_TOKEN | TOKEN_EXPIRED",
  "errors": ["lista de erros adicionais"]
}
```

## 🔐 Autenticação JWT

### Fluxo

1. **Criar Conta** → Endpoint público
2. **Login** → Recebe JWT token
3. **Usar Token** → Em todos os endpoints protegidos
4. **Token Expira** → Fazer login novamente (2 horas)

### Exemplo Completo
```bash
# 1. Criar conta
curl -X POST http://localhost:5000/api/contacorrente \
  -H "Content-Type: application/json" \
  -d '{"numero":12345,"cpf":"12345678901","nome":"João","senha":"senha123"}'

# 2. Login
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"cpfOrNumeroConta":"12345678901","senha":"senha123"}' \
  | jq -r '.token')

# 3. Usar token
curl http://localhost:5000/api/contacorrente/saldo \
  -H "Authorization: Bearer $TOKEN"
```

### Claims no Token
```json
{
  "id_conta_corrente": "...",
  "numero_conta": "12345",
  "cpf": "12345678901",
  "nameid": "...",
  "jti": "...",
  "exp": 1234567890,
  "iss": "BankMore",
  "aud": "BankMoreAPI"
}
```

## 📡 Kafka

### Tópicos

| Tópico | Partições | Eventos |
|--------|-----------|---------|
| `depositos` | 3 | DepositoRealizadoEvent |
| `saques` | 3 | SaqueRealizadoEvent |
| `transferencias` | 3 | TransferenciaRealizadaEvent |
| `tarifacoes` | 3 | TarifacaoRealizadaEvent |

### Fluxo de Tarifas
```
Transferência → Kafka (transferencias)
                      ↓
            Serviço de Tarifas
                      ↓
        Registra no DB + Kafka (tarifacoes)
                      ↓
              API Consumer
                      ↓
          Débito Automático
```

### Comandos Úteis
```bash
# Listar tópicos
docker exec kafka kafka-topics --list --bootstrap-server localhost:9092

# Ver mensagens
docker exec kafka kafka-console-consumer \
  --topic depositos \
  --bootstrap-server localhost:9092 \
  --from-beginning

# Consumer groups
docker exec kafka kafka-consumer-groups \
  --list \
  --bootstrap-server localhost:9092
```

### Kafka UI

Acesse http://localhost:8080 para interface visual completa.

## 🐳 Docker

### Serviços no Docker Compose
```yaml
- zookeeper       - Coordenação Kafka
- kafka           - Message Broker
- kafka-ui        - Interface Web
- contacorrente-api - API Principal
```

### Comandos
```bash
# Iniciar
docker-compose up -d

# Status
docker-compose ps

# Logs
docker-compose logs -f [service]

# Parar
docker-compose down

# Limpar tudo
docker-compose down -v
```

## ☸️ Kubernetes

### Manifests
```
k8s/
├── configmap.yaml          - Configurações
├── deployment.yaml         - API Deployment
├── service.yaml            - Service LoadBalancer
├── hpa.yaml                - Horizontal Pod Autoscaler
├── kafka-deployment.yaml   - Kafka + Zookeeper
```

### Deploy
```bash
# Deploy completo
kubectl apply -f k8s/

# Verificar
kubectl get all

# Logs
kubectl logs -f deployment/contacorrente-api

# Escalar
kubectl scale deployment contacorrente-api --replicas=5

# Port forward
kubectl port-forward service/contacorrente-service 5000:80
```

## 📁 Estrutura do Projeto
```
ContaCorrente.Api/
├── src/
│   ├── ContaCorrente.Api/              # Web API
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Program.cs
│   ├── ContaCorrente.Application/      # CQRS
│   │   ├── UseCases/
│   │   │   ├── Auth/Commands/Login/
│   │   │   ├── ContasCorrentes/
│   │   │   │   ├── Commands/
│   │   │   │   └── Queries/
│   │   │   └── Transferencias/Commands/
│   │   └── DTOs/
│   ├── ContaCorrente.Domain/           # Domínio
│   │   ├── Entities/
│   │   ├── Events/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   ├── Enums/
│   │   └── Exceptions/
│   ├── ContaCorrente.Infrastructure/   # Infra
│   │   ├── Data/
│   │   ├── Repositories/
│   │   ├── Services/
│   │   └── Messaging/
│   └── ContaCorrente.Tarifas/          # Serviço Tarifas
│       ├── Models/
│       ├── Services/
│       └── Program.cs
├── tests/
│   └── ContaCorrente.Tests/
├── k8s/                                 # Kubernetes
├── scripts/                             # Scripts auxiliares
├── postman/                             # Postman Collection
├── docker-compose.yml
├── Dockerfile
├── Makefile
└── README.md
```

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/NovaFuncionalidade`)
3. Commit (`git commit -m 'Adiciona nova funcionalidade'`)
4. Push (`git push origin feature/NovaFuncionalidade`)
5. Abra um Pull Request

## 📝 Licença

Este projeto está sob a licença MIT.

## 🎓 Aprendizado

Este projeto foi desenvolvido como parte de um teste técnico e demonstra:

- ✅ Clean Architecture
- ✅ Domain-Driven Design (DDD)
- ✅ CQRS Pattern
- ✅ Event-Driven Architecture
- ✅ Autenticação JWT
- ✅ Microsserviços
- ✅ Processamento Assíncrono
- ✅ Docker e Kubernetes
- ✅ Testes Automatizados

---

**Desenvolvido com ❤️ usando .NET 8, Apache Kafka e Clean Architecture**

🏦 **BankMore** - _Banking Made Modern_