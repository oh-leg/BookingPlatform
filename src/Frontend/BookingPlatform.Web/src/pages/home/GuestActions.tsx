import { Button, Card } from '../../components';
import styles from './HomePage.module.css';

export interface GuestActionsProps {
  /** Явное действие пользователя: только оно уводит браузер в Keycloak. */
  onLogin: () => void;
}

/** Публичное состояние главной страницы: приглашение войти. */
export function GuestActions({ onLogin }: GuestActionsProps) {
  return (
    <Card
      title="Вход в систему"
      description="Авторизация выполняется в Keycloak по протоколу OIDC (Authorization Code + PKCE)."
    >
      <div className={styles.actions}>
        <Button size="lg" onClick={onLogin}>
          Войти
        </Button>
      </div>
      <p className={styles.muted}>
        Просмотр главной страницы доступен без входа. После авторизации вы вернётесь сюда же —
        появится доступ к тестовому запросу API.
      </p>
    </Card>
  );
}
