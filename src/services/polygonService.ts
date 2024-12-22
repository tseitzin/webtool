import axios from 'axios'
import { secretsService } from './secretService'
import type { 
  PolygonStockSnapshot, 
  PolygonCompanyInfo, 
  StockData, 
  MarketMovers,
  CompanyDetailsResponse,
  CompanyDetails, 
  NewsResponse,
  NewsArticle
} from '../types/polygon'

export class PolygonService {
  private readonly baseUrl = 'https://api.polygon.io'
  private apiKey: string | null = null

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

      // Check if market is closed (price and volume are 0)
      const isMarketClosed = ticker.day.c === 0 && ticker.day.v === 0
      const currentPrice = isMarketClosed ? ticker.prevDay.c : ticker.day.c
      const marketStatus = isMarketClosed ? 'Market is currently closed. Using previous close price for current price.' : 'Market is open'
      
      return {
        symbol: ticker.ticker,
        companyName: company.name,
        price: currentPrice,
        change: ticker.todaysChange,
        marketCap: 0, // Not provided in snapshot
        changePercent: ticker.todaysChangePerc,
        volume: ticker.day.v,
        timestamp: new Date(ticker.updated / 1000000).toISOString(),
        open: ticker.day.o,
        high: ticker.day.h,
        low: ticker.day.l,
        previousClose: ticker.prevDay.c,
        marketStatus
      }
    } catch (error) {
      console.error('Error fetching stock data:', error)
      throw new Error('Failed to fetch stock data')
    }
  }

  async getCompanyDetails(symbol: string): Promise<CompanyDetails> {
    try {
      const apiKey = await this.getApiKey()
      const today = new Date().toISOString().split('T')[0]
      
      const response = await axios.get<CompanyDetailsResponse>(
        `${this.baseUrl}/v3/reference/tickers/${symbol}?date=${today}&apiKey=${apiKey}`
      )

      return response.data.results
    } catch (error) {
      console.error('Error fetching company details:', error)
      throw new Error('Failed to fetch company details')
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

  async getCompanyNews(symbol: string): Promise<NewsArticle[]> {
    try {
      const apiKey = await this.getApiKey()
      const today = new Date()
      const thirtyDaysAgo = new Date(today.setDate(today.getDate() - 30))
        .toISOString()
        .split('T')[0]
      
      const response = await axios.get<NewsResponse>(
        `${this.baseUrl}/v2/reference/news?ticker=${symbol}&published_utc.gt=${thirtyDaysAgo}&order=desc&limit=10&sort=published_utc&apiKey=${apiKey}`
      )
  
      return response.data.results
    } catch (error) {
      console.error('Error fetching company news:', error)
      throw new Error('Failed to fetch company news')
    }
  }
}


export const polygonService = new PolygonService()