<!-- src/views/PortfolioSummary.vue -->
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import api from '../api/axios'
import type { UserOwnedStock } from '../types/portfolio'
import PortfolioSummaryTable from '../components/PortfolioSummaryTable.vue'
import { formatCurrency } from '../utils/formatters'

const router = useRouter()
const auth = useAuthStore()
const stockPositions = ref<UserOwnedStock[]>([])
const loading = ref(true)
const error = ref('')
const totalValue = ref(0)

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/login')
    return
  }
  await fetchPortfolio()
})

const fetchPortfolio = async () => {
  try {
    const response = await api.get('/portfolio')
    stockPositions.value = response.data
    calculateTotalValue()
  } catch (e) {
    error.value = 'Failed to load portfolio'
    console.error('Error:', e)
  } finally {
    loading.value = false
  }
}

const calculateTotalValue = () => {
  totalValue.value = stockPositions.value.reduce((sum, position) => 
    sum + position.currentValue, 0)
}
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
        <!-- Portfolio Value Summary -->
        <div class="bg-white rounded-lg shadow-md p-6 mb-8">
          <h2 class="text-xl font-semibold mb-4">Portfolio Total</h2>
          <div class="text-2xl font-bold text-indigo-600">
            {{ formatCurrency(totalValue) }}
          </div>
          <p class="text-gray-600">Total Portfolio Value</p>
        </div>

        <div>
            <h2 class="text-xl font-semibold mb-2">Stock Portfolio</h2>
        </div>

        <!-- Portfolio Table -->
        <PortfolioSummaryTable 
          v-if="stockPositions.length > 0"
          :positions="stockPositions"
          @update:value="calculateTotalValue"
        />

        <!-- Empty State -->
        <div v-else class="text-center py-8">
          <p class="text-gray-600">No positions in your portfolio yet.</p>
          <button
            @click="router.push('/search-area')"
            class="mt-4 px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Start Building Your Portfolio
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
