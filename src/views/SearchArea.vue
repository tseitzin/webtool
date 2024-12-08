<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { polygonService } from '../services/polygonService'
import { stockService } from '../services/stockService'
import StockSearchResult from '../components/StockSearchResult.vue'
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { StockData } from '../types/polygon'

const router = useRouter()
const auth = useAuthStore()
const selectedStock = ref<StockData | null>(null)
const savedStocks = ref<StockData[]>([])
const error = ref('')
const loading = ref(false)
const searchSymbol = ref('')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/access-denied')
    return
  }
  fetchSavedStocks()
})

const searchStock = async () => {
  if (!searchSymbol.value) return
  
  loading.value = true
  error.value = ''
  try {
    const stockData = await polygonService.getStockSnapshot(searchSymbol.value.toUpperCase())
    selectedStock.value = stockData
  } catch (e: any) {
    error.value = 'Stock not found or error fetching data'
    selectedStock.value = null
  } finally {
    loading.value = false
  }
}

const fetchSavedStocks = async () => {
  try {
    savedStocks.value = await stockService.getSavedStocks()
  } catch (e: any) {
    console.error('Error fetching saved stocks:', e)
  }
}

const toggleFavorite = async (symbol: string) => {
  if (!selectedStock.value) return

  try {
    if (isStockFavorited(symbol)) {
      await stockService.removeSavedStock(symbol)
    } else {
      await stockService.saveStock(selectedStock.value)
    }
    await fetchSavedStocks()
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to update saved stocks'
  }
}

const removeSavedStock = async (symbol: string) => {
  try {
    await stockService.removeSavedStock(symbol)
    await fetchSavedStocks()
  } catch (e: any) {
    error.value = 'Failed to remove from saved stocks'
  }
}

const isStockFavorited = (symbol: string) => {
  return savedStocks.value.some(s => s.symbol === symbol)
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
            :disabled="loading"
            class="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors disabled:opacity-50"
          >
            {{ loading ? 'Searching...' : 'Search' }}
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

      <!-- Saved Stocks Section -->
      <div v-if="savedStocks.length > 0" class="mt-8">
        <h2 class="text-2xl font-bold mb-4">Your Saved Stocks</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div
            v-for="stock in savedStocks"
            :key="stock.symbol"
            class="bg-white rounded-lg shadow-md p-6"
          >
            <div class="flex justify-between items-center mb-4">
              <h3 class="text-xl font-bold">{{ stock.symbol }}</h3>
              <button
                @click="removeSavedStock(stock.symbol)"
                class="text-red-600 hover:text-red-800"
              >
                Remove
              </button>
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <p class="text-sm text-gray-500">Price</p>
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
                <p class="text-sm text-gray-500">Previous Close</p>
                <p class="font-semibold">{{ formatCurrency(stock.previousClose || 0) }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>