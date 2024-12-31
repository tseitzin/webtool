import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import Home from '../views/Home.vue'
import { createRouter, createWebHistory, type Router } from 'vue-router'

describe('Home.vue', () => {
  let router: Router

  beforeEach(() => {
    router = createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: '/',
          name: 'home',
          component: Home
        },
        {
          path: '/login',
          name: 'login',
          component: { template: '<div>Login</div>' }
        },
        {
          path: '/register',
          name: 'register',
          component: { template: '<div>Register</div>' }
        }
      ]
    })
  })

  it('renders welcome message', () => {
    const wrapper = mount(Home, {
      global: {
        plugins: [router]
      }
    })
    expect(wrapper.text()).toContain('Welcome to the Stock Navigator')
  })

  it('displays navigation tiles', () => {
    const wrapper = mount(Home, {
      global: {
        plugins: [router]
      }
    })
    const tiles = wrapper.findAll('.tile')
    expect(tiles).toHaveLength(2)
  })

  it('displays correct tile descriptions', () => {
    const wrapper = mount(Home, {
      global: {
        plugins: [router]
      }
    })
    expect(wrapper.text()).toContain('Login and start trading!')
    expect(wrapper.text()).toContain('Sign-up and start your financial journey')
  })
})