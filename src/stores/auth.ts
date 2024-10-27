import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '../api/axios'

interface User {
  email: string;
  name: string;
  token: string;
}

interface AuthResponse {
  token: string;
  email: string;
  name: string;
}

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const isAuthenticated = ref(false)

  // Initialize state from localStorage
  const initializeAuth = () => {
    const storedUser = localStorage.getItem('user')
    if (storedUser) {
      user.value = JSON.parse(storedUser)
      isAuthenticated.value = true
    }
  }

  async function login(email: string, password: string) {
    try {
      const { data } = await api.post<AuthResponse>('/auth/login', {
        email,
        password,
      }, {
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      })

      user.value = {
        email: data.email,
        name: data.name,
        token: data.token
      }
      isAuthenticated.value = true

      // Store in localStorage
      localStorage.setItem('token', data.token)
      localStorage.setItem('user', JSON.stringify(user.value))
    } catch (error) {
      throw new Error('Invalid credentials')
    }
  }

  async function register(email: string, password: string) {
    try {
      const { data } = await api.post<AuthResponse>('/auth/register', {
        email,
        password,
      }, {
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      })

      user.value = {
        email: data.email,
        name: data.name,
        token: data.token
      }
      isAuthenticated.value = true

      // Store in localStorage
      localStorage.setItem('token', data.token)
      localStorage.setItem('user', JSON.stringify(user.value))
    } catch (error) {
      throw new Error('Registration failed')
    }
  }

  function logout() {
    user.value = null
    isAuthenticated.value = false
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  // Initialize auth state when store is created
  initializeAuth()

  return {
    user,
    isAuthenticated,
    login,
    register,
    logout
  }
})