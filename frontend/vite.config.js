
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, '.', '')

    return {
        plugins: [react()],

        server: {
            host: '0.0.0.0',
            proxy: {
                '/api': {
                    target: env.API_PROXY_TARGET || 'https://localhost:7073',
                    changeOrigin: true,
                    secure: false,
                    rewrite: (path) => path.replace(/^\/api/, ''),
                },
            },
        },
    }
})
