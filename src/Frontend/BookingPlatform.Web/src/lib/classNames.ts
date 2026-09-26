/**
 * Склеивает имена классов, отбрасывая falsy-значения.
 * Нужен, чтобы условные классы CSS-модулей собирались без хардкода строк
 * и без внешних зависимостей.
 *
 * @example
 * classNames(styles.button, isBusy && styles.busy)
 */
export function classNames(...values: Array<string | false | null | undefined>): string {
  return values.filter(Boolean).join(' ');
}
