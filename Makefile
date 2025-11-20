.PHONY: help build run test docker-build docker-run k8s-deploy clean

help:
	@echo "Comandos disponíveis:"
	@echo "  make build         - Compila o projeto"
	@echo "  make run           - Executa a aplicação"
	@echo "  make test          - Executa os testes"
	@echo "  make docker-build  - Build da imagem Docker"
	@echo "  make docker-run    - Executa com Docker Compose"
	@echo "  make k8s-deploy    - Deploy no Kubernetes"
	@echo "  make clean         - Limpa artifacts"

build:
	dotnet build

run:
	cd src/ContaCorrente.Api && dotnet run

test:
	dotnet test --verbosity normal

docker-build:
	docker build -t contacorrente-api:latest .

docker-run:
	docker-compose up -d

k8s-deploy:
	kubectl apply -f k8s/

clean:
	dotnet clean
	rm -rf **/bin **/obj
	rm -f *.db *.db-shm *.db-wal