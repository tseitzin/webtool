import axios from 'axios'
import type { MarketMovers, PolygonCompanyInfo, PolygonStockSnapshot, StockData } from '../types/polygon'
import { secretsService } from './secretService'

export class PolygonService {
  private readonly baseUrl = 'https://api.polygon.io'
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
      const [snapshotResponse, companyResponse] = await Promise.all([
        axios.get<PolygonStockSnapshot>(
          `${this.baseUrl}/v2/snapshot/locale/us/markets/stocks/tickers/${symbol}?apiKey=${apiKey}`
        ),
        axios.get<PolygonCompanyInfo>(
          `${this.baseUrl}/v3/reference/tickers/${symbol}?apiKey=${apiKey}`
        )
      ])

      const { ticker } = snapshotResponse.data
      const { results: company } = companyResponse.data
      
      return {
        symbol: ticker.ticker,
        companyName: company.name,
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

  async getMarketMovers(): Promise<MarketMovers> {
    try {
      const apiKey = await this.getApiKey()
      const [gainersResponse, losersResponse] = await Promise.all([
        axios.get(`${this.baseUrl}/v2/snapshot/locale/us/markets/stocks/gainers?apiKey=${apiKey}`),
        axios.get(`${this.baseUrl}/v2/snapshot/locale/us/markets/stocks/losers?apiKey=${apiKey}`)
      ])

      const gainers = gainersResponse.data.tickers
        .slice(0, 10)
        .map((ticker: any) => ({
          symbol: ticker.ticker,
          price: ticker.day.c,
          changePercent: ticker.todaysChangePerc,
          volume: ticker.day.v
        }))

      const losers = losersResponse.data.tickers
        .slice(0, 10)
        .map((ticker: any) => ({
          symbol: ticker.ticker,
          price: ticker.day.c,
          changePercent: ticker.todaysChangePerc,
          volume: ticker.day.v
        }))

      return { gainers, losers }
    } catch (error) {
      console.error('Error fetching market movers:', error)
      throw new Error('Failed to fetch market movers')
    }
  }
}


export const polygonService = new PolygonService()