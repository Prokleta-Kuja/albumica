import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

import './assets/app.css'
import './assets/bootstrap.min.css'
import './assets/bootstrap.bundle.min.js'

const app = createApp(App)

app.use(router)
app.use(createPinia())

app.mount('#app')
