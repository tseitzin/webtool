<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { polygonService } from '../services/polygonService'
import { stockService } from '../services/stockService'
import StockSearchResult from '../components/StockSearchResult.vue'
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { StockData, MarketMovers } from '../types/polygon'

const router = useRouter()
const auth = useAuthStore()
const selectedStock = ref<StockData | null>(null)
const savedStocks = ref<StockData[]>([])
const marketMovers = ref<MarketMovers | null>(null)
const error = ref('')
const loading = ref(false)
const searchSymbol = ref('')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/access-denied')
    return
  }
  fetchSavedStocks()
  fetchMarketMovers()
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

const fetchMarketMovers = async () => {
  try {
    marketMovers.value = await polygonService.getMarketMovers()
  } catch (e: any) {
    console.error('Error fetching market movers:', e)
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
        <div class="space-y-3">
          <div
            v-for="stock in savedStocks"
            :key="stock.symbol"
            class="bg-white rounded-lg shadow-sm p-4 flex items-center justify-between hover:shadow-md transition-shadow"
          >
            <div class="flex items-center space-x-6 flex-grow">
              <div class="w-24">
                <h3 class="text-lg font-bold">{{ stock.symbol }}</h3>
              </div>
              <div class="w-32">
                <p class="text-sm text-gray-500">Price</p>
                <p class="font-semibold">{{ formatCurrency(stock.price) }}</p>
              </div>
              <div class="w-32">
                <p class="text-sm text-gray-500">Change</p>
                <p :class="['font-semibold', stock.change >= 0 ? 'text-green-600' : 'text-red-600']">
                  {{ formatPercent(stock.changePercent) }}
                </p>
              </div>
              <div class="w-32">
                <p class="text-sm text-gray-500">Volume</p>
                <p class="font-semibold">{{ formatNumber(stock.volume) }}</p>
              </div>
              <div class="w-32">
                <p class="text-sm text-gray-500">Prev Close</p>
                <p class="font-semibold">{{ formatCurrency(stock.previousClose || 0) }}</p>
              </div>
            </div>
            <button
                @click="removeSavedStock(stock.symbol)"
                class="text-red-600 hover:text-red-800 transition-colors"
              >
                Remove
              </button>
          </div>
        </div>
      </div>

      <!-- Market Movers Section -->
      <div v-if="marketMovers" class="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Top Gainers -->
        <div class="bg-white rounded-lg shadow-md p-6">
      <h2 class="text-lg font-bold mb-2 text-green-600">Top 10 Gainers</h2>
      <div class="overflow-x-auto">
        <table class="min-w-full">
          <thead>
            <tr class="text-left text-xs font-medium text-gray-500">
              <th class="pb-1">Symbol</th>
              <th class="pb-1">Price</th>
              <th class="pb-1">Change</th>
              <th class="pb-1">Volume</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100">
            <tr
              v-for="stock in marketMovers.gainers"
              :key="stock.symbol"
              class="hover:bg-gray-50 text-sm"
            >
              <td class="py-1 font-medium">{{ stock.symbol }}</td>
              <td class="py-1">{{ formatCurrency(stock.price) }}</td>
              <td class="py-1 text-green-600 font-semibold">
                {{ formatPercent(stock.changePercent) }}
              </td>
              <td class="py-1">{{ formatNumber(stock.volume) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

        <!-- Top Losers -->
        <div class="bg-white rounded-lg shadow-md p-4">
      <h2 class="text-lg font-bold mb-2 text-red-600">Top 10 Losers</h2>
      <div class="overflow-x-auto">
        <table class="min-w-full">
          <thead>
            <tr class="text-left text-xs font-medium text-gray-500">
              <th class="pb-1">Symbol</th>
              <th class="pb-1">Price</th>
              <th class="pb-1">Change</th>
              <th class="pb-1">Volume</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100">
            <tr
              v-for="stock in marketMovers.losers"
              :key="stock.symbol"
              class="hover:bg-gray-50 text-sm"
            >
              <td class="py-1 font-medium">{{ stock.symbol }}</td>
              <td class="py-1">{{ formatCurrency(stock.price) }}</td>
              <td class="py-1 text-red-600 font-semibold">
                {{ formatPercent(stock.changePercent) }}
              </td>
              <td class="py-1">{{ formatNumber(stock.volume) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

      </div>
    </div>
  </div>
</template>