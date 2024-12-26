<script setup lang="ts">
import { useRouter } from 'vue-router'
import { formatNumber, formatPercent } from '../utils/formatters'

const router = useRouter()

defineProps<{
  title: string
  stocks: Array<{
    symbol: string
    changePercent?: number
    volume?: number
  }>
  type: 'gainers' | 'losers' | 'active'
}>()

const navigateToResearch = (symbol: string) => {
  router.push(`/research/${symbol}`)
}
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
        class="flex justify-between items-center cursor-pointer text-indigo-600 hover:text-indigo-800 hover:bg-gray-50 p-2 rounded-lg transition-colors"
        @click="navigateToResearch(stock.symbol)"
      >
        <span class="font-medium">{{ stock.symbol }}</span>
        <span 
          :class="[
            type === 'gainers' ? 'text-blue-600' :
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