import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '../api/axios'
import { AuthService } from '../services/authService'
import { TokenService } from '../services/tokenService'
import { activityMonitor } from '../utils/activityMonitor'
import { dismissAllToasts } from '../utils/toast'
import type { AuthResponse, User } from '../types/auth'


export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const isAuthenticated = ref(false)
  const authService = new AuthService()
  const tokenService = new TokenService()
  const router = useRouter()
  
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

  function setupAuthenticatedSession(authResponse: AuthResponse) {

    const { token, ...userData } = authResponse
    user.value = { ...userData, token }
    isAuthenticated.value = true

    tokenService.saveToken(token)
    tokenService.saveUser(user.value)
    tokenService.startTokenCheck(() => handleTokenExpiration())

    api.defaults.headers.common['Authorization'] = `Bearer ${token}`
    activityMonitor.startMonitoring(() => handleTokenExpiration())

    user.value = {
      email: authResponse.email,
      name: authResponse.name,
      token: authResponse.token,
      isAdmin: authResponse.isAdmin,
      lastLoginDate: authResponse.lastLoginDate,
      previousLoginDate: authResponse.previousLoginDate,
      createdDate: authResponse.createdDate,
      numberOfLogins: authResponse.numberOfLogins,
      failedLogins: authResponse.failedLogins
    }
  }


  

  const login = async (email: string, password: string) => {
    try {
      const response = await authService.login(email, password)
      setupAuthenticatedSession(response)
      router.push('/landing')
    } catch (error) {
      throw new Error('Invalid credentials')
    }
  }

  const register = async (email: string, password: string, name: string) => {
    try {
      const response = await authService.register(email, password, name)
      setupAuthenticatedSession(response)
      router.push('/landing')
    } catch (error) {
      throw new Error('Registration failed')
    }
  }

  const logout = async () => {
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