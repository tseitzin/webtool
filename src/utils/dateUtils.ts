export const getDateRange = (range: string): { startDate: string, endDate: string } => {
    const today = new Date()
    const endDate = today.toISOString().split('T')[0]
    let startDate: Date
  
    switch (range) {
      case '1W':
        startDate = new Date(today.setDate(today.getDate() - 7))
        break
      case '1M':
        startDate = new Date(today.setMonth(today.getMonth() - 1))
        break
      case '3M':
        startDate = new Date(today.setMonth(today.getMonth() - 3))
        break
      case '6M':
        startDate = new Date(today.setMonth(today.getMonth() - 6))
        break
      case '1Y':
        startDate = new Date(today.setFullYear(today.getFullYear() - 1))
        break
      default:
        startDate = new Date(today.setMonth(today.getMonth() - 1)) // Default to 1 month
    }
  
    return {
      startDate: startDate.toISOString().split('T')[0],
      endDate
    }
  }