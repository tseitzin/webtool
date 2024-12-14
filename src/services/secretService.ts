import api from '../api/axios'

export class SecretsService {
  private static instance: SecretsService
  private polygonApiKey: string = ""
  private coinbaseApiKey: string = ""
  private coinbaseApiSecret: string = ""

  private constructor() {}

  static getInstance(): SecretsService {
    if (!SecretsService.instance) {
      SecretsService.instance = new SecretsService()
    }
    return SecretsService.instance
  }

  async getPolygonApiKey(): Promise<string> {
    if (this.polygonApiKey) {
      return this.polygonApiKey
    }

    try {
      const response = await api.get('/secrets/polygon-api-key')
      this.polygonApiKey = response.data.value
      return this.polygonApiKey
    } catch (error) {
      console.error('Failed to fetch Polygon API key:', error)
      throw new Error('Failed to fetch API key')
    }
  }

  async getCoinbaseApiKey(): Promise<string> {
    if (this.coinbaseApiKey) {
      return this.coinbaseApiKey
    }

    try {
      const response = await api.get('/secrets/coinbase-api-key')
      this.coinbaseApiKey = response.data.value
      return this.coinbaseApiKey
    } catch (error) {
      console.error('Failed to fetch Coinbase API key:', error)
      throw new Error('Failed to fetch API key')
    }
  }

  async getCoinbaseApiSecret(): Promise<string> {
    if (this.coinbaseApiSecret) {
      return this.coinbaseApiSecret
    }

    try {
      const response = await api.get('/secrets/coinbase-api-secret')
      this.coinbaseApiSecret = response.data.value
      return this.coinbaseApiSecret
    } catch (error) {
      console.error('Failed to fetch Coinbase API secret:', error)
      throw new Error('Failed to fetch API secret')
    }
  }
}

export const secretsService = SecretsService.getInstance()