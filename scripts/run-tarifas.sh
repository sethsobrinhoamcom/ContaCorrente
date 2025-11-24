#!/bin/bash

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}   💰 Sistema de Tarifas - BankMore${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

echo -e "${YELLOW}Compilando serviço de tarifas...${NC}"
cd src/ContaCorrente.Tarifas
dotnet build

echo -e "${GREEN}✓ Compilação concluída${NC}\n"

echo -e "${YELLOW}Iniciando serviço de tarifas...${NC}"
echo -e "${YELLOW}Pressione Ctrl+C para parar${NC}\n"

dotnet run