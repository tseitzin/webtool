<script setup lang="ts">
import { useRouter } from 'vue-router'
import type { RelatedCompany } from '../types/polygon'
import { formatCurrency } from '../utils/formatters'
import { computed, ref } from 'vue';

const props = defineProps<{
  companies: RelatedCompany[]
  currentSymbol: string
}>()

const router = useRouter()
const isExpanded = ref(false)

const displayedCompanies = computed(() => {
  return isExpanded.value ? props.companies : props.companies.slice(0, 3)
})

const navigateToCompany = (symbol: string) => {
  if (symbol === props.currentSymbol) return
  router.push(`/research/${symbol}`).
  then(() => { router.go(0) })
}

const formatPriceChange = (current: number, previous: number): string => {
  const change = ((current - previous) / current) * 100
  return `${change >= 0 ? '+' : ''}${change.toFixed(2)}%`
}

const toggleExpanded = () => {
  isExpanded.value = !isExpanded.value
}

</script>

<template>
  <div class="bg-white rounded-lg shadow-md p-6">
    
    <div class="flex justify-between items-center mb-4">
      <h2 v-if="companies.length == 0" class="text-xl font-semibold">No related companies for {{ currentSymbol }}</h2>
      <h2 v-else class="text-xl font-semibold">Related Companies</h2>
      <button
        v-if="companies.length > 3"
        @click="toggleExpanded"
        class="px-4 py-1 text-sm font-medium text-gray-600 hover:text-gray-900 bg-gray-200 hover:bg-gray-300 rounded-md transition-colors"
      >
        {{ isExpanded ? 'Show Less' : 'Show All' }}
      </button>
    </div>


    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
      <button
        v-for="company in displayedCompanies"
        :key="company.ticker"
        @click="navigateToCompany(company.ticker)"
        :class="[
          'p-2 rounded-lg text-left transition-colors',
          company.ticker === currentSymbol
            ? 'bg-indigo-100 text-indigo-700 cursor-default'
            : 'bg-gray-100 text-gray-700 hover:bg-indigo-50 hover:text-indigo-600'
        ]"
      >
        <div class="text-sm text-gray-900">{{ company.ticker }} {{ company.name || 'Loading...' }}</div>
        <div class="mt-2 text-sm">
          <template v-if="company.price">
            {{ formatCurrency(company.price) }}
            <span 
              :class="[
                company.price > (company.previousClose || 0) ? 'text-green-600' : 'text-red-600'
              ]"
            >
              ({{ company.previousClose ? formatPriceChange(company.price, company.previousClose) : '' }})
            </span>
          </template>
          <template v-else>Price: Loading...</template>
        </div>
      </button>
    </div>
  </div>
</template>