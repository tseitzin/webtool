<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { coinbaseService } from '../services/coinbaseService'
import { useCollapsibleSection } from '../composables/useCollapsibleSection'
import CollapsibleSectionHeader from '../components/CollapsibleSectionHeader.vue'
import CryptoSearchResult from '../components/CryptoSearchResult.vue'
import AddToCryptoPortfolioModal from '../components/AddToCryptoPortfolioModal.vue'
import RemoveFromCryptoPortfolioModal from '../components/RemoveFromCryptoPortfolioModal.vue'
import type { CryptoData } from '../types/crypto'
import { cryptoService } from '../services/cryptoService'
import { formatCurrency, formatNumber, formatPercent } from '../utils/formatters'
import api from '../api/axios'
import { showErrorToast } from '../utils/toast'
import { useCryptoSearchHistoryStore } from '../stores/cryptoSearchHistory'
const cryptoSearchHistoryStore = useCryptoSearchHistoryStore()

interface CryptoPortfolio {
  id: number
  symbol: string
  quantity: number
  purchasePrice: number
  currentValue: number
  totalCost: number
}

const router = useRouter()
const auth = useAuthStore()
const savedCryptos = ref<CryptoData[]>([])
const ownedCryptos = ref<CryptoPortfolio[]>([])
const selectedCrypto = ref<CryptoData | null>(null)
const showPortfolioModal = ref(false)
const selectedCryptoForPortfolio = ref<CryptoData | null>(null)
const showRemovePortfolioModal = ref(false)
const error = ref('')
const loading = ref(false)
const searchSymbol = ref('')
const showRemoveModal = ref(false)
const cryptoToRemove = ref<string | null>(null)

const { isExpanded: isIntroExpanded, toggleSection: toggleIntro } = 
  useCollapsibleSection('crypto_intro')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/access-denied')
  }
  await Promise.all([
    fetchSavedCryptos(),
    fetchOwnedCryptos()
  ])
})

const fetchSavedCryptos = async () => {
  try {
    const cryptos = await cryptoService.getSavedCryptos()
    // Sort cryptos alphabetically by symbol
    savedCryptos.value = cryptos.sort((a, b) => a.symbol.localeCompare(b.symbol))
  } catch (e) {
    console.error('Failed to fetch saved cryptos:', e)
  }
}

const searchCrypto = async () => {
  if (!searchSymbol.value) return
  
  loading.value = true
  error.value = ''
  try {
    const crypto = await coinbaseService.getCryptoData(searchSymbol.value.toUpperCase())
    selectedCrypto.value = crypto
    cryptoSearchHistoryStore.addToHistory(crypto)
  } catch (e: any) {
    error.value = 'Cryptocurrency not found'
    selectedCrypto.value = null
  } finally {
    loading.value = false
  }
}

const openPortfolioModal = (crypto: CryptoData) => {
  selectedCryptoForPortfolio.value = crypto
  showPortfolioModal.value = true
}

const clearSearch = () => {
  searchSymbol.value = ''
  selectedCrypto.value = null
  error.value = ''
}

const clearSearchHistory = () => {
  cryptoSearchHistoryStore.clearHistory()
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
    // Sort after modifying the list
    savedCryptos.value.sort((a, b) => a.symbol.localeCompare(b.symbol))
  } catch (e) {
    console.error('Failed to toggle saved crypto:', e)
    error.value = 'Failed to update watchlist'
  }
}

const handleRemoveCrypto = (symbol: string) => {
  // Check if crypto is in portfolio
  const isInPortfolio = ownedCryptos.value.some(crypto => crypto.symbol === symbol)
  if (isInPortfolio) {
    showErrorToast(`Cannot remove ${symbol} from watchlist while it is in your portfolio`, 3000)
    return
  }

  cryptoToRemove.value = symbol
  showRemoveModal.value = true
}

const handlePortfolioSuccess = async () => {
  await refreshData()
}

const getOwnershipInfo = (symbol: string) => {
  const owned = ownedCryptos.value.find(crypto => crypto.symbol === symbol)
  if (!owned) return null
  
  return {
    quantity: owned.quantity,
    value: owned.currentValue
  }
}

const fetchOwnedCryptos = async () => {
  try {
    const response = await api.get('/cryptoportfolio')
    // Sort owned cryptos alphabetically
    ownedCryptos.value = response.data.sort((a: CryptoPortfolio, b: CryptoPortfolio) => 
      a.symbol.localeCompare(b.symbol))
  } catch (e) {
    console.error('Failed to fetch owned cryptos:', e)
  }
}

const openRemovePortfolioModal = (crypto: CryptoData) => {
  selectedCryptoForPortfolio.value = crypto
  showRemovePortfolioModal.value = true
}

const closeRemovePortfolioModal = () => {
  showRemovePortfolioModal.value = false
  selectedCryptoForPortfolio.value = null
  refreshData()
}

const closePortfolioModal = () => {
  showPortfolioModal.value = false
  selectedCryptoForPortfolio.value = null
  refreshData()
}


const refreshData = async () => {
  await Promise.all([
    fetchSavedCryptos(),
    fetchOwnedCryptos()
  ])
}

const isCryptoSaved = (symbol: string): boolean => {
  return savedCryptos.value.some(c => c.symbol === symbol)
}

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}

const handleConfirmRemoval = async () => {
  if (cryptoToRemove.value) {
    try {
      await cryptoService.removeSavedCrypto(cryptoToRemove.value)
      savedCryptos.value = savedCryptos.value
        .filter(c => c.symbol !== cryptoToRemove.value)
        .sort((a, b) => a.symbol.localeCompare(b.symbol))
    } catch (e) {
      console.error('Failed to remove crypto:', e)
    }
  }
  cancelRemoval()
}

const cancelRemoval = () => {
  showRemoveModal.value = false
  cryptoToRemove.value = null
}

</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-4 sm:py-8">
      <!-- Introduction Section -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden mb-4 sm:mb-6">
        <CollapsibleSectionHeader
          title="Cryptocurrency Search & Tracking"
          :is-expanded="isIntroExpanded"
          @toggle="toggleIntro"
        />
        
        <div
          v-show="isIntroExpanded"
          class="p-4 sm:p-6 transition-all duration-300 ease-in-out bg-indigo-100 rounded-xl"
        >
          <p class="mb-4 font-bold text-sm sm:text-base">
            This is where you can search for details on any cryptocurrencies traded in the US markets.
          </p>
          <p class="mb-4 text-sm sm:text-base">
            Use this area to search for and track your favorite cryptocurrencies:
          </p>
          <ul class="list-disc list-inside space-y-2 ml-4 text-sm sm:text-base">
            <li>Enter a cryptocurrency symbol (e.g., BTC, ETH) to view current market data</li>
            <li>Save cryptocurrencies to your watchlist for quick access</li>
            <li>Monitor price changes and market trends</li>
            <li>Track 24-hour trading volumes and price ranges</li>
          </ul>
        </div>
      </div>

      <!-- Search Section -->
      <div class="bg-white rounded-lg shadow-md p-4 sm:p-6 mb-4 sm:mb-6">
        <div class="flex flex-col sm:flex-row gap-4">
          <div class="relative w-full sm:w-48">
            <input
              v-model="searchSymbol"
              type="text"
              placeholder="Enter symbol"
              class="w-full px-3 sm:px-4 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-sm sm:text-base uppercase"
              @keyup.enter="searchCrypto"
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
          <button
            @click="searchCrypto"
            class="w-full sm:w-auto px-4 py-2 bg-indigo-600 text-white text-sm rounded-lg hover:bg-indigo-800 transition-colors"
          >
            Search
          </button>
        </div>

        <!-- Recent Searches -->
        <div v-if="cryptoSearchHistoryStore.searchHistory.length > 0" class="mt-4 mb-3">
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
              v-for="crypto in cryptoSearchHistoryStore.searchHistory"
              :key="crypto.symbol"
              @click="() => { searchSymbol = crypto.symbol; searchCrypto(); }"
              class="px-3 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded-md transition-colors flex items-center gap-2"
            >
              <span>{{ crypto.symbol }}</span>
              <span :class="crypto.change >= 0 ? 'text-green-600' : 'text-red-600'">
                {{ formatPercent(crypto.changePercent) }}
              </span>
            </button>
          </div>
        </div>

        <!-- Error Message -->
        <div 
          v-if="error" 
          class="mt-4 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded text-sm sm:text-base"
        >
          {{ error }}
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="mt-6 bg-white rounded-lg shadow-md p-4 sm:p-8">
          <h1 class="font-bold text-xl sm:text-2xl text-center">Loading Crypto Data</h1>
          <LoadingSpinner size="lg" />
        </div>

        <!-- Search Results -->
        <CryptoSearchResult
          v-if="selectedCrypto"
          :crypto="selectedCrypto"
          :is-saved="isCryptoSaved(selectedCrypto.symbol)"
          @toggle-save="toggleSavedCrypto"
        />
      </div>

      <!-- Saved Cryptocurrencies -->
      <div v-if="savedCryptos.length > 0" class="mt-6 sm:mt-8">
        <h2 class="text-lg sm:text-2xl font-bold mb-4">Your Watchlisted Crypto</h2>
        <div class="space-y-3">
          <div class="grid grid-cols-1 gap-4">
            <div
              v-for="crypto in savedCryptos"
              :key="crypto.symbol"
              class="bg-white rounded-lg shadow-lg hover:shadow-xl transition-shadow p-4"
            >
              <!-- Crypto Header -->
              <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-4">
                <h3 class="sm:text-lg font-bold mb-2 sm:mb-0">{{ crypto.symbol }}</h3>
                <div class="flex gap-2">
                  <button
                    @click="openPortfolioModal(crypto)"
                    class="px-4 py-2 bg-green-600 text-xs text-white rounded-lg hover:bg-green-800 transition-colors"
                  >
                    Buy Crypto
                  </button>
                  <button
                    v-if="getOwnershipInfo(crypto.symbol)"
                    @click="openRemovePortfolioModal(crypto)"
                    class="px-4 py-2 bg-red-600 text-xs text-white rounded-lg hover:bg-red-800 transition-colors"
                  >
                    Sell Crypto
                  </button>
                  <button
                    @click="handleRemoveCrypto(crypto.symbol)"
                    class="px-4 py-2 bg-gray-600 text-xs text-white rounded-lg hover:bg-gray-800 transition-colors"
                  >
                    Remove
                  </button>
                </div>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <div class="bg-gray-100 p-2 rounded-lg">
                  <p class="text-sm text-gray-500">Current Price</p>
                  <p class="text-lg font-semibold">{{ formatCurrency(Number(crypto.price)) }}</p>
                </div>
                <div class="bg-gray-100 p-2 rounded-lg">
                  <p class="text-sm text-gray-500">24h Change</p>
                  <p :class="['text-lg font-semibold', crypto.change >= 0 ? 'text-green-600' : 'text-red-600']">
                    {{ formatChange(crypto.change, crypto.changePercent) }}
                  </p>
                </div>
                <div class="bg-gray-100 p-2 rounded-lg">
                  <p class="text-sm text-gray-500">Volume</p>
                  <p class="text-lg font-semibold">{{ formatNumber(crypto.volume) }}</p>
                </div>
                <div class="bg-gray-100 p-2 rounded-lg">
                  <p class="text-sm text-gray-500">Opening Price</p>
                  <p class="text-lg font-semibold">{{ formatCurrency(Number(crypto.open)) }}</p>
                </div>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mt-2">
                <div class="bg-gray-100 p-2 rounded-lg">
                  <p class="text-sm text-gray-500">24h High</p>
                  <p class="text-lg font-semibold">{{ formatCurrency(crypto.high24h) }}</p>
                </div>
                <div class="bg-gray-100 p-2 rounded-lg">
                  <p class="text-sm text-gray-500">24h Low</p>
                  <p class="text-lg font-semibold">{{ formatCurrency(crypto.low24h) }}</p>
                </div>
              </div>

              <div 
                v-if="getOwnershipInfo(crypto.symbol)"
                class="mt-4 bg-blue-50 p-3 rounded-lg"
              >
                <div class="flex">
                  <div>
                    <span class="font-medium text-blue-800">Amount in Portfolio:</span>
                    <span class="ml-2 text-blue-700">
                      {{ formatNumber(getOwnershipInfo(crypto.symbol)?.quantity || 0) }} {{ crypto.symbol }}
                    </span>
                  </div>
                  <div>
                    <span class="font-medium text-blue-800 ml-6">Total Value:</span>
                    <span class="ml-2 text-blue-700">
                      {{ formatCurrency((getOwnershipInfo(crypto.symbol)?.value || 0)) }}
                    </span>
                  </div>
                </div>
              </div>             
            </div>
          </div>
        </div>
      </div>

      

      <!-- No Saved Cryptos Message -->
      <div v-else class="mt-6 bg-white rounded-lg shadow-md p-4 sm:p-8 text-center">
        <h3 class="text-lg sm:text-xl font-semibold mb-4">No Saved Cryptocurrencies</h3>
        <p class="text-gray-600 mb-6 text-sm sm:text-base">Start building your watchlist by searching and adding cryptocurrencies.</p>
      </div>
    </div>

    <AddToCryptoPortfolioModal
        v-if="showPortfolioModal && selectedCryptoForPortfolio"
        :is-open="showPortfolioModal"
        :symbol="selectedCryptoForPortfolio.symbol"
        :price="Number(selectedCryptoForPortfolio.price)"
        @close="closePortfolioModal"
        @success="handlePortfolioSuccess"
      />

      <RemoveFromCryptoPortfolioModal
        v-if="selectedCryptoForPortfolio"
        :is-open="showRemovePortfolioModal"
        :symbol="selectedCryptoForPortfolio.symbol"
        :price="Number(selectedCryptoForPortfolio.price)"
        @close="closeRemovePortfolioModal"
        @success="handlePortfolioSuccess"
      />

      <ConfirmationModal
        :is-open="showRemoveModal"
        title="Remove Crypto"
        message="Are you sure you want to remove this crypto from your watchlist?"
        @confirm="handleConfirmRemoval"
        @cancel="cancelRemoval"
      />

  </div>
</template>