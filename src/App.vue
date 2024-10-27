<script setup lang="ts">
import Navbar from './components/Navbar.vue'
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <Navbar />
    <main>
      <router-view></router-view>
    </main>
  </div>
</template>