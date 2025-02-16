import { defineStore } from 'pinia'
import { ref, watch } from 'vue'
import type { CryptoData } from '../types/crypto'

const MAX_HISTORY_ITEMS = 5
const STORAGE_KEY = 'crypto-search-history'

export const useCryptoSearchHistoryStore = defineStore('cryptoSearchHistory', () => {
  // Initialize from localStorage if available
  const savedHistory = localStorage.getItem(STORAGE_KEY)
  const initialHistory = savedHistory ? JSON.parse(savedHistory) : []
  
  const searchHistory = ref<CryptoData[]>(initialHistory)

  // Watch for changes and save to localStorage
  watch(
    searchHistory,
    (newHistory) => {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(newHistory))
    },
    { deep: true }
  )

  const addToHistory = (crypto: CryptoData) => {
    // Remove existing entry of the same crypto if it exists
    searchHistory.value = searchHistory.value.filter(c => c.symbol !== crypto.symbol)
    
    // Add new crypto to the beginning of the array
    searchHistory.value.unshift(crypto)
    
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