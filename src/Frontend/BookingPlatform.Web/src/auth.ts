import { UserManager, type User } from 'oidc-client-ts';

// Всё через единый вход NGINX Gateway (http://localhost:8080):
//   /         -> этот фронтенд (nginx)
//   /auth/*   -> NGF напрямую в keycloak-service (YARP НЕ участвует, он только проверяет JWT)
//   /api/*    -> NGF -> YARP (проверка JWT) -> микросервисы
const origin = window.location.origin;

// Публичный OIDC-клиент из deploy/realm.json (PKCE, без client secret).
const CLIENT_ID = 'booking-spa';

export const userManager = new UserManager({
  authority: `${origin}/auth/realms/booking-platform`,
  client_id: CLIENT_ID,
  redirect_uri: `${origin}/`,
  post_logout_redirect_uri: `${origin}/`,
  response_type: 'code',
  scope: 'openid profile email roles',
  loadUserInfo: true,
  automaticSilentRenew: false,
  accessTokenExpiringNotificationTimeInSeconds: 60
});

/** true, если страница открыта как redirect_uri после логина в Keycloak. */
export function isSigninRedirectCallback(): boolean {
  return new URLSearchParams(window.location.search).has('code');
}

/** Текущий пользователь, если его access_token ещё не истёк; иначе null. */
export async function getValidUser(): Promise<User | null> {
  const user = await userManager.getUser();
  return user && !user.expired ? user : null;
}

/** Отображаемое имя: name -> preferred_username -> sub. */
export function displayNameOf(user: User): string | null {
  const profile = user.profile;
  const candidate = profile.name ?? profile.preferred_username ?? profile.sub;
  return typeof candidate === 'string' ? candidate : null;
}
