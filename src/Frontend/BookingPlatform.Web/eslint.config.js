import js from '@eslint/js';
import globals from 'globals';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import tseslint from 'typescript-eslint';

export default tseslint.config(
  { ignores: ['dist', 'node_modules', 'coverage'] },
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      ...tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite
    ],
    languageOptions: {
      ecmaVersion: 2022,
      globals: globals.browser
    },
    rules: {
      // Правило проекта: оформление задаётся классами (CSS Modules + токены темы),
      // а не атрибутом style в разметке.
      'no-restricted-syntax': [
        'error',
        {
          selector: 'JSXAttribute[name.name="style"]',
          message:
            'Инлайн-стили запрещены: используйте классы CSS Modules и токены темы (src/styles/theme.css).'
        }
      ]
    }
  },
  {
    // Конфиг Vite исполняется в Node, а не в браузере.
    files: ['vite.config.ts'],
    languageOptions: {
      globals: globals.node
    }
  }
);
