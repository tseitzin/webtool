import { defineStore } from 'pinia'
import { ref, onBeforeUnmount } from 'vue'
import api from '../api/axios'
import { useRouter } from 'vue-router';
import { isTokenExpired } from '../utils/tokenUtils';
import { activityMonitor } from '../utils/activityMonitor';
import { dismissAllToasts } from '../utils/toast';

interface User {
  email: string;
  name: string;
  token: string;
  isAdmin: boolean;
}

interface AuthResponse {
  token: string;
  email: string;
  name: string;
  isAdmin: boolean;
}

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const isAuthenticated = ref(false)
  const tokenCheckInterval = ref<number | null>(null)
  const router = useRouter()

  const parseJwt = (token: string) => {
    try {
      const base64Url = token.split('.')[1]
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
      const jsonPayload = decodeURIComponent(atob(base64).split('').map(c => {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
      }).join(''))
      return JSON.parse(jsonPayload)
    } catch (e) {
      return null
    }
  }

  const startTokenExpirationCheck = (token: string) => {
    // Clear any existing interval
    if (tokenCheckInterval.value) {
      clearInterval(tokenCheckInterval.value)
    }

    // Set up new interval to check token expiration
    tokenCheckInterval.value = window.setInterval(() => {
      if (isTokenExpired(token)) {
        dismissAllToasts()
        logout()
        router.push('/login')
      }
    }, 30000) // Check every 30 seconds
  }

  // Initialize state from localStorage
  const initializeAuth = () => {
    const token = localStorage.getItem('token')
    const storedUser = localStorage.getItem('user')
    
    if (token && storedUser) {
      if (!isTokenExpired(token)) {
        user.value = JSON.parse(storedUser)
        isAuthenticated.value = true
        startTokenExpirationCheck(token)
        activityMonitor.startMonitoring(() => {
          dismissAllToasts()
          logout()
          router.push('/login')
        })
      } else {
        dismissAllToasts()
        logout()
      }
    }

    // Add event listener for tab/window close
    window.addEventListener('beforeunload', () => {
      dismissAllToasts()
      logout()
    })
  }

  async function login(email: string, password: string) {
    try {
      const { data } = await api.post<AuthResponse>('/auth/login', {
        email,
        password,
        name
      }, {
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      })

      const decodedToken = parseJwt(data.token)
      //const isAdmin = decodedToken?.IsAdmin?.toLowerCase() === 'true'

      user.value = {
        email: data.email,
        name: data.name,
        token: decodedToken,
        isAdmin: data.isAdmin
      }
      isAuthenticated.value = true

      // Store in localStorage
      localStorage.setItem('token', data.token)
      localStorage.setItem('user', JSON.stringify(user.value))

      // Start token expiration check
      startTokenExpirationCheck(data.token)

      // Start activity monitoring
      activityMonitor.startMonitoring(() => {
        dismissAllToasts()
        logout()
        router.push('/login')
      })

    } catch (error) {
      throw new Error('Invalid credentials')
    }
  }

  async function register(email: string, password: string, name: string) {
    try {
      const { data } = await api.post<AuthResponse>('/auth/register', {
        email,
        password,
        name
      }, {
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      })

      const token = data.token
      const decodedToken = JSON.parse(atob(token.split('.')[1]))
      //const isAdmin = decodedToken.IsAdmin === 'True'

      user.value = {
        email: data.email,
        name: data.name,
        token: decodedToken,
        isAdmin: data.isAdmin
      }
      isAuthenticated.value = true

      // Store in localStorage
      localStorage.setItem('token', data.token)
      localStorage.setItem('user', JSON.stringify(user.value))

      // Start token expiration check
      startTokenExpirationCheck(data.token)

      // Start activity monitoring
      activityMonitor.startMonitoring(() => {
        dismissAllToasts()
        logout()
        router.push('/login')
      })

    } catch (error) {
      throw new Error('Registration failed')
    }
  }

  function logout() {
    user.value = null
    isAuthenticated.value = false
    localStorage.removeItem('token')
    localStorage.removeItem('user')

    // Clear token check interval
    if (tokenCheckInterval.value) {
      clearInterval(tokenCheckInterval.value)
      tokenCheckInterval.value = null
    }

    // Stop activity monitoring
    activityMonitor.stopMonitoring()
    dismissAllToasts()
  }

  // Initialize auth state when store is created
  initializeAuth()

  // Cleanup on component unmount
  onBeforeUnmount(() => {
    if (tokenCheckInterval.value) {
      clearInterval(tokenCheckInterval.value)
    }
    // Stop activity monitoring
    activityMonitor.stopMonitoring()
    dismissAllToasts()
  })

  return {
    user,
    isAuthenticated,
    login,
    register,
    logout
  }
})