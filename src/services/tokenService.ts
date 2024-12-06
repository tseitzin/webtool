import { isTokenExpired } from '../utils/tokenUtils'
import type { User } from '../types/auth'

export class TokenService {
  private tokenCheckInterval: number | null = null
  private readonly SESSION_TOKEN_KEY = 'token'
  private readonly SESSION_USER_KEY = 'user'

  getToken(): string | null {
    return sessionStorage.getItem(this.SESSION_TOKEN_KEY)
  }

  getStoredUser(): User | null {
    const userStr = sessionStorage.getItem(this.SESSION_USER_KEY)
    return userStr ? JSON.parse(userStr) : null
  }

  saveToken(token: string): void {
    sessionStorage.setItem(this.SESSION_TOKEN_KEY, token)
  }

  saveUser(user: User): void {
    sessionStorage.setItem(this.SESSION_USER_KEY, JSON.stringify(user))
  }

  clearTokens(): void {
    sessionStorage.removeItem(this.SESSION_TOKEN_KEY)
    sessionStorage.removeItem(this.SESSION_USER_KEY)
    this.stopTokenCheck()
  }

  isTokenExpired(token: string): boolean {
    return isTokenExpired(token)
  }

  startTokenCheck(onExpiration: () => void): void {
    this.stopTokenCheck()
    this.tokenCheckInterval = window.setInterval(() => {
      const token = this.getToken()
      if (token && this.isTokenExpired(token)) {
        onExpiration()
      }
    }, 60000) // Check every minute
  }

  stopTokenCheck(): void {
    if (this.tokenCheckInterval) {
      clearInterval(this.tokenCheckInterval)
      this.tokenCheckInterval = null
    }
  }
}