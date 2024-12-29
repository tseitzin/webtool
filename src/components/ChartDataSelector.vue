<script setup lang="ts">
const dataTypes = [
  { label: 'Price', value: 'price' },
  { label: 'Volume', value: 'volume' }
]

defineProps<{
  selectedType: string
}>()

defineEmits<{
  (e: 'dataTypeSelected', type: string): void
}>()
</script>

<template>
  <div class="flex gap-2">
    <button
      v-for="type in dataTypes"
      :key="type.value"
      @click="$emit('dataTypeSelected', type.value)"
      :class="[
        'px-3 py-1 rounded-md text-sm font-medium transition-colors',
        selectedType === type.value
          ? 'bg-indigo-600 text-white'
          : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
      ]"
    >
      {{ type.label }}
    </button>
  </div>
</template>