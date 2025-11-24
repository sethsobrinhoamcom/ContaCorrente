# 🏦 BankMore - Sistema Bancário Digital

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12-239120?logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-326CE5?logo=kubernetes)](https://kubernetes.io/)
[![Apache Kafka](https://img.shields.io/badge/Apache%20Kafka-Integrated-231F20?logo=apache-kafka)](https://kafka.apache.org/)
[![JWT](https://img.shields.io/badge/JWT-Authentication-000000?logo=json-web-tokens)](https://jwt.io/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> Sistema bancário completo desenvolvido com .NET 8, seguindo os princípios de Clean Architecture, DDD e CQRS, com autenticação JWT e processamento assíncrono de eventos via Apache Kafka.

---

## 📋 Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Funcionalidades](#-funcionalidades)
- [Arquitetura](#-arquitetura)
- [Tecnologias](#-tecnologias)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação](#-instalação)
- [Como Executar](#-como-executar)
- [Endpoints da API](#-endpoints-da-api)
- [Autenticação JWT](#-autenticação-jwt)
- [Apache Kafka](#-apache-kafka)
- [Testes](#-testes)
- [Docker](#-docker)
- [Kubernetes](#-kubernetes)
- [Documentação Adicional](#-documentação-adicional)
- [Contribuindo](#-contribuindo)
- [Licença](#-licença)

---

## 🎯 Sobre o Projeto

**BankMore** é uma solução bancária digital desenvolvida para demonstrar a implementação de:

- ✅ **Clean Architecture** com separação clara de responsabilidades
- ✅ **Domain-Driven Design (DDD)** com entidades de domínio ricas
- ✅ **CQRS Pattern** usando MediatR
- ✅ **Event-Driven Architecture** com Apache Kafka
- ✅ **Autenticação JWT** com segurança robusta
- ✅ **Validação de CPF** brasileira
- ✅ **Idempotência** para operações críticas
- ✅ **Testes Automatizados** com cobertura completa
- ✅ **Containerização** com Docker e Kubernetes

---

## ⚡ Funcionalidades

### Operações Bancárias

| Operação | Descrição | Tarifa | Limite por Operação |
|----------|-----------|--------|---------------------|
| **Criar Conta** | Cadastro de nova conta corrente | Grátis | - |
| **Login** | Autenticação e geração de token JWT | Grátis | - |
| **Depósito** | Crédito em conta corrente | Grátis | R$ 10.000,00 |
| **Saque** | Débito da conta corrente | R$ 0,50 | R$ 5.000,00 |
| **Transferência** | Entre contas da mesma instituição | R$ 1,00 | Ilimitado* |
| **Consulta Saldo** | Saldo atual da conta | Grátis | - |
| **Extrato** | Histórico de movimentações | Grátis | - |
| **Inativar Conta** | Desativa conta corrente | Grátis | - |

<sub>* Limitado ao saldo disponível + tarifas</sub>

### Recursos Técnicos

- 🔐 **Autenticação JWT**: Bearer token com expiração configurável
- 🔒 **Senha Segura**: Hash SHA256 com salt único por usuário
- ✔️ **Validação CPF**: Validação completa de CPF brasileiro
- 🔄 **Idempotência**: Prevenção de operações duplicadas via `X-Idempotency-Key`
- 📊 **Eventos Assíncronos**: Publicação automática no Kafka
- 🚫 **Validações de Negócio**: Contas inativas bloqueadas, saldo suficiente, etc.
- ⚠️ **Tipos de Erro Padronizados**: Respostas consistentes e informativas
- 📝 **Auditoria Completa**: Logs estruturados de todas operações

---

## 🏛️ Arquitetura

### Clean Architecture + CQRS + Event-Driven
```
┌─────────────────────────────────────────────────────────────────┐
│                     Presentation Layer                           │
│  ┌─────────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │  Controllers    │  │  Middleware  │  │  Swagger/OpenAPI │   │
│  │  - Auth         │  │  - JWT       │  │                  │   │
│  │  - ContaCorrente│  │  - Exception │  │                  │   │
│  │  - Transferencia│  │              │  │                  │   │
│  └─────────────────┘  └──────────────┘  └──────────────────┘   │
└──────────────────────────────┬──────────────────────────────────┘
                               │
┌──────────────────────────────▼──────────────────────────────────┐
│                     Application Layer                            │
│  ┌────────────────────────────┐  ┌─────────────────────────┐    │
│  │  Commands (Write)          │  │  Queries (Read)         │    │
│  │  - CriarContaCorrente      │  │  - ObterContaCorrente   │    │
│  │  - RealizarDeposito        │  │  - ObterSaldo           │    │
│  │  - RealizarSaque           │  │  - ObterExtrato         │    │
│  │  - RealizarTransferencia   │  │                         │    │
│  │  - InativarContaCorrente   │  │                         │    │
│  │  - Login                   │  │                         │    │
│  └────────────────────────────┘  └─────────────────────────┘    │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐     │
│  │  Validators     │  │  DTOs           │  │  Mappers    │     │
│  │  (FluentVal)    │  │                 │  │             │     │
│  └─────────────────┘  └─────────────────┘  └─────────────┘     │
└──────────────────────────────┬──────────────────────────────────┘
                               │
┌──────────────────────────────▼──────────────────────────────────┐
│                       Domain Layer                               │
│  ┌──────────────────┐  ┌──────────────────┐  ┌────────────┐    │
│  │  Entities        │  │  Domain Events   │  │  Interfaces│    │
│  │  - ContaCorrente │  │  - DepositoEvent │  │  - IRepo   │    │
│  │  - Movimento     │  │  - SaqueEvent    │  │  - IService│    │
│  │  - Transferencia │  │  - TransfEvent   │  │            │    │
│  │  - Tarifa        │  │                  │  │            │    │
│  └──────────────────┘  └──────────────────┘  └────────────┘    │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │  Enums           │  │  Exceptions      │                    │
│  │  - ErrorType     │  │  - DomainExcep   │                    │
│  └──────────────────┘  └──────────────────┘                    │
└──────────────────────────────┬──────────────────────────────────┘
                               │
┌──────────────────────────────▼──────────────────────────────────┐
│                   Infrastructure Layer                           │
│  ┌──────────────────┐  ┌──────────────────┐  ┌────────────┐    │
│  │  Repositories    │  │  Services        │  │  Messaging │    │
│  │  - Dapper        │  │  - JWT           │  │  - Kafka   │    │
│  │  - SQLite        │  │  - Password      │  │    Producer│    │
│  │                  │  │  - CPF Validator │  │    Consumer│    │
│  └──────────────────┘  └──────────────────┘  └────────────┘    │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Data                                                     │  │
│  │  - DatabaseContext  - DatabaseInitializer                │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Fluxo de Eventos com Kafka
```
┌──────────────┐     ┌────────────┐     ┌─────────────┐
│  Controller  │────▶│  Command   │────▶│ Repository  │
│              │     │  Handler   │     │  (SQLite)   │
└──────────────┘     └─────┬──────┘     └─────────────┘
                           │
                           ▼
                    ┌──────────────┐
                    │ Event        │
                    │ Publisher    │
                    └──────┬───────┘
                           │
                           ▼
                   ┌───────────────┐
                   │ Kafka Broker  │
                   └───┬───────┬───┘
        ┌──────────────┼───────┼──────────────┐
        ▼              ▼       ▼              ▼
  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
  │depositos │  │  saques  │  │transferen│  │tarifacoes│
  │  topic   │  │  topic   │  │cias topic│  │  topic   │
  └─────┬────┘  └─────┬────┘  └─────┬────┘  └─────┬────┘
        │             │             │             │
        └─────────────┴─────────────┴─────────────┘
                           │
                           ▼
                    ┌──────────────┐
                    │   Consumer   │
                    │  (Background │
                    │   Service)   │
                    └──────┬───────┘
                           │
        ┌──────────────────┼──────────────────┐
        ▼                  ▼                  ▼
  ┌──────────┐      ┌──────────┐      ┌──────────┐
  │Notifica  │      │Analytics │      │  Logs    │
  │   ção    │      │          │      │          │
  └──────────┘      └──────────┘      └──────────┘
```

---

## 🚀 Tecnologias

### Backend
- **.NET 8** - Framework principal
- **C# 12** - Linguagem de programação
- **ASP.NET Core** - Web API
- **SQLite** - Banco de dados relacional
- **Dapper** - Micro ORM de alta performance
- **MediatR** - Implementação do padrão CQRS/Mediator
- **FluentValidation** - Validação declarativa
- **FluentResults** - Result Pattern para tratamento de erros

### Segurança
- **JWT Bearer** - Autenticação baseada em tokens
- **SHA256 + Salt** - Criptografia de senhas
- **CPF Validator** - Validação de documentos brasileiros

### Mensageria
- **Apache Kafka 7.5** - Message Broker
- **Confluent.Kafka** - Cliente .NET oficial
- **Apache Zookeeper** - Coordenação do cluster Kafka
- **Kafka UI** - Interface web para gerenciamento

### DevOps & Cloud
- **Docker** - Containerização de aplicações
- **Docker Compose** - Orquestração multi-container
- **Kubernetes** - Orquestração em produção
- **Horizontal Pod Autoscaler** - Escalabilidade automática

### Documentação & Testes
- **Swagger/OpenAPI** - Documentação interativa da API
- **xUnit** - Framework de testes unitários
- **Moq** - Framework de mocking
- **FluentAssertions** - Assertions expressivas

---

## 📁 Estrutura do Projeto
```
ContaCorrente/
│
├── src/
│   ├── ContaCorrente.Api/                    # 🌐 Camada de Apresentação
│   │   ├── Controllers/                      # Endpoints REST
│   │   │   ├── AuthController.cs
│   │   │   ├── ContaCorrenteController.cs
│   │   │   ├── TransferenciaController.cs
│   │   │   ├── HealthController.cs
│   │   │   └── KafkaMonitoringController.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Program.cs
│   │
│   ├── ContaCorrente.Application/            # 📋 Camada de Aplicação (CQRS)
│   │   ├── UseCases/
│   │   │   ├── Auth/
│   │   │   │   └── Commands/
│   │   │   │       └── Login/
│   │   │   │           ├── LoginCommand.cs
│   │   │   │           └── LoginCommandHandler.cs
│   │   │   ├── ContasCorrentes/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CriarContaCorrente/
│   │   │   │   │   ├── InativarContaCorrente/
│   │   │   │   │   ├── RealizarDeposito/
│   │   │   │   │   └── RealizarSaque/
│   │   │   │   └── Queries/
│   │   │   │       ├── ObterContaCorrente/
│   │   │   │       └── ObterExtrato/
│   │   │   └── Transferencias/
│   │   │       └── Commands/
│   │   │           └── RealizarTransferencia/
│   │   ├── DTOs/
│   │   │   ├── ContaCorrenteDto.cs
│   │   │   ├── MovimentoDto.cs
│   │   │   └── TransferenciaDto.cs
│   │   └── Behaviors/
│   │       └── ValidationBehavior.cs
│   │
│   ├── ContaCorrente.Domain/                 # 🎯 Camada de Domínio
│   │   ├── Entities/
│   │   │   ├── ContaCorrenteEntity.cs
│   │   │   ├── Movimento.cs
│   │   │   ├── Transferencia.cs
│   │   │   ├── Tarifa.cs
│   │   │   └── Idempotencia.cs
│   │   ├── Events/
│   │   │   ├── ContaCorrenteCriadaEvent.cs
│   │   │   ├── DepositoRealizadoEvent.cs
│   │   │   ├── SaqueRealizadoEvent.cs
│   │   │   └── TransferenciaRealizadaEvent.cs
│   │   ├── Interfaces/
│   │   │   ├── IContaCorrenteRepository.cs
│   │   │   ├── ITransferenciaRepository.cs
│   │   │   ├── ITarifaRepository.cs
│   │   │   ├── IIdempotenciaRepository.cs
│   │   │   └── IEventPublisher.cs
│   │   ├── Services/
│   │   │   ├── IPasswordService.cs
│   │   │   ├── ICpfValidator.cs
│   │   │   └── IJwtService.cs
│   │   ├── Enums/
│   │   │   └── ErrorType.cs
│   │   └── Exceptions/
│   │       └── DomainException.cs
│   │
│   ├── ContaCorrente.Infrastructure/         # 🔧 Camada de Infraestrutura
│   │   ├── Data/
│   │   │   ├── DatabaseContext.cs
│   │   │   └── DatabaseInitializer.cs
│   │   ├── Repositories/
│   │   │   ├── ContaCorrenteRepository.cs
│   │   │   ├── TransferenciaRepository.cs
│   │   │   ├── TarifaRepository.cs
│   │   │   └── IdempotenciaRepository.cs
│   │   ├── Services/
│   │   │   ├── PasswordService.cs
│   │   │   ├── CpfValidator.cs
│   │   │   └── JwtService.cs
│   │   └── Messaging/
│   │       ├── KafkaEventPublisher.cs
│   │       ├── KafkaEventConsumer.cs
│   │       └── TarifacaoConsumerService.cs
│   │
│   └── ContaCorrente.Tarifas/                # 💰 Serviço de Tarifas (Opcional)
│       ├── Models/
│       │   ├── TransferenciaRealizadaEvent.cs
│       │   └── TarifacaoRealizadaEvent.cs
│       ├── Services/
│       │   └── TarifaConsumerService.cs
│       ├── appsettings.json
│       └── Program.cs
│
├── tests/
│   └── ContaCorrente.Tests/                  # 🧪 Testes Automatizados
│       ├── UseCases/
│       │   ├── CriarContaCorrenteTests.cs
│       │   ├── LoginTests.cs
│       │   ├── RealizarDepositoTests.cs
│       │   ├── RealizarSaqueTests.cs
│       │   └── RealizarTransferenciaTests.cs
│       └── Integration/
│           └── KafkaIntegrationTests.cs
│
├── k8s/                                       # ☸️ Kubernetes Manifests
│   ├── configmap.yaml
│   ├── deployment.yaml
│   ├── service.yaml
│   ├── hpa.yaml
│   └── kafka-deployment.yaml
│
├── scripts/                                   # 📜 Scripts Auxiliares
│   ├── setup-complete.sh
│   ├── test-api-jwt.sh
│   ├── run-tarifas.sh
│   ├── cleanup.sh
│   └── test-kafka.sh
│
├── postman/                                   # 📮 Postman Collection
│   ├── BankMore-API-Complete.postman_collection.json
│   └── BankMore-Environment.postman_environment.json
│
├── docker-compose.yml                         # 🐳 Docker Compose
├── Dockerfile                                 # 🐳 Dockerfile
├── .gitignore
├── ContaCorrente.sln
└── README.md
```

---

## 📦 Pré-requisitos

Certifique-se de ter instalado:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (versão 8.0 ou superior)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para rodar Kafka e containers)
- [Git](https://git-scm.com/) (para clonar o repositório)

**Opcional:**
- [kubectl](https://kubernetes.io/docs/tasks/tools/) (para deploy no Kubernetes)
- [Postman](https://www.postman.com/downloads/) (para testar a API)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)

---

## 🔧 Instalação

### 1. Clone o repositório
```bash
git clone https://github.com/sethsobrinhoamcom/ContaCorrente.git
cd ContaCorrente
```

### 2. Restaure as dependências
```bash
dotnet restore
```

### 3. Compile o projeto
```bash
dotnet build
```

---

## ▶️ Como Executar

### Opção 1: Desenvolvimento Local (sem Kafka)
```bash
cd src/ContaCorrente.Api
dotnet run
```

Acesse: **http://localhost:5058**

### Opção 2: Com Docker Compose (Recomendado)
```bash
# Iniciar todos os serviços (Kafka, Zookeeper, Kafka UI, API)
docker-compose up -d

# Ver logs
docker-compose logs -f contacorrente-api

# Parar todos os serviços
docker-compose down
```

**Serviços disponíveis:**
- 🌐 **API Swagger**: http://localhost:5000
- 📊 **Kafka UI**: http://localhost:8080
- ✅ **Health Check**: http://localhost:5000/health

### Opção 3: Kubernetes
```bash
# Build da imagem Docker
docker build -t contacorrente-api:latest .

# Deploy completo (Kafka + API)
kubectl apply -f k8s/

# Port forward para acessar
kubectl port-forward service/contacorrente-service 5000:80

# Verificar status
kubectl get pods
kubectl get services
```

---

## 📚 Endpoints da API

### 🔓 Endpoints Públicos (sem autenticação)

#### Criar Conta
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

#### Login
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
  "idContaCorrente": "guid-da-conta",
  "numeroConta": "12345",
  "nome": "João Silva"
}
```

### 🔐 Endpoints Protegidos (requerem JWT)

Todos os endpoints abaixo requerem o header:
```
Authorization: Bearer {seu-token-jwt}
```

#### Consultar Saldo
```http
GET /api/contacorrente/saldo
Authorization: Bearer {token}
```

#### Obter Extrato
```http
GET /api/contacorrente/extrato?dataInicio=2024-01-01&dataFim=2024-12-31
Authorization: Bearer {token}
```

#### Realizar Depósito
```http
POST /api/contacorrente/deposito
Authorization: Bearer {token}
Content-Type: application/json
X-Idempotency-Key: {guid-unico}

{
  "valor": 500.00
}
```

#### Realizar Saque
```http
POST /api/contacorrente/saque
Authorization: Bearer {token}
Content-Type: application/json
X-Idempotency-Key: {guid-unico}

{
  "valor": 100.00
}
```

#### Realizar Transferência
```http
POST /api/transferencia
Authorization: Bearer {token}
Content-Type: application/json
X-Idempotency-Key: {guid-unico}

{
  "idContaCorrenteDestino": "guid-conta-destino",
  "valor": 250.00
}
```

#### Inativar Conta
```http
POST /api/contacorrente/inativar
Authorization: Bearer {token}
Content-Type: application/json

{
  "senha": "senha123"
}
```

### 📊 Monitoramento

#### Health Check
```http
GET /health
```

#### Kafka Health
```http
GET /api/kafkamonitoring/health
```

#### Listar Tópicos Kafka
```http
GET /api/kafkamonitoring/topics
```

---

## 🔐 Autenticação JWT

### Como usar:

1. **Criar uma conta** (endpoint público)
2. **Fazer login** (endpoint público) → Recebe o token JWT
3. **Usar o token** em todos os endpoints protegidos

### Exemplo Completo:
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

# 3. Usar o token
curl http://localhost:5000/api/contacorrente/saldo \
  -H "Authorization: Bearer $TOKEN"
```

### Configurações JWT:

- **Emissor (Issuer)**: BankMore
- **Audiência (Audience)**: BankMoreAPI
- **Expiração**: 2 horas (configurável em `appsettings.json`)
- **Algoritmo**: HS256

---

## 📡 Apache Kafka

### Tópicos Criados

| Tópico | Partições | Eventos |
|--------|-----------|---------|
| `depositos` | 3 | Eventos de depósito |
| `saques` | 3 | Eventos de saque |
| `transferencias` | 3 | Eventos de transferência |
| `tarifacoes` | 3 | Eventos de tarifação |

### Kafka UI

Acesse a interface web: **http://localhost:8080**

Funcionalidades:
- Visualizar tópicos e partições
- Ver mensagens em tempo real
- Monitorar consumer groups
- Verificar offsets e lag

### Comandos Úteis
```bash
# Listar tópicos
docker exec kafka kafka-topics --list --bootstrap-server localhost:9092

# Ver mensagens de um tópico
docker exec kafka kafka-console-consumer \
  --topic depositos \
  --bootstrap-server localhost:9092 \
  --from-beginning \
  --max-messages 10

# Ver consumer groups
docker exec kafka kafka-consumer-groups \
  --list \
  --bootstrap-server localhost:9092

# Ver lag do consumer group
docker exec kafka kafka-consumer-groups \
  --describe \
  --group contacorrente-consumer-group \
  --bootstrap-server localhost:9092
```

---

## 🧪 Testes

### Executar Todos os Testes
```bash
dotnet test
```

### Executar com Cobertura
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Executar Testes Específicos
```bash
# Testes de depósito
dotnet test --filter "FullyQualifiedName~RealizarDepositoTests"

# Testes de saque
dotnet test --filter "FullyQualifiedName~RealizarSaqueTests"

# Testes de transferência
dotnet test --filter "FullyQualifiedName~RealizarTransferenciaTests"

# Testes de login/autenticação
dotnet test --filter "FullyQualifiedName~LoginTests"
```

### Teste End-to-End com Script

**Linux/macOS/Git Bash:**
```bash
bash scripts/test-api-jwt.sh
```

**Windows PowerShell:**
```powershell
.\scripts\test-api-jwt.ps1
```

Este script testa automaticamente:
- ✅ Criação de 2 contas
- ✅ Login e obtenção de JWT
- ✅ Depósito de R$ 1.000
- ✅ Saque de R$ 100 (+ tarifa R$ 0,50)
- ✅ Transferência de R$ 250 (+ tarifa R$ 1,00)
- ✅ Consulta de saldos finais
- ✅ Verificações de segurança

---

## 🐳 Docker

### Build da Imagem
```bash
docker build -t contacorrente-api:latest .
```

### Executar Container
```bash
docker run -d -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Data Source=/app/data/contacorrente.db" \
  -e Kafka__BootstrapServers="localhost:9092" \
  contacorrente-api:latest
```

### Docker Compose
```bash
# Iniciar todos os serviços
docker-compose up -d

# Ver logs de um serviço específico
docker-compose logs -f contacorrente-api
docker-compose logs -f kafka

# Parar todos os serviços
docker-compose down

# Parar e remover volumes
docker-compose down -v

# Reiniciar um serviço
docker-compose restart contacorrente-api
```

---

## ☸️ Kubernetes

### Deploy
```bash
# Aplicar todos os manifestos
kubectl apply -f k8s/

# Verificar status
kubectl get pods
kubectl get services
kubectl get hpa

# Ver logs
kubectl logs -f deployment/contacorrente-api

# Port forward
kubectl port-forward service/contacorrente-service 5000:80
```

### Recursos Kubernetes

- **Deployment**: 3 réplicas da API
- **Service**: LoadBalancer na porta 80
- **HPA**: Escalabilidade automática (2-10 pods)
  - CPU: 70%
  - Memory: 80%
- **ConfigMap**: Configurações da aplicação
- **PersistentVolumeClaim**: 1Gi para banco de dados

### Escalar Manualmente
```bash
# Escalar para 5 réplicas
kubectl scale deployment contacorrente-api --replicas=5

# Ver status do HPA
kubectl get hpa -w
```

---

## 📖 Documentação Adicional

- **Swagger/OpenAPI**: http://localhost:5000 (quando a API estiver rodando)
- **Postman Collection**: Arquivo disponível em `postman/`
- **Documentação Kafka**: Veja `docs/KAFKA.md`

---

## 🤝 Contribuindo

Contribuições são bem-vindas! Para contribuir:

1. Faça um Fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/NovaFuncionalidade`)
3. Commit suas mudanças (`git commit -m 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/NovaFuncionalidade`)
5. Abra um Pull Request

### Padrões de Código

- Siga os princípios SOLID
- Escreva testes para novas funcionalidades
- Mantenha a cobertura de testes acima de 80%
- Use nomenclatura clara e descritiva
- Documente APIs públicas

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## 👨‍💻 Autor

**Seth Sobrinho**

- GitHub: [@sethsobrinhoamcom](https://github.com/sethsobrinhoamcom)
- LinkedIn: [Seu LinkedIn](https://linkedin.com/in/seu-perfil)
- Email: seth.sobrinho@example.com

---

## 🙏 Agradecimentos

- Clean Architecture por Robert C. Martin
- Domain-Driven Design por Eric Evans
- Comunidade .NET
- Apache Kafka

---

## 📊 Status do Projeto

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Tests](https://img.shields.io/badge/tests-passing-brightgreen)
![Coverage](https://img.shields.io/badge/coverage-85%25-green)

---

<div align="center">

**⭐ Se este projeto foi útil para você, considere dar uma estrela! ⭐**

**Desenvolvido com ❤️ usando .NET 8, Clean Architecture e Apache Kafka**

</div>