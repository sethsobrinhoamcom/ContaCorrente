# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ContaCorrente.sln", "./"]
COPY ["src/ContaCorrente.Api/ContaCorrente.Api.csproj", "src/ContaCorrente.Api/"]
COPY ["src/ContaCorrente.Application/ContaCorrente.Application.csproj", "src/ContaCorrente.Application/"]
COPY ["src/ContaCorrente.Domain/ContaCorrente.Domain.csproj", "src/ContaCorrente.Domain/"]
COPY ["src/ContaCorrente.Infrastructure/ContaCorrente.Infrastructure.csproj", "src/ContaCorrente.Infrastructure/"]
COPY ["tests/ContaCorrente.Tests/ContaCorrente.Tests.csproj", "tests/ContaCorrente.Tests/"]

# Restore dependencies
RUN dotnet restore "ContaCorrente.sln"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/ContaCorrente.Api"
RUN dotnet build "ContaCorrente.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ContaCorrente.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install SQLite (caso precise de CLI)
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

# Criar diretório para o banco de dados
RUN mkdir -p /app/data

# Variável de ambiente para connection string
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/contacorrente.db"

ENTRYPOINT ["dotnet", "ContaCorrente.Api.dll"]
```

### 2. .dockerignore

**`.dockerignore`**
```
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/.idea
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md
**/*.db
**/*.db-shm
**/*.db-wal