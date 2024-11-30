import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '../api/axios'

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

  // Initialize state from localStorage
  const initializeAuth = () => {
    const storedUser = localStorage.getItem('user')
    if (storedUser) {
      user.value = JSON.parse(storedUser)
      isAuthenticated.value = true
    }

    // Add event listener for tab/window close
    window.addEventListener('beforeunload', () => {
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