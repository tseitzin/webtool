<script setup lang="ts">
defineProps<{
  isOpen: boolean
}>()

defineEmits<{
  (e: 'confirm'): void
  (e: 'cancel'): void
}>()
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
      <h2 class="text-xl font-bold text-red-600 mb-4">Delete Account</h2>
      
      <div class="mb-6">
        <p class="text-gray-700 mb-4">
          Are you sure you want to delete your account? This action cannot be undone.
        </p>
        <p class="text-gray-700 mb-4 font-semibold">
          All your data will be permanently deleted, including:
        </p>
        <ul class="list-disc list-inside text-gray-700 mb-4">
          <li>Saved stocks and cryptocurrencies</li>
          <li>Portfolio and watchlists</li>
          <li>Account settings and preferences</li>
        </ul>
      </div>

      <div class="flex justify-end space-x-4">
        <button
          @click="$emit('cancel')"
          class="px-4 py-2 text-gray-600 hover:text-gray-800"
        >
          Cancel
        </button>
        <button
          @click="$emit('confirm')"
          class="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
        >
          Delete Account
        </button>
      </div>
    </div>
  </div>
</template>