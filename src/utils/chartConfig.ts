import type { ChartOptions } from 'chart.js'
import type { ChartDataType } from '../types/chart'
import { createXAxisConfig, createYAxisConfig } from './chartScales'
import { createChartPlugins } from './chartPlugins'

export const createChartOptions = (
  symbol: string,
  dataType: ChartDataType = 'price'
): ChartOptions<'line'> => {
  const xAxis = createXAxisConfig()
  const yAxis = createYAxisConfig(dataType)

  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: createChartPlugins(symbol, dataType),
    scales: {
      x: xAxis,
      y: yAxis
    },
    layout: {
      padding: {
        left: 15,
        right: 15,
        top: 20,
        bottom: 15
      }
    }
  }
}