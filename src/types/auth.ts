export interface User {
    email: string
    name: string
    token: string
    isAdmin: boolean
    lastLoginDate: string
    createdDate: string
    numberOfLogins: number
    failedLogins: number
  }
  
  export interface AuthResponse {
    token: string
    email: string
    name: string
    isAdmin: boolean
    lastLoginDate: string
    createdDate: string
    numberOfLogins: number
    failedLogins: number
  }