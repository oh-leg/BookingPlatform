/// <reference types="vite/client" />

/**
 * Переменные окружения Vite: значения подставляются на этапе сборки
 * (файлы .env / .env.local или переменные окружения в CI).
 * Все переменные приложения обязаны начинаться с VITE_.
 */
interface ImportMetaEnv {
  /** Authority Keycloak, например https://host/auth/realms/booking-platform. */
  readonly VITE_OIDC_AUTHORITY?: string;
  /** Публичный OIDC-клиент (PKCE). */
  readonly VITE_OIDC_CLIENT_ID?: string;
  /** Куда Keycloak возвращает после входа. */
  readonly VITE_OIDC_REDIRECT_URI?: string;
  /** Куда Keycloak возвращает после выхода. */
  readonly VITE_OIDC_POST_LOGOUT_REDIRECT_URI?: string;
}
