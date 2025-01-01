import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createWebHistory, type Router } from 'vue-router'
import { createPinia, setActivePinia } from 'pinia'
import Landing from '../views/Landing.vue'
import { useAuthStore } from '../stores/auth'

// Mock services
vi.mock('../services/stockService', () => ({
  stockService: {
    getSavedStocks: vi.fn().mockResolvedValue([])
  }
}))

vi.mock('../services/cryptoService', () => ({
  cryptoService: {
    getSavedCryptos: vi.fn().mockResolvedValue([])
  }
}))

describe('Landing.vue', () => {
  let router: Router
  let authStore: ReturnType<typeof useAuthStore>

  beforeEach(() => {
    // Setup router
    router = createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: '/landing',
          name: 'landing',
          component: Landing
        },
        {
          path: '/login',
          name: 'login',
          component: { template: '<div>Login</div>' }
        }
      ]
    })

    // Setup Pinia
    setActivePinia(createPinia())
    authStore = useAuthStore()

    // Mock authenticated user
    vi.spyOn(authStore, 'isAuthenticated', 'get').mockReturnValue(true)
    vi.spyOn(authStore, 'user', 'get').mockReturnValue({
      email: 'test@example.com',
      name: 'Test User',
      token: 'fake-token',
      isAdmin: false,
      lastLoginDate: '2024-02-25T12:00:00Z',
      previousLoginDate: '2024-02-25T12:00:00Z',
      createdDate: '2024-02-25T12:00:00Z',
      numberOfLogins: 1,
      failedLogins: 0
    })
  })

  it('renders welcome message with user name', async () => {
    const wrapper = mount(Landing, {
      global: {
        plugins: [router],
        stubs: {
          NavigationTile: true
        }
      }
    })

    expect(wrapper.text()).toContain('Welcome back, Test User!')
  })

  it('displays navigation tiles', async () => {
    const wrapper = mount(Landing, {
      global: {
        plugins: [router],
        stubs: {
          NavigationTile: true
        }
      }
    })

    const tiles = wrapper.findAllComponents({ name: 'NavigationTile' })
    // Base tiles (non-admin)
    expect(tiles).toHaveLength(5)
  })

  it('displays admin tiles when user is admin', async () => {
    // Mock user as admin
    vi.spyOn(authStore, 'user', 'get').mockReturnValue({
      name: 'Admin User',
      email: 'admin@example.com',
      token: 'fake-token',
      isAdmin: true,
      lastLoginDate: '2024-02-25T12:00:00Z',
      previousLoginDate: '2024-02-25T12:00:00Z',
      createdDate: '2024-02-25T12:00:00Z',
      numberOfLogins: 1,
      failedLogins: 0
    })

    const wrapper = mount(Landing, {
      global: {
        plugins: [router],
        stubs: {
          NavigationTile: true
        }
      }
    })

    const tiles = wrapper.findAllComponents({ name: 'NavigationTile' })
    // Base tiles + admin tiles
    expect(tiles).toHaveLength(7)
  })

  it('redirects to login if not authenticated', async () => {
    // Mock user as not authenticated
    vi.spyOn(authStore, 'isAuthenticated', 'get').mockReturnValue(false)
    
    const pushSpy = vi.spyOn(router, 'push')

    mount(Landing, {
      global: {
        plugins: [router],
        stubs: {
          NavigationTile: true
        }
      }
    })

    expect(pushSpy).toHaveBeenCalledWith('/login')
  })

  it('displays quick overview section', async () => {
    const wrapper = mount(Landing, {
      global: {
        plugins: [router],
        stubs: {
          NavigationTile: true
        }
      }
    })

    expect(wrapper.text()).toContain('Quick Overview')
    expect(wrapper.text()).toContain('Saved Stocks')
    expect(wrapper.text()).toContain('Saved Crypto')
    expect(wrapper.text()).toContain('Last Login')
  })

  it('formats last login date correctly', async () => {
    const wrapper = mount(Landing, {
      global: {
        plugins: [router],
        stubs: {
          NavigationTile: true
        }
      }
    })

    const formattedDate = new Date('2024-02-25T12:00:00Z').toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
    expect(wrapper.text()).toContain(formattedDate)
  })
})