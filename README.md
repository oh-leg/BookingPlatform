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

для доступа к keycloak выполните команду в отдельном терминале:
```bash
kubectl port-forward -n booking service/keycloak 8080:8080
```
при необходимости добавьте маршрутизацию в C:\Windows\System32\drivers\etc\hosts: <br/>
**127.0.0.1 keycloak**

для доступа к API Gateway:
```bash
kubectl port-forward -n booking service/gateway-service 5000:8080
```
---

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