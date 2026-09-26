/** Ошибка вызова API: хранит HTTP-статус, чтобы вызывающий отличал 401 от прочих сбоев. */
export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

/** В catch в TypeScript попадает unknown — приводим его к тексту сообщения. */
export function toErrorMessage(error: unknown): string {
  return error instanceof Error ? error.message : String(error);
}
