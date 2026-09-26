import { ApiError } from './errors';

/** Вызов API через YARP-шлюз: токен Keycloak едет в заголовке Authorization. */
export async function apiFetch(path: string, accessToken: string): Promise<unknown> {
  const response = await fetch(path, {
    headers: { Authorization: `Bearer ${accessToken}` }
  });

  if (response.status === 401) {
    throw new ApiError(401, 'Токен не принят шлюзом (401)');
  }
  if (!response.ok) {
    throw new ApiError(response.status, `HTTP ${response.status}`);
  }

  return (await response.json()) as unknown;
}

/** Диагностический эндпоинт ресурс-сервиса: структура ответа заранее не фиксирована. */
export function fetchTestData(accessToken: string): Promise<unknown> {
  return apiFetch('/api/resources/test-data', accessToken);
}
