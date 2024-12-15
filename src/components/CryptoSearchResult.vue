<script setup lang="ts">
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { CryptoData } from '../types/crypto'

defineProps<{
  crypto: CryptoData
  isSaved: boolean
}>()

const emit = defineEmits<{
  (e: 'toggleSave', symbol: string): void
}>()

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}
</script>

<template>
  <div class="mt-6">
    <div class="flex justify-between items-center mb-3">
      <h3 class="text-lg font-semibold">Crypto Symbol: {{ crypto.symbol }}</h3>
      <button
        v-if="!isSaved"
        @click="emit('toggleSave', crypto.symbol)"
        class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
      >
        Add to Watchlist
      </button>
      <button
        v-else
        @click="emit('toggleSave', crypto.symbol)"
        class="px-2 py-2 bg-red-600 text-white text-sm rounded-lg hover:bg-red-700 transition-colors"
      >
        Remove from Watchlist
      </button>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Price</h3>
        <p class="text-lg font-semibold">{{ formatCurrency(Number(crypto.price)) }}</p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">24h Change</h3>
        <p :class="['text-lg font-semibold', crypto.change >= 0 ? 'text-green-600' : 'text-red-600']">
          {{ formatChange(crypto.change, crypto.changePercent) }}
        </p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">24h Volume</h3>
        <p class="text-lg font-semibold">{{ formatNumber(crypto.volume) }}</p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">24h High/Low</h3>
        <p class="text-lg font-semibold">
          {{ formatCurrency(crypto.high24h) }} / {{ formatCurrency(crypto.low24h) }}
        </p>
      </div>
    </div>

  </div>
</template>