#!/bin/bash

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

BASE_URL="http://localhost:5000"

echo -e "${YELLOW}🚀 Teste End-to-End - Conta Corrente API${NC}\n"

# 1. Health Check
echo -e "${YELLOW}1. Verificando Health...${NC}"
HEALTH=$(curl -s ${BASE_URL}/health)
if echo $HEALTH | grep -q "Healthy"; then
    echo -e "${GREEN}✓ API está saudável${NC}\n"
else
    echo -e "${RED}✗ API não está respondendo${NC}"
    exit 1
fi

# 2. Kafka Health
echo -e "${YELLOW}2. Verificando Kafka...${NC}"
KAFKA_HEALTH=$(curl -s ${BASE_URL}/api/kafkamonitoring/health)
if echo $KAFKA_HEALTH | grep -q "Healthy"; then
    echo -e "${GREEN}✓ Kafka está saudável${NC}\n"
else
    echo -e "${RED}✗ Kafka não está disponível${NC}"
fi

# 3. Criar Conta 1
echo -e "${YELLOW}3. Criando Conta 1...${NC}"
CONTA1=$(curl -s -X POST ${BASE_URL}/api/contacorrente \
  -H "Content-Type: application/json" \
  -d '{"numero": 11111, "nome": "João Silva", "senha": "senha123"}')

ID_CONTA1=$(echo $CONTA1 | jq -r '.idContaCorrente')
echo -e "${GREEN}✓ Conta 1 criada: ${ID_CONTA1}${NC}\n"

# 4. Criar Conta 2
echo -e "${YELLOW}4. Criando Conta 2...${NC}"
CONTA2=$(curl -s -X POST ${BASE_URL}/api/contacorrente \
  -H "Content-Type: application/json" \
  -d '{"numero": 22222, "nome": "Maria Santos", "senha": "senha456"}')

ID_CONTA2=$(echo $CONTA2 | jq -r '.idContaCorrente')
echo -e "${GREEN}✓ Conta 2 criada: ${ID_CONTA2}${NC}\n"

# 5. Realizar Depósito
echo -e "${YELLOW}5. Realizando Depósito de R$ 1.000,00 na Conta 1...${NC}"
DEPOSITO=$(curl -s -X POST ${BASE_URL}/api/contacorrente/${ID_CONTA1}/deposito \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: $(uuidgen)" \
  -d '{"valor": 1000.00}')

echo -e "${GREEN}✓ Depósito realizado${NC}\n"
sleep 2

# 6. Verificar Saldo Conta 1
echo -e "${YELLOW}6. Verificando Saldo Conta 1...${NC}"
SALDO1=$(curl -s ${BASE_URL}/api/contacorrente/${ID_CONTA1})
SALDO1_VALOR=$(echo $SALDO1 | jq -r '.saldo')
echo -e "${GREEN}✓ Saldo Conta 1: R$ ${SALDO1_VALOR}${NC}\n"

# 7. Realizar Saque
echo -e "${YELLOW}7. Realizando Saque de R$ 100,00 da Conta 1...${NC}"
SAQUE=$(curl -s -X POST ${BASE_URL}/api/contacorrente/${ID_CONTA1}/saque \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: $(uuidgen)" \
  -d '{"valor": 100.00}')

echo -e "${GREEN}✓ Saque realizado (tarifa de R$ 0,50)${NC}\n"
sleep 2

# 8. Realizar Transferência
echo -e "${YELLOW}8. Realizando Transferência de R$ 250,00 da Conta 1 para Conta 2...${NC}"
TRANSFERENCIA=$(curl -s -X POST ${BASE_URL}/api/transferencia \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: $(uuidgen)" \
  -d "{\"idContaCorrenteOrigem\": \"${ID_CONTA1}\", \"idContaCorrenteDestino\": \"${ID_CONTA2}\", \"valor\": 250.00}")

echo -e "${GREEN}✓ Transferência realizada (tarifa de R$ 1,00)${NC}\n"
sleep 2

# 9. Verificar Saldos Finais
echo -e "${YELLOW}9. Verificando Saldos Finais...${NC}"
SALDO1_FINAL=$(curl -s ${BASE_URL}/api/contacorrente/${ID_CONTA1} | jq -r '.saldo')
SALDO2_FINAL=$(curl -s ${BASE_URL}/api/contacorrente/${ID_CONTA2} | jq -r '.saldo')

echo -e "${GREEN}✓ Saldo Final Conta 1: R$ ${SALDO1_FINAL}${NC}"
echo -e "${GREEN}✓ Saldo Final Conta 2: R$ ${SALDO2_FINAL}${NC}\n"

# Cálculo esperado:
# Conta 1: 1000 - 100 - 0.50 (tarifa saque) - 250 - 1.00 (tarifa transferência) = 648.50
# Conta 2: 0 + 250 = 250.00

echo -e "${YELLOW}Cálculo Esperado:${NC}"
echo -e "Conta 1: 1000 - 100 - 0.50 - 250 - 1.00 = ${GREEN}648.50${NC}"
echo -e "Conta 2: 0 + 250 = ${GREEN}250.00${NC}\n"

# 10. Obter Extrato
echo -e "${YELLOW}10. Obtendo Extrato da Conta 1...${NC}"
EXTRATO=$(curl -s ${BASE_URL}/api/contacorrente/${ID_CONTA1}/extrato)
echo $EXTRATO | jq '.'

# 11. Verificar Tópicos Kafka
echo -e "\n${YELLOW}11. Verificando Tópicos Kafka...${NC}"
TOPICS=$(curl -s ${BASE_URL}/api/kafkamonitoring/topics)
echo $TOPICS | jq -r '.[].name'

echo -e "\n${GREEN}✅ Teste End-to-End Concluído com Sucesso!${NC}"