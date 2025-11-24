.PHONY: help setup build run test docker-build docker-run k8s-deploy clean test-api tarifas

help:
	@echo "════════════════════════════════════════════════════════"
	@echo "  🏦 BankMore API - Comandos Disponíveis"
	@echo "════════════════════════════════════════════════════════"
	@echo ""
	@echo "  make setup          - Setup completo do ambiente"
	@echo "  make build          - Compila o projeto"
	@echo "  make run            - Executa a API"
	@echo "  make test           - Executa os testes"
	@echo "  make test-api       - Testa a API com JWT"
	@echo "  make docker-build   - Build da imagem Docker"
	@echo "  make docker-run     - Executa com Docker Compose"
	@echo "  make k8s-deploy     - Deploy no Kubernetes"
	@echo "  make tarifas        - Inicia serviço de tarifas"
	@echo "  make clean          - Limpa o ambiente"
	@echo ""
	@echo "════════════════════════════════════════════════════════"

setup:
	@./scripts/setup-complete.sh

build:
	@dotnet build

run:
	@cd src/ContaCorrente.Api && dotnet run

test:
	@dotnet test --verbosity normal

test-api:
	@./scripts/test-api-jwt.sh

docker-build:
	@docker build -t contacorrente-api:latest .

docker-run:
	@docker-compose up -d

k8s-deploy:
	@kubectl apply -f k8s/

tarifas:
	@./scripts/run-tarifas.sh

clean:
	@./scripts/cleanup.sh