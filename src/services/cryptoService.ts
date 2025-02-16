import api from '../api/axios'
import type { CryptoData } from '../types/crypto'

export class CryptoService {
  async saveCrypto(cryptoData: CryptoData): Promise<void> {
    const { symbol, ...data } = cryptoData
    await api.post(`/savedcryptos/${symbol}`, {
      price: Number(data.price),
      open: Number(data.open),
      change: data.change,
      changePercent: data.changePercent,
      volume: data.volume,
      high24h: data.high24h,
      low24h: data.low24h
    })
  }

  async removeSavedCrypto(symbol: string): Promise<void> {
    await api.delete(`/savedcryptos/${symbol}`)
  }

  async addSavedCrypto(symbol: string): Promise<void> {
    await api.post(`/savedcryptos/${symbol}`)
  }

  async getSavedCryptos(): Promise<CryptoData[]> {
    const response = await api.get('/savedcryptos')
    return response.data.map((crypto: any) => ({
      symbol: crypto.symbol,
      price: crypto.price.toString(),
      open: crypto.open.toString(),
      currency: 'USD',
      change: crypto.change,
      changePercent: crypto.changePercent,
      volume: crypto.volume,
      high24h: crypto.high24h,
      low24h: crypto.low24h
    }))
  }
}

export const cryptoService = new CryptoService()