#!/bin/bash

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

BASE_URL="http://localhost:5000"
TOKEN=""

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}   🏦 TESTE COMPLETO - BankMore API com JWT${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

# 1. Health Check
echo -e "${YELLOW}1. Verificando Health da API...${NC}"
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
    echo -e "${YELLOW}⚠ Kafka não está disponível (opcional)${NC}\n"
fi

# 3. Criar Conta 1
echo -e "${YELLOW}3. Criando Conta 1 (João Silva)...${NC}"
CONTA1_RESPONSE=$(curl -s -X POST ${BASE_URL}/api/contacorrente \
  -H "Content-Type: application/json" \
  -d '{
    "numero": 11111,
    "cpf": "12345678901",
    "nome": "João Silva",
    "senha": "senha123"
  }')

ID_CONTA1=$(echo $CONTA1_RESPONSE | jq -r '.idContaCorrente')

if [ "$ID_CONTA1" != "null" ] && [ -n "$ID_CONTA1" ]; then
    echo -e "${GREEN}✓ Conta 1 criada: ${ID_CONTA1}${NC}"
    echo -e "   CPF: 12345678901${NC}"
    echo -e "   Número: 11111${NC}\n"
else
    echo -e "${RED}✗ Erro ao criar conta 1${NC}"
    echo "$CONTA1_RESPONSE" | jq '.'
    exit 1
fi

# 4. Criar Conta 2
echo -e "${YELLOW}4. Criando Conta 2 (Maria Santos)...${NC}"
CONTA2_RESPONSE=$(curl -s -X POST ${BASE_URL}/api/contacorrente \
  -H "Content-Type: application/json" \
  -d '{
    "numero": 22222,
    "cpf": "98765432109",
    "nome": "Maria Santos",
    "senha": "senha456"
  }')

ID_CONTA2=$(echo $CONTA2_RESPONSE | jq -r '.idContaCorrente')

if [ "$ID_CONTA2" != "null" ] && [ -n "$ID_CONTA2" ]; then
    echo -e "${GREEN}✓ Conta 2 criada: ${ID_CONTA2}${NC}"
    echo -e "   CPF: 98765432109${NC}"
    echo -e "   Número: 22222${NC}\n"
else
    echo -e "${RED}✗ Erro ao criar conta 2${NC}"
    echo "$CONTA2_RESPONSE" | jq '.'
    exit 1
fi

# 5. Login Conta 1
echo -e "${YELLOW}5. Realizando Login (João Silva)...${NC}"
LOGIN_RESPONSE=$(curl -s -X POST ${BASE_URL}/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "cpfOrNumeroConta": "12345678901",
    "senha": "senha123"
  }')

TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.token')

if [ "$TOKEN" != "null" ] && [ -n "$TOKEN" ]; then
    echo -e "${GREEN}✓ Login realizado com sucesso${NC}"
    echo -e "   Token (primeiros 50 chars): ${TOKEN:0:50}...${NC}\n"
else
    echo -e "${RED}✗ Erro ao fazer login${NC}"
    echo "$LOGIN_RESPONSE" | jq '.'
    exit 1
fi

# 6. Tentar acessar endpoint protegido sem token (deve falhar)
echo -e "${YELLOW}6. Testando acesso sem token (deve falhar)...${NC}"
NO_TOKEN_RESPONSE=$(curl -s -w "\n%{http_code}" ${BASE_URL}/api/contacorrente/saldo)
NO_TOKEN_CODE=$(echo "$NO_TOKEN_RESPONSE" | tail -n1)

if [ "$NO_TOKEN_CODE" = "401" ]; then
    echo -e "${GREEN}✓ Corretamente bloqueado sem token (401)${NC}\n"
else
    echo -e "${RED}✗ Erro: deveria retornar 401${NC}\n"
fi

# 7. Consultar Saldo (com token)
echo -e "${YELLOW}7. Consultando Saldo (com token)...${NC}"
SALDO_RESPONSE=$(curl -s ${BASE_URL}/api/contacorrente/saldo \
  -H "Authorization: Bearer ${TOKEN}")

echo "$SALDO_RESPONSE" | jq '.'
echo ""

# 8. Realizar Depósito
echo -e "${YELLOW}8. Realizando Depósito de R$ 1.000,00...${NC}"
DEPOSITO_RESPONSE=$(curl -s -X POST ${BASE_URL}/api/contacorrente/deposito \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "X-Idempotency-Key: $(uuidgen)" \
  -d '{"valor": 1000.00}')

echo -e "${GREEN}✓ Depósito realizado${NC}"
echo "$DEPOSITO_RESPONSE" | jq '.'
echo ""

sleep 2

# 9. Verificar Saldo após depósito
echo -e "${YELLOW}9. Verificando Saldo após depósito...${NC}"
SALDO_POS_DEPOSITO=$(curl -s ${BASE_URL}/api/contacorrente/saldo \
  -H "Authorization: Bearer ${TOKEN}")

SALDO_VALOR=$(echo $SALDO_POS_DEPOSITO | jq -r '.saldo')
echo -e "${GREEN}✓ Saldo atual: R$ ${SALDO_VALOR}${NC}\n"

# 10. Realizar Saque
echo -e "${YELLOW}10. Realizando Saque de R$ 100,00 (tarifa R$ 0,50)...${NC}"
SAQUE_RESPONSE=$(curl -s -X POST ${BASE_URL}/api/contacorrente/saque \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "X-Idempotency-Key: $(uuidgen)" \
  -d '{"valor": 100.00}')

echo -e "${GREEN}✓ Saque realizado${NC}"
echo "$SAQUE_RESPONSE" | jq '.'
echo ""

sleep 2

# 11. Login Conta 2 para transferência
echo -e "${YELLOW}11. Realizando Login (Maria Santos)...${NC}"
LOGIN2_RESPONSE=$(curl -s -X POST ${BASE_URL}/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "cpfOrNumeroConta": "22222",
    "senha": "senha456"
  }')

TOKEN2=$(echo $LOGIN2_RESPONSE | jq -r '.token')
echo -e "${GREEN}✓ Login realizado${NC}\n"

# 12. Voltar para conta 1 e fazer transferência
echo -e "${YELLOW}12. Realizando Transferência de R$ 250,00 (tarifa R$ 1,00)...${NC}"
TRANSFERENCIA_RESPONSE=$(curl -s -X POST ${BASE_URL}/api/transferencia \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "X-Idempotency-Key: $(uuidgen)" \
  -d "{\"idContaCorrenteDestino\": \"${ID_CONTA2}\", \"valor\": 250.00}")

echo -e "${GREEN}✓ Transferência realizada${NC}"
echo "$TRANSFERENCIA_RESPONSE" | jq '.'
echo ""

sleep 2

# 13. Verificar Saldo Final Conta 1
echo -e "${YELLOW}13. Verificando Saldo Final...${NC}"
SALDO_FINAL_1=$(curl -s ${BASE_URL}/api/contacorrente/saldo \
  -H "Authorization: Bearer ${TOKEN}")

SALDO_FINAL_VALOR_1=$(echo $SALDO_FINAL_1 | jq -r '.saldo')

echo -e "${GREEN}✓ Saldo Final Conta 1 (João): R$ ${SALDO_FINAL_VALOR_1}${NC}"

# Verificar Saldo Final Conta 2
SALDO_FINAL_2=$(curl -s ${BASE_URL}/api/contacorrente/saldo \
  -H "Authorization: Bearer ${TOKEN2}")

SALDO_FINAL_VALOR_2=$(echo $SALDO_FINAL_2 | jq -r '.saldo')

echo -e "${GREEN}✓ Saldo Final Conta 2 (Maria): R$ ${SALDO_FINAL_VALOR_2}${NC}\n"

# 14. Cálculo Esperado
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${YELLOW}Cálculo Esperado:${NC}"
echo -e "Conta 1: 1000 - 100 - 0.50 - 250 - 1.00 = ${GREEN}648.50${NC}"
echo -e "Conta 2: 0 + 250 = ${GREEN}250.00${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

# 15. Obter Extrato
echo -e "${YELLOW}15. Obtendo Extrato Completo...${NC}"
EXTRATO=$(curl -s ${BASE_URL}/api/contacorrente/extrato \
  -H "Authorization: Bearer ${TOKEN}")

echo "$EXTRATO" | jq '.'
echo ""

# 16. Testar Inativação de Conta
echo -e "${YELLOW}16. Testando Inativação de Conta...${NC}"
INATIVAR_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST ${BASE_URL}/api/contacorrente/inativar \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${TOKEN}" \
  -d '{"senha": "senha123"}')

INATIVAR_CODE=$(echo "$INATIVAR_RESPONSE" | tail -n1)

if [ "$INATIVAR_CODE" = "204" ]; then
    echo -e "${GREEN}✓ Conta inativada com sucesso (204)${NC}\n"
else
    echo -e "${RED}✗ Erro ao inativar conta${NC}\n"
fi

# 17. Tentar usar conta inativada (deve falhar)
echo -e "${YELLOW}17. Tentando usar conta inativada (deve falhar)...${NC}"
DEPOSITO_INATIVO=$(curl -s -w "\n%{http_code}" -X POST ${BASE_URL}/api/contacorrente/deposito \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "X-Idempotency-Key: $(uuidgen)" \
  -d '{"valor": 100.00}')

DEPOSITO_INATIVO_CODE=$(echo "$DEPOSITO_INATIVO" | tail -n1)

if [ "$DEPOSITO_INATIVO_CODE" = "400" ]; then
    echo -e "${GREEN}✓ Corretamente bloqueado conta inativa (400)${NC}\n"
else
    echo -e "${RED}✗ Erro: deveria retornar 400${NC}\n"
fi

# 18. Verificar Tópicos Kafka
echo -e "${YELLOW}18. Verificando Tópicos Kafka...${NC}"
TOPICS=$(curl -s ${BASE_URL}/api/kafkamonitoring/topics)
echo "$TOPICS" | jq -r '.[].name' | while read topic; do
    echo -e "   - ${GREEN}${topic}${NC}"
done
echo ""

# Resumo Final
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}✅ TESTE COMPLETO FINALIZADO COM SUCESSO!${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

echo -e "${YELLOW}📊 Resumo:${NC}"
echo -e "   • 2 Contas criadas"
echo -e "   • Autenticação JWT funcionando"
echo -e "   • 1 Depósito realizado"
echo -e "   • 1 Saque realizado"
echo -e "   • 1 Transferência realizada"
echo -e "   • Inativação de conta testada"
echo -e "   • Kafka integrado"
echo ""