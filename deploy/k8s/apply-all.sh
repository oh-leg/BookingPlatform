#!/bin/bash

set -e  # Останавливаем скрипт при ошибке

echo "Начинаем развертывание Booking Platform в Kubernetes..."

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_FOLDER="$(dirname "$SCRIPT_DIR")"

echo "SCRIPT_DIR: $SCRIPT_DIR"
echo "DEPLOY_FOLDER: $DEPLOY_FOLDER"

# Цвета для вывода
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# 1. Создание namespace
echo -e "${YELLOW}Создание namespace booking...${NC}"
kubectl create namespace booking 2>/dev/null || echo "Namespace booking уже существует"

# 2. PostgreSQL
echo -e "${YELLOW}Развертывание PostgreSQL...${NC}"
kubectl apply -f "$SCRIPT_DIR/keycloak/postgres-pvc.yml"
kubectl apply -f "$SCRIPT_DIR/keycloak/postgres-deployment.yml"
kubectl apply -f "$SCRIPT_DIR/keycloak/postgres-service.yml"

# 3. Keycloak
echo -e "${YELLOW}Развертывание Keycloak...${NC}"
echo "Создание ConfigMap для Keycloak из realm.json..."
kubectl create configmap keycloak-realm \
  -n booking \
  --from-file=realm.json="$DEPLOY_FOLDER/realm.json" \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl apply -f "$SCRIPT_DIR/keycloak/keycloak-deployment.yml"
kubectl apply -f "$SCRIPT_DIR/keycloak/keycloak-service.yml"

# 4. Микросервисы
echo -e "${YELLOW}Развертывание микросервисов...${NC}"
kubectl apply -f "$SCRIPT_DIR/gateway/gateway-deployment.yml"
kubectl apply -f "$SCRIPT_DIR/gateway/gateway-service.yml"

kubectl apply -f "$SCRIPT_DIR/resource-service/resource-service-deployment.yml"
kubectl apply -f "$SCRIPT_DIR/resource-service/resource-service-service.yml"

kubectl apply -f "$SCRIPT_DIR/booking-service/booking-service-deployment.yml"
kubectl apply -f "$SCRIPT_DIR/booking-service/booking-service-service.yml"

kubectl apply -f "$SCRIPT_DIR/notification-service/notification-service-deployment.yml"
kubectl apply -f "$SCRIPT_DIR/notification-service/notification-service-service.yml"

kubectl apply -f "$SCRIPT_DIR/file-service/file-service-deployment.yml"
kubectl apply -f "$SCRIPT_DIR/file-service/file-service-service.yml"

# 5. Ожидание готовности подов
echo -e "${YELLOW}Ожидание готовности подов (15 секунд)...${NC}"
sleep 15

# 6. Проверка статуса
echo -e "${GREEN}Статус подов:${NC}"
kubectl get pods -n booking

echo -e "${GREEN}Статус сервисов:${NC}"
kubectl get services -n booking

echo ""
echo -e "${GREEN}Развертывание завершено!${NC}"
echo -e "Keycloak доступен: ${YELLOW}http://localhost:8080${NC} (port-forward: kubectl port-forward -n booking service/keycloak 8080:8080)"
echo -e "Gateway доступен: ${YELLOW}http://localhost:5000${NC} (port-forward: kubectl port-forward -n booking service/gateway-service 5000:8080)"