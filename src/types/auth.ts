export interface User {
    email: string
    name: string
    token: string
    isAdmin: boolean
  }
  
  export interface AuthResponse {
    token: string
    email: string
    name: string
    isAdmin: boolean
  }