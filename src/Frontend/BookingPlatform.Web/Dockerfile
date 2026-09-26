# --- сборка SPA (Vite + TypeScript) ---
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY . .
# Скрипт build сначала прогоняет tsc (типы), затем vite build — ошибки типов ломают образ.
RUN npm run build

# --- раздача статики ---
FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
