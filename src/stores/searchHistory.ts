import { defineStore } from 'pinia'
import { ref, watch } from 'vue'
import type { StockData } from '../types/polygon'

const MAX_HISTORY_ITEMS = 5
const STORAGE_KEY = 'stock-search-history'

export const useSearchHistoryStore = defineStore('searchHistory', () => {
  // Initialize from localStorage if available
  const savedHistory = localStorage.getItem(STORAGE_KEY)
  const initialHistory = savedHistory ? JSON.parse(savedHistory) : []
  
  const searchHistory = ref<StockData[]>(initialHistory)

  // Watch for changes and save to localStorage
  watch(
    searchHistory,
    (newHistory) => {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(newHistory))
    },
    { deep: true }
  )

  const addToHistory = (stock: StockData) => {
    // Remove existing entry of the same stock if it exists
    searchHistory.value = searchHistory.value.filter(s => s.symbol !== stock.symbol)
    
    // Add new stock to the beginning of the array
    searchHistory.value.unshift(stock)
    
    // Keep only the last MAX_HISTORY_ITEMS items
    if (searchHistory.value.length > MAX_HISTORY_ITEMS) {
      searchHistory.value = searchHistory.value.slice(0, MAX_HISTORY_ITEMS)
    }
  }

  const clearHistory = () => {
    searchHistory.value = []
    localStorage.removeItem(STORAGE_KEY)
  }

  return {
    searchHistory,
    addToHistory,
    clearHistory
  }
})