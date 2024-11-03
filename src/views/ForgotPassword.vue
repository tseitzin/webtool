<script setup lang="ts">
import { ref } from 'vue'
import api from '../api/axios'

const email = ref('')
const message = ref('')
const error = ref('')
const isSubmitting = ref(false)

const handleSubmit = async () => {
  isSubmitting.value = true
  error.value = ''
  message.value = ''
  
  try {
    const response = await api.post('/auth/forgot-password', { email: email.value })
    message.value = response.data.message
    email.value = ''
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
          Forgot Password
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600">
          Enter your email address and we'll send you a link to reset your password.
        </p>
      </div>
      
      <form class="mt-8 space-y-6" @submit.prevent="handleSubmit">
        <div>
          <label for="email" class="sr-only">Email address</label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            class="appearance-none rounded relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
            placeholder="Email address"
          />
        </div>

        <div>
          <button
            type="submit"
            :disabled="isSubmitting"
            class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
          >
            {{ isSubmitting ? 'Sending...' : 'Send Reset Link' }}
          </button>
        </div>
      </form>

      <div v-if="message" class="mt-2 text-center text-sm text-green-600">
        {{ message }}
      </div>
      
      <div v-if="error" class="mt-2 text-center text-sm text-red-600">
        {{ error }}
      </div>

      <div class="text-center">
        <router-link to="/login" class="text-sm text-indigo-600 hover:text-indigo-500">
          Back to Login
        </router-link>
      </div>
    </div>
  </div>
</template>