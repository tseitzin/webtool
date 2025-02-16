export const formatNumber = (num: number) => {
  return new Intl.NumberFormat('en-US').format(num)
}

export const formatCurrency = (num: number) => {
  // For very small numbers (less than 0.01), show more decimal places
  if (Math.abs(num) < .01) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 8,
      maximumFractionDigits: 8
    }).format(num)
  }
  
  // Default currency formatting for normal numbers
  return new Intl.NumberFormat('en-US', { 
    style: 'currency', 
    currency: 'USD' 
  }).format(num)
}

export const formatPercent = (num: number) => {
  return new Intl.NumberFormat('en-US', { 
    style: 'percent', 
    minimumFractionDigits: 2 
  }).format(num / 100)
}