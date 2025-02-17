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

const sellQuantity = ref(0)
const sellPrice = ref(props.price)
const notes = ref('')
const error = ref('')
const loading = ref(false)
const currentlyOwned = ref(0)
const existingPositionId = ref<number | null>(null)
const showSellConfirmation = ref(false)
const totalSaleAmount = computed(() => sellQuantity.value * sellPrice.value)

watch(() => props.price, (newPrice) => {
  if (!sellPrice.value) {
    sellPrice.value = newPrice
  }
})

onMounted(async () => {
  await fetchCurrentPosition()
  sellPrice.value = props.price
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

const handleSell = async () => {
  showSellConfirmation.value = true
}

const confirmSell = async () => {
  if (sellQuantity.value <= 0 || sellQuantity.value > currentlyOwned.value) {
    error.value = 'Invalid sell quantity'
    await logger.warn('Invalid sell quantity', new Error(`Sell quantity ${sellQuantity.value} is over what you own`), {
      sellQuantity: sellQuantity.value,
      currentlyOwned: currentlyOwned.value
    })
    return
  }

  loading.value = true
  error.value = ''

  try {
    await logger.info('Reducing existing crypto position', {
        symbol: props.symbol,
        quantity: sellQuantity.value,
        price: sellPrice.value
      })
    await api.post(`/cryptoportfolio/${existingPositionId.value}/sell`, {
      quantity: sellQuantity.value,
      price: sellPrice.value
    })
    emit('success')
    sellQuantity.value = 0
    sellPrice.value = props.price
    notes.value = ''
    error.value = ''
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
    error.value = `Cannot sell more than ${currentlyOwned.value} units`
    sellQuantity.value = currentlyOwned.value
  } else {
    error.value = ''
  }
}

watch(sellQuantity, (newValue) => {
  if (newValue) {
    validateSellQuantity(newValue)
  }
})

const cancelSell = () => {
  showSellConfirmation.value = false
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 max-w-md w-full mx-2">
      <h2 class="text-xl font-bold mb-4">Sell Crypto Position</h2>
      <p class="mb-1">{{ symbol }} - Current Market Price: {{ formatCurrency(price) }}</p>
      <p class="mb-1">Number in Portfolio: {{ formatNumber(currentlyOwned) }}</p>
      <p v-if="currentlyOwned > 0" class="mb-2">Total Invested: {{formatCurrency(currentlyOwned*price) }}</p>

      <!-- Error Message -->
      <div v-if="error" class="mb-4 text-red-600">{{ error }}</div>

      <!-- Sell Section -->
      <div class="mb-6">
        <h3 class="font-semibold mb-2">Remove Crypto</h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700">Quantity to Remove</label>
            <input
              v-model.number="sellQuantity"
              type="number"
              min="0"
              step="0.000001"
              :max="currentlyOwned"
              :disabled="currentlyOwned === 0"
              @input="(e: Event) => validateSellQuantity(Number((e.target as HTMLInputElement)?.value))"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
          <div>
            <p class="text-xs text-gray-600 mb-1">
              Current price is set, however, if you sold at a different price update the Sell Price
            </p>
            <label class="block text-sm font-medium text-gray-700">Sell Price</label>
            <input
              v-model.number="sellPrice"
              type="number"
              min="0"
              step="0.01"
              :disabled="currentlyOwned === 0"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
        </div>
        <div v-if="showSellConfirmation" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 class="text-lg font-semibold mb-4">Confirm Sale</h3>
            <p>Are you sure you want to sell:</p>
            <ul class="my-4 space-y-2">
              <li class="font-semibold">Crypto: {{ symbol }} </li>
              <li>Quantity: {{ sellQuantity }}</li>
              <li>Price per unit: {{ formatCurrency(sellPrice) }}</li>
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
          class="w-full px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50 mt-4"
        >
          Remove Crypto
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