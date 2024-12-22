import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { StockData } from '../types/polygon'

export const useSearchStore = defineStore('search', () => {
  const lastSearchedStock = ref<StockData | null>(null)
  const lastSearchSymbol = ref('')

  const setLastSearchedStock = (stock: StockData | null) => {
    lastSearchedStock.value = stock
    if (stock) {
      lastSearchSymbol.value = stock.symbol
    }
  }

  const clearSearch = () => {
    lastSearchedStock.value = null
    lastSearchSymbol.value = ''
  }

  return {
    lastSearchedStock,
    lastSearchSymbol,
    setLastSearchedStock,
    clearSearch
  }
})