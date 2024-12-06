import { defineStore } from 'pinia'
import { ref, onBeforeUnmount } from 'vue'
import api from '../api/axios'
import { isTokenExpired } from '../utils/tokenUtils'
import { useRouter } from 'vue-router'
import { activityMonitor } from '../utils/activityMonitor'
import { dismissAllToasts } from '../utils/toast'

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

  // const parseJwt = (token: string) => {
  //   try {
  //     const base64Url = token.split('.')[1]
  //     const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
  //     const jsonPayload = decodeURIComponent(atob(base64).split('').map(c => {
  //       return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
  //     }).join(''))
  //     return JSON.parse(jsonPayload)
  //   } catch (e) {
  //     return null
  //   }
  // }

  const startTokenExpirationCheck = (token: string) => {
    if (tokenCheckInterval.value) {
      clearInterval(tokenCheckInterval.value)
    }

    tokenCheckInterval.value = window.setInterval(() => {
      if (isTokenExpired(token)) {
        dismissAllToasts()
        logout()
        router.push('/login')
      }
    }, 60000) // Check every minute
  }

  const initializeAuth = () => {
    const token = localStorage.getItem('token')
    const storedUser = localStorage.getItem('user')
    
    if (token && storedUser) {
      if (!isTokenExpired(token)) {
        const parsedUser = JSON.parse(storedUser)
        user.value = parsedUser
        isAuthenticated.value = true
        startTokenExpirationCheck(token)
        activityMonitor.startMonitoring(() => {
          dismissAllToasts()
          logout()
          router.push('/login')
        })

        // Set the token in axios headers
        api.defaults.headers.common['Authorization'] = `Bearer ${token}`
      } else {
        // Clear invalid token
        localStorage.removeItem('token')
        localStorage.removeItem('user')
        dismissAllToasts()
      }
    }
  }

  async function login(email: string, password: string) {
    try {
      const { data } = await api.post<AuthResponse>('/auth/login', {
        email,
        password,
        name: ''
      })

      const token = data.token
      user.value = {
        email: data.email,
        name: data.name,
        token: token,
        isAdmin: data.isAdmin
      }
      isAuthenticated.value = true

      localStorage.setItem('token', token)
      localStorage.setItem('user', JSON.stringify(user.value))

      // Set the token in axios headers
      api.defaults.headers.common['Authorization'] = `Bearer ${token}`

      startTokenExpirationCheck(token)
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
      })

      const token = data.token
      user.value = {
        email: data.email,
        name: data.name,
        token: token,
        isAdmin: data.isAdmin
      }
      isAuthenticated.value = true

      localStorage.setItem('token', token)
      localStorage.setItem('user', JSON.stringify(user.value))

      // Set the token in axios headers
      api.defaults.headers.common['Authorization'] = `Bearer ${token}`

      startTokenExpirationCheck(token)
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
    
    // Remove token from axios headers
    delete api.defaults.headers.common['Authorization']
    
    if (tokenCheckInterval.value) {
      clearInterval(tokenCheckInterval.value)
      tokenCheckInterval.value = null
    }

    activityMonitor.stopMonitoring()
    dismissAllToasts()
  }

  // Initialize auth state when the store is created
  initializeAuth()

  onBeforeUnmount(() => {
    if (tokenCheckInterval.value) {
      clearInterval(tokenCheckInterval.value)
    }
    activityMonitor.stopMonitoring()
    dismissAllToasts()
  })

  return {
    user,
    isAuthenticated,
    login,
    register,
    logout,
    initializeAuth
  }
})