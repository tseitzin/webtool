import { polygonService } from './polygonService'
import { stockService } from './stockService'
import type { StockData, MarketMovers } from '../types/polygon'
import { marketMoversService } from './marketMoversService'

export class SearchAreaService {
  private static instance: SearchAreaService
  private refreshInterval: number | null = null
  private readonly REFRESH_INTERVAL = 60000 // 60 seconds

  private constructor() {}

  static getInstance(): SearchAreaService {
    if (!SearchAreaService.instance) {
      SearchAreaService.instance = new SearchAreaService()
    }
    return SearchAreaService.instance
  }

  async refreshData(savedStocks: StockData[], onUpdate: (stocks: StockData[], movers: MarketMovers) => void): Promise<void> {
    try {
      const [updatedStocks, marketMovers] = await Promise.all([
        this.refreshSavedStocks(savedStocks),
        marketMoversService.getMarketMovers()
      ])
      onUpdate(updatedStocks, marketMovers)
    } catch (error) {
      console.error('Failed to refresh data:', error)
      throw error
    }
  }

  private async refreshSavedStocks(savedStocks: StockData[]): Promise<StockData[]> {
    const updatedStocks = await Promise.all(
      savedStocks.map(async (stock) => {
        try {
          const updatedStock = await polygonService.getStockSnapshot(stock.symbol)
          await stockService.saveStock(updatedStock)
          return updatedStock
        } catch (error) {
          console.error(`Failed to update ${stock.symbol}:`, error)
          return stock // Return existing data if update fails
        }
      })
    )
    return updatedStocks
  }

  startAutoRefresh(savedStocks: StockData[], onUpdate: (stocks: StockData[], movers: MarketMovers) => void): void {
    this.stopAutoRefresh()
    
    // Initial refresh
    this.refreshData(savedStocks, onUpdate).catch(console.error)

    // Set up periodic refresh
    this.refreshInterval = window.setInterval(() => {
      this.refreshData(savedStocks, onUpdate).catch(console.error)
    }, this.REFRESH_INTERVAL)
  }

  stopAutoRefresh(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval)
      this.refreshInterval = null
    }
  }
}

export const searchAreaService = SearchAreaService.getInstance()