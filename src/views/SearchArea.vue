<script setup lang="ts">
import { ref, onMounted, watchEffect } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { polygonService } from '../services/polygonService'
import { stockService } from '../services/stockService'
import { searchAreaService } from '../services/searchAreaService'
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import StockSearchResult from '../components/StockSearchResult.vue'
import CollapsibleSectionHeader from '../components/CollapsibleSectionHeader.vue'
import type { StockData, MarketMovers } from '../types/polygon'
import { useCollapsibleSection } from '../composables/useCollapsibleSection'
import { useSearchStore } from '../stores/search'

const router = useRouter()
const auth = useAuthStore()
const searchStore = useSearchStore()
const savedStocks = ref<StockData[]>([])
const selectedStock = ref<StockData | null>(searchStore.lastSearchedStock)
const marketMovers = ref<MarketMovers | null>(null)
const error = ref('')
const loading = ref(false)
const searchSymbol = ref(searchStore.lastSearchSymbol)

const { isExpanded: isIntroExpanded, toggleSection: toggleIntro } = 
  useCollapsibleSection('search_intro')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/access-denied')
    return
  }
  await fetchInitialData()
})

// Cleanup on component unmount
watchEffect((onCleanup) => {
  onCleanup(() => {
    searchAreaService.stopAutoRefresh()
  })
})

const fetchInitialData = async () => {
  loading.value = true
  error.value = ''
  try {
    const stocks = await stockService.getSavedStocks()
    savedStocks.value = stocks
    
    // Start auto-refresh after initial fetch
    searchAreaService.startAutoRefresh(stocks, (updatedStocks, updatedMovers) => {
      savedStocks.value = updatedStocks
      marketMovers.value = updatedMovers
    })
  } catch (e: any) {
    error.value = 'Failed to load stock data'
    console.error('Error:', e)
  } finally {
    loading.value = false
  }
}

const searchStock = async () => {
  if (!searchSymbol.value) return
  
  loading.value = true
  error.value = ''
  const currentStock = searchSymbol.value.toUpperCase()
  try {
    const stock = await polygonService.getStockSnapshot(currentStock)
    selectedStock.value = stock
    searchStore.setLastSearchedStock(stock)
  } catch (e: any) {
    error.value = currentStock + ' is not a valid symbol or is currently unavailable and cannot be used in your analysis.'
    selectedStock.value = null
    searchStore.clearSearch()
  } finally {
    loading.value = false
  }
}

const clearSearch = () => {
  searchSymbol.value = ''
  selectedStock.value = null
  error.value = ''
  searchStore.clearSearch()
}

const toggleSavedStock = async (symbol: string) => {
  try {
    const isCurrentlySaved = savedStocks.value.some(s => s.symbol === symbol)
    
    if (isCurrentlySaved) {
      await stockService.removeSavedStock(symbol)
      savedStocks.value = savedStocks.value.filter(s => s.symbol !== symbol)
    } else if (selectedStock.value) {
      await stockService.saveStock(selectedStock.value)
      savedStocks.value = [...savedStocks.value, selectedStock.value]
    }
  } catch (e: any) {
    error.value = 'Failed to update saved stocks'
  }
}

const isStockSaved = (symbol: string): boolean => {
  return savedStocks.value.some(s => s.symbol === symbol)
}

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}

const navigateToResearch = (symbol: string) => {
  router.push(`/research/${symbol}`)
}

</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-4">

      <!-- Introduction Section -->
      <!-- Introduction Section -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden mb-6">
        <CollapsibleSectionHeader
          title="Stock Search & Tracking"
          :is-expanded="isIntroExpanded"
          @toggle="toggleIntro"
        />
        
        <div
          v-show="isIntroExpanded"
          class="ml-4 mr-4 mb-4 p-6 transition-all duration-300 ease-in-out bg-indigo-100 rounded-xl"
        >
        <p class="mb-4 font-bold">
          This is where you can search for details on any stocks traded in the US markets.
          </p>
          <p class="mb-4">
            Use this area to search for and track your favorite stocks:
          </p>
          <ul class="list-disc list-inside space-y-2 ml-4">
            <li>Enter a stock symbol to view current market data</li>
            <li>Save stocks to your watchlist for quick access</li>
            <li>View detailed company information and performance metrics</li>
            <li>Track market movers and trending stocks</li>
          </ul>
        </div>
      </div>


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
        <div class="flex flex-col sm:flex-row gap-4 items-center">
          <div class="relative w-full sm:w-48">
            <input
              v-model="searchSymbol"
              type="text"
              placeholder="Enter symbol"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent uppercase"
              maxlength="10"
              @keyup.enter="searchStock"
            />
            <button
              v-if="searchSymbol"
              @click="clearSearch"
              class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
              title="Clear search"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </button>
          </div>
          <button
            @click="searchStock"
            class="px-4 py-2 bg-indigo-600 text-white text-sm rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Search
          </button>
        </div>

        <!-- Selected Stock Details -->
        <StockSearchResult
          v-if="selectedStock"
          :stock="selectedStock"
          :is-favorited="isStockSaved(selectedStock.symbol)"
          @toggle-favorite="toggleSavedStock"
        />
      </div>

      <!-- Saved Stocks Section -->
      <div v-if="savedStocks.length > 0" class="mt-8">
        <h2 class="text-2xl font-bold mb-4">Your Saved Stocks</h2>
        <MarketStatusMessage 
        v-if="savedStocks[0]?.marketStatus"
             :market-status="savedStocks[0].marketStatus" 
        />
        <div class="space-y-3">
          <div
            v-for="stock in savedStocks"
            :key="stock.symbol"
            class="bg-white rounded-lg shadow-lg hover:shadow-xl transition-shadow p-4 flex items-center justify-between"
          >
            <div class="flex items-center space-x-9 flex-grow">
              <div class="w-24">
                <p class="text-sm text-gray-500">Symbol</p>
                <h3 class="text-lg font-bold">{{ stock.symbol }}</h3>
              </div>
              <div class="w-48">
                <p class="text-sm text-gray-600">Company Name</p>
                <h3 class="text-sm font-bold text-gray-800">{{ stock.companyName }}</h3>
              </div>
              <div class="w-24">
                <p class="text-sm text-gray-500">Current Price</p>
                <p class="font-semibold">{{ formatCurrency(stock.price) }}</p>
              </div>
              <div class="w-40">
                <p class="text-sm text-gray-500">Today's Change</p>
                <p :class="['font-semibold', stock.change >= 0 ? 'text-green-600' : 'text-red-600']">
                  {{ formatChange(stock.change, stock.changePercent) }}
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
              <div class="w-32">
                <button
                  @click="navigateToResearch(stock.symbol)"
                  class="px-2 py-2 bg-indigo-500 text-sm text-white rounded-lg hover:bg-indigo-700 transition-colors"
                  title="Research Stock"
                >
                    Research Stock
                  </button>
              </div>
            </div>
            <button
                @click="toggleSavedStock(stock.symbol)"
                class="px-2 py-2 bg-red-500 text-sm text-white rounded-lg hover:bg-red-700 transition-colors"
              >
                Remove
              </button>
          </div>
        </div>
        
      </div>

       <!-- Loading State -->
       <div 
        v-if="loading" 
        class="flex justify-center items-center py-8"
      >
        <div class="animate-spin rounded-full h-12 w-12 border-4 border-indigo-500 border-t-transparent"></div>
      </div>


      <!-- Market Movers Section -->
      <div v-if="marketMovers" class="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Market Status Message -->
        <MarketStatusMessage 
          v-if="marketMovers.marketStatus"
          :market-status="marketMovers.marketStatus"
          class="col-span-full"
        />
        <!-- Last Update Time -->
          <div v-if="marketMovers.lastUpdate" class="col-span-full text-sm text-gray-600 mb-2">
            Last updated: {{ new Date(marketMovers.lastUpdate).toLocaleString() }}
          </div>
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