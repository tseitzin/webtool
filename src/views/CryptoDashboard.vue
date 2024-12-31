<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { cryptoService } from '../services/cryptoService'
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { CryptoData } from '../types/crypto'

const router = useRouter()
const auth = useAuthStore()
const savedCryptos = ref<CryptoData[]>([])
//const selectedCrypto = ref<CryptoData | null>(null)

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/login')
    return
  }
  await fetchSavedCryptos()
})

const navigateToCryptoSearch = () => {
  router.push('/crypto')
}

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}

const toggleSavedCrypto = async (symbol: string) => {
  try {
    await cryptoService.removeSavedCrypto(symbol)
    savedCryptos.value = savedCryptos.value.filter(c => c.symbol !== symbol)
  } catch (e) {
    console.error('Failed to remove crypto:', e)
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
        <!-- Saved Cryptocurrencies -->
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
  
        <!-- Saved Cryptocurrencies List -->
        <div v-else class="mt-6 space-y-4">
          <div class="flex flex-row justify-between items-center mb-4">
            <h2 class="text-xl font-bold">Your Crypto Watchlist</h2>
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
  </template>