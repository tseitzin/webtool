<script setup lang="ts">
defineProps<{
  title: string
  description: string
  icon: string
  route: string
}>()
</script>

<template>
  <router-link 
    :to="route"
    class="bg-white p-6 rounded-lg shadow-lg hover:shadow-xl transition-all duration-300 flex flex-col items-center"
  >
    <!-- <img :src="icon" :alt="title" class="w-16 h-16 mb-4" /> -->
    <h3 class="text-lg font-semibold text-gray-800 mb-2">{{ title }}</h3>
    <p class="text-gray-600 text-center text-sm">{{ description }}</p>
  </router-link>
</template>