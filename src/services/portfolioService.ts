import api from '../api/axios'
import type { UserOwnedStock } from '../types/portfolio'

export class PortfolioService {
  async getPortfolio(): Promise<UserOwnedStock[]> {
    const response = await api.get('/portfolio')
    return response.data
  }

  async addPosition(position: {
    symbol: string
    quantity: number
    purchasePrice: number
    purchaseDate: Date
    notes?: string
  }): Promise<void> {
    await api.post('/portfolio', position)
  }

  async sellPosition(id: number, quantity: number): Promise<void> {
    await api.post(`/portfolio/${id}/sell`, { quantity })
  }
}

export const portfolioService = new PortfolioService()
