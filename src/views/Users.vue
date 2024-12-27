<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '../api/axios'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

interface User {
  id: number;
  email: string;
  name: string;
  isAdmin: boolean;
  createdDate: string;
  lastLoginDate: string;
  numberOfLogins: number;
  failedLogins: number;
}

const users = ref<User[]>([])
const error = ref('')
const router = useRouter()
const auth = useAuthStore()
const newPassword = ref('')
const selectedUserId = ref<number | null>(null)
const showResetModal = ref(false)

onMounted(async () => {
  if (!auth.user?.isAdmin) {
    router.push('/access-denied')
  }
  try {
    const response = await api.get('/users')
    users.value = response.data
  } catch (e) {
    error.value = 'Failed to load users'
    if ((e as any).response?.status === 401) {
      router.push('/login')
    }
  }
})

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const deleteUser = async (userId: number) => {
  if (!confirm('Are you sure you want to delete this user?')) return

  try {
    await api.delete(`/users/${userId}`)
    users.value = users.value.filter(user => user.id !== userId)
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to delete user'
  }
}

const openResetModal = (userId: number) => {
  selectedUserId.value = userId
  showResetModal.value = true
  newPassword.value = ''
}

const resetPassword = async () => {
  if (!selectedUserId.value) return

  try {
    await api.post(`/users/${selectedUserId.value}/reset-password`, {
      newPassword: newPassword.value
    })
    showResetModal.value = false
    selectedUserId.value = null
    newPassword.value = ''
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to reset password'
  }
}

</script>

<template>
  <div class="container mx-auto px-4 py-4 sm:py-8">
    <h1 class="text-xl sm:text-2xl font-bold mb-4 sm:mb-6">Registered Users</h1>
    
    <!-- Error Message -->
    <div v-if="error" class="mb-4 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded text-sm sm:text-base">
      {{ error }}
    </div>

    <!-- Users Table - Scrollable Container -->
    <div class="bg-white shadow-md rounded-lg overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr class="text-center bg-blue-200 border text-black font-bold">
              <th class="px-3 sm:px-6 py-2 sm:py-3 text-xs uppercase tracking-wider whitespace-nowrap">
                Name
              </th>
              <th class="px-3 sm:px-6 py-2 sm:py-3 text-xs uppercase tracking-wider whitespace-nowrap">
                Email
              </th>
              <th class="px-3 sm:px-6 py-2 sm:py-3 text-xs uppercase tracking-wider whitespace-nowrap">
                Role
              </th>
              <th class="hidden sm:table-cell px-3 sm:px-6 py-2 sm:py-3 text-xs uppercase tracking-wider whitespace-nowrap">
                Joined
              </th>
              <th class="hidden md:table-cell px-3 sm:px-6 py-2 sm:py-3 text-xs uppercase tracking-wider whitespace-nowrap">
                Last Login
              </th>
              <th class="hidden lg:table-cell px-3 sm:px-6 py-2 sm:py-3 text-xs uppercase tracking-wider whitespace-nowrap">
                Logins
              </th>
              <th class="hidden lg:table-cell px-3 sm:px-6 py-2 sm:py-3 text-xs uppercase tracking-wider whitespace-nowrap">
                Failed
              </th>
              <th v-if="auth.user?.isAdmin" class="px-3 sm:px-6 py-2 sm:py-3 text-xs uppercase tracking-wider whitespace-nowrap">
                Actions
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="user in users" :key="user.id" class="text-center text-black hover:bg-gray-50">
              <td class="px-3 sm:px-6 py-2 sm:py-4 text-xs sm:text-sm font-medium text-gray-900 whitespace-nowrap">
                {{ user.name }}
              </td>
              <td class="px-3 sm:px-6 py-2 sm:py-4 text-xs sm:text-sm whitespace-nowrap">
                {{ user.email }}
              </td>
              <td class="px-3 sm:px-6 py-2 sm:py-4 text-xs sm:text-sm whitespace-nowrap">
                {{ user.isAdmin ? 'Admin' : 'User' }}
              </td>
              <td class="hidden sm:table-cell px-3 sm:px-6 py-2 sm:py-4 text-xs sm:text-sm whitespace-nowrap">
                {{ formatDate(user.createdDate) }}
              </td>
              <td class="hidden md:table-cell px-3 sm:px-6 py-2 sm:py-4 text-xs sm:text-sm whitespace-nowrap">
                {{ user.lastLoginDate }}
              </td>
              <td class="hidden lg:table-cell px-3 sm:px-6 py-2 sm:py-4 text-xs sm:text-sm whitespace-nowrap">
                {{ user.numberOfLogins }}
              </td>
              <td class="hidden lg:table-cell px-3 sm:px-6 py-2 sm:py-4 text-xs sm:text-sm whitespace-nowrap">
                {{ user.failedLogins }}
              </td>
              <td v-if="auth.user?.isAdmin" class="px-3 sm:px-6 py-2 sm:py-4 text-xs sm:text-sm whitespace-nowrap">
                <div class="flex flex-col sm:flex-row gap-2 justify-center">
                  <button
                    v-if="!user.isAdmin"
                    @click="openResetModal(user.id)"
                    class="text-indigo-600 hover:text-indigo-900 text-xs sm:text-sm"
                  >
                    Reset Password
                  </button>
                  <button
                    v-if="!user.isAdmin"
                    @click="deleteUser(user.id)"
                    class="text-red-600 hover:text-red-900 text-xs sm:text-sm"
                  >
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Reset Password Modal -->
    <div v-if="showResetModal" class="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center p-4 z-50">
      <div class="bg-white p-4 sm:p-6 rounded-lg shadow-xl w-full max-w-md">
        <h3 class="text-lg font-medium mb-4">Reset User Password</h3>
        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-1">New Password</label>
          <input
            type="text"
            v-model="newPassword"
            class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 text-sm"
          />
        </div>
        <div class="flex flex-col sm:flex-row justify-end gap-2">
          <button
            @click="showResetModal = false"
            class="w-full sm:w-auto px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md"
          >
            Cancel
          </button>
          <button
            @click="resetPassword"
            class="w-full sm:w-auto px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md"
          >
            Reset Password
          </button>
        </div>
      </div>
    </div>
  </div>
</template>