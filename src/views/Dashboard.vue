<script setup lang="ts">
import { ref, onMounted, watchEffect } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { stockService } from '../services/stockService'
import { dashboardService } from '../services/dashboardService'
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { StockData } from '../types/polygon'
import CollapsibleSectionHeader from '../components/CollapsibleSectionHeader.vue'

const router = useRouter()
const auth = useAuthStore()
const savedStocks = ref<StockData[]>([])
const error = ref('')
const loading = ref(false)
const isWelcomeExpanded = ref(true)

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/login')
    return
  }
  await fetchSavedStocks()
})

// Cleanup on component unmount
watchEffect((onCleanup) => {
  onCleanup(() => {
    dashboardService.stopAutoRefresh()
  })
})

const fetchSavedStocks = async () => {
  loading.value = true
  error.value = ''
  try {
    const stocks = await stockService.getSavedStocks()
    savedStocks.value = stocks
    
    // Start auto-refresh after initial fetch
    dashboardService.startAutoRefresh(stocks, (updatedStocks) => {
      savedStocks.value = updatedStocks
    })
  } catch (e: any) {
    error.value = 'Failed to load saved stocks'
    console.error('Error:', e)
  } finally {
    loading.value = false
  }
}

const removeSavedStock = async (symbol: string) => {
  try {
    await stockService.removeSavedStock(symbol)
    savedStocks.value = savedStocks.value.filter(stock => stock.symbol !== symbol)
  } catch (e: any) {
    error.value = 'Failed to remove stock from saved list'
  }
}

const navigateToSearch = () => {
  router.push('/search-area')
}

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}

const navigateToResearch = (symbol: string) => {
  router.push(`/research/${symbol}`)
}

const toggleWelcome = () => {
  isWelcomeExpanded.value = !isWelcomeExpanded.value
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-4">

      <!-- Welcome Section -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden mb-8">
        <CollapsibleSectionHeader
          :title="`Welcome back, ${auth.user?.name}!`"
          :is-expanded="isWelcomeExpanded"
          @toggle="toggleWelcome"
        />

        <!-- Collapsible Content -->
        <div
          v-show="isWelcomeExpanded"
          class="transition-all duration-300 ease-in-out"
        >
          <div class="p-6 pt-1">
            <div class="prose max-w-none text-gray-600">
              <p class="mb-1">
                This is your personalized dashboard where you can monitor your portfolio and track your favorite stocks in real-time.
              </p>
              <div class="bg-blue-50 p-1 rounded-lg">
                <h2 class="text-lg font-semibold text-blue-900 mb-2">Quick Guide:</h2>
                <ul class="list-disc list-inside space-y-2 text-blue-800">
                  <li>View your saved stocks and their current performance</li>
                  <li>Click the Company Details button on any stock to see detailed analysis and news</li>
                  <li>Track price changes and volume in real-time</li>
                  <li>
                    Need to add more stocks? 
                    <button 
                      @click="navigateToSearch"
                      class="text-indigo-600 hover:text-indigo-800 font-medium"
                    >
                      Visit the Stock Search page
                    </button>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>


      <!-- Error Message -->
      <div v-if="error" class="mb-6 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        {{ error }}
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- Saved Stocks Section -->
      <div class="mb-8">
        <div class="flex flex-row justify-between items-center mb-4">
          <h2 class="text-xl font-bold">Your Saved Stocks</h2>
          <button
            @click="navigateToSearch"
            class="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Search Stocks
          </button>
        </div>

        <!-- No Saved Stocks Message -->
        <div v-if="savedStocks.length === 0" class="bg-white rounded-lg shadow-md p-8 text-center">
          <h3 class="text-xl font-semibold mb-4">No Saved Stocks Yet</h3>
          <p class="text-gray-600 mb-6">Start building your watchlist by adding stocks you want to track.</p>
          <button
            @click="navigateToSearch"
            class="px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Search and Add Stocks
          </button>
        </div>

        <!-- Saved Stocks List -->
        <div v-else class="space-y-4">
          <div
            v-for="stock in savedStocks"
            :key="stock.symbol"
            class="bg-white rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow"
          >
            <div class="flex justify-between items-center">
              <h3 class="text-lg font-bold">{{ stock.symbol }}</h3>
              <button
                @click="removeSavedStock(stock.symbol)"
                class="text-red-600 hover:text-red-800 transition-colors"
              >
                Remove
              </button>
            </div>

            <div class="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-1">
              <div>
                <p class="text-sm text-gray-500">Current Price</p>
                <p class="text-lg font-semibold">{{ formatCurrency(stock.price) }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Change</p>
                <p :class="['text-lg font-semibold', stock.change >= 0 ? 'text-green-600' : 'text-red-600']">
                  {{ formatChange(stock.change, stock.changePercent) }}
                </p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Volume</p>
                <p class="text-lg font-semibold">{{ formatNumber(stock.volume) }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Previous Close</p>
                <p class="text-lg font-semibold">{{ formatCurrency(stock.previousClose || 0) }}</p>
              </div>
            </div>

            <div class="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-1 text-sm text-gray-600">
              <div>
                <span class="text-gray-500">Open:</span>
                <span class="ml-1">{{ stock.open ? formatCurrency(stock.open) : 'N/A' }}</span>
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
            <div class="mt-4">
              <button
                @click="navigateToResearch(stock.symbol)"
                class="px-2 py-2 bg-green-400 text-sm font-bold text-black rounded-lg hover:bg-green-600 transition-colors"
                title="Research Stock"
              >
                Company Details
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>