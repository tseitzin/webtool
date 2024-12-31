<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()
const isMenuOpen = ref(false)

const handleLogout = () => {
  auth.logout()
  router.push('/login')
}

const closeMenu = () => {
  isMenuOpen.value = false
}
</script>

<template>
  <nav class="bg-gray-800">
    <div class="container">
      <!-- Desktop Navigation -->
      <div class="hidden md:flex justify-between items-center py-4">
        <div class="flex space-x-4">
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
              active-class="active"
            >
              Logout
            </button>
          </template>
        </div>
      </div>

      <!-- Mobile Navigation -->
      <div class="md:hidden">
        <div class="flex justify-between items-center py-4">
          <router-link 
            to="/"
            class="text-white text-xl font-bold"
          >
            Stock Nav
          </router-link>
          <button 
            @click="isMenuOpen = !isMenuOpen"
            class="text-gray-300 hover:text-white focus:outline-none"
          >
            <svg 
              class="h-6 w-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path 
                v-if="!isMenuOpen"
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M4 6h16M4 12h16M4 18h16"
              />
              <path
                v-else
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>

        <!-- Mobile Menu -->
        <div 
          v-show="isMenuOpen"
          class="pb-4"
        >
          <div class="flex flex-col space-y-2">
            <router-link 
              v-if="!auth.isAuthenticated"
              to="/" 
              class="mobile_link"
              active-class="active"
              @click="closeMenu"
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
              class="mobile_link"
              active-class="active"
              @click="closeMenu"
            >
              Stock Dashboard
            </router-link>
            <router-link 
              v-if="auth.isAuthenticated"
              to="/crypto-dashboard" 
              class="mobile_link"
              active-class="active"
              @click="closeMenu"
            >
              Crypto Dashboard
            </router-link>
            <router-link 
              v-if="auth.isAuthenticated"
              to="/search-area" 
              class="mobile_link"
              active-class="active"
              @click="closeMenu"
            >
              Stock Search
            </router-link>
            <router-link 
              v-if="auth.isAuthenticated && auth.user?.isAdmin"
              to="/users" 
              class="mobile_link"
              active-class="active"
              @click="closeMenu"
            >
              Users
            </router-link>
            <router-link 
              v-if="auth.isAuthenticated && auth.user?.isAdmin"
              to="/audit-logs" 
              class="mobile_link"
              active-class="active"
              @click="closeMenu"
            >
              Audit Logs
            </router-link>

            <template v-if="!auth.isAuthenticated">
              <router-link 
                to="/login" 
                class="mobile_link"
                active-class="active"
                @click="closeMenu"
              >
                Login
              </router-link>
              <router-link 
                to="/register" 
                class="mobile_link"
                active-class="active"
                @click="closeMenu"
              >
                Register
              </router-link>
            </template>
            <template v-else>
              <router-link 
                to="/account" 
                class="mobile_link"
                active-class="active"
                @click="closeMenu"
              >
                Account
              </router-link>
              <button 
                @click="() => { handleLogout(); closeMenu(); }"
                class="mobile_link text-left"
              >
                Logout
              </button>
            </template>
          </div>
        </div>
      </div>
    </div>
  </nav>
</template>

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