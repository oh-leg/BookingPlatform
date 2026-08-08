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
# Поднять полностью всю систему в docker:
docker-compose -f deploy/docker-compose.yml up -d

# Поднять только keycloak для разработки в VS
docker-compose --env-file .env.dev -f deploy/docker-compose.yml up -d keycloak-db keycloak
```

Будут подняты следующие сервисы:

| Сервис | Порт |
| :--- | :--- |
| PostgreSQL (Keycloak) | `5433` |
| Keycloak | `8080` |
| Gateway(YARP)  | `5000` |

### 2. Настройка Keycloak

#### 2.1. Доступ к консоли администратора

Откройте браузер: http://localhost:8080

Логин: `admin`  
Пароль: `admin`

#### 2.2. Создание Realm

1. Наведите курсор на мастера в левом верхнем углу → **"Create Realm"**
2. Название: `booking-platform`
3. Нажмите **"Create"**

#### 2.3. Создание клиента (Client)

1. В левом меню выберите **Clients** → **Create Client**
2. **Client ID:** `booking-api`
3. **Client Protocol:** `OpenID Connect`
4. Нажмите **"Next"**
5. Включите **"Client authentication"** → `ON`
6. **Valid redirect URIs:** `http://localhost:5000/*`
7. Нажмите **"Save"**
8. Перейдите на вкладку **"Credentials"** и скопируйте **Client Secret**

#### 2.4. Создание пользователя

1. В левом меню выберите **Users** → **Create New User**
2. **Username:** `testuser`
3. **Email:** `test@example.com`
4. Нажмите **"Create"**
5. Перейдите на вкладку **Credentials**, установите пароль (`password`) и снимите галочку **"Temporary"**.

---

### 3. Проверка работы

#### 3.1. Получение JWT-токена

```bash
curl -X POST http://localhost:8080/realms/booking-platform/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=booking-api" \
  -d "client_secret=CLIENT_SECRET" \
  -d "username=testuser" \
  -d "password=password" \
  -d "grant_type=password"
  ```

  Сохраните access_token из ответа.

#### 3.2. Проверка Gateway

В браузере GET http://localhost:5000/api/resources/scalar
предварительно с помощью расширения добавить в заголовки токен
Authorization: Bearer {ACCESS_TOKEN}
Получим страницу Scalar сервиса resource-service

