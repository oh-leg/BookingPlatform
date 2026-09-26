import type { ReactNode } from 'react';
import { classNames } from '../lib/classNames';
import styles from './Card.module.css';

export interface CardProps {
  /** Заголовок блока (рендерится как h2 — на странице уже есть h1). */
  title?: ReactNode;
  /** Пояснение под заголовком. */
  description?: ReactNode;
  className?: string;
  children: ReactNode;
}

/** Карточка-поверхность: отдельный смысловой блок страницы. */
export function Card({ title, description, className, children }: CardProps) {
  const hasHeader = title !== undefined || description !== undefined;

  return (
    <section className={classNames(styles.card, className)}>
      {hasHeader && (
        <header className={styles.header}>
          {title !== undefined && <h2 className={styles.title}>{title}</h2>}
          {description !== undefined && <p className={styles.description}>{description}</p>}
        </header>
      )}
      <div className={styles.body}>{children}</div>
    </section>
  );
}
