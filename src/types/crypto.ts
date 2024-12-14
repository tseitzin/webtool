export interface CryptoPrice {
    amount: string
    currency: string
  }
  
  export interface CryptoData {
    symbol: string
    price: string
    currency: string
    open: number
    change: number
    changePercent: number
    volume: number
    high24h: number
    low24h: number
  }
  
  export interface SavedCrypto extends CryptoData {
    savedAt: string
  }