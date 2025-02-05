<script setup lang="ts">
import { ref, onMounted, watchEffect } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { stockService } from '../services/stockService'
import { dashboardService } from '../services/dashboardService'
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { StockData } from '../types/polygon'
import { useStockRemoval } from '../composables/useStockRemoval'
import AddToPortfolioModal from '../components/AddToPortfolioModal.vue'
import RemoveFromPortfolioModal from '../components/RemoveFromPortfolioModal.vue'
import { portfolioService } from '../services/portfolioService'
import type { UserOwnedStock } from '../types/portfolio'
import { showErrorToast } from '../utils/toast'


const router = useRouter()
const auth = useAuthStore()
const savedStocks = ref<StockData[]>([])
const { showRemoveModal, stockToRemove, confirmRemoval, cancelRemoval } = useStockRemoval()
const showPortfolioModal = ref(false)
const showRemovePortfolioModal = ref(false)
const selectedStockForPortfolio = ref<StockData | null>(null)
const ownedStocks = ref<UserOwnedStock[]>([])

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
  try {
    const [stocks, owned] = await Promise.all([
      stockService.getSavedStocks(),
      portfolioService.getPortfolio()
    ])
    savedStocks.value = stocks.sort((a, b) => a.symbol.localeCompare(b.symbol))
    ownedStocks.value = owned
    
    // Start auto-refresh after initial fetch
    dashboardService.startAutoRefresh(stocks, (updatedStocks) => {
      savedStocks.value = updatedStocks
    })
  } catch (e) {
    console.error('Error fetching stocks:', e)
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

const handleRemoveStock = (symbol: string) => {
  // Check if stock is in portfolio
  const isInPortfolio = ownedStocks.value.some(stock => stock.symbol === symbol)
  
  if (isInPortfolio) {
    showErrorToast(`Cannot remove ${symbol} from watchlist while it is in your portfolio`, 3000)
    return
    
  }
  
  confirmRemoval(symbol)
}

const handlePortfolioSuccess = async () => {
  // Refresh data after successful portfolio update
  await fetchSavedStocks()
}


const handleConfirmRemoval = async () => {
  if (stockToRemove.value) {
    await removeSavedStock(stockToRemove.value)
    cancelRemoval()
  }
}

const removeSavedStock = async (symbol: string) => {
  try {
    await stockService.removeSavedStock(symbol)
    savedStocks.value = savedStocks.value.filter(stock => stock.symbol !== symbol)
  } catch (e) {
    console.error('Failed to remove stock:', e)
  }
}

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}

const navigateToSearch = () => {
  router.push('/search-area')
}

const navigateToResearch = (symbol: string) => {
  router.push(`/research/${symbol}`)
}

const openPortfolioModal = (stock: StockData) => {
  selectedStockForPortfolio.value = stock
  showPortfolioModal.value = true
}

const closePortfolioModal = () => {
  showPortfolioModal.value = false
  selectedStockForPortfolio.value = null
}

const openRemovePortfolioModal = (stock: StockData) => {
  selectedStockForPortfolio.value = stock
  showRemovePortfolioModal.value = true
}

const closeRemovePortfolioModal = () => {
  showRemovePortfolioModal.value = false
  selectedStockForPortfolio.value = null
}

</script>

<template>
    <div class="min-h-screen bg-gray-100">
      <div class="container mx-auto px-4 py-4">
        <!-- Saved Stocks Section -->
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
          <div class="flex flex-row justify-between items-center mb-4">
            <h2 class="text-xl font-bold">Your Stock Watchlist</h2>
            <button
              @click="navigateToSearch"
              class="px-4 py-2 bg-indigo-600 text-xs text-white rounded-lg hover:bg-indigo-700 transition-colors"
            >
              Search Stocks
            </button>
          </div>
  
          <MarketStatusMessage 
            v-if="savedStocks[0]?.marketStatus"
            :market-status="savedStocks[0].marketStatus" 
          />
  
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
                  class="px-4 py-2 bg-blue-600 text-xs text-white rounded-lg hover:bg-blue-800 transition-colors"
                >
                  Research Stock
                </button>
                <button
                  @click="openPortfolioModal(stock)"
                  class="px-4 py-2 bg-green-600 text-xs text-white rounded-lg hover:bg-green-800 transition-colors"
                >
                  Buy Stock
                </button>
                <button
                  v-if="getOwnershipInfo(stock.symbol)"
                  @click="openRemovePortfolioModal(stock)"
                  class="px-4 py-2 bg-red-600 text-xs text-white rounded-lg hover:bg-red-800 transition-colors"
                >
                  Sell Stock
                </button>
                <button
                  @click="handleRemoveStock(stock.symbol)"
                  class="px-4 py-2 bg-gray-600 text-xs text-white rounded-lg hover:bg-gray-800 transition-colors"
                >
                  Remove
                </button>
              </div>
            </div>
  
            <!-- Stock Details Grid -->
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              <div class="bg-gray-100 p-2 rounded-lg">
                <p class="text-sm text-gray-500">Current Price</p>
                <p class="text-lg font-semibold">{{ formatCurrency(stock.price) }}</p>
              </div>
              <div class="bg-gray-100 p-2 rounded-lg">
                <p class="text-sm text-gray-500">Today's Change</p>
                <p :class="['text-lg font-semibold', stock.change >= 0 ? 'text-green-600' : 'text-red-600']">
                  {{ formatChange(stock.change, stock.changePercent) }}
                </p>
              </div>
              <div class="bg-gray-100 p-2 rounded-lg">
                <p class="text-sm text-gray-500">Volume</p>
                <p class="text-lg font-semibold">{{ formatNumber(stock.volume) }}</p>
              </div>
              <div class="bg-gray-100 p-2 rounded-lg">
                <p class="text-sm text-gray-500">Previous Close</p>
                <p class="text-lg font-semibold">{{ formatCurrency(stock.previousClose || 0) }}</p>
              </div>
            </div>
  
            <!-- Additional Details Grid -->
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-2">
              <div class="bg-gray-100 p-2 rounded-lg">
                <p class="text-sm text-gray-500">Open</p>
                <p class="text-lg font-semibold">{{ stock.open ? formatCurrency(stock.open) : 'N/A' }}</p>
              </div>
              <div class="bg-gray-100 p-2 rounded-lg">
                <p class="text-sm text-gray-500">High</p>
                <p class="text-lg font-semibold">{{ stock.high ? formatCurrency(stock.high) : 'N/A' }}</p>
              </div>
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

        <AddToPortfolioModal
          v-if="selectedStockForPortfolio"
          :is-open="showPortfolioModal"
          :symbol="selectedStockForPortfolio.symbol"
          :price="selectedStockForPortfolio.price"
          @close="closePortfolioModal"
          @success="handlePortfolioSuccess"
        />

        <RemoveFromPortfolioModal
          v-if="selectedStockForPortfolio"
          :is-open="showRemovePortfolioModal"
          :symbol="selectedStockForPortfolio.symbol"
          :price="selectedStockForPortfolio.price"
          @close="closeRemovePortfolioModal"
          @success="handlePortfolioSuccess"
        />
  
        <ConfirmationModal
          :is-open="showRemoveModal"
          title="Remove Stock"
          message="Are you sure you want to remove this stock from your watchlist?"
          @confirm="handleConfirmRemoval"
          @cancel="cancelRemoval"
        />
      </div>
    </div>
  </template>