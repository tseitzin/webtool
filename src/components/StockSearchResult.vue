<script setup lang="ts">
import router from '../router';
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { UserOwnedStock } from '../types/portfolio'
import { portfolioService } from '../services/portfolioService'
import { onMounted, ref } from 'vue';

defineProps<{
  stock: {
    symbol: string
    companyName: string
    price: number
    change: number
    changePercent: number
    volume: number
    previousClose: number
    marketStatus: string
  }
  isFavorited: boolean
}>()

onMounted(async () => {
  fetchSavedStocks()
})

const ownedStocks = ref<UserOwnedStock[]>([])

const emit = defineEmits<{
  (e: 'toggleFavorite', symbol: string): void
}>()

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}

const navigateToResearch = (symbol: string) => {
  router.push(`/research/${symbol}`)
}

const fetchSavedStocks = async () => {
  try {
    const owned = await portfolioService.getPortfolio()
    ownedStocks.value = owned
    
  } catch (e) {
    console.error('Error fetching stocks:', e)
  }
}

// Add helper function to get ownership info
const getOwnershipInfo = (symbol: string) => {
    const owned = ownedStocks.value.find(stock => stock.symbol === symbol)
    if (!owned) return null
    
    return {
      shares: owned.quantity,
      value: owned.quantity * owned.averagePurchasePrice
    }
  }
</script>

<template>
  <div class="mt-2">
    <div class="flex justify-between items-center">
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
              v-if="!isFavorited"
              @click="emit('toggleFavorite', stock.symbol)"
              class="px-1 py-2 bg-green-600 text-white text-sm rounded-lg hover:bg-green-700 transition-colors"
            >
              Add to Saved Stocks
            </button>
            <button
              v-else
              @click="emit('toggleFavorite', stock.symbol)"
              class="px-1 py-2 bg-red-600 text-white text-sm rounded-lg hover:bg-red-700 transition-colors"
            >
              Remove Saved Stock
            </button>
          </div>
        </div>
    

    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 mt-1">
      <div class="bg-gray-100 p-2 rounded-lg">
        <h3 class="text-sm text-gray-500">Current Price</h3>
        <p class="text-lg font-semibold">{{ formatCurrency(stock.price) }}</p>
      </div>
      <div class="bg-gray-100 p-2 rounded-lg">
        <h3 class="text-sm text-gray-500">Change</h3>
        <p :class="['text-lg font-semibold', stock.change >= 0 ? 'text-green-600' : 'text-red-600']">
          {{ formatChange(stock.change, stock.changePercent) }}
        </p>
      </div>
      <div class="bg-gray-100 p-2 rounded-lg">
        <h3 class="text-sm text-gray-500">Volume</h3>
        <p class="text-lg font-semibold">{{ formatNumber(stock.volume) }}</p>
      </div>
      <div class="bg-gray-100 p-2 rounded-lg">
        <h3 class="text-sm text-gray-500">Previous Close</h3>
        <p class="text-lg font-semibold">{{ formatCurrency(stock.previousClose) }}</p>
      </div>
      <div v-if="stock.marketStatus.includes('closed')" class="col-span-full bg-yellow-50 p-4 rounded-lg">
        <p class="text-sm text-yellow-700">{{ stock.marketStatus }}</p>
      </div>
    </div>
    <div class="flex justify-between items-center mt-2 mb-3">
      <button
        @click="navigateToResearch(stock.symbol)"
        class="px-2 py-2 bg-indigo-500 text-sm text-white rounded-lg hover:bg-indigo-700 transition-colors"
        title="Research Stock"
      >
        Research Stock
      </button>
    </div>

    <div 
          v-if="getOwnershipInfo(stock.symbol)"
          class="mt-4 bg-blue-50 p-3 rounded-lg"
        >
          <div class="flex">
            <div>
              <span class="font-medium text-blue-800">Number in Portfolio:</span>
              <span class="ml-2 text-blue-700">
                {{ formatNumber(getOwnershipInfo(stock.symbol)?.shares || 0) }} shares
              </span>
            </div>
            <div>
              <span class="font-medium text-blue-800 ml-6">Total Value:</span>
              <span class="ml-2 text-blue-700">
                {{ formatCurrency((getOwnershipInfo(stock.symbol)?.value || 0)) }}
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
</template>