import { createApp } from 'vue'
import './style.css'
import './assets/styles/flat-design-system.css'
import App from './App.vue'
import router from './router'
import { createPinia } from 'pinia'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia) // Khởi tạo Pinia
app.use(router)
app.mount('#app')