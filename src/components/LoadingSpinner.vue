<script setup lang="ts">
defineProps<{
  size?: 'sm' | 'md' | 'lg'
}>()
</script>

<template>
  <div class="flex justify-center items-center">
    <div 
      :class="[
        'animate-spin rounded-full border-t-transparent',
        size === 'sm' ? 'h-6 w-6 border-2' : 
        size === 'lg' ? 'h-12 w-12 border-4' : 
        'h-8 w-8 border-3',
        'border-indigo-500'
      ]"
    />
  </div>
</template>