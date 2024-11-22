import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
  base: '/',
  plugins: [vue()],
  build: {
    sourcemap: true,
    outDir: './api/wwwroot',
    emptyOutDir: true,
    rollupOptions: {
      output: {
        manualChunks: {
          'vendor': ['vue', 'vue-router', 'pinia', '@vueuse/core'],
        }
      }
    }
  },
  server: {
    port: 5173,
    host: true
  }
})