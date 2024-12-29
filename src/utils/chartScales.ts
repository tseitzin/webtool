import type { ScaleOptions } from 'chart.js'
import { formatCurrency, formatNumber } from './formatters'
import type { ChartDataType } from '../types/chart'

export const createXAxisConfig = (): Partial<ScaleOptions<'category'>> => ({
  ticks: {
    maxRotation: 45,
    minRotation: 45,
    font: { size: 11 }
  },
  grid: {
    display: false
  }
})

export const createYAxisConfig = (dataType: ChartDataType): Partial<ScaleOptions<'linear'>> => ({
  beginAtZero: false,
  ticks: {
    callback: (value) => {
      const numValue = value as number
      return dataType === 'volume' 
        ? formatNumber(numValue)
        : formatCurrency(numValue)
    },
    font: { size: 11 }
  },
  grid: {
    display: true,
    color: 'rgba(0, 0, 0, 0.1)'
  }
})