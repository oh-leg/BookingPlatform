import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { classNames } from '../lib/classNames';
import styles from './Button.module.css';

/** Визуальные варианты кнопки: набор задаётся классами из темы. */
export type ButtonVariant = 'primary' | 'secondary' | 'danger';

/** Размеры кнопки. */
export type ButtonSize = 'md' | 'lg';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /** Визуальный вариант, по умолчанию — основной. */
  variant?: ButtonVariant;
  size?: ButtonSize;
  /** Идёт асинхронная операция: кнопка блокируется и помечается для скринридеров. */
  busy?: boolean;
  /** Растянуть кнопку на всю ширину контейнера. */
  block?: boolean;
  children: ReactNode;
}

const VARIANT_CLASS: Record<ButtonVariant, string> = {
  primary: styles.primary,
  secondary: styles.secondary,
  danger: styles.danger
};

const SIZE_CLASS: Record<ButtonSize, string> = {
  md: styles.md,
  lg: styles.lg
};

/** Базовая кнопка приложения: любые стили приходят из класса, не из пропсов. */
export function Button({
  variant = 'primary',
  size = 'md',
  busy = false,
  block = false,
  className,
  type = 'button',
  disabled,
  children,
  ...rest
}: ButtonProps) {
  return (
    <button
      {...rest}
      type={type}
      className={classNames(styles.button, VARIANT_CLASS[variant], SIZE_CLASS[size], block && styles.block, className)}
      disabled={Boolean(disabled) || busy}
      aria-busy={busy || undefined}
    >
      {children}
    </button>
  );
}
