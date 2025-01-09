<script setup lang="ts">
defineProps<{
  marketStatus: string
}>()
</script>

<template>
  <div v-if="marketStatus.includes('closed')" class="bg-yellow-50 p-4 rounded-lg mb-4">
    <p class="text-red-700 font-semibold">{{ marketStatus }}</p>
  </div>
</template>