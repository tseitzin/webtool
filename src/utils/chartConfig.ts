import { formatCurrency } from './formatters'
import type { ChartOptions } from './chartTypes'

export const createChartOptions = (symbol: string): ChartOptions => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top',
      align: 'center',
      labels: {
        boxWidth: 20,
        padding: 15,
        font: { size: 12 }
      }
    },
    title: {
      display: true,
      text: `Historical Data for ${symbol}`,
      font: {
        size: 18,
        weight: 'bold',
        family: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif"
      },
      padding: { top: 10, bottom: 20 }
    },
    tooltip: {
      callbacks: {
        label: (context) => `${context.dataset.label}: ${formatCurrency(context.raw as number)}`
      }
    }
  },
  scales: {
    x: {
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
      beginAtZero: false,
      ticks: {
        callback: (value) => formatCurrency(value),
        font: { size: 11 }
      },
      grid: {
        display: true,
        color: 'rgba(0, 0, 0, 0.1)'
      }
    }
  }
})