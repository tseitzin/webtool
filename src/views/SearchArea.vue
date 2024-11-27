<script setup lang="ts">
import { ref, onMounted } from 'vue'
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
}

interface MarketSummary {
  totalVolume: number
  averageChange: number
  topGainers: StockData[]
  topLosers: StockData[]
  mostActive: StockData[]
}

const router = useRouter()
const auth = useAuthStore()
const marketSummary = ref<MarketSummary | null>(null)
const selectedStock = ref<StockData | null>(null)
const error = ref('')
const loading = ref(false)
const searchSymbol = ref('')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/access-denied')
    return
  }
  await fetchMarketSummary()
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

const formatNumber = (num: number) => {
  return new Intl.NumberFormat('en-US').format(num)
}

const formatCurrency = (num: number) => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(num)
}

const formatPercent = (num: number) => {
  return new Intl.NumberFormat('en-US', { style: 'percent', minimumFractionDigits: 2 }).format(num / 100)
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-8">Stock Search Area</h1>

      <!-- Error Message -->
      <div v-if="error" class="mb-6 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        {{ error }}
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- Stock Search -->
      <div class="bg-white rounded-lg shadow-md p-6 mb-6">
        <div class="flex gap-4">
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
        <div v-if="selectedStock" class="mt-4">
          <div class="bg-gray-200 p-2 rounded-lg">
              <h1 class="text-center ml-3 text-xl font-bold">{{ selectedStock.symbol }}</h1>
            </div>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div class="bg-gray-100 p-4 rounded-lg">
              <h3 class="text-sm text-gray-500">Price</h3>
              <p class="text-lg font-semibold">{{ formatCurrency(selectedStock.price) }}</p>
            </div>
            <div class="bg-gray-100 p-4 rounded-lg">
              <h3 class="text-sm text-gray-500">Change</h3>
              <p :class="['text-lg font-semibold', selectedStock.change >= 0 ? 'text-green-600' : 'text-red-600']">
                {{ formatCurrency(selectedStock.change) }} ({{ formatPercent(selectedStock.changePercent) }})
              </p>
            </div>
            <div class="bg-gray-100 p-4 rounded-lg">
              <h3 class="text-sm text-gray-500">Volume</h3>
              <p class="text-lg font-semibold">{{ formatNumber(selectedStock.volume) }}</p>
            </div>
            <div class="bg-gray-100 p-4 rounded-lg">
              <h3 class="text-sm text-gray-500">Market Cap</h3>
              <p class="text-lg font-semibold">{{ formatCurrency(selectedStock.marketCap) }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Market Summary -->
      <div v-if="marketSummary" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <!-- Top Gainers -->
        <div class="bg-white rounded-lg shadow-md p-6">
          <div>
            <h2 class="bg-gray-200 rounded-lg p-3 text-center text-xl font-semibold mb-4 text-green-600">Top Gainers</h2>
          </div>
          <div class="space-y-4">
            <div v-for="stock in marketSummary.topGainers" :key="stock.symbol" class="flex justify-between items-center">
              <span class="font-medium">{{ stock.symbol }}</span>
              <span class="text-green-600">{{ formatPercent(stock.changePercent) }}</span>
            </div>
          </div>
        </div>

        <!-- Top Losers -->
        <div class="bg-white rounded-lg shadow-md p-6">
          <div>
            <h2 class="bg-gray-200 rounded-lg p-3 text-center text-xl font-semibold mb-4 text-red-600">Top Losers</h2>
          </div>
          <div class="space-y-4">
            <div v-for="stock in marketSummary.topLosers" :key="stock.symbol" class="flex justify-between items-center">
              <span class="font-medium">{{ stock.symbol }}</span>
              <span class="text-red-600">{{ formatPercent(stock.changePercent) }}</span>
            </div>
          </div>
        </div>

        <!-- Most Active -->
        <div class="bg-white rounded-lg shadow-md p-6">
          <div>
            <h2 class="bg-gray-200 rounded-lg p-3 text-center text-xl font-semibold mb-4 text-indigo-600">Most Active</h2>
          </div>
          <div class="space-y-4">
            <div v-for="stock in marketSummary.mostActive" :key="stock.symbol" class="flex justify-between items-center">
              <span class="font-medium">{{ stock.symbol }}</span>
              <span class="text-gray-600">{{ formatNumber(stock.volume) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
