<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import api from '../api/axios'

interface AuditLog {
  id: number
  event: string
  email: string
  success: boolean
  failureReason: string | null
  ipAddress: string | null
  timestamp: string
}

interface Pagination {
  currentPage: number
  pageSize: number
  totalCount: number
  totalPages: number
}

const router = useRouter()
const auth = useAuthStore()
const logs = ref<AuditLog[]>([])
const error = ref('')
const loading = ref(false)
const currentPage = ref(1)
const pageSize = ref(10)

// Initialize pagination with default values
const pagination = ref<Pagination>({
  currentPage: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 0
})

// Filter states
const startDate = ref('')
const endDate = ref('')
const emailFilter = ref('')
const successFilter = ref('')

// Sorting states
const sortBy = ref('timestamp')
const sortOrder = ref('desc')

// Computed properties for pagination
const pageNumbers = computed(() => {
  const pages: number[] = []
  const currentPage = pagination.value.currentPage
  const totalPages = pagination.value.totalPages
  const maxPagesToShow = 5

  let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2))
  let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1)

  if (endPage - startPage + 1 < maxPagesToShow) {
    startPage = Math.max(1, endPage - maxPagesToShow + 1)
  }

  for (let i = startPage; i <= endPage; i++) {
    pages.push(i)
  }

  return pages
})

const showingFrom = computed(() => {
  const totalCount = pagination.value.totalCount
  if (totalCount === 0) return 0
  return ((pagination.value.currentPage - 1) * pagination.value.pageSize) + 1
})

const showingTo = computed(() => {
  return Math.min(
    pagination.value.currentPage * pagination.value.pageSize,
    pagination.value.totalCount
  )
})

onMounted(async () => {
  if (!auth.user?.isAdmin) {
    router.push('/')
    return
  }
  await fetchLogs()
})

const fetchLogs = async () => {
  loading.value = true
  error.value = ''
  
  try {
    let url = '/audit/logs?'
    if (startDate.value) url += `startDate=${startDate.value}&`
    if (endDate.value) url += `endDate=${endDate.value}&`
    if (emailFilter.value) url += `email=${encodeURIComponent(emailFilter.value)}&`
    if (successFilter.value) url += `success=${successFilter.value === 'true'}&`
    url += `sortBy=${sortBy.value}&`
    url += `sortOrder=${sortOrder.value}&`
    url += `page=${currentPage.value}&`
    url += `pageSize=${pageSize.value}`

    const response = await api.get(url)
    logs.value = response.data.logs
    
    // Update pagination with response data
    pagination.value = {
      currentPage: response.data.pagination.currentPage,
      pageSize: response.data.pagination.pageSize,
      totalCount: response.data.pagination.totalCount,
      totalPages: response.data.pagination.totalPages
    }
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to fetch audit logs'
    if (e.response?.status === 403) {
      router.push('/')
    }
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
  emailFilter.value = ''
  successFilter.value = ''
  currentPage.value = 1
  fetchLogs()
}

const handleSort = (column: string) => {
  if (sortBy.value === column) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortBy.value = column
    sortOrder.value = 'asc'
  }
  currentPage.value = 1
  fetchLogs()
}

const getSortIcon = (column: string) => {
  if (sortBy.value !== column) return '↕'
  return sortOrder.value === 'asc' ? '↑' : '↓'
}

const changePage = (page: number) => {
  currentPage.value = page
  fetchLogs()
}
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <h1 class="text-2xl font-bold mb-6">Audit Logs</h1>

    <!-- Filters -->
    <div class="bg-white p-4 rounded-lg shadow mb-6">
      <h2 class="text-lg font-semibold mb-4">Filters</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
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
          <label class="block text-sm font-medium text-gray-700">Email</label>
          <input
            type="text"
            v-model="emailFilter"
            placeholder="Filter by email"
            class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          />
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700">Status</label>
          <select
            v-model="successFilter"
            class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          >
            <option value="">All</option>
            <option value="true">Success</option>
            <option value="false">Failed</option>
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
          @click="fetchLogs"
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
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-indigo-500 border-t-transparent"></div>
    </div>

    <!-- Logs Table -->
    <div v-else class="bg-white shadow-md rounded-lg overflow-hidden">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="text-center bg-blue-200 border text-black font-bold">
          <tr>
            <th 
              @click="handleSort('timestamp')"
              class="px-6 py-3 text-xs uppercase tracking-wider cursor-pointer hover:bg-gray-100"
            >
              Timestamp {{ getSortIcon('timestamp') }}
            </th>
            <th 
              @click="handleSort('email')"
              class="px-6 py-3 text-xs uppercase tracking-wider cursor-pointer hover:bg-gray-100"
            >
              Email {{ getSortIcon('email') }}
            </th>
            <th 
              @click="handleSort('event')"
              class="px-6 py-3 text-xs uppercase tracking-wider cursor-pointer hover:bg-gray-100"
            >
              Event {{ getSortIcon('event') }}
            </th>
            <th 
              @click="handleSort('success')"
              class="px-6 py-3 text-xs uppercase tracking-wider cursor-pointer hover:bg-gray-100"
            >
              Status {{ getSortIcon('success') }}
            </th>
            <th 
              @click="handleSort('ipaddress')"
              class="px-6 py-3 text-xs uppercase tracking-wider cursor-pointer hover:bg-gray-100"
            >
              IP Address {{ getSortIcon('ipaddress') }}
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Failure Reason
            </th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr class="text-center text-black" v-for="log in logs" :key="log.id">
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ formatDate(log.timestamp) }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ log.email }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ log.event }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap">
              <span
                :class="[
                  'px-2 inline-flex text-xs leading-5 font-semibold rounded-full',
                  log.success
                    ? 'bg-green-100 text-green-800'
                    : 'bg-red-100 text-red-800'
                ]"
              >
                {{ log.success ? 'Success' : 'Failed' }}
              </span>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ log.ipAddress || 'N/A' }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ log.failureReason || 'N/A' }}
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination -->
      <div v-if="pagination.totalCount > 0" class="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
        <div class="flex-1 flex justify-between sm:hidden">
          <button
            @click="changePage(pagination.currentPage - 1)"
            :disabled="pagination.currentPage === 1"
            class="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
          >
            Previous
          </button>
          <button
            @click="changePage(pagination.currentPage + 1)"
            :disabled="pagination.currentPage === pagination.totalPages"
            class="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
          >
            Next
          </button>
        </div>
        <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
          <div>
            <p class="text-sm text-gray-700">
              Showing
              <span class="font-medium">{{ showingFrom }}</span>
              to
              <span class="font-medium">{{ showingTo }}</span>
              of
              <span class="font-medium">{{ pagination.totalCount }}</span>
              results
            </p>
          </div>
          <div>
            <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
              <button
                @click="changePage(1)"
                :disabled="pagination.currentPage === 1"
                class="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
              >
                <span class="sr-only">First</span>
                ««
              </button>
              <button
                @click="changePage(pagination.currentPage - 1)"
                :disabled="pagination.currentPage === 1"
                class="relative inline-flex items-center px-2 py-2 border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
              >
                <span class="sr-only">Previous</span>
                «
              </button>
              <button
                v-for="page in pageNumbers"
                :key="page"
                @click="changePage(page)"
                :class="[
                  'relative inline-flex items-center px-4 py-2 border text-sm font-medium',
                  page === pagination.currentPage
                    ? 'z-10 bg-indigo-50 border-indigo-500 text-indigo-600'
                    : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50'
                ]"
              >
                {{ page }}
              </button>
              <button
                @click="changePage(pagination.currentPage + 1)"
                :disabled="pagination.currentPage === pagination.totalPages"
                class="relative inline-flex items-center px-2 py-2 border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
              >
                <span class="sr-only">Next</span>
                »
              </button>
              <button
                @click="changePage(pagination.totalPages)"
                :disabled="pagination.currentPage === pagination.totalPages"
                class="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
              >
                <span class="sr-only">Last</span>
                »»
              </button>
            </nav>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>