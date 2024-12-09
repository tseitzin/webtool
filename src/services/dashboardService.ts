import { polygonService } from './polygonService'
import { stockService } from './stockService'
import type { StockData } from '../types/polygon'

export class DashboardService {
  private static instance: DashboardService
  private refreshInterval: number | null = null
  private readonly REFRESH_INTERVAL = 30000 // 30 seconds

  private constructor() {}

  static getInstance(): DashboardService {
    if (!DashboardService.instance) {
      DashboardService.instance = new DashboardService()
    }
    return DashboardService.instance
  }

  async refreshStockData(savedStocks: StockData[]): Promise<StockData[]> {
    try {
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
    } catch (error) {
      console.error('Failed to refresh stock data:', error)
      throw error
    }
  }

  startAutoRefresh(savedStocks: StockData[], onUpdate: (stocks: StockData[]) => void): void {
    this.stopAutoRefresh()
    
    // Initial refresh
    this.refreshStockData(savedStocks)
      .then(updatedStocks => onUpdate(updatedStocks))
      .catch(console.error)

    // Set up periodic refresh
    this.refreshInterval = window.setInterval(async () => {
      try {
        const updatedStocks = await this.refreshStockData(savedStocks)
        onUpdate(updatedStocks)
      } catch (error) {
        console.error('Auto-refresh failed:', error)
      }
    }, this.REFRESH_INTERVAL)
  }

  stopAutoRefresh(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval)
      this.refreshInterval = null
    }
  }
}

export const dashboardService = DashboardService.getInstance()