import { classNames } from '../lib/classNames';
import styles from './CodeBlock.module.css';

export interface CodeBlockProps {
  /** Текст для вывода как есть (например, отформатированный JSON). */
  code: string;
  /** Подпись над блоком. */
  label?: string;
  className?: string;
}

/** Моноширинный блок для технического вывода: ответ API, текст ошибки. */
export function CodeBlock({ code, label, className }: CodeBlockProps) {
  return (
    <figure className={classNames(styles.codeBlock, className)}>
      {label !== undefined && <figcaption className={styles.label}>{label}</figcaption>}
      <pre className={styles.pre}>
        <code className={styles.code}>{code}</code>
      </pre>
    </figure>
  );
}
