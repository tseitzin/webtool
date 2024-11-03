<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '../api/axios'

const route = useRoute()
const router = useRouter()
const newPassword = ref('')
const confirmPassword = ref('')
const error = ref('')
const isSubmitting = ref(false)

const token = route.query.token as string
const email = route.query.email as string

onMounted(() => {
  if (!token || !email) {
    router.push('/login')
  }
})

const handleSubmit = async () => {
  if (newPassword.value !== confirmPassword.value) {
    error.value = 'Passwords do not match'
    return
  }

  isSubmitting.value = true
  error.value = ''

  try {
    await api.post('/auth/reset-password', {
      token,
      email,
      newPassword: newPassword.value
    })
    router.push('/login?reset=success')
  } catch (e: any) {
    error.value = e.response?.data?.message || 'An error occurred'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
          Reset Password
        </h2>
      </div>
      
      <form class="mt-8 space-y-6" @submit.prevent="handleSubmit">
        <div class="rounded-md shadow-sm -space-y-px">
          <div>
            <label for="new-password" class="sr-only">New Password</label>
            <input
              id="new-password"
              v-model="newPassword"
              type="password"
              required
              class="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-t-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
              placeholder="New Password"
            />
          </div>
          <div>
            <label for="confirm-password" class="sr-only">Confirm Password</label>
            <input
              id="confirm-password"
              v-model="confirmPassword"
              type="password"
              required
              class="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-b-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
              placeholder="Confirm Password"
            />
          </div>
        </div>

        <div>
          <button
            type="submit"
            :disabled="isSubmitting"
            class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
          >
            {{ isSubmitting ? 'Resetting...' : 'Reset Password' }}
          </button>
        </div>
      </form>

      <div v-if="error" class="mt-2 text-center text-sm text-red-600">
        {{ error }}
      </div>
    </div>
  </div>
</template>