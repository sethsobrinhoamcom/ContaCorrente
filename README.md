# 🏦 Conta Corrente API - Sistema Bancário Completo

API REST para gerenciamento de contas correntes com operações bancárias (depósito, saque, transferência), desenvolvida com .NET 8, Clean Architecture, CQRS e processamento assíncrono com Apache Kafka.

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Funcionalidades](#funcionalidades)
- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Executando a Aplicação](#executando-a-aplicação)
- [Docker e Kubernetes](#docker-e-kubernetes)
- [Kafka](#kafka)
- [Endpoints da API](#endpoints-da-api)
- [Testes](#testes)
- [Monitoramento](#monitoramento)

## 🎯 Visão Geral

Sistema bancário que permite:
- ✅ Criação e gerenciamento de contas correntes
- ✅ Operações de depósito, saque e transferência
- ✅ Consulta de saldo e extrato
- ✅ Sistema de tarifas automáticas
- ✅ Idempotência para operações críticas
- ✅ Processamento assíncrono de eventos com Kafka
- ✅ Monitoramento e observabilidade

## ⚡ Funcionalidades

### Operações Bancárias
- **Criar Conta Corrente**: Cadastro de novas contas com senha criptografada
- **Depósito**: Crédito em conta (limite: R$ 10.000 por operação)
- **Saque**: Débito em conta com tarifa de R$ 0,50
- **Transferência**: Entre contas com tarifa de R$ 1,00
- **Extrato**: Consulta de movimentações com filtros de período
- **Saldo**: Consulta de saldo atualizado

### Recursos Técnicos
- **Idempotência**: Prevenção de operações duplicadas via `X-Idempotency-Key`
- **Eventos Assíncronos**: Publicação no Kafka para cada operação
- **Validações**: FluentValidation com regras de negócio
- **Auditoria**: Logs de todas as operações
- **Health Checks**: Endpoints para monitoramento

## 🚀 Tecnologias

### Backend
- **.NET 8** - Framework principal
- **C# 12** - Linguagem de programação
- **ASP.NET Core** - Web API
- **SQLite** - Banco de dados
- **Dapper** - Micro ORM para acesso a dados
- **MediatR** - CQRS pattern
- **FluentValidation** - Validação de dados
- **FluentResults** - Tratamento de resultados

### Mensageria
- **Apache Kafka** - Message broker
- **Confluent.Kafka** - Cliente .NET para Kafka
- **Zookeeper** - Coordenação do Kafka

### Infraestrutura
- **Docker** - Containerização
- **Docker Compose** - Orquestração local
- **Kubernetes** - Orquestração em produção
- **Kafka UI** - Interface web para Kafka

### Testes
- **xUnit** - Framework de testes
- **Moq** - Mock para testes
- **FluentAssertions** - Assertions expressivas

## 🏛️ Arquitetura

### Clean Architecture + CQRS
```
┌─────────────────────────────────────────────────────────┐
│                    API Layer (Controllers)               │
│  - ContaCorrenteController                              │
│  - TransferenciaController                              │
│  - KafkaMonitoringController                            │
└───────────────────┬─────────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────────┐
│              Application Layer (CQRS)                    │
│  Commands:                      Queries:                │
│  - CriarContaCorrente          - ObterContaCorrente    │
│  - RealizarDeposito            - ObterExtrato          │
│  - RealizarSaque                                        │
│  - RealizarTransferencia                                │
└───────────────────┬─────────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────────┐
│                 Domain Layer                             │
│  - Entities (ContaCorrente, Movimento, etc)             │
│  - Events (DepositoRealizadoEvent, etc)                 │
│  - Interfaces (Repositories, Services)                  │
└───────────────────┬─────────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────────┐
│            Infrastructure Layer                          │
│  - Repositories (Dapper + SQLite)                       │
│  - KafkaEventPublisher                                  │
│  - KafkaEventConsumer                                   │
│  - PasswordService                                      │
└─────────────────────────────────────────────────────────┘
```

### Fluxo de Eventos com Kafka
```
┌──────────────┐       ┌──────────┐       ┌─────────────────┐
│   API POST   │──────▶│  Command │──────▶│   Repository    │
│   /deposito  │       │  Handler │       │   (SQLite)      │
└──────────────┘       └────┬─────┘       └─────────────────┘
                            │
                            ▼
                    ┌───────────────┐
                    │ EventPublisher│
                    │    (Kafka)    │
                    └───────┬───────┘
                            │
                ┌───────────┼───────────┐
                ▼           ▼           ▼
         ┌──────────┐ ┌──────────┐ ┌──────────┐
         │depositos │ │  saques  │ │transferen│
         │  topic   │ │  topic   │ │cias topic│
         └─────┬────┘ └─────┬────┘ └─────┬────┘
               │            │            │
               └────────────┼────────────┘
                            ▼
                    ┌───────────────┐
                    │EventConsumer  │
                    │(Background)   │
                    └───────┬───────┘
                            │
                ┌───────────┼───────────┐
                ▼           ▼           ▼
         ┌──────────┐ ┌──────────┐ ┌──────────┐
         │Analytics │ │Notificação│ │  Logs   │
         └──────────┘ └──────────┘ └──────────┘
```

## 📦 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

**Opcional:**
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Postman](https://www.postman.com/downloads/)

## 💻 Instalação

### 1. Clone o repositório
```bash
git clone https://github.com/seu-usuario/contacorrente-api.git
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

### Opção 1: Executar Localmente (sem Kafka)
```bash
cd src/ContaCorrente.Api
dotnet run
```

Acesse: `http://localhost:5000`

### Opção 2: Docker Compose (RECOMENDADO)
```bash
# Iniciar todos os serviços
docker-compose up -d

# Ver logs
docker-compose logs -f contacorrente-api

# Parar
docker-compose down
```

**Serviços disponíveis:**
- API: `http://localhost:5000`
- Swagger: `http://localhost:5000`
- Kafka UI: `http://localhost:8080`

### Opção 3: Kubernetes
```bash
# 1. Build da imagem
docker build -t contacorrente-api:latest .

# 2. Deploy completo (Kafka + API)
kubectl apply -f k8s/

# 3. Verificar pods
kubectl get pods

# 4. Port forward
kubectl port-forward service/contacorrente-service 5000:80

# 5. Acessar
open http://localhost:5000
```

## 🔥 Kafka

### Tópicos Criados Automaticamente

- **depositos**: Eventos de depósito
- **saques**: Eventos de saque
- **transferencias**: Eventos de transferência

### Acessar Kafka UI
```bash
# Com Docker Compose
open http://localhost:8080
```

### Comandos Úteis do Kafka
```bash
# Listar tópicos
docker exec kafka kafka-topics --list --bootstrap-server localhost:9092

# Ver mensagens de um tópico
docker exec kafka kafka-console-consumer \
  --topic depositos \
  --bootstrap-server localhost:9092 \
  --from-beginning

# Criar tópico manualmente
docker exec kafka kafka-topics \
  --create \
  --topic novo-topico \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1

# Descrever tópico
docker exec kafka kafka-topics \
  --describe \
  --topic depositos \
  --bootstrap-server localhost:9092

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

## 📚 Endpoints da API

### Conta Corrente

#### Criar Conta
```http
POST /api/contacorrente
Content-Type: application/json

{
  "numero": 12345,
  "nome": "João Silva",
  "senha": "senha123"
}
```

#### Obter Conta
```http
GET /api/contacorrente/{id}
```

#### Realizar Depósito
```http
POST /api/contacorrente/{id}/deposito
Content-Type: application/json
X-Idempotency-Key: unique-key-123

{
  "valor": 500.00
}
```

#### Realizar Saque
```http
POST /api/contacorrente/{id}/saque
Content-Type: application/json
X-Idempotency-Key: unique-key-456

{
  "valor": 100.00
}
```

#### Obter Extrato
```http
GET /api/contacorrente/{id}/extrato?dataInicio=2024-01-01&dataFim=2024-12-31
```

### Transferência

#### Realizar Transferência
```http
POST /api/transferencia
Content-Type: application/json
X-Idempotency-Key: unique-key-789

{
  "idContaCorrenteOrigem": "origem-id",
  "idContaCorrenteDestino": "destino-id",
  "valor": 250.00
}
```

### Monitoramento Kafka

#### Kafka Health
```http
GET /api/kafkamonitoring/health
```

#### Listar Tópicos
```http
GET /api/kafkamonitoring/topics
```

#### Info de Tópico
```http
GET /api/kafkamonitoring/topics/depositos
```

#### Listar Consumer Groups
```http
GET /api/kafkamonitoring/consumer-groups
```

#### Lag do Consumer Group
```http
GET /api/kafkamonitoring/consumer-groups/contacorrente-consumer-group/lag
```

### Health Check
```http
GET /health
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
# Testes de depósito
dotnet test --filter "FullyQualifiedName~RealizarDepositoTests"

# Testes de saque
dotnet test --filter "FullyQualifiedName~RealizarSaqueTests"

# Testes de transferência
dotnet test --filter "FullyQualifiedName~RealizarTransferenciaTests"
```

### Teste End-to-End
```bash
chmod +x scripts/e2e-test.sh
./scripts/e2e-test.sh
```

### Teste do Kafka
```bash
chmod +x scripts/test-kafka.sh
./scripts/test-kafka.sh
```

## 📊 Monitoramento

### Logs da Aplicação
```bash
# Docker Compose
docker-compose logs -f contacorrente-api

# Kubernetes
kubectl logs -f deployment/contacorrente-api
```

### Métricas do Kafka

Acesse Kafka UI: `http://localhost:8080`

### Healthchecks
```bash
# API Health
curl http://localhost:5000/health

# Kafka Health
curl http://localhost:5000/api/kafkamonitoring/health
```

## 🔒 Segurança

- ✅ Senhas criptografadas com SHA256 + Salt
- ✅ Idempotência para prevenir operações duplicadas
- ✅ Validações de entrada com FluentValidation
- ✅ Tratamento global de exceções
- ✅ Logs de auditoria

## 📈 Performance

- **SQLite**: Banco de dados leve e rápido
- **Dapper**: ORM de alta performance
- **Kafka**: Processamento assíncrono
- **CQRS**: Separação de leitura e escrita
- **Connection Pooling**: Reutilização de conexões

## 🐛 Troubleshooting

### Kafka não inicia
```bash
# Verificar logs
docker-compose logs kafka

# Reiniciar serviços
docker-compose down
docker-compose up -d
```

### API não conecta ao Kafka

Verifique o `appsettings.json`:
```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092"  // ou "kafka:29092" no Docker
  }
}
```

### Banco de dados corrompido
```bash
# Parar API
docker-compose stop contacorrente-api

# Remover banco
docker volume rm contacorrente_contacorrente-data

# Reiniciar
docker-compose up -d
```

## 📝 Estrutura de Tarifas

| Operação      | Tarifa   |
|---------------|----------|
| Depósito      | Grátis   |
| Saque         | R$ 0,50  |
| Transferência | R$ 1,00  |

## 📄 Licença

Este projeto está sob a licença MIT.

## 👥 Contribuindo

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/NovaFuncionalidade`)
3. Commit suas mudanças (`git commit -m 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/NovaFuncionalidade`)
5. Abra um Pull Request

## 📞 Contato

- Email: seu.email@example.com
- LinkedIn: [seu-perfil](https://linkedin.com/in/seu-perfil)
- GitHub: [@seu-usuario](https://github.com/seu-usuario)

---

**Desenvolvido com ❤️ usando .NET 8 e Apache Kafka**