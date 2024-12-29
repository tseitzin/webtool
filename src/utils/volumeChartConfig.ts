import { formatNumber } from './formatters'
import type { ChartOptions, ChartScales } from './chartTypes'

export const createVolumeChartOptions = (baseOptions: ChartOptions): ChartOptions => {
  // Ensure both x and y scales are defined
  const scales: ChartScales = {
    x: baseOptions.scales?.x || {
      ticks: {
        maxRotation: 45,
        minRotation: 45,
        font: { size: 11 }
      },
      grid: {
        display: false
      }
    },
    y: {
      ticks: {
        callback: (value) => formatNumber(value),
        font: { size: 11 }
      },
      title: {
        display: true,
        text: 'Volume'
      },
      grid: {
        display: true,
        color: 'rgba(0, 0, 0, 0.1)'
      }
    }
  }

  return {
    ...baseOptions,
    scales,
    plugins: {
      ...baseOptions.plugins,
      tooltip: {
        callbacks: {
          label: (context) => `Volume: ${formatNumber(context.raw as number)}`
        }
      }
    }
  }
}