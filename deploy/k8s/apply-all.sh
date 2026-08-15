#!/bin/bash

set -e  # Останавливаем скрипт при ошибке

echo "🚀 Начинаем развертывание Booking Platform в Kubernetes..."

# Определяем директории
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_FOLDER="$(dirname "$SCRIPT_DIR")"

# Цвета для вывода
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${YELLOW}📂 SCRIPT_DIR: $SCRIPT_DIR${NC}"
echo -e "${YELLOW}📂 DEPLOY_FOLDER: $DEPLOY_FOLDER${NC}"

# ----------------------------------------------------------------------
# 1. Установка CRD Gateway API (стандартные)
# ----------------------------------------------------------------------
echo -e "${YELLOW}📡 Установка стандартных CRD Gateway API...${NC}"
if kubectl get crd gateways.gateway.networking.k8s.io &>/dev/null; then
    echo "✅ CRD Gateway API уже установлены"
else
    echo "Устанавливаю стандартные CRD Gateway API..."
    kubectl apply -f https://github.com/kubernetes-sigs/gateway-api/releases/download/v1.6.1/standard-install.yaml
    echo "⏳ Ожидание регистрации CRD (10 секунд)..."
    sleep 10
fi

# ----------------------------------------------------------------------
# 2. Установка CRD от NGINX (расширения)
# ----------------------------------------------------------------------
echo -e "${YELLOW}📡 Установка расширенных CRD от NGINX...${NC}"
if kubectl get crd authenticationfilters.gateway.nginx.org &>/dev/null; then
    echo "✅ Расширенные CRD NGINX уже установлены"
else
    echo "Устанавливаю расширенные CRD NGINX..."
    kubectl apply --server-side --force-conflicts -f https://raw.githubusercontent.com/nginx/nginx-gateway-fabric/v2.6.6/deploy/crds.yaml
    echo "⏳ Ожидание регистрации CRD (10 секунд)..."
    sleep 10
fi

# ----------------------------------------------------------------------
# 3. Создание namespace booking
# ----------------------------------------------------------------------
echo -e "${YELLOW}📦 Создание namespace booking...${NC}"
kubectl create namespace booking 2>/dev/null || echo "Namespace booking уже существует"

# ----------------------------------------------------------------------
# 4. PostgreSQL
# ----------------------------------------------------------------------
echo -e "${YELLOW}🐘 Развертывание PostgreSQL...${NC}"
kubectl apply -f "$SCRIPT_DIR/keycloak/postgres-pvc.yml"
kubectl apply -f "$SCRIPT_DIR/keycloak/postgres-deployment.yml"
kubectl apply -f "$SCRIPT_DIR/keycloak/postgres-service.yml"

# ----------------------------------------------------------------------
# 5. Keycloak
# ----------------------------------------------------------------------
echo -e "${YELLOW}🔐 Развертывание Keycloak...${NC}"
echo "Создание ConfigMap для Keycloak из realm.json..."
kubectl create configmap keycloak-realm \
  -n booking \
  --from-file=realm.json="$DEPLOY_FOLDER/realm.json" \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl apply -f "$SCRIPT_DIR/keycloak/keycloak-deployment.yml"
kubectl apply -f "$SCRIPT_DIR/keycloak/keycloak-service.yml"

# ----------------------------------------------------------------------
# 6. Микросервисы (Gateway, Resource, Booking, Notification, File)
# ----------------------------------------------------------------------
echo -e "${YELLOW}⚙️ Развертывание микросервисов...${NC}"
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

# ----------------------------------------------------------------------
# 7. NGINX Gateway Fabric (установка через манифест nodeport)
# ----------------------------------------------------------------------
echo -e "${YELLOW}🌐 Установка NGINX Gateway Fabric...${NC}"
if kubectl get deployment -n nginx-gateway ngf-nginx-gateway-fabric 2>/dev/null; then
    echo "✅ NGINX Gateway Fabric уже установлен"
else
    echo "Устанавливаю NGINX Gateway Fabric через манифест..."
    kubectl create namespace nginx-gateway 2>/dev/null || echo "Namespace уже существует"
    kubectl apply -f https://raw.githubusercontent.com/nginx/nginx-gateway-fabric/v2.6.6/deploy/nodeport/deploy.yaml
    echo "⏳ Ожидание готовности NGINX Gateway Fabric (30 секунд)..."
    sleep 30
fi

# ----------------------------------------------------------------------
# 8. Gateway API (ReferenceGrant, Gateway, HTTPRoute)
# ----------------------------------------------------------------------
echo -e "${YELLOW}📡 Настройка Gateway API...${NC}"
kubectl apply -f "$SCRIPT_DIR/ingress/reference-grant.yml"
kubectl apply -f "$SCRIPT_DIR/ingress/gateway.yml"
kubectl apply -f "$SCRIPT_DIR/ingress/httproute.yml"

# ----------------------------------------------------------------------
# 9. Ожидание готовности подов
# ----------------------------------------------------------------------
echo -e "${YELLOW}⏳ Ожидание готовности подов (30 секунд)...${NC}"
sleep 30

# ----------------------------------------------------------------------
# 10. Проверка статуса
# ----------------------------------------------------------------------
echo -e "${GREEN}✅ Статус подов в namespace booking:${NC}"
kubectl get pods -n booking

echo -e "${GREEN}✅ Статус подов в namespace nginx-gateway:${NC}"
kubectl get pods -n nginx-gateway

echo -e "${GREEN}✅ Статус сервисов:${NC}"
kubectl get services -n booking

echo ""
echo -e "${GREEN}🎉 Развертывание завершено!${NC}"
echo -e "🌐 Для доступа к Gateway выполни: ${YELLOW}kubectl port-forward -n default service/http-gateway-nginx 8080:80${NC}"
echo -e "🔐 Keycloak доступен через Gateway по адресу: ${YELLOW}http://localhost:8080${NC}"

kubectl port-forward -n default service/http-gateway-nginx 8080:80