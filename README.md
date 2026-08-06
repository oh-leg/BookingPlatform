# BookingPlatform — Система бронирования ресурсов

## Описание проекта
BookingPlatform — универсальная платформа для бронирования ресурсов (переговорные, оборудование, специалисты).  
Построена на микросервисной архитектуре с использованием **.NET 10**, **Docker**, **Kubernetes**, **Keycloak**, **RabbitMQ** и **SignalR**.

---

## Технологический стек

- **Backend:** .NET 10, ASP.NET Core WebAPI, Dapper, MediatR
- **Базы данных:** PostgreSQL, MongoDB, Redis
- **Аутентификация:** Keycloak (OIDC, JWT)
- **Очереди:** RabbitMQ
- **Real-time:** SignalR
- **API Gateway:** YARP
- **Контейнеризация:** Docker, Kubernetes
- **Логирование:** Serilog + Seq

---

## Локальный запуск

### 1. Запуск инфраструктуры (Docker Compose)

Из корня проекта выполните команду:

```bash
docker-compose -f deploy/docker-compose.yml up -d
```

Будут подняты следующие сервисы:

| Сервис | Порт |
| :--- | :--- |
| PostgreSQL (Keycloak) | `5433` |
| Keycloak | `8080` |