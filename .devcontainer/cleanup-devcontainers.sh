#!/usr/bin/env bash
set -e

# Run docker compose down
COMPOSE_FILE="docker-compose.yml"
echo "Stopping and removing devcontainer services..."
docker compose -f "$COMPOSE_FILE" down --remove-orphans --volumes -v

echo "Docker compose stop complete."
