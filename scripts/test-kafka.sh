#!/bin/bash

echo "🚀 Testing Kafka Integration"

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

KAFKA_CONTAINER="kafka"
BOOTSTRAP_SERVERS="localhost:9092"

echo -e "${YELLOW}1. Verificando se Kafka está rodando...${NC}"
if ! docker ps | grep -q $KAFKA_CONTAINER; then
    echo "❌ Kafka não está rodando. Execute: docker-compose up -d"
    exit 1
fi
echo -e "${GREEN}✓ Kafka está rodando${NC}"

echo -e "${YELLOW}2. Listando tópicos...${NC}"
docker exec $KAFKA_CONTAINER kafka-topics --list --bootstrap-server localhost:9092

echo -e "${YELLOW}3. Criando tópicos de teste...${NC}"
docker exec $KAFKA_CONTAINER kafka-topics --create --topic depositos --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1 --if-not-exists
docker exec $KAFKA_CONTAINER kafka-topics --create --topic saques --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1 --if-not-exists
docker exec $KAFKA_CONTAINER kafka-topics --create --topic transferencias --bootstrap-server localhost:9092 --partitions 3 --replication-factor 1 --if-not-exists

echo -e "${GREEN}✓ Tópicos criados${NC}"

echo -e "${YELLOW}4. Descrevendo tópico 'depositos'...${NC}"
docker exec $KAFKA_CONTAINER kafka-topics --describe --topic depositos --bootstrap-server localhost:9092

echo -e "${YELLOW}5. Produzindo mensagem de teste...${NC}"
echo '{"idContaCorrente":"teste-123","valor":100.50,"dataMovimento":"2024-11-24T10:00:00"}' | \
docker exec -i $KAFKA_CONTAINER kafka-console-producer --topic depositos --bootstrap-server localhost:9092

echo -e "${GREEN}✓ Mensagem produzida${NC}"

echo -e "${YELLOW}6. Consumindo mensagens (Ctrl+C para parar)...${NC}"
docker exec $KAFKA_CONTAINER kafka-console-consumer --topic depositos --bootstrap-server localhost:9092 --from-beginning --max-messages 10

echo -e "${GREEN}✓ Teste completo!${NC}"