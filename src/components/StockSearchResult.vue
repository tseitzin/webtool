<script setup lang="ts">
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'

defineProps<{
  stock: {
    symbol: string
    price: number
    change: number
    changePercent: number
    volume: number
    marketCap: number
  }
  isFavorited: boolean
}>()

const emit = defineEmits<{
  (e: 'toggleFavorite', symbol: string): void
}>()
</script>

<template>
  <div class="mt-4">
    <div class="flex justify-between items-center mb-4">
      <h3 class="text-lg font-semibold">{{ stock.symbol }}</h3>
      <button
        v-if="!isFavorited"
        @click="emit('toggleFavorite', stock.symbol)"
        class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
      >
        Add to Favorites
      </button>
      <button
        v-else
        @click="emit('toggleFavorite', stock.symbol)"
        class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
      >
        Remove from Favorites
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
          {{ formatCurrency(stock.change) }} ({{ formatPercent(stock.changePercent) }})
        </p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Volume</h3>
        <p class="text-lg font-semibold">{{ formatNumber(stock.volume) }}</p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Market Cap</h3>
        <p class="text-lg font-semibold">{{ formatCurrency(stock.marketCap) }}</p>
      </div>
    </div>
  </div>
</template>