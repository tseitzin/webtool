<script setup lang="ts">
import { ref, onMounted, watchEffect } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import api from '../api/axios'

interface StockData {
  symbol: string
  price: number
  change: number
  changePercent: number
  volume: number
  marketCap: number
  timestamp: string
  open?: number
  high?: number
  low?: number
  previousClose?: number
}

interface FavoriteStock {
  symbol: string
  addedAt: string
}

const router = useRouter()
const auth = useAuthStore()
const favoriteStocks = ref<StockData[]>([])
const error = ref('')
const loading = ref(false)
const refreshInterval = ref<number | null>(null)

// Auto-refresh data every 60 seconds
const startAutoRefresh = () => {
  refreshInterval.value = window.setInterval(() => {
    fetchFavoriteStockData()
  }, 60000)
}

const stopAutoRefresh = () => {
  if (refreshInterval.value) {
    clearInterval(refreshInterval.value)
    refreshInterval.value = null
  }
}

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/login')
    return
  }
  await fetchFavoriteStockData()
  startAutoRefresh()
})

// Cleanup on component unmount
watchEffect((onCleanup) => {
  onCleanup(() => {
    stopAutoRefresh()
  })
})

const fetchFavoriteStockData = async () => {
  loading.value = true
  error.value = ''
  try {
    // First, get the user's favorite stocks
    const favoritesResponse = await api.get<FavoriteStock[]>('/favorites')
    const favorites = favoritesResponse.data

    // Then fetch the current data for each favorite stock
    const stockPromises = favorites.map(fav => 
      api.get<StockData>(`/stockdata/${fav.symbol}`)
    )
    const responses = await Promise.all(stockPromises)
    favoriteStocks.value = responses.map(r => r.data)
  } catch (e: any) {
    error.value = 'Failed to load favorite stocks'
    console.error('Error:', e)
  } finally {
    loading.value = false
  }
}

const removeFromFavorites = async (symbol: string) => {
  try {
    await api.delete(`/favorites/${symbol}`)
    favoriteStocks.value = favoriteStocks.value.filter(stock => stock.symbol !== symbol)
  } catch (e: any) {
    error.value = 'Failed to remove from favorites'
  }
}

const formatNumber = (num: number) => {
  return new Intl.NumberFormat('en-US').format(num)
}

const formatCurrency = (num: number) => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(num)
}

const formatPercent = (num: number) => {
  return new Intl.NumberFormat('en-US', { style: 'percent', minimumFractionDigits: 2 }).format(num / 100)
}

const navigateToSearch = () => {
  router.push('/search-area')
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-8">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-2xl font-bold">{{ auth.user?.name }}'s Dashboard</h1>
        <button
          @click="navigateToSearch"
          class="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
        >
          Search Stocks
        </button>
      </div>

      <!-- Error Message -->
      <div v-if="error" class="mb-6 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        {{ error }}
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- No Favorites Message -->
      <div v-else-if="favoriteStocks.length === 0" class="bg-white rounded-lg shadow-md p-8 text-center">
        <h2 class="text-xl font-semibold mb-4">No Favorite Stocks Yet</h2>
        <p class="text-gray-600 mb-6">Start building your watchlist by adding stocks you want to track.</p>
        <button
          @click="navigateToSearch"
          class="px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
        >
          Search and Add Stocks
        </button>
      </div>

      <!-- Favorite Stocks Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
        <div
          v-for="stock in favoriteStocks"
          :key="stock.symbol"
          class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-bold">{{ stock.symbol }}</h2>
            <button
              @click="removeFromFavorites(stock.symbol)"
              class="text-red-600 hover:text-green-800 text-xs transition-colors"
              title="Remove from watchlist"
            >
            Remove
            </button>
          </div>

          <div class="grid grid-cols-2 gap-1">
            <div>
              <p class="text-sm text-gray-500">Current Price</p>
              <p class="text-lg font-semibold">{{ formatCurrency(stock.price) }}</p>
            </div>
            <div>
              <p class="text-sm text-gray-500">Change</p>
              <p :class="['text-lg font-semibold', stock.change >= 0 ? 'text-green-600' : 'text-red-600']">
                {{ formatPercent(stock.changePercent) }}
              </p>
            </div>
            <div>
              <p class="text-sm text-gray-500">Volume</p>
              <p class="font-semibold">{{ formatNumber(stock.volume) }}</p>
            </div>
            <div>
              <p class="text-sm text-gray-500">Market Cap</p>
              <p class="font-semibold">{{ formatCurrency(stock.marketCap) }}</p>
            </div>
          </div>

          <div class="mt-4 pt-4 border-t border-gray-200">
            <div class="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span class="text-gray-500">Open:</span>
                <span class="ml-1">{{ stock.open ? formatCurrency(stock.open) : 'N/A' }}</span>
              </div>
              <div>
                <span class="text-gray-500">Previous Close:</span>
                <span class="ml-1">{{ stock.previousClose ? formatCurrency(stock.previousClose) : 'N/A' }}</span>
              </div>
              <div>
                <span class="text-gray-500">High:</span>
                <span class="ml-1">{{ stock.high ? formatCurrency(stock.high) : 'N/A' }}</span>
              </div>
              <div>
                <span class="text-gray-500">Low:</span>
                <span class="ml-1">{{ stock.low ? formatCurrency(stock.low) : 'N/A' }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>