<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter, useRoute } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const email = ref('')
const password = ref('')
const error = ref('')
const successMessage = ref('')

onMounted(() => {
  if (route.query.reset === 'success') {
    successMessage.value = 'Password has been reset successfully. Please login with your new password.'
  }
})

const handleSubmit = async () => {
  try {
    await auth.login(email.value, password.value)
    router.push('/dashboard')
  } catch (e) {
    error.value = 'Invalid credentials'
  }
}
</script>

<template>
  <div class="min-h-screen flex justify-center items-start bg-gradient-to-br from-gray-100 to-gray-300 py-12 px-4 sm:px-6 lg:px-8">
    <!-- Enhanced login box with branding, informative text, and sleek styling -->
    <div class="max-w-xl w-full space-y-9 bg-white p-12 rounded-lg shadow-2xl">
      
      <!-- Logo and Welcome Message -->
      <div class="text-center">
        <!-- <img src="/logo.png" alt="Stock Navigator Logo" class="mx-auto h-12 w-12" /> -->
        <h2 class="text-3xl font-bold text-gray-900 mt-4">Login to Stock Navigator</h2>
        <p class="text-sm text-gray-600 mt-2">
          Discover insights, stay informed, and make smarter investment decisions.
        </p>
      </div>

      <!-- Login Form -->
      <form class="mt-8 space-y-6" @submit.prevent="handleSubmit">
        <div class="rounded-md shadow-sm -space-y-px">
          <div>
            <label for="email" class="sr-only">Email address</label>
            <input
              id="email"
              v-model="email"
              type="email"
              required
              class="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-t-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
              placeholder="Email address"
            />
          </div>
          <div>
            <label for="password" class="sr-only">Password</label>
            <input
              id="password"
              v-model="password"
              type="password"
              required
              class="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-b-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
              placeholder="Password"
            />
          </div>
        </div>

        <!-- Remember Me and Forgot Password Links -->
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <input id="remember_me" name="remember_me" type="checkbox" class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded">
            <label for="remember_me" class="ml-2 block text-sm text-gray-900">Remember me</label>
          </div>
          <div class="text-sm">
            <router-link to="/forgot-password" class="font-medium text-indigo-600 hover:text-indigo-500">
              Forgot your password?
            </router-link>
          </div>
        </div>

        <!-- Sign-In Button -->
        <div>
          <button
            type="submit"
            class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
          >
            Sign in
          </button>
        </div>
      </form>

      <!-- Display Error Message if Login Fails -->
      <p v-if="error" class="mt-2 text-center text-sm text-red-600">
        {{ error }}
      </p>

      <!-- Sign-Up Prompt for New Users -->
      <p class="text-center text-sm text-gray-600">
        New to Stock Navigator?
        <router-link to="/register" class="font-medium text-indigo-600 hover:text-indigo-500">
          Create an account
        </router-link>
      </p>
    </div>
  </div>
</template>

