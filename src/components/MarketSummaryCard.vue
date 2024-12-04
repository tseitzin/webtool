<script setup lang="ts">
import { formatNumber, formatPercent } from '../utils/formatters'

defineProps<{
  title: string
  stocks: Array<{
    symbol: string
    changePercent?: number
    volume?: number
  }>
  type: 'gainers' | 'losers' | 'active'
}>()
</script>

<template>
  <div class="bg-white rounded-lg shadow-md p-6">
    <div>
      <h2 
        :class="[
          'rounded-lg p-3 text-center text-xl font-semibold mb-4',
          type === 'gainers' ? 'bg-gray-200 text-green-600' :
          type === 'losers' ? 'bg-gray-200 text-red-600' :
          'bg-gray-200 text-indigo-600'
        ]"
      >
        {{ title }}
      </h2>
    </div>
    <div class="space-y-4">
      <div 
        v-for="stock in stocks" 
        :key="stock.symbol" 
        class="flex justify-between items-center"
      >
        <span class="font-medium">{{ stock.symbol }}</span>
        <span 
          :class="[
            type === 'gainers' ? 'text-green-600' :
            type === 'losers' ? 'text-red-600' :
            'text-gray-600'
          ]"
        >
          {{ type === 'active' 
            ? formatNumber(stock.volume || 0)
            : formatPercent(stock.changePercent || 0) 
          }}
        </span>
      </div>
    </div>
  </div>
</template>