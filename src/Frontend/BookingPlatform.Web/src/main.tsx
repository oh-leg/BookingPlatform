import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import { initTheme } from './lib/theme';
import './index.css';

// Тема выставляется до первого рендера, чтобы не было вспышки чужих стилей.
initTheme();

const container = document.getElementById('root');
if (!container) {
  throw new Error('В index.html не найден корневой элемент #root');
}

createRoot(container).render(
  <StrictMode>
    <App />
  </StrictMode>
);
