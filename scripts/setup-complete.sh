#!/bin/bash

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}   🚀 Setup Completo - BankMore API${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

# 1. Verificar dependências
echo -e "${YELLOW}1. Verificando dependências...${NC}"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}✗ .NET SDK não encontrado. Instale: https://dotnet.microsoft.com/download${NC}"
    exit 1
fi
echo -e "${GREEN}✓ .NET SDK encontrado: $(dotnet --version)${NC}"

if ! command -v docker &> /dev/null; then
    echo -e "${RED}✗ Docker não encontrado. Instale: https://www.docker.com/products/docker-desktop/${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Docker encontrado: $(docker --version)${NC}"

if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}✗ Docker Compose não encontrado${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Docker Compose encontrado: $(docker-compose --version)${NC}\n"

# 2. Restaurar dependências
echo -e "${YELLOW}2. Restaurando dependências NuGet...${NC}"
dotnet restore
echo -e "${GREEN}✓ Dependências restauradas${NC}\n"

# 3. Compilar solução
echo -e "${YELLOW}3. Compilando solução...${NC}"
dotnet build --no-restore
echo -e "${GREEN}✓ Compilação concluída${NC}\n"

# 4. Executar testes
echo -e "${YELLOW}4. Executando testes...${NC}"
dotnet test --no-build --verbosity quiet
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Todos os testes passaram${NC}\n"
else
    echo -e "${RED}✗ Alguns testes falharam${NC}\n"
fi

# 5. Iniciar Docker Compose
echo -e "${YELLOW}5. Iniciando serviços Docker (Kafka, Zookeeper, Kafka UI)...${NC}"
docker-compose up -d

echo -e "${GREEN}✓ Serviços Docker iniciados${NC}\n"

# 6. Aguardar serviços
echo -e "${YELLOW}6. Aguardando serviços ficarem saudáveis...${NC}"
sleep 10

# Verificar Zookeeper
echo -e "   Verificando Zookeeper..."
if docker ps | grep -q zookeeper; then
    echo -e "${GREEN}   ✓ Zookeeper rodando${NC}"
fi

# Verificar Kafka
echo -e "   Verificando Kafka..."
if docker ps | grep -q kafka; then
    echo -e "${GREEN}   ✓ Kafka rodando${NC}"
fi

# Verificar Kafka UI
echo -e "   Verificando Kafka UI..."
if docker ps | grep -q kafka-ui; then
    echo -e "${GREEN}   ✓ Kafka UI rodando${NC}"
fi

echo ""

# 7. Criar tópicos Kafka
echo -e "${YELLOW}7. Criando tópicos Kafka...${NC}"

docker exec kafka kafka-topics --create \
  --topic depositos \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1 \
  --if-not-exists 2>/dev/null

docker exec kafka kafka-topics --create \
  --topic saques \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1 \
  --if-not-exists 2>/dev/null

docker exec kafka kafka-topics --create \
  --topic transferencias \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1 \
  --if-not-exists 2>/dev/null

docker exec kafka kafka-topics --create \
  --topic tarifacoes \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1 \
  --if-not-exists 2>/dev/null

echo -e "${GREEN}✓ Tópicos criados${NC}\n"

# 8. Build da imagem da API
echo -e "${YELLOW}8. Construindo imagem Docker da API...${NC}"
docker build -t contacorrente-api:latest .
echo -e "${GREEN}✓ Imagem construída${NC}\n"

# 9. Resumo
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}✅ SETUP COMPLETO!${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

echo -e "${YELLOW}📋 Próximos passos:${NC}\n"

echo -e "${YELLOW}1. Iniciar a API:${NC}"
echo -e "   cd src/ContaCorrente.Api"
echo -e "   dotnet run"
echo -e ""

echo -e "${YELLOW}2. Ou iniciar com Docker Compose (tudo junto):${NC}"
echo -e "   docker-compose up -d contacorrente-api"
echo -e ""

echo -e "${YELLOW}3. Iniciar o serviço de tarifas (OPCIONAL):${NC}"
echo -e "   ./scripts/run-tarifas.sh"
echo -e ""

echo -e "${YELLOW}4. Acessar serviços:${NC}"
echo -e "   • API Swagger:  ${BLUE}http://localhost:5000${NC}"
echo -e "   • Kafka UI:     ${BLUE}http://localhost:8080${NC}"
echo -e ""

echo -e "${YELLOW}5. Executar testes:${NC}"
echo -e "   ./scripts/test-api-jwt.sh"
echo -e ""

echo -e "${GREEN}Divirta-se! 🚀${NC}\n"