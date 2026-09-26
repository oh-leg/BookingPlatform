import { useCallback, useEffect, useRef, useState, type CSSProperties } from 'react';
import { fetchTestData } from './api';
import { displayNameOf, getValidUser, isSigninRedirectCallback, userManager } from './auth';
import { ApiError, toErrorMessage } from './errors';

const buttonStyle: CSSProperties = {
  padding: '10px 18px',
  fontSize: '15px',
  borderRadius: '6px',
  border: '1px solid #ccc',
  cursor: 'pointer',
  background: '#fff'
};

const preStyle: CSSProperties = {
  background: '#f5f5f5',
  padding: 16,
  borderRadius: 6,
  overflow: 'auto',
  fontSize: '14px'
};

/** Что показываем в блоке результата под кнопками. */
type Outcome =
  | { kind: 'none' }
  | { kind: 'payload'; payload: unknown }
  | { kind: 'error'; error: string }
  | { kind: 'fatal'; fatal: string };

function outcomeJson(outcome: Outcome): string | null {
  switch (outcome.kind) {
    case 'payload':
      return JSON.stringify(outcome.payload, null, 2);
    case 'error':
      return JSON.stringify({ error: outcome.error }, null, 2);
    case 'fatal':
      return JSON.stringify({ fatal: outcome.fatal }, null, 2);
    case 'none':
      return null;
  }
}

export default function App() {
  const [userName, setUserName] = useState<string | null>(null);
  const [ready, setReady] = useState(false);
  const [busy, setBusy] = useState(false);
  const [outcome, setOutcome] = useState<Outcome>({ kind: 'none' });

  // Эффект должен отработать ровно один раз: code в Keycloak одноразовый,
  // поэтому повторный прогон под React.StrictMode мы отсекаем.
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return;
    started.current = true;

    void (async () => {
      try {
        // Возврат из Keycloak после логина: в URL есть code — завершаем PKCE-обмен.
        if (isSigninRedirectCallback()) {
          await userManager.signinRedirectCallback();
          window.history.replaceState({}, document.title, window.location.pathname);
        }

        const current = await getValidUser();
        if (current) {
          setUserName(displayNameOf(current));
        } else {
          // Не авторизованы -> редирект на страницу аутентификации Keycloak.
          await userManager.signinRedirect();
        }
      } catch (error) {
        setOutcome({ kind: 'fatal', fatal: toErrorMessage(error) });
      } finally {
        setReady(true);
      }
    })();
  }, []);

  const testApi = useCallback(async () => {
    setBusy(true);
    setOutcome({ kind: 'none' });
    try {
      const current = await getValidUser();
      if (!current) {
        await userManager.signinRedirect();
        return;
      }
      setOutcome({ kind: 'payload', payload: await fetchTestData(current.access_token) });
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        await userManager.signinRedirect(); // токен протух — логинимся заново
        return;
      }
      setOutcome({ kind: 'error', error: toErrorMessage(error) });
    } finally {
      setBusy(false);
    }
  }, []);

  const logout = useCallback(async () => {
    await userManager.signoutRedirect();
  }, []);

  if (!ready) {
    return <div style={{ fontFamily: 'sans-serif', padding: 40 }}>Загрузка…</div>;
  }

  const json = outcomeJson(outcome);

  return (
    <div style={{ fontFamily: 'sans-serif', maxWidth: 720, margin: '60px auto', padding: '0 16px' }}>
      <h1>Booking Platform</h1>
      {userName && (
        <p>
          Вы вошли как <strong>{userName}</strong>
        </p>
      )}
      <div style={{ display: 'flex', gap: 12, margin: '24px 0' }}>
        <button style={buttonStyle} onClick={() => void testApi()} disabled={busy}>
          {busy ? 'Запрос…' : 'Тест API'}
        </button>
        <button style={{ ...buttonStyle, background: '#ffecec' }} onClick={() => void logout()}>
          Выйти
        </button>
      </div>
      {json !== null && <pre style={preStyle}>{json}</pre>}
    </div>
  );
}
