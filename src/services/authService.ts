import api from '../api/axios'
import type { AuthResponse } from '../types/auth'

export class AuthService {
  async login(email: string, password: string): Promise<AuthResponse> {
    const { data } = await api.post<AuthResponse>('/auth/login', {
      email,
      password,
      name: ''
    })
    return data
  }

  async register(email: string, password: string, name: string): Promise<AuthResponse> {
    const { data } = await api.post<AuthResponse>('/auth/register', {
      email,
      password,
      name
    })
    return data
  }
}