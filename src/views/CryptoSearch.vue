<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { coinbaseService } from '../services/coinbaseService'
import { useCollapsibleSection } from '../composables/useCollapsibleSection'
import CollapsibleSectionHeader from '../components/CollapsibleSectionHeader.vue'
import CryptoSearchResult from '../components/CryptoSearchResult.vue'
import type { CryptoData } from '../types/crypto'

const router = useRouter()
const auth = useAuthStore()
const savedCryptos = ref<CryptoData[]>([])
const selectedCrypto = ref<CryptoData | null>(null)
const error = ref('')
const loading = ref(false)
const searchSymbol = ref('')

const { isExpanded: isIntroExpanded, toggleSection: toggleIntro } = 
  useCollapsibleSection('crypto_intro')

onMounted(() => {
  if (!auth.isAuthenticated) {
    router.push('/access-denied')
  }
})

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
  const isCurrentlySaved = savedCryptos.value.some(c => c.symbol === symbol)
  
  if (isCurrentlySaved) {
    savedCryptos.value = savedCryptos.value.filter(c => c.symbol !== symbol)
  } else if (selectedCrypto.value) {
    savedCryptos.value = [...savedCryptos.value, selectedCrypto.value]
  }
}

const isCryptoSaved = (symbol: string): boolean => {
  return savedCryptos.value.some(c => c.symbol === symbol)
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-8">
      <!-- Introduction Section -->
      <div class="bg-white rounded-lg shadow-lg overflow-hidden mb-6">
        <CollapsibleSectionHeader
          title="Cryptocurrency Search & Tracking"
          :is-expanded="isIntroExpanded"
          @toggle="toggleIntro"
        />
        
        <div
          v-show="isIntroExpanded"
          class="p-6 transition-all duration-300 ease-in-out"
        >
          <p class="text-gray-600 mb-4">
            Use this area to search for and track your favorite cryptocurrencies:
          </p>
          <ul class="list-disc list-inside text-gray-600 space-y-2 ml-4">
            <li>Enter a cryptocurrency symbol (e.g., BTC, ETH) to view current market data</li>
            <li>Save cryptocurrencies to your watchlist for quick access</li>
            <li>Monitor price changes and market trends</li>
            <li>Track 24-hour trading volumes and price ranges</li>
          </ul>
        </div>
      </div>

      <!-- Search Section -->
      <div class="bg-white rounded-lg shadow-md p-6 mb-6">
        <div class="flex flex-col sm:flex-row gap-4">
          <div class="flex-1 relative">
            <input
              v-model="searchSymbol"
              type="text"
              placeholder="Enter crypto symbol (e.g., BTC)..."
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              @keyup.enter="searchCrypto"
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
            @click="searchCrypto"
            class="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Search
          </button>
        </div>

        <!-- Error Message -->
        <div 
          v-if="error" 
          class="mt-4 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded"
        >
          {{ error }}
        </div>

        <!-- Loading State -->
        <div 
          v-if="loading" 
          class="mt-4 flex justify-center"
        >
          <div class="animate-spin rounded-full h-8 w-8 border-4 border-indigo-500 border-t-transparent"></div>
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
      <div v-if="savedCryptos.length > 0" class="bg-white rounded-lg shadow-md p-6">
        <h2 class="text-xl font-semibold mb-4">Your Watchlist</h2>
        <div class="space-y-4">
          <CryptoSearchResult
            v-for="crypto in savedCryptos"
            :key="crypto.symbol"
            :crypto="crypto"
            :is-saved="true"
            @toggle-save="toggleSavedCrypto"
          />
        </div>
      </div>
    </div>
  </div>
</template>