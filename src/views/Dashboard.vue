<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const selectedStock = ref('')
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
  <div class="min-h-screen bg-slate-50 rounded-lg shadow mb-4">
    <!-- Hero Section -->
    <div class="container mx-auto px-6 py-8">
      <div class="text-center mb-12">
        <h1 class="text-5xl md:text-4xl font-extrabold text-gray-900 mb-4">
          Welcome, {{ auth.user?.name }}!
        </h1>
        <p class="text-lg md:text-xl text-gray-700 max-w-2xl mx-auto leading-relaxed">
          Your personalized stock dashboard is here. Track and manage your investments easily, and let us help you make informed decisions to grow your financial portfolio.
        </p>
      </div>

      <!-- Stocks owned -->
      <div class="max-w-5xl mx-auto">
        <h2 class="text-3xl font-semibold text-gray-800 mb-4 text-center">Stocks you Own</h2>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8 pb-12">
          <button
            v-for="favorite in favoriteStocks"
            :key="favorite.name"
            @click="selectedStock = favorite.name"
            class="group bg-white p-6 rounded-lg shadow-lg transform transition-all duration-300 hover:scale-105"
          >
            <div class="text-center mb-4">
              <p class="text-lg font-bold text-gray-800 group-hover:text-blue-600 transition-colors">
                Stock: {{ favorite.name }}
              </p>
              <p class="font-semibold text-blue-700">
                Current Price: ${{ favorite.price }}
              </p>
            </div>
            <div class="text-left pl-4 font-semibold text-gray-800 text-sm">
              <p>Opening Price: <!-- ${{ favorite.openingPrice }}  --></p>
              <p>Daily High: <!-- ${{ favorite.dailyHigh }}  --> </p>
              <p>Daily Low: <!-- ${{ favorite.dailyLow }}  --> </p> 
            </div>
          </button>
        </div>
      </div>

      <!-- Favorited Stocks -->
      <div class="max-w-5xl mx-auto">
        <h2 class="text-3xl font-semibold text-gray-800 mb-4 text-center">Stocks you Favorited</h2>
        <div class="grid grid-cols-1 md:grid-cols-3 mb-10 gap-8">
          <button
            v-for="favorite in favoriteStocks"
            :key="favorite.name"
            @click="selectedStock = favorite.name"
            class="group bg-white p-6 rounded-lg shadow-lg transform transition-all duration-300 hover:scale-105"
          >
            <div class="text-center mb-4">
              <p class="text-lg font-bold text-gray-800 group-hover:text-blue-600 transition-colors">
                Stock: {{ favorite.name }}
              </p>
              <p class="font-semibold text-blue-700">
                Current Price: ${{ favorite.price }}
              </p>
            </div>
            <div class="text-left pl-4 font-semibold text-gray-800 text-sm">
              <p>Opening Price: <!-- ${{ favorite.openingPrice }}  --></p>
              <p>Daily High: <!-- ${{ favorite.dailyHigh }}  --> </p>
              <p>Daily Low: <!-- ${{ favorite.dailyLow }}  --> </p> 
            </div>
          </button>
        </div>
      </div>


      <!-- Featured Stocks to Watch -->
      <div class="max-w-5xl mx-auto text-center">
        <h2 class="text-3xl font-semibold text-gray-800 mb-4">Stocks to Watch</h2>
        <div class="flex flex-wrap justify-center gap-4">
          <button
            v-for="stock in ['Apple', 'Microsoft', 'Tesla', 'Amazon']"
            :key="stock"
            @click="selectedStock = stock"
            class="px-8 py-3 bg-gray-200 text-gray-800 rounded-lg shadow transition-transform transform hover:scale-105 hover:bg-blue-100"
          >
            {{ stock }}
          </button>
        </div>
      </div>

      <!-- Portfolio Overview Section -->
      <div class="max-w-5xl mx-auto mt-16 text-center">
        <h2 class="text-3xl font-semibold text-gray-800 mb-8">Portfolio Overview</h2>
        <div class="flex flex-col md:flex-row gap-8 justify-center">
          <div class="p-6 bg-white rounded-lg shadow-lg hover:shadow-xl transition-shadow duration-300">
            <h3 class="text-2xl font-bold text-gray-800 mb-4">Total Value</h3>
            <p class="text-4xl font-semibold text-blue-700"> <!-- $ {{ totalPortfolioValue }} --> </p>
          </div>
          <div class="p-6 bg-white rounded-lg shadow-lg hover:shadow-xl transition-shadow duration-300">
            <h3 class="text-2xl font-bold text-gray-800 mb-4">Growth Rate</h3>
            <p class="text-4xl font-semibold text-green-600"> <!--  {{ growthRate }}% --> </p>
          </div>
          <div class="p-6 bg-white rounded-lg shadow-lg hover:shadow-xl transition-shadow duration-300">
            <h3 class="text-2xl font-bold text-gray-800 mb-4">Top Performing Stock</h3>
            <p class="text-xl font-semibold text-gray-700"> <!-- {{ topStock.name }} --> </p>
            <p class="text-lg text-blue-600"> <!-- ${{ topStock.price }} --> </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>

</style>
