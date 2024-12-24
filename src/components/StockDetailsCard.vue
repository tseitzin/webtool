<script setup lang="ts">
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { StockData } from '../types/polygon'

defineProps<{
  stock: StockData
}>()

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}
</script>

<template>
  <div class="bg-white rounded-lg shadow-md p-6 mb-6">
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Current Price</h3>
        <p class="text-lg font-semibold">{{ formatCurrency(stock.price) }}</p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Today's Change</h3>
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

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-4">
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Open</h3>
        <p class="text-lg font-semibold">{{ stock.open ? formatCurrency(stock.open) : 'N/A' }}</p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">High</h3>
        <p class="text-lg font-semibold">{{ stock.high ? formatCurrency(stock.high) : 'N/A' }}</p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Low</h3>
        <p class="text-lg font-semibold">{{ stock.low ? formatCurrency(stock.low) : 'N/A' }}</p>
      </div>
    </div>

    <MarketStatusMessage 
      v-if="stock.marketStatus"
      :market-status="stock.marketStatus"
      class="mt-4"
    />
  </div>
</template>