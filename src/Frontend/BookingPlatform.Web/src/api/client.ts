import { ApiError } from './errors';

export interface RequestOptions {
  /** Позволяет отменить запрос, например при размонтировании компонента. */
  signal?: AbortSignal;
}

const UNAUTHORIZED_MESSAGE = 'Токен не принят шлюзом (401)';

function requestHeaders(accessToken: string): Record<string, string> {
  return {
    Accept: 'application/json',
    Authorization: `Bearer ${accessToken}`
  };
}

/** Вызов API через YARP-шлюз: токен Keycloak едет в заголовке Authorization. */
export async function apiFetch<T = unknown>(
  path: string,
  accessToken: string,
  options: RequestOptions = {}
): Promise<T> {
  const response = await fetch(path, {
    headers: requestHeaders(accessToken),
    signal: options.signal
  });

  if (response.status === 401) {
    throw new ApiError(401, UNAUTHORIZED_MESSAGE);
  }
  if (!response.ok) {
    throw new ApiError(response.status, `HTTP ${response.status}`);
  }

  return (await response.json()) as T;
}

/** Диагностический эндпоинт ресурс-сервиса: структура ответа заранее не фиксирована. */
export function fetchTestData(accessToken: string, options?: RequestOptions): Promise<unknown> {
  return apiFetch('/api/resources/test-data', accessToken, options);
}
