<script setup lang="ts">
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

const handleLogout = () => {
  auth.logout()
  router.push('/')
}
</script>

<template>
  <nav class="bg-gray-800 p-4">
    <div class="container">
      <div class="navbar_div">
        <router-link 
          to="/" 
          class="navbar_link"
          active-class="active"
        >
          Home
        </router-link>
        <router-link 
          to="/search-area" 
          class="navbar_link"
          active-class="active"
        >
          Bible Search
        </router-link>
        <router-link 
          v-if="auth.isAuthenticated && auth.user?.isAdmin"
          to="/users" 
          class="navbar_link"
          active-class="active"
        >
          Users
        </router-link>
          <router-link 
            v-if="auth.isAuthenticated && auth.user?.isAdmin"
            to="/audit-logs" 
            class="navbar_link"
            active-class="active"
          >
            Audit Logs
        </router-link>
      </div>
      <div class="navbar_div">
        <template v-if="!auth.isAuthenticated">
          <router-link 
            to="/login" 
            class="navbar_link"
            active-class="active"
          >
            Login
          </router-link>
          <router-link 
            to="/register" 
            class="navbar_link"
            active-class="active"
          >
            Register
          </router-link>
        </template>
        <template v-else>
          <router-link 
            to="/account" 
            class="navbar_link"
            active-class="active"
          >
            Account
          </router-link>
          <button 
            @click="handleLogout" 
            class="navbar_link"
            active-class="active"
          >
            Logout
          </button>
        </template>
      </div>
    </div>
  </nav>
</template>

<style scoped>
.container {
    max-width: 100%; /* Adjusts to responsive sizes based on screen width */
    margin-left: auto; /* Centers the container */
    margin-right: auto; /* Centers the container */
    padding-left: 3rem; 
    padding-right: 3rem; 
    display: flex; /* Makes the container a flexbox */
    align-items: center; /* Centers items vertically */
    justify-content: space-between; /* Distributes space evenly between items */
}

.navbar_div {
    display: flex; /* Makes the container a flexbox */
    gap: 1rem; /* Adds 1rem of horizontal space between each child (same as space-x-4) */
}

.navbar_link {
  color: #c8ced8; /* text-gray-300 */
  padding-left: 0.75rem; /* px-3 */
  padding-right: 0.75rem; /* px-3 */
  padding-top: 0.5rem; /* py-2 */
  padding-bottom: 0.5rem; /* py-2 */
  border-radius: 0.375rem; /* rounded-md */
  font-size: 1.25rem; /* text-xl */
  font-weight: 500; /* font-medium */
  transition: color 0.2s;
}

.navbar_link:hover {
    color: #ffffff;
}

.navbar_link.active {
    color: #a0c676; /* Color when the link is clicked */
}

</style>