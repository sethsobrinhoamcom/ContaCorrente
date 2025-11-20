#!/bin/bash

echo "Running tests..."

# Restore
dotnet restore

# Build
dotnet build --no-restore

# Run tests
dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

echo "Tests completed!"