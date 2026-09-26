import { Button, Card, CodeBlock } from '../../components';
import styles from './HomePage.module.css';
import { useTestApi } from './useTestApi';

export interface TestApiPanelProps {
  /** Access token активной сессии Keycloak. */
  accessToken: string | null;
  /** Реакция на 401 от шлюза: сессия считается истёкшей. */
  onSessionExpired: (message: string) => void;
}

/** Текущая реализация диагностики: запрос к ресурс-сервису через YARP-шлюз. */
export function TestApiPanel({ accessToken, onSessionExpired }: TestApiPanelProps) {
  const { result, pending, run } = useTestApi({ accessToken, onSessionExpired });

  return (
    <Card
      title="Тест API"
      description="Запрос к /api/resources/test-data через YARP-шлюз с токеном Keycloak."
    >
      <div className={styles.actions}>
        <Button onClick={run} busy={pending}>
          {pending ? 'Запрос…' : 'Тест API'}
        </Button>
      </div>

      {result.kind === 'failure' && (
        <p className={styles.alert} role="alert">
          {result.message}
        </p>
      )}

      {result.kind === 'success' && (
        <CodeBlock label="Ответ API" code={formatPayload(result.payload)} />
      )}
    </Card>
  );
}

/** JSON.stringify может вернуть undefined (для undefined/функций) — подстраховываемся. */
function formatPayload(payload: unknown): string {
  return JSON.stringify(payload, null, 2) ?? String(payload);
}
