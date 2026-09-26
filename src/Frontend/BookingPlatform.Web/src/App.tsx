import { HomePage } from './pages/home';
import styles from './App.module.css';

/**
 * Корневой компонент приложения.
 * Пока страница одна — публичная «Главная»; оболочка задаёт общий каркас.
 */
export default function App() {
  return (
    <div className={styles.shell}>
      <HomePage />
    </div>
  );
}
