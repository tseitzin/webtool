<script setup lang="ts">
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'

defineProps<{
  stock: {
    symbol: string
    companyName: string
    price: number
    change: number
    changePercent: number
    volume: number
    previousClose: number
  }
  isFavorited: boolean
}>()

const emit = defineEmits<{
  (e: 'toggleFavorite', symbol: string): void
}>()

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}
</script>

<template>
  <div class="mt-4">
    <div class="flex justify-between items-center mb-3">
      <h3 class="text-lg font-semibold">Company: {{ stock.companyName }}</h3>
      <h3 class="text-lg font-semibold">Stock Symbol: {{ stock.symbol }}</h3>
      <button
        v-if="!isFavorited"
        @click="emit('toggleFavorite', stock.symbol)"
        class="px-1 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
      >
        Add to Saved Stock
      </button>
      <button
        v-else
        @click="emit('toggleFavorite', stock.symbol)"
        class="px-1 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
      >
        Remove Saved Stock
      </button>
    </div>
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Price</h3>
        <p class="text-lg font-semibold">{{ formatCurrency(stock.price) }}</p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Change</h3>
        <p :class="['text-lg font-semibold', stock.change >= 0 ? 'text-green-600' : 'text-red-600']">
          {{ formatChange(stock.change, stock.changePercent) }}
        </p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Volume</h3>
        <p class="text-lg font-semibold">{{ formatNumber(stock.volume) }}</p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Previous Close</h3>
        <p class="text-lg font-semibold">{{ formatCurrency(stock.previousClose) }}</p>
      </div>
    </div>
  </div>
</template>