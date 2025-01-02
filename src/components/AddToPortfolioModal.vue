// src/components/AddToPortfolioModal.vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '../api/axios'
import { formatCurrency } from '../utils/formatters'

const props = defineProps<{
  isOpen: boolean
  symbol: string
  price: number
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success'): void
}>()

const quantity = ref(0)
const sellQuantity = ref(0)
const notes = ref('')
const error = ref('')
const loading = ref(false)
const currentlyOwned = ref(0)
const existingPositionId = ref<number | null>(null)

onMounted(async () => {
  await fetchCurrentPosition()
})

const fetchCurrentPosition = async () => {
  try {
    const response = await api.get('/portfolio')
    const positions = response.data
    const position = positions.find((p: any) => p.symbol === props.symbol)
    if (position) {
      currentlyOwned.value = position.quantity
      existingPositionId.value = position.id
    } else {
      currentlyOwned.value = 0
      existingPositionId.value = null
    }
  } catch (e) {
    console.error('Failed to fetch portfolio:', e)
  }
}

const handleBuy = async () => {
  if (quantity.value <= 0) {
    error.value = 'Quantity must be greater than 0'
    return
  }

  loading.value = true
  error.value = ''

  console.log("Existing position: " + existingPositionId.value)
  console.log("Currently owned: " + currentlyOwned.value)

  try {
    if (existingPositionId.value) 
    {
        console.log("HERE")
      const test = await api.put(`/portfolio/${existingPositionId.value}`, {
        quantity: currentlyOwned.value + quantity.value,
        notes: notes.value
      })
      console.log(test)
    } else {
      await api.post('/portfolio', {
        symbol: props.symbol,
        quantity: quantity.value,
        purchasePrice: props.price,
        purchaseDate: new Date().toISOString(),
        notes: notes.value
      })
    }
    emit('success')
    emit('close')
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to add to portfolio'
  } finally {
    loading.value = false
  }
}

const handleSell = async () => {
  if (sellQuantity.value <= 0 || sellQuantity.value > currentlyOwned.value) {
    error.value = 'Invalid sell quantity'
    return
  }

  loading.value = true
  error.value = ''

  try {
    await api.post(`/portfolio/${existingPositionId.value}/sell`, {
      quantity: sellQuantity.value
    })
    emit('success')
    emit('close')
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to sell position'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
      <h2 class="text-xl font-bold mb-4">Manage Portfolio Position</h2>
      <p class="mb-4">{{ symbol }} - Current Price: {{ formatCurrency(price) }}</p>
      <p class="mb-4">Currently Owned: {{ currentlyOwned }}</p>

      <!-- Error Message -->
      <div v-if="error" class="mb-4 text-red-600">{{ error }}</div>

      <!-- Buy Section -->
      <div class="mb-6">
        <h3 class="font-semibold mb-2">Buy More Shares</h3>
        <div class="flex gap-4 mb-4">
          <div class="flex-1">
            <label class="block text-sm font-medium text-gray-700">Quantity to Buy</label>
            <input
              v-model.number="quantity"
              type="number"
              min="0"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
        </div>
        <button
          @click="handleBuy"
          :disabled="loading || quantity <= 0"
          class="w-full px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
        >
          Buy Shares
        </button>
      </div>

      <!-- Sell Section -->
      <div class="mb-6">
        <h3 class="font-semibold mb-2">Sell Shares</h3>
        <div class="flex gap-4 mb-4">
          <div class="flex-1">
            <label class="block text-sm font-medium text-gray-700">Quantity to Sell</label>
            <input
              v-model.number="sellQuantity"
              type="number"
              min="0"
              :max="currentlyOwned"
              :disabled="currentlyOwned === 0"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
        </div>
        <button
          @click="handleSell"
          :disabled="loading || sellQuantity <= 0 || sellQuantity > currentlyOwned || currentlyOwned === 0"
          class="w-full px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
        >
          Sell Shares
        </button>
      </div>

      <!-- Notes -->
      <div class="mb-6">
        <label class="block text-sm font-medium text-gray-700">Notes</label>
        <textarea
          v-model="notes"
          rows="3"
          class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
        ></textarea>
      </div>

      <!-- Cancel Button -->
      <button
        @click="$emit('close')"
        class="w-full px-4 py-2 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300"
      >
        Cancel
      </button>
    </div>
  </div>
</template>
