<script setup lang="ts">
const timeRanges = [
  { label: '1W', value: '1W' },
  { label: '1M', value: '1M' },
  { label: '3M', value: '3M' },
  { label: '6M', value: '6M' },
  { label: '1Y', value: '1Y' }
]

defineProps<{
  selectedRange: string
}>()

defineEmits<{
  (e: 'rangeSelected', range: string): void
}>()
</script>

<template>
  <div class="flex gap-2 mb-4">
    <button
      v-for="range in timeRanges"
      :key="range.value"
      @click="$emit('rangeSelected', range.value)"
      :class="[
        'px-3 py-1 rounded-md text-sm font-medium transition-colors',
        selectedRange === range.value
          ? 'bg-indigo-600 text-white'
          : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
      ]"
    >
      {{ range.label }}
    </button>
  </div>
</template>