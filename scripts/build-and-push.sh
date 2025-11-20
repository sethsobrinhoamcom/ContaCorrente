#!/bin/bash

# Build da imagem Docker
echo "Building Docker image..."
docker build -t contacorrente-api:latest .

# Tag para registry (opcional)
# docker tag contacorrente-api:latest your-registry/contacorrente-api:latest

# Push para registry (opcional)
# docker push your-registry/contacorrente-api:latest

echo "Build completed!"