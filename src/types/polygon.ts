export interface PolygonStockSnapshot {
    ticker: {
      ticker: string
      todaysChangePerc: number
      todaysChange: number
      updated: number
      day: {
        o: number
        h: number
        l: number
        c: number
        v: number
        vw: number
      }
      min: {
        av: number
        t: number
        n: number
        o: number
        h: number
        l: number
        c: number
        v: number
        vw: number
      }
      prevDay: {
        o: number
        h: number
        l: number
        c: number
        v: number
        vw: number
      }
    }
    status: string
    request_id: string
  }

  export interface PolygonCompanyInfo {
    request_id: string
    results: {
      ticker: string
      name: string
      market: string
      locale: string
      primary_exchange: string
      type: string
      active: boolean
      currency_name: string
      market_cap: number
    }
    status: string
  }
  
  export interface StockData {
    symbol: string
    companyName: string
    price: number
    change: number
    changePercent: number
    volume: number
    marketCap: number
    timestamp: string
    open?: number
    high?: number
    low?: number
    previousClose: number
  }

  export interface MarketMover {
    symbol: string
    price: number
    changePercent: number
    volume: number
  }
  
  export interface MarketMovers {
    gainers: MarketMover[]
    losers: MarketMover[]
  }