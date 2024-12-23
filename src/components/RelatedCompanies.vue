<script setup lang="ts">
import { useRouter } from 'vue-router'
import type { RelatedCompany } from '../types/polygon'

const props = defineProps<{
  companies: RelatedCompany[]
  currentSymbol: string
}>()

const router = useRouter()

const navigateToCompany = (symbol: string) => {
  if (symbol === props.currentSymbol) return
  router.push(`/research/${symbol}`).
  then(() => { router.go(0) })
}
</script>

<template>
  <div class="bg-white rounded-lg shadow-md p-6">
    <h2 class="text-xl font-semibold mb-4">Related Companies</h2>
    <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-3">
      <button
        v-for="company in companies"
        :key="company.ticker"
        @click="navigateToCompany(company.ticker)"
        :class="[
          'px-4 py-2 rounded-lg text-sm font-medium transition-colors',
          company.ticker === currentSymbol
            ? 'bg-indigo-100 text-indigo-700 cursor-default'
            : 'bg-gray-100 text-gray-700 hover:bg-indigo-50 hover:text-indigo-600'
        ]"
      >
        {{ company.ticker }}
      </button>
    </div>
  </div>
</template>