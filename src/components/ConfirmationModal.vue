<script setup lang="ts">
defineProps<{
  isOpen: boolean
  title: string
  message: string
}>()

defineEmits<{
  (e: 'confirm'): void
  (e: 'cancel'): void
}>()
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
      <h2 class="text-xl font-bold mb-4">{{ title }}</h2>
      <p class="mb-6 text-gray-600">{{ message }}</p>
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
          Confirm
        </button>
      </div>
    </div>
  </div>
</template>