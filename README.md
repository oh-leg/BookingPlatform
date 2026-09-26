# BookingPlatform — Система бронирования ресурсов

## Описание проекта
BookingPlatform — универсальная платформа для бронирования ресурсов (переговорные, оборудование, специалисты).  
Построена на микросервисной архитектуре с использованием **.NET 10**, **Docker**, **Kubernetes**, **Keycloak**, **RabbitMQ** и **SignalR**.

## Технологический стек

- **Backend:** .NET 10, ASP.NET Core WebAPI, Dapper, MediatR
- **Базы данных:** PostgreSQL, MongoDB, Redis
- **Аутентификация:** Keycloak (OIDC, JWT)
- **Очереди:** RabbitMQ
- **Real-time:** SignalR
- **API Gateway:** YARP
- **Контейнеризация:** Docker, Kubernetes
- **Логирование:** Serilog + Seq


## Запуск инфраструктуры

### Docker Compose

Из корня проекта выполните команду:

```bash
# Поднять систему в docker
docker-compose -f deploy/docker-compose.yml up -d
```

```bash
# Поднять необходимые контейнеры для разработки в VS
docker-compose --env-file .env.dev -f deploy/docker-compose.yml up -d keycloak-db keycloak redis minio minio-init
```

Будут подняты следующие сервисы:

| Сервис | Порт |
| :--- | :--- |
| PostgreSQL (Keycloak) | `5433` |
| Keycloak | `8080` |
| Redis | `6379` | 
| MinIO API | `9000` | 
| MinIO Console | `9001` |
| Gateway(YARP)  | `5000` |

### Kubernetes
---
В docker desktop создайте кластер, затем из корня проекта выполните команду:

```bash
# Поднять систему в Kubernetes
./deploy/k8s/apply-all.sh
```

После этого приложение доступно по адресу **http://localhost:8080** — `kubectl port-forward` не нужен:
data-plane сервис NGF (`http-gateway-nginx`) объявлен как `LoadBalancer` (см. `deploy/k8s/ingress/nginx-proxy.yml`),
и Docker Desktop сам публикует его на localhost. Проверить можно так:

```bash
kubectl -n default get svc http-gateway-nginx   # TYPE=LoadBalancer, EXTERNAL-IP заполнен
```

| Что | Адрес |
| :--- | :--- |
| SPA (frontend) | `http://localhost:8080` |
| Keycloak (и admin console `/auth/admin`) | `http://localhost:8080/auth` |
| API через YARP | `http://localhost:8080/api/...` |

Если нужен прямой доступ к сервисам мимо Gateway, используйте port-forward на свободные порты
(порт 8080 занят LoadBalancer'ом):

```bash
kubectl port-forward -n booking service/keycloak 8081:8080       # Keycloak напрямую
kubectl port-forward -n booking service/gateway-service 5000:8080 # YARP напрямую
```

> ⚠️ Порт 8080 на хосте занимает LoadBalancer-сервис Gateway, поэтому не поднимайте
> одновременно docker-compose стек (там Keycloak тоже проброшен на 8080) — будет конфликт портов.
> Если в вашем кластере нет LB-контроллера (`EXTERNAL-IP` = `<pending>`), вернитесь к
> `kubectl port-forward -n default service/http-gateway-nginx 8080:8080`.

при необходимости добавьте маршрутизацию в C:\Windows\System32\drivers\etc\hosts: <br/>
**127.0.0.1 keycloak**
---

### Обновление фронтенда после правок в коде

Кластер раздаёт SPA из собранного образа, поэтому изменения в
`src/Frontend/BookingPlatform.Web` нужно пересобрать в образ и перекатить:

```bash
# 1. поднимите номер тега в deploy/k8s/frontend/frontend-deployment.yml (web1 -> web2)
# 2. соберите образ с этим тегом (внутри образа выполняется npm ci + tsc + vite build)
docker build -t deploy-frontend:web2 src/Frontend/BookingPlatform.Web
# 3. примените манифест — поднимется под со свежим образом
kubectl apply -f deploy/k8s/frontend/frontend-deployment.yml
```

> ⚠️ Пересборка с **тем же** тегом не обновит кластер: при `imagePullPolicy: IfNotPresent`
> узел использует уже закэшированный образ. Всегда поднимайте версию тега.
> Быстрая проверка, что отдаётся новая сборка: `curl -s http://localhost:8080/ | grep assets/index`
> — имя файла бандла содержит хеш содержимого и меняется при каждой правке.

### 2. Проверка работы

#### 2.1. Получение JWT-токена

```bash
curl -X POST http://localhost:8080/realms/booking-platform/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=booking-api" \
  -d "client_secret=CLIENT_SECRET" \
  -d "username=testuser" \
  -d "password=password" \
  -d "grant_type=password"
  ```