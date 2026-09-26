import { useCallback, useEffect, useRef, useState } from 'react';
import type { User } from 'oidc-client-ts';
import { toErrorMessage } from '../api';
import {
  authErrorFromUrl,
  clearAuthParamsFromUrl,
  completeSigninRedirect,
  displayNameOf,
  getValidUser,
  isSigninRedirectCallback,
  signOut,
  startSignin,
  userManager
} from './oidc';

/** Состояние сессии: страница «Главная» живёт в любом из этих состояний. */
export type AuthStatus = 'checking' | 'anonymous' | 'authenticated';

export interface AuthState {
  status: AuthStatus;
  /** Имя для приветствия (только в статусе authenticated). */
  userName: string | null;
  /** Access token для вызовов API (только в статусе authenticated). */
  accessToken: string | null;
  /** Текст ошибки входа/сессии — показывается пользователю. */
  error: string | null;
}

export interface UseAuthResult extends AuthState {
  /** Запускает вход: браузер уходит на страницу Keycloak. */
  login: () => Promise<void>;
  /** Завершает сессию в Keycloak и возвращает на главную. */
  logout: () => Promise<void>;
  /** Сбрасывает сессию в анонимную, например когда API ответил 401. */
  endSession: (message: string) => void;
}

const CHECKING_STATE: AuthState = {
  status: 'checking',
  userName: null,
  accessToken: null,
  error: null
};

const ANONYMOUS_STATE: AuthState = {
  status: 'anonymous',
  userName: null,
  accessToken: null,
  error: null
};

function stateForUser(user: User | null): AuthState {
  if (user === null) {
    return ANONYMOUS_STATE;
  }

  return {
    status: 'authenticated',
    userName: displayNameOf(user),
    accessToken: user.access_token,
    error: null
  };
}

/**
 * Сессия пользователя.
 *
 * Главная страница не зависит от авторизации: отсутствие сессии — обычное
 * состояние (anonymous), а не повод уводить пользователя в Keycloak.
 * Редирект на вход происходит только по явному действию — кнопке «Войти».
 */
export function useAuth(): UseAuthResult {
  const [state, setState] = useState<AuthState>(CHECKING_STATE);

  // Инициализация должна пройти ровно один раз: code в Keycloak одноразовый,
  // поэтому повторный прогон под React.StrictMode мы отсекаем.
  const initialized = useRef(false);

  useEffect(() => {
    if (initialized.current) return;
    initialized.current = true;

    void (async () => {
      const callbackError = authErrorFromUrl();
      if (callbackError !== null) {
        clearAuthParamsFromUrl();
        setState({ ...ANONYMOUS_STATE, error: callbackError });
        return;
      }

      try {
        // Возврат из Keycloak после логина: в URL есть code — завершаем PKCE-обмен.
        if (isSigninRedirectCallback()) {
          await completeSigninRedirect();
        }

        setState(stateForUser(await getValidUser()));
      } catch (error) {
        clearAuthParamsFromUrl();
        setState({ ...ANONYMOUS_STATE, error: toErrorMessage(error) });
      }
    })();
  }, []);

  const login = useCallback(async (): Promise<void> => {
    setState((previous) => ({ ...previous, error: null }));

    try {
      await startSignin();
    } catch (error) {
      setState({ ...ANONYMOUS_STATE, error: toErrorMessage(error) });
    }
  }, []);

  const logout = useCallback(async (): Promise<void> => {
    try {
      await signOut();
    } catch (error) {
      setState({ ...ANONYMOUS_STATE, error: toErrorMessage(error) });
    }
  }, []);

  const endSession = useCallback((message: string): void => {
    // Токен протух: чистим хранилище oidc-client-ts и показываем публичную главную.
    void userManager.removeUser();
    setState({ ...ANONYMOUS_STATE, error: message });
  }, []);

  return { ...state, login, logout, endSession };
}
