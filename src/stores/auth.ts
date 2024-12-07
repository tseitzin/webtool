import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '../api/axios'
import { AuthService } from '../services/authService'
import { TokenService } from '../services/tokenService'
import { activityMonitor } from '../utils/activityMonitor'
import { dismissAllToasts } from '../utils/toast'
import type { User, AuthResponse } from '../types/auth'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const isAuthenticated = ref(false)
  const router = useRouter()
  const authService = new AuthService()
  const tokenService = new TokenService()
  let isInitialized = false

  const initializeAuth = () => {
    if (isInitialized) return
    
    const token = tokenService.getToken()
    const storedUser = tokenService.getStoredUser()
    
    if (token && storedUser && !tokenService.isTokenExpired(token)) {
      user.value = storedUser
      isAuthenticated.value = true
      tokenService.startTokenCheck(() => handleTokenExpiration())
      activityMonitor.startMonitoring(() => handleTokenExpiration())
      api.defaults.headers.common['Authorization'] = `Bearer ${token}`
    } else {
      tokenService.clearTokens()
      dismissAllToasts()
    }
    
    isInitialized = true
  }

  const handleTokenExpiration = () => {
    dismissAllToasts()
    logout()
    router.push('/login')
  }

  async function login(email: string, password: string) {
    try {
      const response = await authService.login(email, password)
      setupAuthenticatedSession(response)
    } catch (error) {
      throw new Error('Invalid credentials')
    }
  }

  async function register(email: string, password: string, name: string) {
    try {
      const response = await authService.register(email, password, name)
      setupAuthenticatedSession(response)
    } catch (error) {
      throw new Error('Registration failed')
    }
  }

  function setupAuthenticatedSession(authResponse: AuthResponse) {
    const { token, ...userData } = authResponse
    user.value = { ...userData, token }
    isAuthenticated.value = true

    tokenService.saveToken(token)
    tokenService.saveUser(user.value)
    tokenService.startTokenCheck(() => handleTokenExpiration())
    
    api.defaults.headers.common['Authorization'] = `Bearer ${token}`
    activityMonitor.startMonitoring(() => handleTokenExpiration())
  }

  function logout() {
    user.value = null
    isAuthenticated.value = false
    tokenService.clearTokens()
    delete api.defaults.headers.common['Authorization']
    activityMonitor.stopMonitoring()
    dismissAllToasts()
  }

  return {
    user,
    isAuthenticated,
    login,
    register,
    logout,
    initializeAuth
  }
})