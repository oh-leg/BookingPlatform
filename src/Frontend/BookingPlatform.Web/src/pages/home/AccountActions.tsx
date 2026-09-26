import { Button, Card } from '../../components';
import styles from './HomePage.module.css';

export interface AccountActionsProps {
  /** Имя авторизованного пользователя. */
  userName: string | null;
  onLogout: () => void;
}

/** Состояние авторизованного пользователя: профиль и выход из системы. */
export function AccountActions({ userName, onLogout }: AccountActionsProps) {
  return (
    <Card
      title="Профиль"
      description="Сессия Keycloak активна: токен автоматически подставляется в запросы к API."
    >
      <p className={styles.profile}>
        Вы вошли как <strong>{userName ?? 'пользователь'}</strong>
      </p>
      <div className={styles.actions}>
        <Button variant="danger" onClick={onLogout}>
          Выйти
        </Button>
      </div>
    </Card>
  );
}
