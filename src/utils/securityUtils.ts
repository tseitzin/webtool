import { jwtDecode } from 'jwt-decode'
import { Buffer } from 'buffer'

interface SecurityHeaders {
  [key: string]: string
}

export const getSecurityHeaders = (): SecurityHeaders => ({
  'X-Content-Type-Options': 'nosniff',
  'X-Frame-Options': 'DENY',
  'X-XSS-Protection': '1; mode=block',
  'Strict-Transport-Security': 'max-age=31536000; includeSubDomains',
  'Content-Security-Policy': "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline';"
})

export const sanitizeInput = (input: string): string => {
  return input.replace(/[<>]/g, '')
    .replace(/&/g, '&amp;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;')
    .replace(/\//g, '&#x2F;')
}

export const validateToken = (token: string): boolean => {
  try {
    const decoded = jwtDecode(token)
    return !!decoded
  } catch {
    return false
  }
}

export const encryptData = (data: string): string => {
  // For client-side encryption, we'll use a simpler approach
  // since we can't securely store encryption keys in the browser
  return Buffer.from(data).toString('base64')
}

export const decryptData = (encryptedData: string): string => {
  try {
    return Buffer.from(encryptedData, 'base64').toString('utf-8')
  } catch {
    return ''
  }
}

export const hashData = (data: string): string => {
  // Simple hash function for client-side use
  let hash = 0
  for (let i = 0; i < data.length; i++) {
    const char = data.charCodeAt(i)
    hash = ((hash << 5) - hash) + char
    hash = hash & hash
  }
  return hash.toString(36)
}