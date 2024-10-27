<script setup lang="ts">
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <h1 class="text-2xl font-bold mb-4">Welcome {{ auth.user?.name }}!</h1>
    <p class="text-gray-600">
      This is a protected page that can only be accessed when logged in.
    </p>
  </div>
</template>