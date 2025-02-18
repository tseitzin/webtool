<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import api from '../api/axios'
import { formatCurrency, formatNumber } from '../utils/formatters'
import { useLogger } from '../composables/useLogger'

const logger = useLogger()

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
const purchasePrice = ref(props.price)
const notes = ref('')
const error = ref('')
const loading = ref(false)
const currentlyOwned = ref(0)
const existingPositionId = ref<number | null>(null)
const showBuyConfirmation = ref(false)
const totalPurchaseAmount = computed(() => quantity.value * purchasePrice.value)

watch(() => props.price, (newPrice) => {
  if (!purchasePrice.value) {
    purchasePrice.value = newPrice
  }
})

onMounted(async () => {
  await fetchCurrentPosition()
  purchasePrice.value = props.price
})

const fetchCurrentPosition = async () => {
  try {
    const response = await api.get('/cryptoportfolio')
    const positions = response.data
    const position = positions.find((p: any) => p.symbol === props.symbol)
    if (position) {
      currentlyOwned.value = position.quantity
      existingPositionId.value = position.id
    } else {
      currentlyOwned.value = 0
      existingPositionId.value = null
    }

    await logger.info('Fetched current crypto position', {
      symbol: props.symbol,
      currentlyOwned: currentlyOwned.value,
      existingPositionId: existingPositionId.value
    })
  } catch (e) {
    console.error('Failed to fetch portfolio:', e)
    await logger.error('Failed to fetch crypto portfolio', e as Error, {
      symbol: props.symbol
    })
  }
}

const handleBuy = async () => {
  if (quantity.value <= 0) {
    error.value = 'Quantity must be greater than 0'
    await logger.error('Quantity must be greater than 0', new Error(`User entered a number less than 0`), {
      sellQuantity: quantity.value,
      currentlyOwned: currentlyOwned.value
    })
    return
  }
  if (purchasePrice.value <= 0) {
    error.value = 'Purchase price must be greater than 0'
    return
  }
  showBuyConfirmation.value = true
}

const confirmBuy = async () => {
  loading.value = true
  error.value = ''

  try {
    if (existingPositionId.value) {
      await logger.info('Adding to existing crypto position', {
        symbol: props.symbol,
        quantity: quantity.value,
        price: purchasePrice.value,
        total: totalPurchaseAmount.value
      })

      await api.put(`/cryptoportfolio/${existingPositionId.value}`, {
        QuantityToBuy: quantity.value,
        QuantityAlreadyOwned: currentlyOwned.value + quantity.value,
        purchasePrice: purchasePrice.value,
        purchaseDate: new Date().toISOString(),
        notes: notes.value
      })
    } else {
      await logger.info('Adding new crypto position', {
        symbol: props.symbol,
        quantity: quantity.value,
        price: purchasePrice.value,
        total: totalPurchaseAmount.value
      })

      await api.post('/cryptoportfolio', {
        symbol: props.symbol,
        quantity: quantity.value,
        purchasePrice: purchasePrice.value,
        purchaseDate: new Date().toISOString(),
        notes: notes.value
      })
    }
    emit('success')
    quantity.value = 0
    notes.value = ''
    error.value = ''
    await fetchCurrentPosition()
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to add to portfolio'
    await logger.error('Failed to add crypto to portfolio', e as Error, {
      symbol: props.symbol,
      quantity: quantity.value,
      price: purchasePrice.value,
      error: error.value
    })
  } finally {
    loading.value = false
    showBuyConfirmation.value = false
  }
}

const cancelBuy = () => {
  showBuyConfirmation.value = false
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 max-w-md w-full mx-2">
      <h2 class="text-xl font-bold mb-4">Add/Increase Crypto Position</h2>
      <p class="mb-1">{{ symbol }} - Current Market Price: {{ formatCurrency(price) }}</p>
      <p class="mb-1">Number in Portfolio: {{ formatNumber(currentlyOwned) }}</p>
      <p v-if="currentlyOwned > 0" class="mb-2">Total Invested: {{formatCurrency(currentlyOwned*price) }}</p>

      <!-- Error Message -->
      <div v-if="error" class="mb-4 text-red-600">{{ error }}</div>

      <!-- Buy Section -->
      <div class="mb-3">
        <h3 class="font-semibold mb-2">Add Crypto</h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700">Quantity to Add</label>
            <input
              v-model.number="quantity"
              type="number"
              min="0"
              step="0.000001"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
          <div>
            <p class="text-xs text-gray-600 mb-1">
              Current price is set, however, if you purchased at a different price update the purchase price
            </p>
            <label class="block text-sm font-medium text-gray-700">Purchase Price</label>
            <input
              v-model.number="purchasePrice"
              type="number"
              min="0"
              step="0.01"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
        </div>
        
        <div v-if="showBuyConfirmation" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 class="text-lg font-semibold mb-2">Confirm Purchase</h3>
            <p>Are you sure you want to buy:</p>
            <ul class="my-4 space-y-2">
              <li>Crypto: {{ symbol }} </li>
              <li>Quantity: {{ quantity }}</li>
              <li>Price per unit: {{ formatCurrency(purchasePrice) }}</li>
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
          class="w-full px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 mt-4"
        >
          Add Crypto
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