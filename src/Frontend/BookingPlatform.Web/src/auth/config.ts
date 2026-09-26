/**
 * Настройки OIDC-клиента.
 *
 * Всё через единый вход NGINX Gateway:
 *   /         -> этот фронтенд (nginx)
 *   /auth/*   -> NGF напрямую в keycloak-service (YARP НЕ участвует, он только проверяет JWT)
 *   /api/*    -> NGF -> YARP (проверка JWT) -> микросервисы
 *
 * По умолчанию адреса строятся от текущего origin (SPA живёт за шлюзом).
 * Переопределение — переменными окружения сборки Vite (VITE_OIDC_*).
 */
const origin = window.location.origin;

export const oidcSettings = {
  /** Публичный OIDC-клиент из deploy/realm.json (PKCE, без client secret). */
  clientId: import.meta.env.VITE_OIDC_CLIENT_ID ?? 'booking-spa',
  authority: import.meta.env.VITE_OIDC_AUTHORITY ?? `${origin}/auth/realms/booking-platform`,
  /** Возврат после входа и после выхода — на главную страницу. */
  redirectUri: import.meta.env.VITE_OIDC_REDIRECT_URI ?? `${origin}/`,
  postLogoutRedirectUri: import.meta.env.VITE_OIDC_POST_LOGOUT_REDIRECT_URI ?? `${origin}/`,
  /** Роли в токене нужны проверкам на стороне API. */
  scope: 'openid profile email roles'
} as const;
