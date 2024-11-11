<script setup lang="ts">
import { ref, onMounted } from 'vue'
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

const router = useRouter()
const auth = useAuthStore()
const logs = ref<AuditLog[]>([])
const error = ref('')
const loading = ref(false)

// Filter states
const startDate = ref('')
const endDate = ref('')
const emailFilter = ref('')
const successFilter = ref('')

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

    const response = await api.get(url)
    logs.value = response.data
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
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Timestamp
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Email
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Event
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Status
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              IP Address
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Failure Reason
            </th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr class="text-center" v-for="log in logs" :key="log.id">
            <td class="px-6 py-4 whitespace-nowrap text-sm text-black">
              {{ formatDate(log.timestamp) }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-black">
              {{ log.email }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-black">
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
            <td class="px-6 py-4 whitespace-nowrap text-sm text-black">
              {{ log.ipAddress || 'N/A' }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-black">
              {{ log.failureReason || 'N/A' }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>