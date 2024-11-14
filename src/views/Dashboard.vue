<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const searchQuery = ref('')
const selectedStock = ref('')
const selectedChapter = ref('1')
const selectedVerse = ref('1')
const router = useRouter()
const auth = useAuthStore()

const favoriteStocks = [
  {
    name: 'MyStock1',
    price: '0.43'
  },
  {
    name: 'MyStock2',
    price: '0.77'
  },
  {
    name: 'MyStock3',
    price: '0.65'
  }
]


</script>

<template>
  <div class="min-h-screen bg-gradient-to-b from-blue-50 to-white">
    <!-- Hero Section -->
    <div class="container mx-auto px-4 py-12">
      <div class="text-center mb-12">
        <h1 class="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
          Welcome to the Stock Navigator
        </h1>
        <p class="text-xl text-gray-600 max-w-2xl mx-auto">
          Explore various stocks, their price and any news related.
        </p>
        <p class="text-xl text-gray-600 max-w-2xl mx-auto">
          We hope that this helps you with building your portfolio.
        </p>
      </div>

      <!-- Featured Stocks -->
      <div class="max-w-4xl mx-auto">
        <h2 class="text-2xl font-semibold text-gray-800 mb-6 text-center">Favorited Stocks</h2>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <button
            v-for="(favorite) in favoriteStocks"
            :key="favorite.name"
            @click="selectedStock = favorite.name"
            class="bg-white p-6 rounded-lg shadow-lg hover:shadow-xl hover:bg-gray-200 transition-colors"
          >
            <p class="text-center text-gray-700 mb-4 font-bold">
              Name: {{ favorite.name }}
            </p>
            <p class="text-center text-blue-600 font-semibold">
              Current Price: {{ favorite.price }}
            </p>
          </button>
        </div>
      </div>

      <!-- Quick Links -->
      <div class="max-w-4xl mx-auto mt-12 text-center">
        <h2 class="text-2xl font-semibold text-gray-800 mb-6">Featured Stocks to Watch</h2>
        <div class="flex flex-wrap justify-center gap-4">
          <button
            v-for="stock in ['Stock1', 'Stock2', 'Stock3', 'Stock4']"
            :key="stock"
            @click="selectedStock = stock"
            class="px-6 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
          >
            {{ stock }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
input[type="number"] {
  -moz-appearance: textfield;
}
input[type="number"]::-webkit-outer-spin-button,
input[type="number"]::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}
</style>