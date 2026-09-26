import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// SPA собирается в статику и раздаётся nginx'ом из контейнера;
// /api/* и /auth/* обрабатывает YARP-шлюз (см. httproute в deploy/k8s).
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: 'dist'
  }
});
