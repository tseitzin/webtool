import api from '../api/axios'
import type { StockData } from '../types/polygon'

export class StockService {
  async saveStock(stockData: StockData): Promise<void> {
    const { symbol, ...data } = stockData
    await api.post(`/savedstocks/${symbol}`, data)
  }

  async removeSavedStock(symbol: string): Promise<void> {
    await api.delete(`/savedstocks/${symbol}`)
  }

  async getSavedStocks(): Promise<StockData[]> {
    const response = await api.get('/savedstocks')
    return response.data
  }
}

export const stockService = new StockService()