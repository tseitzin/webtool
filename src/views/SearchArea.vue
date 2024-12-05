<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import api from '../api/axios'
import MarketSummaryCard from '../components/MarketSummaryCard.vue'
import StockSearchResult from '../components/StockSearchResult.vue'

interface StockData {
  symbol: string
  price: number
  change: number
  changePercent: number
  volume: number
  marketCap: number
  timestamp: string
}

interface MarketSummary {
  totalVolume: number
  averageChange: number
  topGainers: StockData[]
  topLosers: StockData[]
  mostActive: StockData[]
}

interface FavoriteStock {
  symbol: string
  addedAt: string
}

const router = useRouter()
const auth = useAuthStore()
const marketSummary = ref<MarketSummary | null>(null)
const selectedStock = ref<StockData | null>(null)
const favorites = ref<FavoriteStock[]>([])
const favoriteStocks = ref<StockData[]>([])
const error = ref('')
const loading = ref(false)
const searchSymbol = ref('')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/access-denied')
    return
  }
  await fetchMarketSummary()
  fetchFavorites()
})

const fetchMarketSummary = async () => {
  loading.value = true
  error.value = ''
  try {
    const response = await api.get('/stockdata/summary')
    marketSummary.value = response.data
  } catch (e: any) {
    error.value = 'Failed to load market summary'
    console.error('Error:', e)
  } finally {
    loading.value = false
  }
}

const fetchFavorites = async () => {
  try {
    const response = await api.get('/favorites')
    favorites.value = response.data
    await fetchFavoriteStockData()
  } catch (e: any) {
    console.error('Error fetching favorites:', e)
  }
}

const fetchFavoriteStockData = async () => {
  try {
    const stockPromises = favorites.value.map(fav => 
      api.get(`/stockdata/${fav.symbol}`)
    )
    const responses = await Promise.all(stockPromises)
    favoriteStocks.value = responses.map(r => r.data)
  } catch (e: any) {
    console.error('Error fetching favorite stock data:', e)
  }
}

const toggleFavorite = async (symbol: string) => {
  try {
    if (isStockFavorited(symbol)) {
      await api.delete(`/favorites/${symbol}`)
    } else {
      await api.post(`/favorites/${symbol}`)
    }
    await fetchFavorites()
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to update favorites'
  }
}

const isStockFavorited = (symbol: string) => {
  return favorites.value.some(f => f.symbol === symbol)
}

const searchStock = async () => {
  if (!searchSymbol.value) return
  
  loading.value = true
  error.value = ''
  try {
    const response = await api.get(`/stockdata/${searchSymbol.value}`)
    selectedStock.value = response.data
  } catch (e: any) {
    error.value = 'Stock not found'
    selectedStock.value = null
  } finally {
    loading.value = false
  }
}

</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-8">Stock Search Area</h1>

      <!-- Error Message -->
      <div 
        v-if="error" 
        class="mb-6 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded"
      >
        {{ error }}
      </div>

      <!-- Loading State -->
      <div 
        v-if="loading" 
        class="flex justify-center items-center py-8"
      >
        <div class="animate-spin rounded-full h-12 w-12 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- Stock Search -->
      <div class="bg-white rounded-lg shadow-md p-6 mb-6">
        <div class="flex flex-col sm:flex-row gap-4">
          <input
            v-model="searchSymbol"
            type="text"
            placeholder="Enter stock symbol..."
            class="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            @keyup.enter="searchStock"
          />
          <button
            @click="searchStock"
            class="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Search
          </button>
        </div>

        <!-- Selected Stock Details -->
        <StockSearchResult
          v-if="selectedStock"
          :stock="selectedStock"
          :is-favorited="isStockFavorited(selectedStock.symbol)"
          @toggle-favorite="toggleFavorite"
        />
      </div>

      <!-- Market Summary -->
      <div 
        v-if="marketSummary" 
        class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
      >
        <MarketSummaryCard
          title="Top Gainers"
          :stocks="marketSummary.topGainers"
          type="gainers"
        />
        <MarketSummaryCard
          title="Top Losers"
          :stocks="marketSummary.topLosers"
          type="losers"
        />
        <MarketSummaryCard
          title="Most Active"
          :stocks="marketSummary.mostActive"
          type="active"
        />
      </div>
    </div>
  </div>
</template>