import type { ChartOptions } from 'chart.js'
import { formatCurrency, formatNumber } from './formatters'
import type { ChartDataType } from '../types/chart'

export const createChartPlugins = (
  symbol: string, 
  dataType: ChartDataType
): ChartOptions<'line'>['plugins'] => ({
  legend: {
    position: 'top' as const,
    align: 'center' as const,
    labels: {
      boxWidth: 20,
      padding: 15,
      font: { size: 12 }
    }
  },
  title: {
    display: true,
    text: `Historical ${dataType === 'volume' ? 'Volume' : 'Price'} Data for ${symbol}`,
    font: {
      size: 18,
      weight: 'bold',
      family: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif"
    },
    padding: { top: 10, bottom: 20 }
  },
  tooltip: {
    callbacks: {
      label: (context) => {
        const value = context.raw as number
        return dataType === 'volume'
          ? `Volume: ${formatNumber(value)}`
          : `${context.dataset.label}: ${formatCurrency(value)}`
      }
    }
  }
})