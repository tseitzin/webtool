<script setup lang="ts">
import router from '../router';
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { UserOwnedStock } from '../types/portfolio'
import { portfolioService } from '../services/portfolioService'
import { onMounted, ref } from 'vue';
import { showErrorToast } from '../utils/toast'

const props = defineProps<{
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
const showRemoveModal = ref(false)
const stockToRemove = ref<string | null>(null)

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

const handleRemoveStock = (symbol: string) => {
  // Check if stock is in portfolio when trying to remove it
  const isInPortfolio = ownedStocks.value.some(stock => stock.symbol === symbol)
  if (isInPortfolio) {
    showErrorToast(`Cannot remove ${symbol} from watchlist while it is in your portfolio`, 3000)
    return
  }
  stockToRemove.value = symbol
  showRemoveModal.value = true
}

const handleConfirmRemoval = () => {
  if (stockToRemove.value) {
    emit('toggleFavorite', stockToRemove.value)
  }
  cancelRemoval()
}

const cancelRemoval = () => {
  showRemoveModal.value = false
  stockToRemove.value = null
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
        <h3 class="text-lg font-bold">{{ props.stock.symbol }}</h3>
        <p class="text-sm text-gray-600">{{ props.stock.companyName }}</p>
      </div>
      <div class="flex gap-2">
        <button
          @click="navigateToResearch(props.stock.symbol)"
          class="px-4 py-2 bg-blue-600 text-xs text-white rounded-lg hover:bg-blue-800 transition-colors"
        >
          Research Stock
        </button>
        <button
          v-if="!props.isFavorited"
          @click="emit('toggleFavorite', props.stock.symbol)"
          class="px-4 py-2 bg-green-600 text-xs text-white rounded-lg hover:bg-green-800 transition-colors"
        >
          Add to Watchlist
        </button>
        <button
          v-else
          @click="handleRemoveStock(props.stock.symbol)"
          class="px-4 py-2 bg-gray-600 text-xs text-white rounded-lg hover:bg-gray-800 transition-colors"
        >
          Remove
        </button>
      </div>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mt-4">
      <div class="bg-gray-100 p-2 rounded-lg">
        <p class="text-sm text-gray-500">Current Price</p>
        <p class="text-lg font-semibold">{{ formatCurrency(props.stock.price) }}</p>
      </div>
      <div class="bg-gray-100 p-2 rounded-lg">
        <p class="text-sm text-gray-500">Today's Change</p>
        <p :class="['text-lg font-semibold', props.stock.change >= 0 ? 'text-green-600' : 'text-red-600']">
          {{ formatChange(props.stock.change, props.stock.changePercent) }}
        </p>
      </div>
      <div class="bg-gray-100 p-2 rounded-lg">
        <p class="text-sm text-gray-500">Volume</p>
        <p class="text-lg font-semibold">{{ formatNumber(props.stock.volume) }}</p>
      </div>
      <div class="bg-gray-100 p-2 rounded-lg">
        <p class="text-sm text-gray-500">Previous Close</p>
        <p class="text-lg font-semibold">{{ formatCurrency(props.stock.previousClose) }}</p>
      </div>
    </div>

    <div 
      v-if="getOwnershipInfo(props.stock.symbol)"
      class="mt-4 bg-blue-50 p-3 rounded-lg"
    >
      <div class="flex">
        <div>
          <span class="font-medium text-blue-800">Number in Portfolio:</span>
          <span class="ml-2 text-blue-700">
            {{ formatNumber(getOwnershipInfo(props.stock.symbol)?.shares || 0) }} shares
          </span>
        </div>
        <div>
          <span class="font-medium text-blue-800 ml-6">Total Value:</span>
          <span class="ml-2 text-blue-700">
            {{ formatCurrency((getOwnershipInfo(props.stock.symbol)?.value || 0)) }}
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

    <div v-if="props.stock.marketStatus.includes('closed')" class="mt-4 bg-yellow-50 p-4 rounded-lg">
      <p class="text-sm text-yellow-700">{{ props.stock.marketStatus }}</p>
    </div>

    <ConfirmationModal
      :is-open="showRemoveModal"
      title="Remove Stock"
      message="Are you sure you want to remove this stock from your watchlist?"
      @confirm="handleConfirmRemoval"
      @cancel="cancelRemoval"
    />
  </div>
</template>