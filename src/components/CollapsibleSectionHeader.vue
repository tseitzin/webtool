<script setup lang="ts">
defineProps<{
  title: string
  isExpanded: boolean
}>()

defineEmits<{
  (e: 'toggle'): void
}>()
</script>

<template>
  <div 
    class="p-4 flex justify-between items-center cursor-pointer hover:bg-gray-100"
    @click="$emit('toggle')"
  >
    <h1 class="text-md sm:text-2xl font-semibold text-gray-900">
      {{ title }}
    </h1>
    <button 
      class="px-4 py-1 sm:text-sm text-xs font-medium text-gray-600 hover:text-gray-900 bg-gray-200 hover:bg-gray-400 rounded-md transition-colors"
      :aria-label="isExpanded ? 'Collapse intro' : 'Expand intro'"
      :title="isExpanded ? 'Collapse intro' : 'Expand intro'"
    >
      {{ isExpanded ? 'Collapse Intro' : 'Expand Intro' }}
    </button>
  </div>
</template>