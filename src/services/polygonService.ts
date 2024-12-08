import axios from 'axios'
import type { PolygonStockSnapshot, StockData } from '../types/polygon'
import { secretsService } from './secretService'

//const POLYGON_API_KEY = 'RhKpsxpuUpC9QkFx_4nd_Gd8Fuezoqae'

export class PolygonService {
  private readonly baseUrl = 'https://api.polygon.io/v2'
  private apiKey: string = ""

  private async getApiKey(): Promise<string> {
    if (!this.apiKey) {
      this.apiKey = await secretsService.getPolygonApiKey()
    }
    return this.apiKey
  }

  async getStockSnapshot(symbol: string): Promise<StockData> {
    try {
      const apiKey = await this.getApiKey()
      const response = await axios.get<PolygonStockSnapshot>(
        `${this.baseUrl}/snapshot/locale/us/markets/stocks/tickers/${symbol}?apiKey=${apiKey}`
      )

      const { ticker } = response.data
      
      return {
        symbol: ticker.ticker,
        price: ticker.day.c,
        change: ticker.todaysChange,
        changePercent: ticker.todaysChangePerc,
        volume: ticker.day.v,
        marketCap: 0, // Not provided in snapshot
        timestamp: new Date(ticker.updated / 1000000).toISOString(),
        open: ticker.day.o,
        high: ticker.day.h,
        low: ticker.day.l,
        previousClose: ticker.prevDay.c
      }
    } catch (error) {
      console.error('Error fetching stock data:', error)
      throw new Error('Failed to fetch stock data')
    }
  }
}

export const polygonService = new PolygonService()