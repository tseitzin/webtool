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
// import api from '../api/axios'

const router = useRouter()
const auth = useAuthStore()
const savedStocks = ref<StockData[]>([])
const { showRemoveModal, stockToRemove, confirmRemoval, cancelRemoval } = useStockRemoval()
const showPortfolioModal = ref(false)
const selectedStockForPortfolio = ref<StockData | null>(null)

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
    const stocks = await stockService.getSavedStocks()
    savedStocks.value = stocks
    
    // Start auto-refresh after initial fetch
    dashboardService.startAutoRefresh(stocks, (updatedStocks) => {
      savedStocks.value = updatedStocks
    })
  } catch (e) {
    console.error('Error fetching saved stocks:', e)
  }
}

const handleRemoveStock = (symbol: string) => {
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

// const handleAddToPortfolio = async (data: { quantity: number, notes: string }) => {
//   try {
//     if (!selectedStockForPortfolio.value) return
    
//     await api.post('/portfolio', {
//       symbol: selectedStockForPortfolio.value?.symbol,
//       quantity: data.quantity,
//       purchasePrice: selectedStockForPortfolio.value?.price,
//       notes: data.notes
//     })
//     showPortfolioModal.value = false
//     selectedStockForPortfolio.value = null
//     // Show success message using your toast system
//   } catch (error) {
//     console.error('Failed to add to portfolio:', error)
//     // Show error message
//   }
// }

// Add methods:
const openPortfolioModal = (stock: StockData) => {
  selectedStockForPortfolio.value = stock
  showPortfolioModal.value = true
}

const closePortfolioModal = () => {
  showPortfolioModal.value = false
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
              class="px-4 py-2 bg-indigo-600 text-sm text-white rounded-lg hover:bg-indigo-700 transition-colors"
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