<script setup lang="ts">
import { ref, onMounted, watchEffect } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { stockService } from '../services/stockService'
import { dashboardService } from '../services/dashboardService'
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { StockData } from '../types/polygon'
import CollapsibleSectionHeader from '../components/CollapsibleSectionHeader.vue'
import { useCollapsibleSection } from '../composables/useCollapsibleSection'
import { CryptoData } from '../types/crypto'
import { cryptoService } from '../services/cryptoService'

const router = useRouter()
const auth = useAuthStore()
const savedStocks = ref<StockData[]>([])
const savedCryptos = ref<CryptoData[]>([])
const selectedCrypto = ref<CryptoData | null>(null)
const errorStock = ref('')
const loading = ref(false)

const { isExpanded: isWelcomeExpanded, toggleSection: toggleWelcome } = 
  useCollapsibleSection('dashboard_welcome')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/login')
    return
  }
  await fetchSavedStocks()
  await fetchSavedCryptos()
})

// Cleanup on component unmount
watchEffect((onCleanup) => {
  onCleanup(() => {
    dashboardService.stopAutoRefresh()
  })
})

const fetchSavedStocks = async () => {
  loading.value = true
  errorStock.value = ''
  try {
    const stocks = await stockService.getSavedStocks()
    savedStocks.value = stocks
    
    // Start auto-refresh after initial fetch
    dashboardService.startAutoRefresh(stocks, (updatedStocks) => {
      savedStocks.value = updatedStocks
    })
  } catch (e: any) {
    errorStock.value = 'Failed to load saved stocks'
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
    errorStock.value = 'Failed to remove stock from saved list'
  }
}

const navigateToSearch = () => {
  router.push('/search-area')
}

const navigateToCryptoSearch = () => {
  router.push('/crypto')
}

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}

const navigateToResearch = (symbol: string) => {
  router.push(`/research/${symbol}`)
}

const toggleSavedCrypto = async (symbol: string) => {
  try {
    const isCurrentlySaved = savedCryptos.value.some(c => c.symbol === symbol)
    
    if (isCurrentlySaved) {
      await cryptoService.removeSavedCrypto(symbol)
      savedCryptos.value = savedCryptos.value.filter(c => c.symbol !== symbol)
    } else if (selectedCrypto.value) {
      await cryptoService.saveCrypto(selectedCrypto.value)
      savedCryptos.value = [...savedCryptos.value, selectedCrypto.value]
    }
  } catch (e) {
    console.error('Failed to toggle saved crypto:', e)
    errorStock.value = 'Failed to update watchlist'
  }
}

const fetchSavedCryptos = async () => {
  try {
    savedCryptos.value = await cryptoService.getSavedCryptos()
  } catch (e) {
    console.error('Failed to fetch saved cryptos:', e)
  }
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-4">

      <!-- Welcome Section -->
      <div class="bg-gray-50 rounded-lg shadow-lg overflow-hidden mb-10">
        <CollapsibleSectionHeader
          :title="`Welcome back, ${auth.user?.name}!`"
          :is-expanded="isWelcomeExpanded"
          @toggle="toggleWelcome"
        />
        
        <div
          v-show="isWelcomeExpanded"
          class="ml-4 mr-4 mb-4 p-6 transition-all duration-300 ease-in-out bg-indigo-100 rounded-xl"
        >
          <p class="mb-4 font-bold">
          This is your personalized dashboard where you can monitor your portfolio and track your favorite stocks and cryptocurrenices in real-time.
          </p>
          <p class="mb-4">
            Here you can:
          </p>
          <ul class="list-disc list-inside space-y-2 ml-4">
            <li>View your saved stocks and crypto to see their current performance</li>
            <li>Track your total portfolio of stocks and crypto and monitor changes</li>
            <li>Get quick access to detailed stock and crypto information</li>
            <li>Monitor market trends and stock movements</li>
          </ul>
        </div>
      </div>


      <!-- Error Message -->
      <div v-if="errorStock" class="mb-6 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        {{ errorStock }}
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- Saved Stocks Section -->
      <div class="mb-8">       

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
          <div class="flex flex-row justify-between items-center mb-4">
          <h2 class="text-xl font-bold">Your Stocks Watchlist</h2>
          <button
            @click="navigateToSearch"
            class="px-4 py-2 bg-indigo-600 text-sm text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Search Stocks
          </button>
        </div>
          <div
            v-for="stock in savedStocks"
            :key="stock.symbol"
            class="bg-white rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow"
          >
            <div class="flex justify-between items-center">
              <h3 class="text-lg font-bold">{{ stock.symbol }} - {{ stock.companyName }}</h3>
              <button
                @click="removeSavedStock(stock.symbol)"
                class="text-red-600 hover:text-red-800 transition-colors"
              >
                Remove
              </button>
            </div>

            <div class="grid grid-cols-2 sm:grid-cols-4 gap-4 ml-4 mt-1">
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

            <div class="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-1 ml-4 text-sm text-gray-600">
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

        <!-- No Saved Crypto Message -->
        <div v-if="savedCryptos.length === 0" class="mt-6 bg-white rounded-lg shadow-md p-8 text-center">
          <h3 class="text-xl font-semibold mb-4">No Saved Crypto Yet</h3>
          <p class="text-gray-600 mb-6">Start building your watchlist by adding crypto you want to track.</p>
          <button
            @click="navigateToCryptoSearch"
            class="px-6 py-3 bg-orange-600 text-white rounded-lg hover:bg-orange-700 transition-colors"
          >
            Search and Add Crypto
          </button>
        </div>

        <!-- Saved Cryptocurrencies -->
      <div v-else class="mt-12">
        <div class="flex flex-row justify-between items-center mb-4">
          <h2 class="text-xl font-bold">Your Cryptocurrency Watchlist</h2>
          <button
            @click="navigateToCryptoSearch"
            class="px-4 py-2 bg-orange-600 text-sm text-white rounded-lg hover:bg-orange-700 transition-colors"
          >
            Search Crypto
          </button>
        </div>
        <div class="space-y-3">
          <div class="grid grid-cols-1 gap-4">
            <div
              v-for="crypto in savedCryptos"
              :key="crypto.symbol"
              :crypto="crypto"
              :is-saved="true"
              @toggle-save="toggleSavedCrypto"
              class="bg-white rounded-lg shadow-md p-4 hover:shadow-lg transition-shadow"
              >
                <div class="flex justify-between items-center">
                  <h3 class="text-lg font-bold">{{ crypto.symbol }}</h3>
                  <button
                    @click="toggleSavedCrypto(crypto.symbol)"
                    class="text-red-600 hover:text-red-800 transition-colors"
                  >
                    Remove
                  </button>
                </div>

                <div class="grid grid-cols-2 sm:grid-cols-3 gap-4 ml-4 mt-1">
                
                  <div class="w-24">
                    <p class="text-sm text-gray-600">Current Price</p>
                    <h3 class="text-lg font-semibold">{{ formatCurrency(Number(crypto.price)) }}</h3>
                  </div>
                  <div class="w-48">
                    <p class="text-sm text-gray-500">24 Hour Change</p>
                    <p :class="['text-lg font-semibold', crypto.change >= 0 ? 'text-green-600' : 'text-red-600']">
                        {{ formatChange(crypto.change, crypto.changePercent) }}
                    </p>
                  </div>
                  <div class="w-32">
                    <p class="text-sm text-gray-500">Volume</p>
                    <h3 class="text-lg font-semibold">{{ formatNumber(crypto.volume) }}</h3>
                  </div>
                  
                </div>

                <div class="grid grid-cols-2 sm:grid-cols-3 gap-4 mt-1 ml-4 text-sm text-gray-600">
                  <div>
                    <span class="text-gray-500">Today's Opening:</span>
                    <span class="ml-1">{{ formatCurrency(Number(crypto.open)) }}</span>
                  </div>
                  <div>
                    <span class="text-gray-500">24 Hour High:</span>
                    <span class="ml-1">{{ crypto.high24h ? formatCurrency(Number(crypto.high24h)) : 'N/A' }}</span>
                  </div>
                  <div>
                    <span class="text-gray-500">24 Hour Low:</span>
                    <span class="ml-1">{{ crypto.low24h ? formatCurrency(Number(crypto.low24h)) : 'N/A' }}</span>
                  </div>
                </div>






            </div>
          </div>
        </div>
      </div>

      </div>
    </div>
  </div>
</template>