<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { cryptoService } from '../services/cryptoService'
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { CryptoData } from '../types/crypto'
import { useCollapsibleSection } from '../composables/useCollapsibleSection'
import CollapsibleSectionHeader from '../components/CollapsibleSectionHeader.vue'
import AddToCryptoPortfolioModal from '../components/AddToCryptoPortfolioModal.vue'
import RemoveFromCryptoPortfolioModal from '../components/RemoveFromCryptoPortfolioModal.vue'
import api from '../api/axios'
import { showErrorToast } from '../utils/toast'

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
const error = ref('')
const loading = ref(false)
const showPortfolioModal = ref(false)
const showRemovePortfolioModal = ref(false)
const selectedCryptoForPortfolio = ref<CryptoData | null>(null)
const ownedCryptos = ref<CryptoPortfolio[]>([])
const showRemoveModal = ref(false)
const cryptoToRemove = ref<string | null>(null)

const { isExpanded: isIntroExpanded, toggleSection: toggleIntro } = 
  useCollapsibleSection('crypto_dashboard_intro')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/login')
    return
  }
  await Promise.all([
    fetchSavedCryptos(),
    fetchOwnedCryptos()
  ])
})

const navigateToCryptoSearch = () => {
  router.push('/crypto')
}

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
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

const fetchSavedCryptos = async () => {
  loading.value = true
  try {
    const cryptos = await cryptoService.getSavedCryptos()
    // Sort cryptos alphabetically by symbol
    savedCryptos.value = cryptos.sort((a, b) => a.symbol.localeCompare(b.symbol))
  } catch (e) {
    console.error('Failed to fetch saved cryptos:', e)
    error.value = 'Failed to load saved cryptocurrencies'
  } finally {
    loading.value = false
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

const getOwnershipInfo = (symbol: string) => {
  const owned = ownedCryptos.value.find(crypto => crypto.symbol === symbol)
  if (!owned) return null
  
  return {
    quantity: owned.quantity,
    value: owned.currentValue
  }
}

const openPortfolioModal = (crypto: CryptoData) => {
  selectedCryptoForPortfolio.value = crypto
  showPortfolioModal.value = true
}

const closePortfolioModal = () => {
  showPortfolioModal.value = false
  selectedCryptoForPortfolio.value = null
  refreshData()
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

const handlePortfolioSuccess = async () => {
  await refreshData()
}

const refreshData = async () => {
  await Promise.all([
    fetchSavedCryptos(),
    fetchOwnedCryptos()
  ])
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-4">
      <!-- Introduction Section -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden mb-6">
        <CollapsibleSectionHeader
          title="Crypto Dashboard Overview"
          :is-expanded="isIntroExpanded"
          @toggle="toggleIntro"
        />
        
        <div
          v-show="isIntroExpanded"
          class="p-4 sm:p-6 transition-all duration-300 ease-in-out bg-orange-100 rounded-xl"
        >
          <p class="mb-4 font-bold text-sm sm:text-base">
            Welcome to your Cryptocurrency Dashboard - your central hub for monitoring digital assets.
          </p>
          <p class="mb-4 text-sm sm:text-base">
            This dashboard provides comprehensive features for tracking your cryptocurrency investments:
          </p>
          <ul class="list-disc list-inside space-y-2 ml-4 text-sm sm:text-base">
            <li>Monitor real-time cryptocurrency prices and market data</li>
            <li>Track key metrics including 24-hour price changes and trading volume</li>
            <li>View detailed price statistics including daily highs and lows</li>
            <li>Easily manage your cryptocurrency watchlist</li>
            <li>Buy and sell cryptocurrencies in your portfolio</li>
            <li>Track your investment performance</li>
          </ul>
          <p class="mt-4 text-sm sm:text-base italic">
            Note: Cryptocurrency data updates automatically to provide you with the latest market information.
          </p>
        </div>
      </div>

      <!-- Error Message -->
      <div v-if="error" class="mb-6 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        {{ error }}
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <LoadingSpinner size="lg" />
      </div>

      <!-- Saved Cryptocurrencies -->
      <div v-else-if="savedCryptos.length === 0" class="mt-6 bg-white rounded-lg shadow-md p-8 text-center">
        <h3 class="text-xl font-semibold mb-4">No Saved Cryptocurrencies</h3>
        <p class="text-gray-600 mb-6">Start building your watchlist by adding cryptocurrencies you want to track.</p>
        <button
          @click="navigateToCryptoSearch"
            class="px-4 py-2 bg-indigo-600 text-xs text-white rounded-lg hover:bg-indigo-700 transition-colors"
        >
          Search and Add Crypto
        </button>
      </div>

      <!-- Saved Cryptocurrencies List -->
      <div v-else class="mt-6 space-y-4">
        <div class="flex flex-row justify-between items-center mb-4">
          <h2 class="text-xl font-bold">Your Crypto Watchlist</h2>
          <button
            @click="navigateToCryptoSearch"
            class="px-4 py-2 bg-indigo-600 text-xs text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Search Crypto
          </button>
        </div>
        
        <div class="space-y-3">
          <div class="grid grid-cols-1 gap-4">
            <div
              v-for="crypto in savedCryptos"
              :key="crypto.symbol"
              class="bg-white rounded-lg shadow-lg hover:shadow-xl transition-shadow p-4"
            >
              <div class="flex justify-between items-center mb-2">
                <h3 class="text-lg font-bold">{{ crypto.symbol }}</h3>
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

              <div 
                v-else 
                class="mt-4 bg-gray-50 p-3 rounded-lg"
              >
                <p class="font-semibold text-gray-600">Not currently in portfolio</p>
              </div>


            </div>
          </div>
        </div>
        
      </div>

      <!-- Modals -->
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
  </div>
</template>