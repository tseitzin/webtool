import type { ChartOptions } from 'chart.js'

export type ChartDataType = 'price' | 'volume'

export interface ChartConfig {
  options: ChartOptions<'line'>
  data: {
    labels: string[]
    datasets: Array<{
      label: string
      data: number[]
      borderColor?: string
      backgroundColor?: string
      tension?: number
      fill?: boolean
      type?: string
      borderWidth?: number
    }>
  }
}