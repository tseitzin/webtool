// src/services/logService.ts
import api from '../api/axios'

export interface LogEntry {
  level: 'info' | 'error' | 'warn'
  message: string
  context?: Record<string, any>
  error?: Error
  timestamp?: string
}

class LogService {
  async log(entry: LogEntry): Promise<void> {
    try {
      await api.post('/logs', {
        ...entry,
        timestamp: new Date().toISOString(),
        error: entry.error ? {
          message: entry.error.message,
          stack: entry.error.stack
        } : undefined
      })
    } catch (error) {
      console.error('Failed to send log to server:', error)
    }
  }

  async info(message: string, context?: Record<string, any>): Promise<void> {
    await this.log({ level: 'info', message, context })
  }

  async error(message: string, error: Error, context?: Record<string, any>): Promise<void> {
    await this.log({ level: 'error', message, error, context })
  }

  async warn(message: string, context?: Record<string, any>, _p0?: { sellQuantity: number; currentlyOwned: number }): Promise<void> {
    await this.log({ level: 'warn', message, context })
  }
}

export const logService = new LogService()
