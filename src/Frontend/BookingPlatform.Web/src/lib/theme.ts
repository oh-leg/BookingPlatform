/**
 * Тема оформления: активация набора токенов из src/styles/theme.css.
 *
 * Компоненты не знают о теме — они используют переменные, а активный набор
 * выбирается атрибутом data-theme на <html>. Добавление новой темы сводится
 * к блоку `:root[data-theme='...']` в theme.css и вызову applyTheme().
 */

export type ThemeName = 'light' | 'dark';

const DARK_MODE_QUERY = '(prefers-color-scheme: dark)';

/** Проставляет активную тему на корневой элемент документа. */
export function applyTheme(theme: ThemeName): void {
  document.documentElement.dataset.theme = theme;
}

/** Тема, которую запрашивает операционная система. */
export function getSystemTheme(): ThemeName {
  return window.matchMedia(DARK_MODE_QUERY).matches ? 'dark' : 'light';
}

/**
 * Выставляет тему до первого рендера и дальше повторяет её за системой.
 * Возвращает функцию отписки — пригодится в тестах и при повторной инициализации.
 */
export function initTheme(): () => void {
  const media = window.matchMedia(DARK_MODE_QUERY);
  const sync = (): void => applyTheme(media.matches ? 'dark' : 'light');

  sync();
  media.addEventListener('change', sync);

  return () => media.removeEventListener('change', sync);
}
