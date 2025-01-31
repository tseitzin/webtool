import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import Login from '../views/Login.vue'
import Register from '../views/Register.vue'
import ForgotPassword from '../views/ForgotPassword.vue'
import ResetPassword from '../views/ResetPassword.vue'
import Account from '../views/Account.vue'
import Users from '../views/Users.vue'
import AuditLogs from '../views/AuditLogs.vue'
import SearchArea from '../views/SearchArea.vue'
import ResearchStock from '../views/ResearchStock.vue'
import { useAuthStore } from '../stores/auth'
import AccessDenied from '../views/AccessDenied.vue'
import StockDashboard from '../views/StockDashboard.vue'
import CryptoDashboard from '../views/CryptoDashboard.vue'
import { storeToRefs } from 'pinia'
import Landing from '../views/Landing.vue'
import CryptoSearch from '../views/CryptoSearch.vue'
import PortfolioSummary from '../views/PortfolioSummary.vue'
import TransactionHistory from '../views/TransactionHistory.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home,
    },
    {
      path: '/landing',
      name: 'landing',
      component: Landing,
      meta: { requiresAuth: true }
    },
    {
      path: '/search-area',
      name: 'search-area',
      component: SearchArea,
      meta: { requiresAuth: true }
    },
    {
      path: '/crypto',
      name: 'crypto',
      component: CryptoSearch,
      meta: { requiresAuth: true }
    },
    {
      path: '/stock-dashboard',
      name: 'stock-dashboard',
      component: StockDashboard,
      meta: { requiresAuth: true }
    },
    {
      path: '/crypto-dashboard',
      name: 'crypto-dashboard',
      component: CryptoDashboard,
      meta: { requiresAuth: true }
    },
    {
      path: '/research/:symbol',
      name: 'research-stock',
      component: ResearchStock,
      meta: { requiresAuth: true }
    },
    {
      path: '/users',
      name: 'users',
      component: Users,
      meta: { requiresAuth: true, requiresAdmin: true }
    },
    {
      path: '/portfolio-summary',
      name: 'portfolio-summary',
      component: PortfolioSummary,
      meta: { requiresAuth: true }
    },
    {
      path: '/audit-logs',
      name: 'audit-logs',
      component: AuditLogs,
      meta: { requiresAuth: true, requiresAdmin: true }
    },
    {
      path: '/transaction-history',
      name: 'transaction-history',
      component: TransactionHistory,
      meta: { requiresAuth: true }
    },
    {
      path: '/account',
      name: 'account',
      component: Account,
      meta: { requiresAuth: true }
    },
    {
      path: '/login',
      name: 'login',
      component: Login
    },
    {
      path: '/register',
      name: 'register',
      component: Register
    },
    {
      path: '/access-denied',
      name: 'accessdenied',
      component: AccessDenied
    },
    {
      path: '/forgot-password',
      name: 'forgot-password',
      component: ForgotPassword
    },
    {
      path: '/reset-password',
      name: 'reset-password',
      component: ResetPassword
    }
  ],
  scrollBehavior(){
    return { top: 0, behavior: 'smooth'}
  }
})

router.beforeEach(async (to, _from, next) => {
  const authStore = useAuthStore()
  const { isAuthenticated, user } = storeToRefs(authStore)
  
  // Initialize auth state if not already done
  if (!isAuthenticated.value) {
    authStore.initializeAuth()
  }

  if (to.meta.requiresAuth && !isAuthenticated.value) {
    next('/login')
  } else if (to.meta.requiresAdmin && !user.value?.isAdmin) {
    next('/access-denied')
  } else {
    next()
  }
})

export default router