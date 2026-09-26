import { useAuth } from '../../auth';
import { Card } from '../../components';
import { AccountActions } from './AccountActions';
import { GuestActions } from './GuestActions';
import { TestApiPanel } from './TestApiPanel';
import styles from './HomePage.module.css';

/**
 * Главная страница.
 *
 * Публичная по умолчанию: заголовок и содержимое показываются без авторизации,
 * и никакого автоматического редиректа в Keycloak нет. Состояние сессии лишь
 * добавляет блоки — приглашение войти либо профиль с тестом API.
 */
export function HomePage() {
  const { status, userName, accessToken, error, login, logout, endSession } = useAuth();

  return (
    <main className={styles.page}>
      <header className={styles.hero}>
        <p className={styles.brand}>Booking Platform</p>
        <h1 className={styles.title}>Главная</h1>
        <p className={styles.lead}>
          Публичная страница сервиса бронирования: открывается без входа в систему.
        </p>
      </header>

      {error !== null && (
        <p className={styles.alert} role="alert">
          {error}
        </p>
      )}

      {status === 'checking' && (
        <Card>
          <p className={styles.muted}>Проверяем сессию…</p>
        </Card>
      )}

      {status === 'anonymous' && <GuestActions onLogin={() => void login()} />}

      {status === 'authenticated' && (
        <>
          <AccountActions userName={userName} onLogout={() => void logout()} />
          <TestApiPanel accessToken={accessToken} onSessionExpired={endSession} />
        </>
      )}
    </main>
  );
}
