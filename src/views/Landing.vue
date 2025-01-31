<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import NavigationTile from '../components/NavigationTile.vue'
import { stockService } from '../services/stockService';
import { cryptoService } from '../services/cryptoService';
import api from '../api/axios';

const auth = useAuthStore()
const router = useRouter()
const savedStocksCount = ref()
const savedCryptoCount = ref()
const ownedStocksCount = ref(0)
const ownedCryptoCount = ref(0)

onMounted(async () => {
    if (!auth.isAuthenticated) {
        router.push('/login')
        return
    }
    
    try {
        const [stocks, cryptos, portfolio] = await Promise.all([
        stockService.getSavedStocks(),
        cryptoService.getSavedCryptos(),
        api.get('/portfolio')
        ])
        savedStocksCount.value = stocks.length
        savedCryptoCount.value = cryptos.length

        // Count unique stocks in portfolio
        const uniqueStocks = new Set(portfolio.data.map((item: any) => item.symbol))
        ownedStocksCount.value = uniqueStocks.size
        
        // For future crypto portfolio implementation
        ownedCryptoCount.value = 0
    } catch (e) {
        console.error('Error fetching counts:', e)
    }
})

const navigationTiles = ref([
  {
    title: 'Stock Dashboard',
    description: 'View and manage your saved stocks in one place. Monitor real-time prices and performance.',
    icon: '../assets/stock-market.png',
    route: '/stock-dashboard'
  },
  {
    title: 'Stock Search',
    description: 'Search for stocks, view detailed information, and add them to your watchlist.',
    icon: '/search-stock.svg',
    route: '/search-area'
  },
  {
    title: 'Crypto Dashboard',
    description: 'Track your cryptocurrency portfolio with live price updates and market data.',
    icon: '/dashboard-crypto.svg',
    route: '/crypto-dashboard'
  },
  {
    title: 'Crypto Search',
    description: 'Explore cryptocurrencies, analyze market data, and track your favorites.',
    icon: '/search-crypto.svg',
    route: '/crypto'
  },
  {
    title: 'Portfolio Summary',
    description: 'View your complete portfolio with performance metrics and analytics.',
    icon: '/portfolio.svg',
    route: '/portfolio-summary'
  },
  {
    title: 'Transaction History',
    description: 'Review your complete history of stock purchases and sales.',
    icon: '/transactions.svg',
    route: '/transaction-history'
  },
  {
    title: 'Account Settings',
    description: 'Manage your profile, security settings, and preferences.',
    icon: '/account.svg',
    route: '/account'
  }
])

const formatDate = (date: string | undefined) => {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    })
}

// Add admin tiles if user is admin
if (auth.user?.isAdmin) {
  navigationTiles.value.push(
    {
      title: 'User Management',
      description: 'Manage user accounts, permissions, and system access.',
      icon: '/users.svg',
      route: '/users'
    },
    {
      title: 'Audit Logs',
      description: 'View system activity, user actions, and security events.',
      icon: '/audit.svg',
      route: '/audit-logs'
    }
  )
}
</script>

<template>
  <div class="min-h-screen bg-gray-100 py-6">
    <div class="container mx-auto px-4">
      <!-- Welcome Section -->
      <div class="text-center mb-4">
        <h1 class="text-3xl font-bold text-gray-900 mb-4">
          Welcome back, {{ auth.user?.name }}!
        </h1>
        <p class="text-gray-600 max-w-2xl mx-auto">
          Navigate through your financial journey with our comprehensive tools and features.
          Choose from the options below to get started.
        </p>
      </div>

      <!-- Navigation Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 max-w-7xl mx-auto">
        <NavigationTile
          v-for="tile in navigationTiles"
          :key="tile.route"
          v-bind="tile"
        />
      </div>

      <!-- Quick Stats Section -->
      <!-- Quick Stats Section -->
      <div class="mt-12 bg-white rounded-lg shadow-lg p-6">
        <h2 class="text-xl font-semibold mb-4">Quick Overview</h2>
        <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 xl:grid-cols-5 gap-4 max-w-7xl mx-auto">
          <!-- Saved Stocks -->
          <div 
            @click="router.push('/stock-dashboard')"
            class="bg-gray-50 p-4 rounded-lg text-center cursor-pointer hover:bg-gray-100 transition-colors"
          >
            <h3 class="text-sm text-gray-500">Saved Stocks</h3>
            <p class="text-2xl font-semibold text-indigo-600">{{ savedStocksCount }}</p>
          </div>
          
          <!-- Saved Crypto -->
          <div 
            @click="router.push('/crypto-dashboard')"
            class="bg-gray-50 p-4 rounded-lg text-center cursor-pointer hover:bg-gray-100 transition-colors"
          >
            <h3 class="text-sm text-gray-500">Saved Crypto</h3>
            <p class="text-2xl font-semibold text-orange-600">{{ savedCryptoCount }}</p>
          </div>
          
          <!-- Owned Stocks -->
          <div 
            @click="router.push('/portfolio-summary')"
            class="bg-gray-50 p-4 rounded-lg text-center cursor-pointer hover:bg-gray-100 transition-colors"
          >
            <h3 class="text-sm text-gray-500">Stocks Owned</h3>
            <p class="text-2xl font-semibold text-green-600">{{ ownedStocksCount }}</p>
          </div>
          
          <!-- Owned Crypto -->
          <div 
            @click="router.push('/portfolio-summary')"
            class="bg-gray-50 p-4 rounded-lg text-center cursor-pointer hover:bg-gray-100 transition-colors"
          >
            <h3 class="text-sm text-gray-500">Crypto Owned</h3>
            <p class="text-2xl font-semibold text-purple-600">{{ ownedCryptoCount }}</p>
          </div>
          
          <!-- Last Login -->
          <div class="bg-gray-50 p-4 rounded-lg text-center">
            <h3 class="text-sm text-gray-500">Last Login</h3>
            <p class="text-sm font-medium text-gray-800">
              {{ formatDate(auth.user?.previousLoginDate) }}
            </p>
          </div>
        </div>
      </div>


    </div>
  </div>
</template>
