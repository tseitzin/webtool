export interface PortfolioStock {
    id: number
    userId: number
    symbol: string
    quantity: number
    purchasePrice: number
    purchaseDate: string
    notes?: string
  }

  export interface UserOwnedStock {
    id: number
    userId: number
    symbol: string
    quantity: number
    purchasePrice: number
    purchaseDate: string
    notes?: string
  }
  
  