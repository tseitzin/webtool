<script setup lang="ts">
import { formatNumber, formatCurrency, formatPercent } from '../utils/formatters'
import type { CryptoData } from '../types/crypto'
import { onMounted, ref } from 'vue';
import { cryptoService } from '../services/cryptoService';
import { showErrorToast } from '../utils/toast';
import api from '../api/axios';
import RemoveFromCryptoPortfolioModal from '../components/RemoveFromCryptoPortfolioModal.vue'
import ConfirmationModal from '../components/ConfirmationModal.vue'

interface CryptoPortfolio {
  id: number
  symbol: string
  quantity: number
  purchasePrice: number
  currentValue: number
  totalCost: number
}

const savedCryptos = ref<CryptoData[]>([])
const ownedCryptos = ref<CryptoPortfolio[]>([])
const showRemoveModal = ref(false)
const cryptoToRemove = ref<string | null>(null)
const selectedCryptoForPortfolio = ref<CryptoData | null>(null)
const showRemovePortfolioModal = ref(false)

const props = defineProps<{
  crypto: CryptoData
  isSaved: boolean
}>()

const emit = defineEmits<{
  (e: 'toggleSave', symbol: string): void
}>()

onMounted(async () => {
  await Promise.all([
    fetchSavedCryptos(),
    fetchOwnedCryptos()
  ])
})

const fetchSavedCryptos = async () => {
  try {
    const cryptos = await cryptoService.getSavedCryptos()
    // Sort cryptos alphabetically by symbol
    savedCryptos.value = cryptos.sort((a, b) => a.symbol.localeCompare(b.symbol))
  } catch (e) {
    console.error('Failed to fetch saved cryptos:', e)
  }
}

const closeRemovePortfolioModal = () => {
  showRemovePortfolioModal.value = false
  selectedCryptoForPortfolio.value = null
  refreshData()
}

const refreshData = async () => {
  await Promise.all([
    fetchSavedCryptos(),
    fetchOwnedCryptos()
  ])
}

const handlePortfolioSuccess = async () => {
  await refreshData()
}

const fetchOwnedCryptos = async () => {
  try {
    const response = await api.get('/cryptoportfolio')
    // Sort owned cryptos alphabetically
    ownedCryptos.value = response.data.sort((a: CryptoPortfolio, b: CryptoPortfolio) => 
      a.symbol.localeCompare(b.symbol))
  } catch (e) {
    console.error('Failed to fetch owned cryptos:', e)
  }
}

const formatChange = (change: number, changePercent: number): string => {
  return `${formatCurrency(change)} (${formatPercent(changePercent)})`
}

const getOwnershipInfo = (symbol: string) => {
  console.log('Checking ownership for:', symbol)
  console.log('Current owned cryptos:', ownedCryptos.value)
  
  const owned = ownedCryptos.value.find(crypto => crypto.symbol === symbol)
  console.log('Found ownership:', owned)
  
  if (!owned) return null
  
  return {
    quantity: owned.quantity,
    value: owned.currentValue
  }
}

const handleRemoveCrypto = (symbol: string) => {
  // Check if crypto is in portfolio
  console.log(symbol)
  const isInPortfolio = ownedCryptos.value.some(crypto => crypto.symbol === symbol)
  console.log('isInPortfolio:', isInPortfolio)
  
  if (isInPortfolio) {
    showErrorToast(`Cannot remove ${symbol} from watchlist while it is in your portfolio`, 3000)
    return
  }

  cryptoToRemove.value = symbol
  showRemoveModal.value = true
}

const handleConfirmRemoval = async () => {
  if (cryptoToRemove.value) {
    try {
        emit('toggleSave', cryptoToRemove.value)
    } catch (e) {
      console.error('Failed to remove crypto:', e)
    }
  }
  cancelRemoval()
}

const cancelRemoval = () => {
  showRemoveModal.value = false
  cryptoToRemove.value = null
}
</script>

<template>
  <div class="mt-6">
    <div class="flex justify-between items-center mb-3">
      <h3 class="text-lg font-semibold">Crypto Symbol: {{ crypto.symbol }}</h3>
      <button
        v-if="!props.isSaved"
         @click="emit('toggleSave', props.crypto.symbol)"
        class="px-4 py-2 bg-green-600 text-xs text-white rounded-lg hover:bg-green-800 transition-colors"
      >
        Add to Watchlist
      </button>
      <button
        v-else
        @click="handleRemoveCrypto(props.crypto.symbol)"
        class="px-4 py-2 bg-gray-600 text-xs text-white rounded-lg hover:bg-gray-800 transition-colors"
      >
        Remove
      </button>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Price</h3>
        <p class="text-lg font-semibold">{{ formatCurrency(Number(crypto.price)) }}</p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">24h Change</h3>
        <p :class="['text-lg font-semibold', crypto.change >= 0 ? 'text-green-600' : 'text-red-600']">
          {{ formatChange(crypto.change, crypto.changePercent) }}
        </p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">24h Volume</h3>
        <p class="text-lg font-semibold">{{ formatNumber(crypto.volume) }}</p>
      </div>
      <div class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">24h High/Low</h3>
        <p class="text-lg font-semibold">
          {{ formatCurrency(crypto.high24h) }} / {{ formatCurrency(crypto.low24h) }}
        </p>
      </div>
      <div v-if="getOwnershipInfo(crypto.symbol)" class="bg-gray-50 p-4 rounded-lg">
        <h3 class="text-sm text-gray-500">Your Position</h3>
        <p class="text-lg font-semibold">
          Quantity: {{ getOwnershipInfo(crypto.symbol)?.quantity }}
        </p>
        <p class="text-lg font-semibold">
          Value: {{ formatCurrency(getOwnershipInfo(crypto.symbol)?.value || 0) }}
        </p>
      </div>
    </div>

    <RemoveFromCryptoPortfolioModal
        v-if="selectedCryptoForPortfolio"
        :is-open="showRemovePortfolioModal"
        :symbol="selectedCryptoForPortfolio.symbol"
        :price="Number(selectedCryptoForPortfolio.price)"
        @close="closeRemovePortfolioModal"
        @success="handlePortfolioSuccess"
      />

      <ConfirmationModal
        :is-open="showRemoveModal"
        title="Remove Crypto"
        message="Are you sure you want to remove this crypto from your watchlist?"
        @confirm="handleConfirmRemoval"
        @cancel="cancelRemoval"
      />

  </div>
</template>