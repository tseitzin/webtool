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
  <div class="container mx-auto px-4 py-8">
    <h1 class="text-2xl font-bold mb-6">Registered Users</h1>
    <div class="bg-white shadow-md rounded-lg overflow-hidden">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr class="text-center bg-blue-200 border text-black font-bold">
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Name
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Email
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Role
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Joined
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Last Login
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Logins
            </th>
            <th class="px-6 py-3 text-xs uppercase tracking-wider">
              Failed
            </th>
            <th v-if="auth.user?.isAdmin" class="px-6 py-3 text-xs uppercase tracking-wider">
              Actions
            </th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr class="text-center text-black" v-for="user in users" :key="user.id">
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
              {{ user.name }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ user.email }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ user.isAdmin ? 'Admin' : 'User' }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ formatDate(user.createdDate) }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ user.lastLoginDate }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ user.numberOfLogins }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">
              {{ user.failedLogins }}
            </td>
            <td v-if="auth.user?.isAdmin" class="px-6 py-4 whitespace-nowrap text-sm">
              <div class="flex space-x-2">
                <button
                  v-if="!user.isAdmin"
                  @click="openResetModal(user.id)"
                  class="text-indigo-600 hover:text-indigo-900"
                >
                  Reset Password
                </button>
                <button
                  v-if="!user.isAdmin"
                  @click="deleteUser(user.id)"
                  class="text-red-600 hover:text-red-900"
                >
                  Delete
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Reset Password Modal -->
    <div v-if="showResetModal" class="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center">
      <div class="bg-white p-6 rounded-lg shadow-xl w-full max-w-md">
        <h3 class="text-lg font-medium mb-4">Reset User Password</h3>
        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700">New Password</label>
          <input
            type="text"
            v-model="newPassword"
            class="mt-1 block w-full rounded-md border-black border-2 shadow-sm focus:border-black focus:ring-indigo-500"
          />
        </div>
        <div class="flex justify-end space-x-2">
          <button
            @click="showResetModal = false"
            class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md"
          >
            Cancel
          </button>
          <button
            @click="resetPassword"
            class="px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md"
          >
            Reset Password
          </button>
        </div>
      </div>
    </div>
  </div>
</template>