<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import api from '../api/axios'
import type { UserOwnedStock } from '../types/portfolio'
import PortfolioSummaryTable from '../components/PortfolioSummaryTable.vue'
import CryptoPortfolioSummaryTable from '../components/CryptoPortfolioSummaryTable.vue'
import { formatCurrency } from '../utils/formatters'

interface CryptoPosition {
  id: number
  symbol: string
  quantity: number
  averagePurchasePrice: number
  currentValue: number
  gainLoss?: number
  gainLossPercent?: number
}

const router = useRouter()
const auth = useAuthStore()
const stockPositions = ref<UserOwnedStock[]>([])
const cryptoPositions = ref<CryptoPosition[]>([])
const loading = ref(true)
const error = ref('')

const stockValue = ref(0)
const cryptoValue = ref(0)

const totalPortfolioValue = computed(() => stockValue.value + cryptoValue.value)

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/login')
    return
  }
  await Promise.all([
    fetchStockPortfolio(),
    fetchCryptoPortfolio()
  ])
})

const fetchStockPortfolio = async () => {
  try {
    const response = await api.get('/portfolio')
    stockPositions.value = response.data
    calculateStockValue()
  } catch (e) {
    error.value = 'Failed to load stock portfolio'
    console.error('Error:', e)
  } finally {
    loading.value = false
  }
}

const fetchCryptoPortfolio = async () => {
  try {
    const response = await api.get('/cryptoportfolio')
    cryptoPositions.value = response.data
    calculateCryptoValue()
  } catch (e) {
    error.value = 'Failed to load crypto portfolio'
    console.error('Error:', e)
  } finally {
    loading.value = false
  }
}

const calculateStockValue = () => {
  stockValue.value = stockPositions.value.reduce((sum, position) => 
    sum + position.currentValue, 0)
}

const calculateCryptoValue = () => {
  cryptoValue.value = cryptoPositions.value.reduce((sum, position) => 
    sum + position.currentValue, 0)
}

const updateStockValue = (newValue: number) => {
  stockValue.value = newValue
}

const updateCryptoValue = (newValue: number) => {
  cryptoValue.value = newValue
}

// Watch for changes in component values
watch([stockValue, cryptoValue], () => {
  // The totalPortfolioValue computed property will automatically update
  console.log('Portfolio values updated:', {
    stocks: stockValue.value,
    crypto: cryptoValue.value,
    total: totalPortfolioValue.value
  })
})
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-6">Portfolio Summary</h1>

      <!-- Error Message -->
      <div v-if="error" class="mb-6 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        {{ error }}
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <LoadingSpinner size="lg" />
      </div>

      <!-- Portfolio Content -->
      <div v-else>
        <!-- Total Portfolio Value Summary -->
        <div class="bg-white rounded-lg shadow-md p-6 mb-8">
          <h2 class="text-xl font-semibold mb-4">Portfolio Overview</h2>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="bg-blue-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">Stock Portfolio Value</p>
              <p class="text-lg font-semibold">{{ formatCurrency(stockValue) }}</p>
            </div>
            <div class="bg-orange-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">Crypto Portfolio Value</p>
              <p class="text-lg font-semibold">{{ formatCurrency(cryptoValue) }}</p>
            </div>
            <div class="bg-green-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">Total Portfolio Value</p>
              <p class="text-lg font-semibold">{{ formatCurrency(totalPortfolioValue) }}</p>
            </div>
          </div>
        </div>

        <!-- Stock Portfolio Section -->
        <div class="mb-8">
          <h2 class="text-xl font-semibold mb-4">Stock Portfolio</h2>
          <PortfolioSummaryTable 
            v-if="stockPositions.length > 0"
            :positions="stockPositions"
            @update:value="updateStockValue"
          />
          <div v-else class="bg-white rounded-lg shadow-md p-6 text-center">
            <p class="text-gray-600">No stock positions in your portfolio.</p>
            <button
              @click="router.push('/search-area')"
              class="mt-4 px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
            >
              Add Stocks to Portfolio
            </button>
          </div>
        </div>

        <!-- Crypto Portfolio Section -->
        <div>
          <h2 class="text-xl font-semibold mb-4">Crypto Portfolio</h2>
          <CryptoPortfolioSummaryTable
            v-if="cryptoPositions.length > 0"
            :positions="cryptoPositions"
            @update:value="updateCryptoValue"
          />
          <div v-else class="bg-white rounded-lg shadow-md p-6 text-center">
            <p class="text-gray-600">No crypto positions in your portfolio.</p>
            <button
              @click="router.push('/crypto')"
              class="mt-4 px-6 py-2 bg-orange-600 text-white rounded-lg hover:bg-orange-700"
            >
              Add Crypto to Portfolio
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>