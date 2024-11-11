<script setup lang="ts">
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

const handleLogout = () => {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <nav class="bg-gray-800 p-4">
    <div class="container mx-auto flex items-center justify-between">
      <div class="flex space-x-4">
        <router-link 
          to="/" 
          class="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
        >
          Home
        </router-link>
        <router-link 
          v-if="auth.isAuthenticated && auth.user?.isAdmin"
          to="/users" 
          class="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
        >
          Users
        </router-link>
          <router-link 
            v-if="auth.isAuthenticated && auth.user?.isAdmin"
            to="/audit-logs" 
            class="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
          >
            Audit Logs
        </router-link>
      </div>
      <div class="flex space-x-4">
        <template v-if="!auth.isAuthenticated">
          <router-link 
            to="/login" 
            class="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
          >
            Login
          </router-link>
          <router-link 
            to="/register" 
            class="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
          >
            Register
          </router-link>
        </template>
        <template v-else>
          <router-link 
            to="/account" 
            class="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
          >
            Account
          </router-link>
          <button 
            @click="handleLogout" 
            class="text-gray-300 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
          >
            Logout
          </button>
        </template>
      </div>
    </div>
  </nav>
</template>