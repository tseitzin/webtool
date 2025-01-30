<script setup lang="ts">
import { ref, onMounted, watchEffect } from 'vue'
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { UserOwnedStock } from '../types/portfolio'
import { polygonService } from '../services/polygonService'
import router from '../router';

const props = defineProps<{
  positions: UserOwnedStock[]
}>()

const portfolioTotal = ref(0)
const totalGainLoss = ref(0)
const totalGainLossPercent = ref(0)

const updatePortfolioValues = async () => {
  let total = 0
  let totalCost = 0

  for (const position of props.positions) {
    try {
      const currentData = await polygonService.getStockSnapshot(position.symbol)
      position.currentValue = currentData.price * position.quantity
      position.gainLoss = position.currentValue - (position.averagePurchasePrice * position.quantity)
      position.gainLossPercent = (position.gainLoss / (position.averagePurchasePrice * position.quantity)) * 100
      total += position.currentValue
      totalCost += position.averagePurchasePrice * position.quantity
    } catch (error) {
      console.error(`Error updating ${position.symbol}:`, error)
    }
  }

  portfolioTotal.value = total
  totalGainLoss.value = total - totalCost
  totalGainLossPercent.value = (totalGainLoss.value / totalCost) * 100
}

// Update values every minute
watchEffect((onCleanup) => {
  const interval = setInterval(updatePortfolioValues, 60000)
  onCleanup(() => clearInterval(interval))
})

const navigateToResearch = (symbol: string) => {
  router.push(`/research/${symbol}`)
}

onMounted(updatePortfolioValues)
</script>

<template>
  <div class="bg-white rounded-lg shadow-lg p-6">
    <div class="overflow-x-auto">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Symbol</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Quantity</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Current Price</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Current Value</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Gain/Loss</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Gain/Loss %</th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr v-for="position in positions" :key="position.symbol">
            <td 
              class="px-6 py-4 cursor-pointer whitespace-nowrap text-sm text-black-600 hover:text-blue-800 hover:font-bold"
              @click="navigateToResearch(position.symbol)"
            >
              {{ position.symbol }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ formatNumber(position.quantity) }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ formatCurrency(position.currentValue / position.quantity) }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ formatCurrency(position.currentValue) }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm" :class="position.gainLoss >= 0 ? 'text-green-600' : 'text-red-600'">
              {{ formatCurrency(position.gainLoss) }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm" :class="position.gainLossPercent >= 0 ? 'text-green-600' : 'text-red-600'">
              {{ formatPercent(position.gainLossPercent) }}
            </td>
          </tr>
        </tbody>
        <tfoot class="bg-gray-50 font-semibold">
          <tr>
            <td colspan="3" class="px-6 py-4 text-sm text-gray-900">Portfolio Total:</td>
            <td class="px-6 py-4 text-sm text-gray-900">{{ formatCurrency(portfolioTotal) }}</td>
            <td class="px-6 py-4 text-sm" :class="totalGainLoss >= 0 ? 'text-green-600' : 'text-red-600'">
              {{ formatCurrency(totalGainLoss) }}
            </td>
            <td class="px-6 py-4 text-sm" :class="totalGainLossPercent >= 0 ? 'text-green-600' : 'text-red-600'">
              {{ formatPercent(totalGainLossPercent) }}
            </td>
          </tr>
        </tfoot>
      </table>
    </div>
  </div>
</template>