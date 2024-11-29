<script setup lang="ts">
import { ref, onMounted, watchEffect, computed } from 'vue'
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

interface OwnedStock {
  id: number
  symbol: string
  quantity: number
  purchasePrice: number
  purchaseDate: string
  notes?: string
}

interface AddPositionForm {
  symbol: string
  quantity: number
  purchasePrice: number
  purchaseDate: string
  notes: string
}

const router = useRouter()
const auth = useAuthStore()
const favoriteStocks = ref<StockData[]>([])
const ownedStocks = ref<(StockData & { position: OwnedStock })[]>([])
const error = ref('')
const loading = ref(false)
const refreshInterval = ref<number | null>(null)
const showAddPositionModal = ref(false)
const showSellModal = ref(false)
const selectedStock = ref<(StockData & { position: OwnedStock }) | null>(null)
const sellQuantity = ref<number>(0)
const addPositionForm = ref<AddPositionForm>({
  symbol: '',
  quantity: 0,
  purchasePrice: 0,
  purchaseDate: new Date().toISOString().split('T')[0],
  notes: ''
})

const totalInvestment = computed(() => {
  return ownedStocks.value.reduce((total, stock) => {
    return total + (stock.position.quantity * stock.position.purchasePrice)
  }, 0)
})

const currentValue = computed(() => {
  return ownedStocks.value.reduce((total, stock) => {
    return total + (stock.position.quantity * stock.price)
  }, 0)
})

const totalReturn = computed(() => {
  return currentValue.value - totalInvestment.value
})

const returnPercentage = computed(() => {
  if (totalInvestment.value === 0) return 0
  return (totalReturn.value / totalInvestment.value) * 100
})

const openPurchaseModal = (stock: StockData) => {
  // Check if we already own this stock
  const existingPosition = ownedStocks.value.find(s => s.symbol === stock.symbol)
  
  addPositionForm.value = {
    symbol: stock.symbol,
    quantity: 0,
    purchasePrice: stock.price,
    purchaseDate: new Date().toISOString().split('T')[0],
    notes: existingPosition?.position.notes || ''
  }
  showAddPositionModal.value = true
}

const openSellModal = (stock: StockData & { position: OwnedStock }) => {
  selectedStock.value = stock
  sellQuantity.value = 0
  showSellModal.value = true
}

const sellPosition = async () => {
  if (!selectedStock.value || !sellQuantity.value) return

  try {
    await api.post(`/portfolio/${selectedStock.value.position.id}/sell`, {
      quantity: sellQuantity.value
    })
    showSellModal.value = false
    await fetchPortfolioData()
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to sell position'
  }
}

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
  await fetchAllStockData()
  startAutoRefresh()
})

// Cleanup on component unmount
watchEffect((onCleanup) => {
  onCleanup(() => {
    stopAutoRefresh()
  })
})

const fetchAllStockData = async () => {
  loading.value = true
  error.value = ''
  try {
    await Promise.all([
      fetchFavoriteStockData(),
      fetchPortfolioData()
    ])
  } catch (e: any) {
    error.value = 'Failed to load stock data'
    console.error('Error:', e)
  } finally {
    loading.value = false
  }
}

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

const fetchPortfolioData = async () => {
  const portfolioResponse = await api.get<OwnedStock[]>('/portfolio')
  const portfolio = portfolioResponse.data

  const stockPromises = portfolio.map(pos => 
    api.get<StockData>(`/stockdata/${pos.symbol}`)
  )
  const responses = await Promise.all(stockPromises)
  ownedStocks.value = responses.map((r, i) => ({
    ...r.data,
    position: portfolio[i]
  }))
}

const removeFromFavorites = async (symbol: string) => {
  try {
    await api.delete(`/favorites/${symbol}`)
    favoriteStocks.value = favoriteStocks.value.filter(stock => stock.symbol !== symbol)
  } catch (e: any) {
    error.value = 'Failed to remove from favorites'
  }
}

const addPosition = async () => {
  try {
    // Check if we already own this stock
    const existingPosition = ownedStocks.value.find(s => s.symbol === addPositionForm.value.symbol)
    
    if (existingPosition) {
      // Update existing position
      await api.put(`/portfolio/${existingPosition.position.id}`, {
        quantity: existingPosition.position.quantity + addPositionForm.value.quantity,
        notes: addPositionForm.value.notes
      })
    } else {
      // Create new position
      await api.post('/portfolio', {
        symbol: addPositionForm.value.symbol,
        quantity: addPositionForm.value.quantity,
        purchasePrice: addPositionForm.value.purchasePrice,
        purchaseDate: new Date(addPositionForm.value.purchaseDate),
        notes: addPositionForm.value.notes
      })
    }
    
    showAddPositionModal.value = false
    await fetchPortfolioData()
  } catch (e: any) {
    error.value = 'Failed to add position'
  }
}

// const removePosition = async (id: number) => {
//   if (!confirm('Are you sure you want to remove this position?')) return

//   try {
//     await api.delete(`/portfolio/${id}`)
//     ownedStocks.value = ownedStocks.value.filter(stock => stock.position.id !== id)
//   } catch (e: any) {
//     error.value = 'Failed to remove position'
//   }
// }

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
    <div class="container mx-auto px-4 py-4">
      <div class="flex justify-between items-center mb-4">
        <h1 class="text-3xl font-bold">{{ auth.user?.name }}'s Dashboard</h1>
      </div>
      <div class="flex flex-row justify-between items-center mb-4">
        <p class="text-xl font-bold">Stock Watchlist</p>
        <button
          @click="navigateToSearch"
          class="px-2 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
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
            <div class="flex space-x-2">
              <button
                  @click="openPurchaseModal(stock)"
                  class="text-green-600 hover:text-green-800 transition-colors flex items-center"
                  title="Purchase stock"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-1" viewBox="0 0 20 20" fill="currentColor">
                    <path d="M4 4a2 2 0 00-2 2v1h16V6a2 2 0 00-2-2H4z" />
                    <path fill-rule="evenodd" d="M18 9H2v5a2 2 0 002 2h12a2 2 0 002-2V9zM4 13a1 1 0 011-1h1a1 1 0 110 2H5a1 1 0 01-1-1zm5-1a1 1 0 100 2h1a1 1 0 100-2H9z" clip-rule="evenodd" />
                  </svg>
                  Purchase
              </button>
              
            </div>
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
              <div>
                <button
                @click="removeFromFavorites(stock.symbol)"
                class="text-red-600 hover:text-green-800 text-xs transition-colors"
                title="Remove from watchlist"
              >
              Remove
              </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="mb-6 mt-9">
        <hr class="h-px bg-gray-700 border-0 dark:bg-gray-700">
      </div>

      <!-- Portfolio Section -->
      <div>
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-xl font-bold">Your Portfolio</h2>
          <!-- <button
            @click="showAddPositionModal = true"
            class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
          >
            Add Position
          </button> -->
        </div>

        <div v-if="ownedStocks.length === 0" class="bg-white rounded-lg shadow-md p-8 text-center">
          <h3 class="text-xl font-semibold mb-4">No Positions Yet</h3>
          <p class="text-gray-600 mb-6">Start building your portfolio by adding your stock positions.</p>
          <button
            @click="showAddPositionModal = true"
            class="px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
          >
            Add Your First Position
          </button>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div
            v-for="stock in ownedStocks"
            :key="stock.position.id"
            class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
          >
            <div class="flex justify-between items-center mb-4">
              <h3 class="text-xl font-bold">{{ stock.symbol }}</h3>
              <button
                @click="openSellModal(stock)"
                class="text-red-600 hover:text-red-800 transition-colors flex items-center"
                title="Sell position"
              >
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-1" viewBox="0 0 20 20" fill="currentColor">
                  <path fill-rule="evenodd" d="M4 4a2 2 0 00-2 2v1h16V6a2 2 0 00-2-2H4z" />
                  <path fill-rule="evenodd" d="M18 9H2v5a2 2 0 002 2h12a2 2 0 002-2V9zM4 13a1 1 0 011-1h1a1 1 0 110 2H5a1 1 0 01-1-1zm5-1a1 1 0 100 2h1a1 1 0 100-2H9z" clip-rule="evenodd" />
                </svg>
                Sell
              </button>
            </div>

            <div class="grid grid-cols-2 gap-4 mb-4">
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
            </div>

            <div class="border-t border-gray-200 pt-4">
              <div class="grid grid-cols-2 gap-4 mb-4">
                <div>
                  <p class="text-sm text-gray-500">Quantity</p>
                  <p class="text-lg font-semibold">{{ stock.position.quantity }}</p>
                </div>
                <div>
                  <p class="text-sm text-gray-500">Purchase Price</p>
                  <p class="text-lg font-semibold">{{ formatCurrency(stock.position.purchasePrice) }}</p>
                </div>
              </div>

              <div class="grid grid-cols-2 gap-4">
                <div>
                  <p class="text-sm text-gray-500">Total Cost</p>
                  <p class="text-lg font-semibold">
                    {{ formatCurrency(stock.position.quantity * stock.position.purchasePrice) }}
                  </p>
                </div>
                <div>
                  <p class="text-sm text-gray-500">Current Value</p>
                  <p class="text-lg font-semibold">
                    {{ formatCurrency(stock.position.quantity * stock.price) }}
                  </p>
                </div>
              </div>

              <div v-if="stock.position.notes" class="mt-4 pt-4 border-t border-gray-200">
                <p class="text-sm text-gray-500">Notes</p>
                <p class="text-sm">{{ stock.position.notes }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="mb-6 mt-9">
        <hr class="h-px bg-gray-700 border-0 dark:bg-gray-700">
      </div>

      <div class="flex flex-row justify-between items-center mt-5">
        <h2 class="text-xl font-bold mb-4">Portfolio Summary</h2>
      </div>

      <!-- Portfolio Summary -->
      <div v-if="ownedStocks.length > 0" class="bg-white rounded-lg shadow-md p-6 mb-8">
        
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <p class="text-sm text-gray-500">Total Investment</p>
            <p class="text-xl font-semibold">{{ formatCurrency(totalInvestment) }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500">Current Value</p>
            <p class="text-xl font-semibold">{{ formatCurrency(currentValue) }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500">Total Return</p>
            <p :class="['text-xl font-semibold', totalReturn >= 0 ? 'text-green-600' : 'text-red-600']">
              {{ formatCurrency(totalReturn) }}
            </p>
          </div>
          <div>
            <p class="text-sm text-gray-500">Return %</p>
            <p :class="['text-xl font-semibold', returnPercentage >= 0 ? 'text-green-600' : 'text-red-600']">
              {{ formatPercent(returnPercentage) }}
            </p>
          </div>
        </div>
      </div>

    <!-- Add Position Modal -->
    <div v-if="showAddPositionModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-lg shadow-xl p-6 w-full max-w-md">
        <h2 class="text-xl font-bold mb-4">Add Position</h2>
        
        <form @submit.prevent="addPosition" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700">Symbol</label>
            <input
              v-model="addPositionForm.symbol"
              type="text"
              required
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
              placeholder="e.g., AAPL"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700">Quantity</label>
            <input
              v-model="addPositionForm.quantity"
              type="number"
              step="0.0001"
              required
              min="0.0001"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700">Purchase Price</label>
            <input
              v-model="addPositionForm.purchasePrice"
              type="number"
              step="0.01"
              required
              min="0.01"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700">Purchase Date</label>
            <input
              v-model="addPositionForm.purchaseDate"
              type="date"
              required
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700">Notes</label>
            <textarea
              v-model="addPositionForm.notes"
              rows="3"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
              placeholder="Add any notes about this position..."
            ></textarea>
          </div>

          <div class="flex justify-end space-x-3">
            <button
              type="button"
              @click="showAddPositionModal = false"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md"
            >
              Cancel
            </button>
            <button
              type="submit"
              class="px-4 py-2 text-sm font-medium text-white bg-green-600 hover:bg-green-700 rounded-md"
            >
              Add Position
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Sell Position Modal -->
    <div v-if="showSellModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-lg shadow-xl p-6 w-full max-w-md">
        <h2 class="text-xl font-bold mb-4">Sell Position</h2>
        
        <div v-if="selectedStock" class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <p class="text-sm text-gray-500">Symbol</p>
              <p class="text-lg font-semibold">{{ selectedStock.symbol }}</p>
            </div>
            <div>
              <p class="text-sm text-gray-500">Current Price</p>
              <p class="text-lg font-semibold">{{ formatCurrency(selectedStock.price) }}</p>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <p class="text-sm text-gray-500">Shares Owned</p>
              <p class="text-lg font-semibold">{{ selectedStock.position.quantity }}</p>
            </div>
            <div>
              <p class="text-sm text-gray-500">Total Value</p>
              <p class="text-lg font-semibold">{{ formatCurrency(selectedStock.position.quantity * selectedStock.price) }}</p>
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700">Quantity to Sell</label>
            <input
              v-model="sellQuantity"
              type="number"
              step="0.0001"
              required
              :max="selectedStock.position.quantity"
              min="0.0001"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>

          <div class="bg-gray-50 p-4 rounded-lg">
            <p class="text-sm text-gray-500">Sale Summary</p>
            <p class="text-lg font-semibold">{{ formatCurrency(sellQuantity * selectedStock.price) }}</p>
          </div>

          <div class="flex justify-end space-x-3">
            <button
              type="button"
              @click="showSellModal = false"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md"
            >
              Cancel
            </button>
            <button
              @click="sellPosition"
              :disabled="!sellQuantity || sellQuantity <= 0 || sellQuantity > selectedStock.position.quantity"
              class="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-md disabled:opacity-50"
            >
              Sell Shares
            </button>
          </div>
        </div>
      </div>
    </div>

    </div>
  </div>
</template>