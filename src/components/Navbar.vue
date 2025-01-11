<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()
const isMenuOpen = ref(false)
const isAdminDropdownOpen = ref(false)

const handleLogout = () => {
  auth.logout()
  router.push('/login')
}

const closeMenu = () => {
  isMenuOpen.value = false
  isAdminDropdownOpen.value = false
}

const toggleAdminDropdown = () => {
  isAdminDropdownOpen.value = !isAdminDropdownOpen.value
}

// Close dropdown when clicking outside
const handleClickOutside = (event: MouseEvent) => {
  const dropdown = document.getElementById('admin-dropdown')
  const button = document.getElementById('admin-dropdown-button')
  if (dropdown && button && !dropdown.contains(event.target as Node) && !button.contains(event.target as Node)) {
    isAdminDropdownOpen.value = false
  }
}

// Add event listener for click outside
onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<template>
  <nav class="bg-gray-800">
    <div class="container">
      <!-- Desktop Navigation -->
      <div class="hidden md:flex justify-between items-center py-4">
        <div class="flex space-x-4">
          <!-- Existing navigation links -->
          <router-link 
            v-if="!auth.isAuthenticated"
            to="/" 
            class="navbar_link"
            active-class="active"
          >
            Home
          </router-link>
          <router-link 
            v-if="auth.isAuthenticated"
            to="/landing" 
            class="navbar_link"
            active-class="active"
          >
            Home
          </router-link>
          <router-link 
            v-if="auth.isAuthenticated"
            to="/stock-dashboard" 
            class="navbar_link"
            active-class="active"
          >
            Stock Dashboard
          </router-link>
          <router-link 
            v-if="auth.isAuthenticated"
            to="/search-area" 
            class="navbar_link"
            active-class="active"
          >
            Stock Search
          </router-link>
          <router-link 
            v-if="auth.isAuthenticated"
            to="/crypto-dashboard" 
            class="navbar_link"
            active-class="active"
          >
            Crypto Dashboard
          </router-link>
          <router-link 
            v-if="auth.isAuthenticated"
            to="/crypto" 
            class="navbar_link"
            active-class="active"
          >
            Crypto Search
          </router-link>
          <router-link 
            v-if="auth.isAuthenticated"
            to="/portfolio-summary" 
            class="navbar_link"
            active-class="active"
          >
            Your Portfolio
          </router-link>

          <!-- Admin Dropdown -->
          <div v-if="auth.isAuthenticated && auth.user?.isAdmin" class="relative">
            <button
              id="admin-dropdown-button"
              @click="toggleAdminDropdown"
              class="navbar_link flex items-center"
              :class="{ 'active': isAdminDropdownOpen }"
            >
              Admin
              <svg
                class="ml-1 h-4 w-4"
                :class="{ 'transform rotate-180': isAdminDropdownOpen }"
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />
              </svg>
            </button>
            
            <!-- Dropdown Menu -->
            <div
              v-show="isAdminDropdownOpen"
              id="admin-dropdown"
              class="absolute right-0 mt-2 w-48 rounded-md shadow-lg bg-white ring-1 ring-black ring-opacity-5"
            >
              <div class="py-1">
                <router-link
                  to="/users"
                  class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  @click="isAdminDropdownOpen = false"
                >
                  Users
                </router-link>
                <router-link
                  to="/audit-logs"
                  class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  @click="isAdminDropdownOpen = false"
                >
                  Audit Logs
                </router-link>
              </div>
            </div>
          </div>
        </div>

        <!-- Rest of the navigation items -->
        <div class="flex space-x-4">
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
            >
              Logout
            </button>
          </template>
        </div>
      </div>

      <!-- Mobile Navigation -->
      <!-- Update the mobile menu to include the Admin dropdown -->
      <div class="md:hidden">
        <!-- ... existing mobile header ... -->
        <div v-show="isMenuOpen" class="pb-4">
          <div class="flex flex-col space-y-2">
            <!-- ... existing mobile links ... -->
            
            <!-- Admin Section for Mobile -->
            <div v-if="auth.isAuthenticated && auth.user?.isAdmin" class="px-4">
              <div class="text-gray-400 text-xs uppercase tracking-wider mb-2">Admin</div>
              <router-link
                to="/users"
                class="mobile_link"
                @click="closeMenu"
              >
                Users
              </router-link>
              <router-link
                to="/audit-logs"
                class="mobile_link"
                @click="closeMenu"
              >
                Audit Logs
              </router-link>
            </div>
          </div>
        </div>
      </div>
    </div>
  </nav>
</template>

<style scoped>
/* Keep existing styles */
</style>


<style scoped>
.container {
  max-width: 100%;
  margin-left: auto;
  margin-right: auto;
  padding-left: 1rem;
  padding-right: 1rem;
}

.navbar_link {
  color: #c8ced8;
  padding: 0.5rem 0.75rem;
  border-radius: 0.375rem;
  font-size: 1rem;
  font-weight: 500;
  transition: color 0.2s;
}

.navbar_link:hover {
  color: #ffffff;
}

.navbar_link.active {
  color: #a0c676;
}

.mobile_link {
  color: #c8ced8;
  padding: 0.75rem 1rem;
  display: block;
  font-size: 1rem;
  font-weight: 500;
  transition: all 0.2s;
}

.mobile_link:hover {
  color: #ffffff;
  background-color: rgba(255, 255, 255, 0.1);
}

.mobile_link.active {
  color: #a0c676;
  background-color: rgba(160, 198, 118, 0.1);
}
</style>