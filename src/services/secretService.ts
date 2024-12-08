import api from '../api/axios'

export class SecretsService {
  private static instance: SecretsService
  private polygonApiKey: string = ""

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
}

export const secretsService = SecretsService.getInstance()