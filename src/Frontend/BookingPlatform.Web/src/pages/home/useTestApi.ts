import { useCallback, useEffect, useRef, useState } from 'react';
import { ApiError, fetchTestData, toErrorMessage } from '../../api';

/** Результат диагностического запроса к API. */
export type TestApiResult =
  | { kind: 'idle' }
  | { kind: 'success'; payload: unknown }
  | { kind: 'failure'; message: string };

export interface UseTestApiOptions {
  /** Токен активной сессии; без него запрос не имеет смысла. */
  accessToken: string | null;
  /** Вызывается, когда шлюз ответил 401 — сессия больше не годится. */
  onSessionExpired: (message: string) => void;
}

export interface UseTestApiResult {
  result: TestApiResult;
  /** Запрос выполняется: кнопка блокируется. */
  pending: boolean;
  run: () => void;
}

const SESSION_EXPIRED_MESSAGE = 'Сессия истекла — войдите заново.';

/** Логика кнопки «Тест API»: запрос, состояние загрузки и разбор ошибок. */
export function useTestApi({ accessToken, onSessionExpired }: UseTestApiOptions): UseTestApiResult {
  const [result, setResult] = useState<TestApiResult>({ kind: 'idle' });
  const [pending, setPending] = useState(false);
  const currentRequest = useRef<AbortController | null>(null);

  // Незавершённый запрос отменяем при размонтировании страницы.
  useEffect(() => () => currentRequest.current?.abort(), []);

  const execute = useCallback(async (): Promise<void> => {
    if (accessToken === null) {
      onSessionExpired(SESSION_EXPIRED_MESSAGE);
      return;
    }

    currentRequest.current?.abort();
    const controller = new AbortController();
    currentRequest.current = controller;

    setPending(true);
    setResult({ kind: 'idle' });

    try {
      const payload = await fetchTestData(accessToken, { signal: controller.signal });
      setResult({ kind: 'success', payload });
    } catch (error) {
      if (controller.signal.aborted) {
        return; // запрос отменили мы сами — это не ошибка
      }
      if (error instanceof ApiError && error.status === 401) {
        onSessionExpired(SESSION_EXPIRED_MESSAGE);
        return;
      }
      setResult({ kind: 'failure', message: toErrorMessage(error) });
    } finally {
      if (currentRequest.current === controller) {
        currentRequest.current = null;
        setPending(false);
      }
    }
  }, [accessToken, onSessionExpired]);

  const run = useCallback((): void => {
    void execute();
  }, [execute]);

  return { result, pending, run };
}
