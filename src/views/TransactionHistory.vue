<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import api from '../api/axios'
import { formatCurrency, formatNumber } from '../utils/formatters'

interface Transaction {
  id: number
  symbol: string
  type: string
  transactionType: string
  quantity: number
  price: number
  transactionTotal: number
  transactionDate: string
}

const router = useRouter()
const auth = useAuthStore()
const transactions = ref<Transaction[]>([])
const loading = ref(true)
const error = ref('')

// Filter states
const startDate = ref('')
const endDate = ref('')
const symbolFilter = ref('')
const typeFilter = ref('')
const transactionTypeFilter = ref('')

// Sorting states
const sortBy = ref('transactionDate')
const sortOrder = ref('desc')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/login')
    return
  }
  await fetchTransactions()
})

const fetchTransactions = async () => {
  loading.value = true
  error.value = ''
  
  try {
    let url = '/transaction?'
    if (startDate.value) {
      const formattedStartDate = new Date(startDate.value).toISOString()
      url += `startDate=${formattedStartDate}&`
    }
    if (endDate.value) {
      const formattedEndDate = new Date(endDate.value).toISOString()
      url += `endDate=${formattedEndDate}&`
    }
    if (symbolFilter.value) {
      url += `symbol=${encodeURIComponent(symbolFilter.value.trim())}&`
    }
    if (typeFilter.value) {
      url += `type=${typeFilter.value}&`
    }
    if (transactionTypeFilter.value) {
      url += `transactionType=${transactionTypeFilter.value}&`
    }
    url += `sortBy=${sortBy.value}&`
    url += `sortOrder=${sortOrder.value}`

    const response = await api.get(url)
    transactions.value = response.data
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to fetch transactions'
  } finally {
    loading.value = false
  }
}


const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString()
}

const clearFilters = () => {
  startDate.value = ''
  endDate.value = ''
  symbolFilter.value = ''
  typeFilter.value = ''
  transactionTypeFilter.value = ''
  fetchTransactions()
}

const handleSort = (column: string) => {
  if (sortBy.value === column) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortBy.value = column
    sortOrder.value = 'asc'
  }
  fetchTransactions()
}

const getSortIcon = (column: string) => {
  if (sortBy.value !== column) return '↕'
  return sortOrder.value === 'asc' ? '↑' : '↓'
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-6">Transaction History</h1>

      <!-- Filters -->
      <div class="bg-white p-4 rounded-lg shadow mb-6">
        <h2 class="text-lg font-semibold mb-4">Filters</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700">Start Date</label>
            <input
              type="datetime-local"
              v-model="startDate"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">End Date</label>
            <input
              type="datetime-local"
              v-model="endDate"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Symbol</label>
            <input
              type="text"
              v-model="symbolFilter"
              placeholder="Filter by symbol"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Purchase Type</label>
            <select
              v-model="typeFilter"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            >
              <option value="">All</option>
              <option value="STOCK">Stock</option>
              <option value="CRYPTO">Crypto</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Transaction Type</label>
            <select
              v-model="transactionTypeFilter"
              class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            >
              <option value="">All</option>
              <option value="BUY">Buy</option>
              <option value="SELL">Sell</option>
            </select>
          </div>
        </div>
        <div class="mt-4 flex justify-end space-x-2">
          <button
            @click="clearFilters"
            class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md"
          >
            Clear Filters
          </button>
          <button
            @click="fetchTransactions"
            class="px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md"
          >
            Apply Filters
          </button>
        </div>
      </div>

      <!-- Error Message -->
      <div v-if="error" class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
        {{ error }}
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-4">
        <LoadingSpinner size="lg" />
      </div>

      <!-- Transactions Table -->
      <div v-else class="bg-white shadow-md rounded-lg overflow-hidden">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th 
                  @click="handleSort('transactionDate')"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                >
                  Date {{ getSortIcon('transactionDate') }}
                </th>
                <th 
                  @click="handleSort('type')"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                >
                  Purchase Type {{ getSortIcon('type') }}
                </th>
                <th 
                  @click="handleSort('symbol')"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                >
                  Symbol {{ getSortIcon('symbol') }}
                </th>
                <th 
                  @click="handleSort('transactionType')"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                >
                  Type {{ getSortIcon('transactionType') }}
                </th>
                <th 
                  @click="handleSort('quantity')"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                >
                  Quantity {{ getSortIcon('quantity') }}
                </th>
                <th 
                  @click="handleSort('price')"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                >
                  Price {{ getSortIcon('price') }}
                </th>
                <th 
                  @click="handleSort('transactionTotal')"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                >
                  Total {{ getSortIcon('transactionTotal') }}
                </th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="transaction in transactions" :key="transaction.id">
                <td class="px-6 py-4 whitespace-nowrap text-sm">
                  {{ formatDate(transaction.transactionDate) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span
                    :class="[
                      'px-2 inline-flex text-xs leading-5 font-semibold rounded-full',
                      transaction.type === 'STOCK'
                        ? 'bg-blue-100 text-green-800'
                        : 'bg-orange-100 text-orange-700'
                    ]"
                  >
                    {{ transaction.type }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  {{ transaction.symbol }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span
                    :class="[
                      'px-2 inline-flex text-xs leading-5 font-semibold rounded-full',
                      transaction.transactionType === 'BUY'
                        ? 'bg-green-100 text-green-800'
                        : 'bg-red-100 text-red-800'
                    ]"
                  >
                    {{ transaction.transactionType }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm">
                  {{ formatNumber(transaction.quantity) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm">
                  {{ formatCurrency(transaction.price) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm">
                  {{ formatCurrency(transaction.transactionTotal) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>