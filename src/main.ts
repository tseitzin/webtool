import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import Toast from 'vue-toastification'
import 'vue-toastification/dist/index.css'
import './style.css'
import App from './App.vue'
import NewsArticleCard from './components/NewsArticleCard.vue'
import CryptoSearchResult from './components/CryptoSearchResult.vue'
import MarketStatusMessage from './components/MarketStatusMessage.vue'
import LoadingSpinner from './components/LoadingSpinner.vue'

const app = createApp(App)
const pinia = createPinia()

const toastOptions = {
  position: 'top-right',
  timeout: 5000,
  closeOnClick: true,
  pauseOnFocusLoss: true,
  pauseOnHover: true,
  draggable: true,
  draggablePercent: 0.6,
  showCloseButtonOnHover: false,
  hideProgressBar: true,
  closeButton: 'button',
  icon: true,
  rtl: false
}

app.use(pinia)
app.use(router)
app.use(Toast, toastOptions)
app.component('NewsArticleCard', NewsArticleCard);
app.component('CryptoSearchResult', CryptoSearchResult);
app.component('MarketStatusMessage', MarketStatusMessage);
app.component('LoadingSpinner', LoadingSpinner)

app.mount('#app')