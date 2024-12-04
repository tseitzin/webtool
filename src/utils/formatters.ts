export const formatNumber = (num: number) => {
    return new Intl.NumberFormat('en-US').format(num)
  }
  
  export const formatCurrency = (num: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(num)
  }
  
  export const formatPercent = (num: number) => {
    return new Intl.NumberFormat('en-US', { style: 'percent', minimumFractionDigits: 2 }).format(num / 100)
  }