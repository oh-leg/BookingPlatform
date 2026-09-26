import { UserManager, type User } from 'oidc-client-ts';
import { oidcSettings } from './config';

/** Единственный экземпляр клиента OIDC на всё приложение. */
export const userManager = new UserManager({
  authority: oidcSettings.authority,
  client_id: oidcSettings.clientId,
  redirect_uri: oidcSettings.redirectUri,
  post_logout_redirect_uri: oidcSettings.postLogoutRedirectUri,
  response_type: 'code',
  scope: oidcSettings.scope,
  loadUserInfo: true,
  automaticSilentRenew: false,
  accessTokenExpiringNotificationTimeInSeconds: 60
});

/** Параметры, которые Keycloak добавляет к redirect_uri. */
const CALLBACK_PARAMS = ['code', 'state', 'session_state', 'iss', 'error', 'error_description'] as const;

function callbackParams(): URLSearchParams {
  return new URLSearchParams(window.location.search);
}

/** true, если страница открыта как redirect_uri после логина в Keycloak. */
export function isSigninRedirectCallback(): boolean {
  return callbackParams().has('code');
}

/** Текст ошибки, если Keycloak вернул пользователя с ?error=... (например, вход отменён). */
export function authErrorFromUrl(): string | null {
  const params = callbackParams();
  const error = params.get('error');
  return error === null ? null : (params.get('error_description') ?? error);
}

/** Убирает служебные параметры Keycloak из адресной строки. */
export function clearAuthParamsFromUrl(): void {
  const params = callbackParams();
  const hasCallbackParams = CALLBACK_PARAMS.some((param) => params.has(param));
  if (!hasCallbackParams) return;

  window.history.replaceState({}, document.title, window.location.pathname);
}

/** Завершает PKCE-обмен: code одноразовый, поэтому вызывать нужно ровно один раз. */
export async function completeSigninRedirect(): Promise<void> {
  await userManager.signinRedirectCallback();
  clearAuthParamsFromUrl();
}

/** Текущий пользователь, если его access_token ещё не истёк; иначе null. */
export async function getValidUser(): Promise<User | null> {
  const user = await userManager.getUser();
  return user !== null && !user.expired ? user : null;
}

/** Отображаемое имя: name -> preferred_username -> sub. */
export function displayNameOf(user: User): string | null {
  const profile = user.profile;
  const candidate = profile.name ?? profile.preferred_username ?? profile.sub;
  return typeof candidate === 'string' ? candidate : null;
}

/** Уводит браузер на страницу входа Keycloak. */
export function startSignin(): Promise<void> {
  return userManager.signinRedirect();
}

/** Завершает сессию в Keycloak и возвращает пользователя на главную страницу. */
export function signOut(): Promise<void> {
  return userManager.signoutRedirect();
}
