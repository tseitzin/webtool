<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import api from '../api/axios'
import router from '../router';

const auth = useAuthStore()
const currentPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const newEmail = ref('')
const successMessage = ref('')
const error = ref('')

// Password visibility toggles
const showCurrentPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/access-denied')
  }
})

const updatePassword = async () => {
  if (newPassword.value !== confirmPassword.value) {
    error.value = 'New passwords do not match'
    return
  }

  if (newPassword.value.length < 6) {
    error.value = 'Password must be at least 6 characters long'
    return
  }

  try {
    await api.post('/users/update-password', {
      currentPassword: currentPassword.value,
      newPassword: newPassword.value
    })
    successMessage.value = 'Password updated successfully'
    error.value = ''
    currentPassword.value = ''
    newPassword.value = ''
    confirmPassword.value = ''
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to update password'
    successMessage.value = ''
  }
}

const updateEmail = async () => {
  if (!newEmail.value) {
    error.value = 'Email is required'
    return
  }

  try {
    await api.post('/users/update-email', {
      newEmail: newEmail.value
    })
    auth.user!.email = newEmail.value
    localStorage.setItem('user', JSON.stringify(auth.user))
    successMessage.value = 'Email updated successfully'
    error.value = ''
    newEmail.value = ''
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to update email'
    successMessage.value = ''
  }
}
</script>

<template>
  <div class="min-h-screen bg-gray-100 py-12 px-4 sm:px-6 lg:px-8">
    <p class="text-2xl font-bold text-center mb-4">Welcome {{ auth.user?.name }}</p>
    <p class="text-lg mb-4 text-center">This is the Account Area. Here you can change your password or your email.</p>
    <div class="max-w-3xl mx-auto">
      <div class="bg-white shadow-md rounded-lg p-6 mb-6">
        <h2 class="text-2xl font-bold mb-6">Account Settings</h2>

        <!-- Success Message -->
        <div v-if="successMessage" class="mb-4 p-4 bg-green-100 text-green-700 rounded">
          {{ successMessage }}
        </div>

        <!-- Error Message -->
        <div v-if="error" class="mb-4 p-4 bg-red-100 text-red-700 rounded">
          {{ error }}
        </div>

        <!-- Change Password Section -->
        <div class="mb-8">
          <h3 class="text-lg font-semibold mb-4">Change Password</h3>
          <form @submit.prevent="updatePassword" class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700">Current Password</label>
              <div class="mt-1 relative rounded-md shadow-sm">
                <input
                  :type="showCurrentPassword ? 'text' : 'password'"
                  v-model="currentPassword"
                  required
                  class="block w-full rounded-md border-gray-300 border-2 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 pr-10"
                />
                <button
                  type="button"
                  @mousedown="showCurrentPassword = true"
                  @mouseup="showCurrentPassword = false"
                  @mouseleave="showCurrentPassword = false"
                  class="absolute inset-y-0 right-0 px-3 flex items-center group"
                >
                  <span class="absolute hidden group-hover:block bg-gray-900 text-white text-xs rounded py-1 px-2 right-0 top-7 whitespace-nowrap">
                    Hold to show password
                  </span>
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5 text-gray-400">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z" />
                    <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                </button>
              </div>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">New Password</label>
              <div class="mt-1 relative rounded-md shadow-sm">
                <input
                  :type="showNewPassword ? 'text' : 'password'"
                  v-model="newPassword"
                  required
                  minlength="6"
                  class="block w-full rounded-md border-gray-300 border-2 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 pr-10"
                />
                <button
                  type="button"
                  @mousedown="showNewPassword = true"
                  @mouseup="showNewPassword = false"
                  @mouseleave="showNewPassword = false"
                  class="absolute inset-y-0 right-0 px-3 flex items-center group"
                >
                  <span class="absolute hidden group-hover:block bg-gray-900 text-white text-xs rounded py-1 px-2 right-0 top-7 whitespace-nowrap">
                    Hold to show password
                  </span>
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5 text-gray-400">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z" />
                    <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                </button>
              </div>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Confirm New Password</label>
              <div class="mt-1 relative rounded-md shadow-sm">
                <input
                  :type="showConfirmPassword ? 'text' : 'password'"
                  v-model="confirmPassword"
                  required
                  minlength="6"
                  class="block w-full rounded-md border-gray-300 border-2 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 pr-10"
                />
                <button
                  type="button"
                  @mousedown="showConfirmPassword = true"
                  @mouseup="showConfirmPassword = false"
                  @mouseleave="showConfirmPassword = false"
                  class="absolute inset-y-0 right-0 px-3 flex items-center group"
                >
                  <span class="absolute hidden group-hover:block bg-gray-900 text-white text-xs rounded py-1 px-2 right-0 top-7 whitespace-nowrap">
                    Hold to show password
                  </span>
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5 text-gray-400">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z" />
                    <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                </button>
              </div>
            </div>
            <button
              type="submit"
              class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              Update Password
            </button>
          </form>
        </div>

        <!-- Change Email Section -->
        <div>
          <h3 class="text-lg font-semibold mb-4">Change Email</h3>
          <form @submit.prevent="updateEmail" class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700">New Email Address</label>
              <input
                type="email"
                v-model="newEmail"
                required
                class="mt-1 block w-full rounded-md border-gray-300 border-2 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
              />
            </div>
            <button
              type="submit"
              class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              Update Email
            </button>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>