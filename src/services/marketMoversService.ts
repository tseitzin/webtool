import api from '../api/axios'
import { polygonService } from './polygonService'
import type { MarketMovers } from '../types/polygon'

class MarketMoversService {
  private static instance: MarketMoversService

  private constructor() {}

  static getInstance(): MarketMoversService {
    if (!MarketMoversService.instance) {
      MarketMoversService.instance = new MarketMoversService()
    }
    return MarketMoversService.instance
  }

  async getMarketMovers(): Promise<MarketMovers> {
    try {
      // First try to get fresh data from Polygon
      const polygonData = await polygonService.getMarketMovers()
      
      // If we got data, update our database
      if (polygonData.gainers.length > 0 || polygonData.losers.length > 0) {
        await api.post('/marketmovers', {
          gainers: polygonData.gainers,
          losers: polygonData.losers
        })
        return polygonData
      }

      // If no fresh data, get stored data
      const response = await api.get('/marketmovers')
      return {
        gainers: response.data.gainers,
        losers: response.data.losers,
        marketStatus: response.data.marketStatus,
        lastUpdate: response.data.lastUpdate
      }
    } catch (error) {
      console.error('Error fetching market movers:', error)
      throw new Error('Failed to fetch market movers')
    }
  }
}

export const marketMoversService = MarketMoversService.getInstance()