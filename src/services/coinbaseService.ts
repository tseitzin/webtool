import axios from 'axios'
import { secretsService } from './secretService'
import type { CryptoData } from '../types/crypto'

export class CoinbaseService {
  private readonly baseUrl = 'https://api.coinbase.com/v2'
  private readonly baseUrl2 = 'https://api.exchange.coinbase.com/products'
  private apiKey: string | null = null
  private apiSecret: string | null = null

  private async getCredentials(): Promise<{ apiKey: string, apiSecret: string }> {
    if (!this.apiKey || !this.apiSecret) {
      this.apiKey = await secretsService.getCoinbaseApiKey()
      this.apiSecret = await secretsService.getCoinbaseApiSecret()
    }
    return { apiKey: this.apiKey, apiSecret: this.apiSecret }
  }

  async getCryptoData(symbol: string): Promise<CryptoData> {
    try {
      const { apiKey } = await this.getCredentials()
      const [spotPrice, stats] = await Promise.all([
        axios.get(`${this.baseUrl}/prices/${symbol}-USD/spot`, {
          headers: { 'CB-ACCESS-KEY': apiKey }
        }),
        axios.get(`${this.baseUrl2}/${symbol}-USD/stats`, {
          headers: { 'CB-ACCESS-KEY': apiKey }
        })
      ])

      const price = spotPrice.data.data.amount
      const { high, low, volume, last, open } = stats.data
      const change = Number(last) - Number(open)
      const changePercent = (change / Number(open)) * 100

      return {
        symbol,
        open,
        price,
        currency: 'USD',
        change,
        changePercent,
        volume: Number(volume),
        high24h: Number(high),
        low24h: Number(low)
      }
    } catch (error) {
      console.error('Error fetching crypto data:', error)
      throw new Error('Failed to fetch crypto data')
    }
  }

  async getSupportedCurrencies(): Promise<string[]> {
    try {
      const { apiKey } = await this.getCredentials()
      const response = await axios.get(`${this.baseUrl}/currencies`, {
        headers: { 'CB-ACCESS-KEY': apiKey }
      })
      return response.data.data.map((currency: any) => currency.id)
    } catch (error) {
      console.error('Error fetching supported currencies:', error)
      throw new Error('Failed to fetch supported currencies')
    }
  }
}

export const coinbaseService = new CoinbaseService()