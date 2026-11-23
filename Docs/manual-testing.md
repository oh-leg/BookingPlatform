# Ручной запуск и проверка BookingPlatform (docker-compose)

Сценарий проверяет связку Keycloak → Gateway → BookingService → ResourceService → PostgreSQL:
получение токенов, расчёт слотов, создание и отмену броней, защиту от двойного бронирования,
подтверждение администратором и историю изменений.

Команды написаны для bash (macOS / Linux). На Windows используйте Git Bash или WSL.
Все команды выполняются из корня репозитория.

## 0. Подготовка

- Docker Desktop, `curl`, `jq`.
- **macOS:** Проверить отключен ли «Приёмник AirPlay» (Системные настройки → Основные → AirDrop и Handoff),
  иначе порт `5000` будет занят.
- Для входа в админку Keycloak из браузера добавьте в hosts строку `127.0.0.1 keycloak`
  (Windows: `C:\Windows\System32\drivers\etc\hosts`, macOS/Linux: `/etc/hosts`).

## 1. Запуск

```bash
docker compose -f deploy/docker-compose.yml up -d --build \
  keycloak-db keycloak booking-db booking-service resource-db resource-service gateway
docker compose -f deploy/docker-compose.yml ps
```

Ожидается: `booking-db`, `keycloak-db`, `resource-db` — `healthy`, остальные — `Up`.
Вместе с `gateway` поднимутся и его зависимости `notification-service` и `file-service`.

Keycloak стартует 20–60 секунд:

```bash
until curl -sf -o /dev/null http://localhost:8080/realms/booking-platform; do sleep 3; done; echo "Keycloak готов"
```

## 2. Сервисы живы

```bash
curl -s -o /dev/null -w "BookingService /health  -> %{http_code}\n" http://localhost:5002/health
curl -s -o /dev/null -w "Gateway /health         -> %{http_code}\n" http://localhost:5000/health
curl -s -o /dev/null -w "ResourceService /openapi/v1.json -> %{http_code}\n" http://localhost:5001/openapi/v1.json
docker logs booking-booking-service 2>&1 | grep -i "миграц"
```

Ожидается: везде `200`; при первом запуске в логе есть `Применение миграции 0001_initial`.

## 3. Токены

```bash
SECRET=$(jq -r '.clients[] | select(.clientId=="booking-api") | .secret' deploy/realm.json)
token() { curl -s -X POST http://localhost:8080/realms/booking-platform/protocol/openid-connect/token \
  -d grant_type=password -d client_id=booking-api -d client_secret="$SECRET" \
  -d username="$1" -d password=password | jq -r .access_token; }
USER_TOKEN=$(token testuser); ADMIN_TOKEN=$(token adminuser)

# содержимое токена
echo "$ADMIN_TOKEN" | jq -R 'split(".")[1] | gsub("-";"+") | gsub("_";"/") | @base64d | fromjson | {sub, preferred_username, roles: .realm_access.roles}'
```

Ожидается: в токене есть `sub`, у `adminuser` — роль `booking-admin`.
Если Keycloak отвечает `HTTPS required` или в токене нет `sub` — база Keycloak создана до правок
`realm.json`.

## 4. Тестовый ресурс (ResourceService, порт 5001)

Расписание задаётся по московскому времени, `dayOfWeek` — число (1 — понедельник … 5 — пятница).

```bash
R=http://localhost:5001/api/resources
RID=$(curl -s -X POST $R -H 'Content-Type: application/json' \
  -d '{"resourceTypeName":"Кабинет","name":"Кабинет врача №1","capacity":1}' | tr -d '"')

# Услуга без подтверждения: 60 мин, буфер после 15 мин
O1=$(curl -s -X POST $R/$RID/offers -H 'Content-Type: application/json' \
  -d '{"name":"Консультация","durationMinutes":60,"price":1500,"currency":"RUB","bufferBeforeMinutes":0,"bufferAfterMinutes":15,"cancellationDeadlineHours":0,"requiresConfirmation":false}' | tr -d '"')

# Услуга с подтверждением: 30 мин
O2=$(curl -s -X POST $R/$RID/offers -H 'Content-Type: application/json' \
  -d '{"name":"Осмотр","durationMinutes":30,"price":800,"currency":"RUB","bufferBeforeMinutes":0,"bufferAfterMinutes":0,"cancellationDeadlineHours":0,"requiresConfirmation":true}' | tr -d '"')

# Расписание пн–пт 09:00–13:00 МСК для обеих услуг
for O in $O1 $O2; do for d in 1 2 3 4 5; do
  curl -s -o /dev/null -X POST $R/$RID/offers/$O/schedule -H 'Content-Type: application/json' \
    -d "{\"dayOfWeek\":$d,\"startTime\":\"09:00:00\",\"endTime\":\"13:00:00\"}"; done; done

curl -s -o /dev/null -w "activate -> %{http_code}\n" -X POST $R/$RID/activate
echo "RID=$RID O1=$O1 O2=$O2"
```

Ожидается: три GUID, `activate -> 200`.

## 5. Сценарий бронирования (через Gateway, порт 5000)

Укажите **будний день в будущем**, на который у ресурса ещё нет броней.
Время в запросах — UTC: 09:00 МСК = 06:00Z.

```bash
DAY=2026-11-17            
B=http://localhost:5000/api/bookings
U="Authorization: Bearer $USER_TOKEN"; A="Authorization: Bearer $ADMIN_TOKEN"
J='Content-Type: application/json'
```

| # | Действие | Команда | Ожидается |
|---|---|---|---|
| 5.1 | Без токена | `curl -s -o /dev/null -w '%{http_code}\n' "$B/slots?resourceId=$RID&offerId=$O1&from=$DAY&to=$DAY"` | `401` |
| 5.2 | Свободные слоты | `curl -s -H "$U" "$B/slots?resourceId=$RID&offerId=$O1&from=$DAY&to=$DAY" \| jq -r '.[].startAt'` | 13 слотов 06:00 … 09:00 UTC с шагом 15 мин |
| 5.3 | Бронь на 10:00 МСК | `BID=$(curl -s -X POST $B -H "$U" -H "$J" -d "{\"resourceId\":\"$RID\",\"offerId\":\"$O1\",\"startAt\":\"${DAY}T07:00:00Z\",\"comment\":\"ручная проверка\"}" \| jq -r .id); curl -s -H "$U" $B/$BID \| jq .status` | `"Confirmed"` |
| 5.4 | Тот же слот повторно | `curl -s -X POST $B -H "$A" -H "$J" -d "{\"resourceId\":\"$RID\",\"offerId\":\"$O1\",\"startAt\":\"${DAY}T07:00:00Z\"}" \| jq .status` | `409` |
| 5.5 | Пересечение с буфером | то же со `startAt` `${DAY}T08:00:00Z` | `409` (бронь 07:00–08:00 + 15 мин буфера) |
| 5.6 | Время вне сетки 15 мин | то же со `startAt` `${DAY}T08:20:00Z` | `409` |
| 5.7 | Слоты после брони | команда 5.2 | 4 слота: 08:15, 08:30, 08:45, 09:00 UTC |
| 5.8 | Мои брони | `curl -s -H "$U" "$B?from=${DAY}T00:00:00Z&to=${DAY}T23:59:59Z" \| jq '.[] \| {id, status, startAt}'` | бронь из 5.3 |
| 5.9 | Отмена | `curl -s -X POST $B/$BID/cancel -H "$U" -H "$J" -d '{"reason":"передумал"}' \| jq .status` | `"Cancelled"` |
| 5.10 | Повторная отмена | та же команда | `409` |
| 5.11 | История | `curl -s -H "$U" $B/$BID/history \| jq -c '.[] \| {fromStatus, toStatus, reason}'` | 2 записи: `null → Confirmed`, `Confirmed → Cancelled` |
| 5.12 | Слот освободился | команда 5.2 | снова 13 слотов |
| 5.13 | Бронь с подтверждением | `BID2=$(curl -s -X POST $B -H "$U" -H "$J" -d "{\"resourceId\":\"$RID\",\"offerId\":\"$O2\",\"startAt\":\"${DAY}T09:00:00Z\"}" \| jq -r .id); curl -s -H "$U" $B/$BID2 \| jq .status` | `"Pending"` |
| 5.14 | Подтверждение обычным пользователем | `curl -s -o /dev/null -w '%{http_code}\n' -X POST $B/$BID2/confirm -H "$U"` | `403` |
| 5.15 | Подтверждение администратором | `curl -s -X POST $B/$BID2/confirm -H "$A" \| jq .status` | `"Confirmed"` |
| 5.16 | Брони ресурса (только админ) | `curl -s -H "$A" "$B/resources/$RID?from=${DAY}T00:00:00Z&to=${DAY}T23:59:59Z" \| jq -c '.[] \| {status, startAt}'` | 2 брони, включая отменённые: `Cancelled` 07:00Z и `Confirmed` 09:00Z |
| 5.17 | Бронь в прошлом | `curl -s -X POST $B -H "$U" -H "$J" -d "{\"resourceId\":\"$RID\",\"offerId\":\"$O1\",\"startAt\":\"2020-01-06T07:00:00Z\"}" \| jq .status` | `400` |
| 5.18 | Неизвестный ресурс | `curl -s -X POST $B -H "$U" -H "$J" -d "{\"resourceId\":\"$(uuidgen)\",\"offerId\":\"$O1\",\"startAt\":\"${DAY}T07:00:00Z\"}" \| jq .status` | `404` |

Ресурсы через Gateway:

```bash
curl -s -H "$U" http://localhost:5000/api/resources | jq '.[].name'               # список ресурсов (по одному на каждый прогон шага 4)
curl -s -o /dev/null -w '%{http_code}\n' http://localhost:5000/api/resources      # 401
```

## 6. Проверка в PostgreSQL

```bash
docker exec booking-booking-db psql -U booking_user -d booking -c \
  "select id, status, start_at, end_at, cancelled_at from bookings order by created_at;"
docker exec booking-booking-db psql -U booking_user -d booking -c \
  "select booking_id, from_status, to_status, changed_at from booking_status_history order by changed_at;"
docker exec booking-booking-db psql -U booking_user -d booking -c "\d bookings" | grep ex_bookings_no_overlap
```

Ожидается: отменённая бронь не удалена (`cancelled_at` заполнен), у каждой брони есть записи
в истории, время хранится в UTC (`+00`), есть ограничение `ex_bookings_no_overlap`
(`EXCLUDE USING gist`).

## 7. Остановка и очистка

```bash
docker compose -f deploy/docker-compose.yml stop        # остановить, данные сохранятся
docker compose -f deploy/docker-compose.yml down        # удалить контейнеры, volume останутся
docker compose -f deploy/docker-compose.yml down -v     # ⚠️ удалить и все данные (Keycloak, БД, MongoDB)
```

