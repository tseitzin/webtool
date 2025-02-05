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
import type { StockData, MarketMovers, CompanySearchResult } from '../types/polygon'
import { useCollapsibleSection } from '../composables/useCollapsibleSection'
import { useSearchStore } from '../stores/search'
import { useSearchHistoryStore } from '../stores/searchHistory'
import { useStockRemoval } from '../composables/useStockRemoval'
import api from '../api/axios'
import AddToPortfolioModal from '../components/AddToPortfolioModal.vue'
import { useLogger } from '../composables/useLogger'
import type { UserOwnedStock } from '../types/portfolio'
import { portfolioService } from '../services/portfolioService'
import { showErrorToast } from '../utils/toast'

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
const searchStore = useSearchStore()
const savedStocks = ref<StockData[]>([])
const selectedStock = ref<StockData | null>(searchStore.lastSearchedStock)
const marketMovers = ref<MarketMovers | null>(null)
const error = ref('')
const loading = ref(false)
const marketLoading = ref(false)
const searchSymbol = ref(searchStore.lastSearchSymbol)
const showPortfolioModal = ref(false)
const selectedStockForPortfolio = ref<StockData | null>(null)
const logger = useLogger()
const ownedStocks = ref<UserOwnedStock[]>([])
const searchHistoryStore = useSearchHistoryStore()
const searchQuery = ref('')
const searchResults = ref<CompanySearchResult[]>([])
const showSearchResults = ref(false)
const isSearchingCompany = ref(false)


const { showRemoveModal, stockToRemove, confirmRemoval, cancelRemoval } = useStockRemoval()

const { isExpanded: isIntroExpanded, toggleSection: toggleIntro } = 
  useCollapsibleSection('search_intro')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/access-denied')
    return
  }
  await Promise.all([
    fetchInitialData(),
    fetchMarketSummary()
  ])
  
})

// Cleanup on component unmount
watchEffect((onCleanup) => {
  onCleanup(() => {
    searchAreaService.stopAutoRefresh()
  })
})

const handleRemoveStock = (symbol: string) => {
  // Check if stock is in portfolio
  const isInPortfolio = ownedStocks.value.some(stock => stock.symbol === symbol)
  
  if (isInPortfolio) {
    showErrorToast(`Cannot remove ${symbol} from watchlist while it is in your portfolio`, 3000)
    return
  }
  
  confirmRemoval(symbol)
}

const handleConfirmRemoval = async () => {
  if (stockToRemove.value) {
    await toggleSavedStock(stockToRemove.value)
    cancelRemoval()
  }
}

const searchCompanies = async () => {
    if (!searchQuery.value || searchQuery.value.length < 2) {
      searchResults.value = []
      showSearchResults.value = false
      return
    }

    isSearchingCompany.value = true
    try {
      searchResults.value = await polygonService.searchCompanies(searchQuery.value)
      showSearchResults.value = true
    } catch (e) {
      console.error('Failed to search companies:', e)
      searchResults.value = []
    } finally {
      isSearchingCompany.value = false
    }
  }

  const selectCompany = (result: CompanySearchResult) => {
    searchSymbol.value = result.ticker
    searchQuery.value = ''
    showSearchResults.value = false
    searchStock()
  }

const fetchInitialData = async () => {
  loading.value = true
  error.value = ''
  try {
    const [stocks, owned] = await Promise.all([
      stockService.getSavedStocks(),
      portfolioService.getPortfolio()
    ])
    savedStocks.value = stocks.sort((a, b) => a.symbol.localeCompare(b.symbol))
    ownedStocks.value = owned
    
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

// Add helper function to get ownership info
  const getOwnershipInfo = (symbol: string) => {
    const owned = ownedStocks.value.find(stock => stock.symbol === symbol)
    if (!owned) return null
    
    return {
      shares: owned.quantity,
      value: owned.quantity * owned.averagePurchasePrice
    }
  }


const fetchMarketSummary = async () => {
  marketLoading.value = true
  error.value = ''
  try {
    const response = await api.get('/stockdata/summary')
    marketSummary.value = response.data
  } catch (e: any) {
    error.value = 'Failed to load market summary'
    console.error('Error:', e)
  } finally {
    marketLoading.value = false
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
    searchHistoryStore.addToHistory(stock) 

    await logger.info('Stock search result returned', {
        symbol: selectedStock.value.symbol,
        price: selectedStock.value.price,
        companyName: selectedStock.value.companyName,
        volume: selectedStock.value.volume,
        dateTime: new Date().toISOString()
      })
  } catch (e: any) {
    error.value = currentStock + ' is not a valid symbol or is currently unavailable and cannot be used in your analysis.'
    selectedStock.value = null
    searchStore.clearSearch()

    await logger.error(currentStock + ' is not a valid symbol or is currently unavailable and cannot be used in your analysis.', e as Error, {
      symbol: currentStock,
      dateTime: new Date().toISOString()
    })

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

const clearSearchHistory = () => {
  searchHistoryStore.clearHistory()
}

const toggleSavedStock = async (symbol: string) => {
  try {
    const isCurrentlySaved = savedStocks.value.some(s => s.symbol === symbol)
    
    if (isCurrentlySaved) {
      await stockService.removeSavedStock(symbol)
      savedStocks.value = savedStocks.value.filter(s => s.symbol !== symbol)

      await logger.info('Stock removed from watchlist', {
        symbol: symbol,
        dateTime: new Date().toISOString()

      })
    } else if (selectedStock.value) {
      await stockService.saveStock(selectedStock.value)
      savedStocks.value = [...savedStocks.value, selectedStock.value]

      await logger.info('Stock added to watchlist', {
        symbol: symbol,
        dateTime: new Date().toISOString()

      })
    }
  } catch (e: any) {
    error.value = 'Failed to update saved stocks'

    await logger.error('Failed to update saved stocks', e as Error, {
      symbol: savedStocks.value,
      dateTime: new Date().toISOString()
    })
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

const fetchSavedStocks = async () => {
  try {
    const stocks = await stockService.getSavedStocks()
    savedStocks.value = stocks
    
    
    // Start auto-refresh after initial fetch
    searchAreaService.startAutoRefresh(stocks, (updatedStocks) => {
      savedStocks.value = updatedStocks
    })
  } catch (e) {
    console.error('Error fetching saved stocks:', e)
  }
}

const openPortfolioModal = (stock: StockData) => {
  selectedStockForPortfolio.value = stock
  showPortfolioModal.value = true
}

const closePortfolioModal = () => {
  showPortfolioModal.value = false
  selectedStockForPortfolio.value = null
  location.reload()
}

</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-4 sm:py-8">
      <!-- Introduction Section -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden mb-6">
        <CollapsibleSectionHeader
          title="Stock Search & Tracking"
          :is-expanded="isIntroExpanded"
          @toggle="toggleIntro"
        />
        
        <div
          v-show="isIntroExpanded"
          class="p-4 sm:p-6 transition-all duration-300 ease-in-out bg-indigo-100 rounded-xl"
        >
          <p class="mb-4 font-bold text-sm sm:text-base">
            This is where you can search for details on any stocks traded in the US markets.
          </p>
          <p class="mb-4 text-sm sm:text-base">
            Use this area to search for and track your favorite stocks:
          </p>
          <ul class="list-disc list-inside space-y-2 ml-4 text-sm sm:text-base">
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
        class="mb-6 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded text-sm sm:text-base"
      >
        {{ error }}
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="mt-6 bg-white rounded-lg shadow-md p-4 sm:p-8">
        <h1 class="font-bold text-xl sm:text-2xl text-center">Loading Stocks</h1>
        <LoadingSpinner size="lg" />
      </div>

      <!-- Stock Search -->
      <div class="bg-white rounded-lg shadow-md p-4 sm:p-6 mb-6">
        <div class="flex flex-col sm:flex-row gap-4 items-center">
          <div class="relative w-full sm:w-48">
            <input
              v-model="searchSymbol"
              type="text"
              placeholder="Enter symbol"
              class="w-full px-3 sm:px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent uppercase text-sm sm:text-base"
              maxlength="10"
              @keyup.enter="searchStock"
            />
            <button
              v-if="searchSymbol"
              @click="clearSearch"
              class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
              title="Clear search"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 sm:h-5 sm:w-5" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </button>
          </div>
          <!-- Company Name Search -->
          <div class="relative w-full sm:w-96">
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Or search by company name"
              class="w-full px-3 sm:px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-sm sm:text-base"
              @input="searchCompanies"
            />
            <div
              v-if="showSearchResults && searchResults.length > 0"
              class="absolute z-10 w-full mt-1 bg-white rounded-md shadow-lg max-h-60 overflow-auto"
            >
              <ul class="py-1">
                <li
                  v-for="result in searchResults"
                  :key="result.ticker"
                  @click="selectCompany(result)"
                  class="px-4 py-2 hover:bg-gray-100 cursor-pointer"
                >
                  <div class="font-medium">{{ result.name }}</div>
                  <div class="text-sm text-gray-600">{{ result.ticker }}</div>
                </li>
              </ul>
            </div>
            <div
              v-if="isSearchingCompany"
              class="absolute right-3 top-1/2 transform -translate-y-1/2"
            >
              <LoadingSpinner size="sm" />
            </div>
          </div>

          <button
            @click="searchStock"
            class="w-full sm:w-auto px-4 py-2 bg-indigo-600 text-white text-sm rounded-lg hover:bg-indigo-800 transition-colors"
          >
            Search
          </button>
        </div>

        <div v-if="searchHistoryStore.searchHistory.length > 0" class="mt-4 mb-3">
          <div class="flex mb-2">
            <h3 class="text-sm font-medium text-gray-700">Recent Searches:</h3>
            <button
              @click="clearSearchHistory"
              class="text-xs text-red-600 hover:text-red-900 hover:bg-gray-200 transition-colors ml-4"
            >
              Clear History
            </button>
          </div>
          <div class="flex flex-wrap gap-2">
            <button
              v-for="stock in searchHistoryStore.searchHistory"
              :key="stock.symbol"
              @click="() => { searchSymbol = stock.symbol; searchStock(); }"
              class="px-3 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded-md transition-colors flex items-center gap-2"
            >
              <span>{{ stock.symbol }}</span>
              <span :class="stock.change >= 0 ? 'text-green-600' : 'text-red-600'">
                {{ formatPercent(stock.changePercent) }}
              </span>
            </button>
          </div>
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
    <div class="flex flex-row justify-between items-center mb-4">
      <h2 class="text-xl sm:text-2xl font-bold">Your Watchlisted Stocks</h2>
    </div>

    <MarketStatusMessage 
      v-if="savedStocks[0]?.marketStatus"
      :market-status="savedStocks[0].marketStatus" 
    />

    <div class="space-y-4">
      <div
        v-for="stock in savedStocks"
        :key="stock.symbol"
        class="bg-white rounded-lg shadow-lg hover:shadow-xl transition-shadow p-4"
      >
        <!-- Stock Header -->
        <div class="flex justify-between items-center mb-1">
          <div>
            <h3 class="text-lg font-bold">{{ stock.symbol }}</h3>
            <p class="text-sm text-gray-600">{{ stock.companyName }}</p>
          </div>
          <div class="flex gap-2">
            <button
              @click="navigateToResearch(stock.symbol)"
              class="px-4 py-2 bg-green-600 text-sm text-white rounded-lg hover:bg-green-800 transition-colors"
            >
              Research
            </button>
            <button
              @click="openPortfolioModal(stock)"
              class="px-4 py-2 bg-blue-600 text-sm text-white rounded-lg hover:bg-blue-800 transition-colors"
            >
              Update Portfolio
            </button>
            <button
              @click="handleRemoveStock(stock.symbol)"
              class="px-4 py-2 bg-red-600 text-sm text-white rounded-lg hover:bg-red-800 transition-colors"
            >
              Remove
            </button>
          </div>
        </div>

        <!-- Stock Details Grid -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <!-- Current Price -->
          <div class="bg-gray-100 p-2 rounded-lg">
            <p class="text-sm text-gray-500">Current Price</p>
            <p class="text-lg font-semibold">{{ formatCurrency(stock.price) }}</p>
          </div>

          <!-- Today's Change -->
          <div class="bg-gray-100 p-2 rounded-lg">
            <p class="text-sm text-gray-500">Today's Change</p>
            <p :class="['text-lg font-semibold', stock.change >= 0 ? 'text-green-600' : 'text-red-600']">
              {{ formatChange(stock.change, stock.changePercent) }}
            </p>
          </div>

          <!-- Volume -->
          <div class="bg-gray-100 p-2 rounded-lg">
            <p class="text-sm text-gray-500">Volume</p>
            <p class="text-lg font-semibold">{{ formatNumber(stock.volume) }}</p>
          </div>

          <!-- Previous Close -->
          <div class="bg-gray-100 p-2 rounded-lg">
            <p class="text-sm text-gray-500">Previous Close</p>
            <p class="text-lg font-semibold">{{ formatCurrency(stock.previousClose || 0) }}</p>
          </div>
        </div>

        <!-- Additional Details Grid -->
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-2">
          <!-- Open -->
          <div class="bg-gray-100 p-2 rounded-lg">
            <p class="text-sm text-gray-500">Open</p>
            <p class="text-lg font-semibold">{{ stock.open ? formatCurrency(stock.open) : 'N/A' }}</p>
          </div>

          <!-- High -->
          <div class="bg-gray-100 p-2 rounded-lg">
            <p class="text-sm text-gray-500">High</p>
            <p class="text-lg font-semibold">{{ stock.high ? formatCurrency(stock.high) : 'N/A' }}</p>
          </div>

          <!-- Low -->
          <div class="bg-gray-100 p-2 rounded-lg">
            <p class="text-sm text-gray-500">Low</p>
            <p class="text-lg font-semibold">{{ stock.low ? formatCurrency(stock.low) : 'N/A' }}</p>
          </div>
        </div>

        <div 
              v-if="getOwnershipInfo(stock.symbol)"
              class="mt-4 bg-blue-50 p-3 rounded-lg"
            >
              <div class="flex">
                <div>
                  <span class="font-medium text-blue-800">Number in Portfolio:</span>
                  <span class="ml-2 text-blue-700">
                    {{ formatNumber(getOwnershipInfo(stock.symbol)?.shares || 0) }} shares
                  </span>
                </div>
                <div>
                  <span class="font-medium text-blue-800 ml-6">Total Value:</span>
                  <span class="ml-2 text-blue-700">
                    {{ formatCurrency((getOwnershipInfo(stock.symbol)?.value || 0)) }}
                  </span>
                </div>
              </div>
            </div>
            <div 
              v-else 
              class="mt-4 bg-gray-50 p-3 rounded-lg"
            >
              <p class="font-semibold text-gray-600">Not currently in portfolio</p>
            </div>
      </div>

      
    </div>
  </div>

      <!-- Market Movers Section -->
      <div class="flex flex-row justify-between items-center mt-6 mb-1">
      <h2 class="text-xl sm:text-2xl font-bold">Market Movers</h2>
    </div>
      <div v-if="marketMovers" class="mt-1 space-y-4">
        <!-- Market Status Message -->
        <MarketStatusMessage 
          v-if="marketMovers.marketStatus"
          :market-status="marketMovers.marketStatus"
        />

        <!-- Last Update Time -->
        <div v-if="marketMovers.lastUpdate" class="text-xs sm:text-sm text-gray-600">
          Last updated: {{ new Date(marketMovers.lastUpdate).toLocaleString() }}
        </div>
        <div class="ml-2 sm:font-semibold">
          Click on any of the Gainers or Losers symbol name to learn more about that stock.
        </div>

        <!-- Movers Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <!-- Top Gainers -->
          <div class="bg-white rounded-lg shadow-md p-4 sm:p-6">
            <h2 class="text-base sm:text-lg font-bold mb-2 text-green-600">Top 10 Gainers</h2>
            <div class="overflow-x-auto">
              <table class="min-w-full">
                <thead>
                  <tr class="text-left text-xs font-medium font-bold text-gray-500">
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
                    class="text-xs sm:text-sm"
                  >
                    <td 
                      class="py-2 cursor-pointer text-black-600 hover:text-blue-800 hover:font-bold"
                      @click="navigateToResearch(stock.symbol)"
                    >
                      {{ stock.symbol }}
                    </td>
                    <td class="py-2">{{ formatCurrency(stock.price) }}</td>
                    <td class="py-2 text-green-600 font-semibold">
                      {{ formatPercent(stock.changePercent) }}
                    </td>
                    <td class="py-2">{{ formatNumber(stock.volume) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Top Losers -->
          <div class="bg-white rounded-lg shadow-md p-4 sm:p-6">
            <h2 class="text-base sm:text-lg font-bold mb-2 text-red-600">Top 10 Losers</h2>
            <div class="overflow-x-auto">
              <table class="min-w-full">
                <thead>
                  <tr class="text-left text-xs font-medium font-bold text-gray-500">
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
                    class="text-xs sm:text-sm"
                  >
                    <td 
                      class="py-2 cursor-pointer text-black-600 hover:text-blue-800 hover:font-bold"
                      @click="navigateToResearch(stock.symbol)"
                    >
                      {{ stock.symbol }}
                    </td>
                    <td class="py-2">{{ formatCurrency(stock.price) }}</td>
                    <td class="py-2 text-red-600 font-semibold">
                      {{ formatPercent(stock.changePercent) }}
                    </td>
                    <td class="py-2">{{ formatNumber(stock.volume) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State for Market Movers -->
      <div v-if="!marketMovers" class="mt-6 bg-white rounded-lg shadow-md p-4 sm:p-8">
        <h1 class="font-bold text-xl sm:text-2xl text-center">Loading Market Movers</h1>
        <LoadingSpinner size="lg" />
      </div>

      <ConfirmationModal
        :is-open="showRemoveModal"
        title="Remove Stock"
        message="Are you sure you want to remove this stock from your watchlist?"
        @confirm="handleConfirmRemoval"
        @cancel="cancelRemoval"
      />

      <AddToPortfolioModal
        v-if="showPortfolioModal && selectedStockForPortfolio"
        :is-open="showPortfolioModal"
        :symbol="selectedStockForPortfolio.symbol"
        :price="selectedStockForPortfolio.price"
        @close="closePortfolioModal"
        @success="fetchSavedStocks"
      />

    </div>
  </div>
</template>