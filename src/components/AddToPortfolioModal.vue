// src/components/AddToPortfolioModal.vue
<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import api from '../api/axios'
import { formatCurrency, formatNumber } from '../utils/formatters'

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
const showSellConfirmation = ref(false)
const totalSaleAmount = computed(() => sellQuantity.value * props.price)
const showBuyConfirmation = ref(false)
const totalPurchaseAmount = computed(() => quantity.value * props.price)

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
  showBuyConfirmation.value = true
}

const confirmBuy = async () => {

  loading.value = true
  error.value = ''

  try {
    if (existingPositionId.value) 
    {
        await api.put(`/portfolio/${existingPositionId.value}`, {
        QuantityToBuy: quantity.value,
        QuantityAlreadyOwned: currentlyOwned.value + quantity.value,
        notes: notes.value
      })
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
    // emit('close')
    quantity.value = 0
    notes.value = ''
    error.value = ''
    // Refresh current position
    await fetchCurrentPosition()
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to add to portfolio'
  } finally {
    loading.value = false
    showBuyConfirmation.value = false
  }
}

const handleSell = async () => {
  showSellConfirmation.value = true
}

const confirmSell = async () => {
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
    // emit('close')
    sellQuantity.value = 0
    notes.value = ''
    error.value = ''
    // Refresh current position
    await fetchCurrentPosition()
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to sell position'
  } finally {
    loading.value = false
    showSellConfirmation.value = false
  }
}

const validateSellQuantity = (value: number) => {
  if (value > currentlyOwned.value) {
    error.value = `Cannot sell more than ${currentlyOwned.value} shares`
    sellQuantity.value = currentlyOwned.value
  } else {
    error.value = ''
  }
}

// Add watcher for sellQuantity
watch(sellQuantity, (newValue) => {
  if (newValue) {
    validateSellQuantity(newValue)
  }
})

const cancelSell = () => {
  showSellConfirmation.value = false
}

const cancelBuy = () => {
  showBuyConfirmation.value = false
}

</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
      <h2 class="text-xl font-bold mb-4">Manage Portfolio Position</h2>
      <p class="mb-4">{{ symbol }} - Current Price: {{ formatCurrency(price) }}</p>
      <p class="mb-1">Number in Portfolio: {{ formatNumber(currentlyOwned) }}</p>
      <p v-if="currentlyOwned > 0" class="mb-4">Total Invested: {{formatCurrency(currentlyOwned*price) }}</p>

      <!-- Error Message -->
      <div v-if="error" class="mb-4 text-red-600">{{ error }}</div>

      <!-- Buy Section -->
      <div class="mb-6">
        <h3 class="font-semibold mb-2">Add Shares</h3>
        <div class="flex gap-4 mb-4">
          <div class="flex-1">
            <label class="block text-sm font-medium text-gray-700">Quantity to Add</label>
            <input
              v-model.number="quantity"
              type="number"
              min="0"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
        </div>
        <div v-if="showBuyConfirmation" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
      <h3 class="text-lg font-semibold mb-4">Confirm Purchase</h3>
      <p>Are you sure you want to buy:</p>
      <ul class="my-4 space-y-2">
        <li>Stock: {{ symbol }} </li>
        <li>Quantity: {{ quantity }} shares</li>
        <li>Price per share: {{ formatCurrency(price) }}</li>
        <li>Total purchase amount: {{ formatCurrency(totalPurchaseAmount) }}</li>
      </ul>
      <div class="flex justify-end space-x-4">
        <button
          @click="cancelBuy"
          class="px-4 py-2 text-gray-600 hover:text-gray-800"
        >
          Cancel
        </button>
        <button
          @click="confirmBuy"
          class="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
        >
          Confirm Purchase
        </button>
      </div>
    </div>
  </div>
        <button
          @click="handleBuy"
          :disabled="loading || quantity <= 0"
          class="w-full px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
        >
          Add Shares
        </button>
      </div>

      <!-- Sell Section -->
      <div class="mb-6">
        <h3 class="font-semibold mb-2">Remove Shares</h3>
        <div class="flex gap-4 mb-4">
          <div class="flex-1">
            <label class="block text-sm font-medium text-gray-700">Quantity to Remove</label>
            <input
              v-model.number="sellQuantity"
              type="number"
              min="0"
              :max="currentlyOwned"
              :disabled="currentlyOwned === 0"
              @input="(e) => validateSellQuantity(Number((e.target as HTMLInputElement)?.value))"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
        </div>
        <div v-if="showSellConfirmation" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 class="text-lg font-semibold mb-4">Confirm Sale</h3>
            <p>Are you sure you want to sell:</p>
            <ul class="my-4 space-y-2">
              <li class="font-semibold">Stock: {{ symbol }} </li>
              <li>Quantity: {{ sellQuantity }} shares</li>
              <li>Price per share: {{ formatCurrency(price) }}</li>
              <li>Total sale amount: {{ formatCurrency(totalSaleAmount) }}</li>
            </ul>
            <div class="flex justify-end space-x-4">
              <button
                @click="cancelSell"
                class="px-4 py-2 text-gray-600 hover:text-gray-800"
              >
                Cancel
              </button>
              <button
                @click="confirmSell"
                class="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
              >
                Confirm Sale
              </button>
            </div>
          </div>
        </div>
        <button
          @click="handleSell"
          :disabled="loading || sellQuantity <= 0 || sellQuantity > currentlyOwned || currentlyOwned === 0"
          class="w-full px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
        >
          Remove Shares
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
        class="w-full px-4 py-2 bg-gray-400 text-gray-800 rounded-lg hover:bg-gray-600 hover:text-white"
      >
        Close
      </button>
    </div>
  </div>
</template>
