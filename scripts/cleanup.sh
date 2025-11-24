#!/bin/bash

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${YELLOW}🧹 Limpando ambiente...${NC}\n"

# Parar containers
echo -e "${YELLOW}Parando containers Docker...${NC}"
docker-compose down -v
echo -e "${GREEN}✓ Containers parados e volumes removidos${NC}\n"

# Limpar builds
echo -e "${YELLOW}Limpando builds...${NC}"
dotnet clean
echo -e "${GREEN}✓ Builds limpos${NC}\n"

# Remover banco de dados
echo -e "${YELLOW}Removendo banco de dados...${NC}"
rm -f src/ContaCorrente.Api/contacorrente.db*
rm -f contacorrente.db*
echo -e "${GREEN}✓ Banco de dados removido${NC}\n"

# Remover imagens Docker (opcional)
read -p "Deseja remover as imagens Docker também? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Removendo imagens Docker...${NC}"
    docker rmi contacorrente-api:latest 2>/dev/null
    echo -e "${GREEN}✓ Imagens removidas${NC}\n"
fi

echo -e "${GREEN}✅ Limpeza concluída!${NC}\n"