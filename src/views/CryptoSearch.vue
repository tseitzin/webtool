<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { coinbaseService } from '../services/coinbaseService'
import { useCollapsibleSection } from '../composables/useCollapsibleSection'
import CollapsibleSectionHeader from '../components/CollapsibleSectionHeader.vue'
import CryptoSearchResult from '../components/CryptoSearchResult.vue'
import type { CryptoData } from '../types/crypto'
import { cryptoService } from '../services/cryptoService'
import { formatCurrency, formatNumber, formatPercent } from '../utils/formatters'

const router = useRouter()
const auth = useAuthStore()
const savedCryptos = ref<CryptoData[]>([])
const selectedCrypto = ref<CryptoData | null>(null)
const error = ref('')
const loading = ref(false)
const searchSymbol = ref('')

const { isExpanded: isIntroExpanded, toggleSection: toggleIntro } = 
  useCollapsibleSection('crypto_intro')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/access-denied')
  }
  await fetchSavedCryptos()
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
  } catch (e: any) {
    error.value = 'Cryptocurrency not found'
    selectedCrypto.value = null
  } finally {
    loading.value = false
  }
}

const clearSearch = () => {
  searchSymbol.value = ''
  selectedCrypto.value = null
  error.value = ''
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

const isCryptoSaved = (symbol: string): boolean => {
  return savedCryptos.value.some(c => c.symbol === symbol)
}

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
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
            class="w-full sm:w-auto px-4 py-2 bg-indigo-600 text-white text-sm rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Search
          </button>
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
                <button
                  @click="toggleSavedCrypto(crypto.symbol)"
                  class="w-full sm:w-auto px-4 py-2 bg-red-600 text-white text-sm rounded-lg hover:bg-red-700 transition-colors"
                >
                  Remove from Watchlist
                </button>
              </div>

              <!-- Crypto Details Grid -->
              <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <!-- Price Info -->
                <div class="bg-gray-50 p-3 rounded-lg">
                  <p class="text-sm text-gray-600">Current Price</p>
                  <p class="text-base sm:text-lg font-semibold">{{ formatCurrency(Number(crypto.price)) }}</p>
                </div>

                <!-- Change Info -->
                <div class="bg-gray-50 p-3 rounded-lg">
                  <p class="text-sm text-gray-600">24h Change</p>
                  <p :class="['text-base sm:text-lg font-semibold', crypto.change >= 0 ? 'text-green-600' : 'text-red-600']">
                    {{ formatChange(crypto.change, crypto.changePercent) }}
                  </p>
                </div>

                <!-- Volume Info -->
                <div class="bg-gray-50 p-3 rounded-lg">
                  <p class="text-sm text-gray-600">Volume</p>
                  <p class="text-base sm:text-lg font-semibold">{{ formatNumber(crypto.volume) }}</p>
                </div>

                <!-- High/Low Info -->
                <div class="bg-gray-50 p-3 rounded-lg">
                  <p class="text-sm text-gray-600">24h High/Low</p>
                  <p class="text-base sm:text-lg font-semibold">
                    {{ formatCurrency(crypto.high24h) }} / {{ formatCurrency(crypto.low24h) }}
                  </p>
                </div>
              </div>

              <!-- Additional Details -->
              <div class="mt-4 grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div class="bg-gray-50 p-3 rounded-lg">
                  <p class="text-sm text-gray-600">Opening Price</p>
                  <p class="text-base font-semibold">{{ formatCurrency(Number(crypto.open)) }}</p>
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
  </div>
</template>