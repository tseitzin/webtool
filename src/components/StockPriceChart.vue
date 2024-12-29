<script setup lang="ts">
import { ref, computed } from 'vue'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js'
import type { HistoricalDataPoint } from '../types/polygon'
import { formatNumber } from '../utils/formatters'
import { createChartOptions } from '../utils/chartConfig'
import TimeRangeSelector from './TimeRangeSelector.vue'
import ChartDataSelector from './ChartDataSelector.vue'

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
)

const props = defineProps<{
  data: HistoricalDataPoint[]
  symbol: string
}>()

const emit = defineEmits<{
  (e: 'timeRangeChanged', range: string): void
}>()

const selectedRange = ref('1M')
const selectedDataType = ref('price')

const handleRangeChange = (range: string) => {
  selectedRange.value = range
  emit('timeRangeChanged', range)
}

const chartData = computed(() => {
  const labels = props.data.map(point => new Date(point.t).toLocaleDateString())
  
  if (selectedDataType.value === 'volume') {
    return {
      labels,
      datasets: [{
        label: 'Volume',
        data: props.data.map(point => point.v),
        borderColor: 'rgb(75, 192, 192)',
        tension: 0.1,
        fill: false
      }]
    }
  }

  return {
    labels,
    datasets: [
      {
        label: 'Closing Price',
        data: props.data.map(point => point.c),
        borderColor: 'rgb(75, 192, 192)',
        tension: 0.1,
        fill: false
      },
      {
        label: 'Opening Price',
        data: props.data.map(point => point.o),
        borderColor: 'rgb(153, 102, 255)',
        tension: 0.1,
        fill: false
      }
    ]
  }
})

const chartOptions = computed(() => {
  const baseOptions = createChartOptions(props.symbol)
  
  if (selectedDataType.value === 'volume') {
    return {
      ...baseOptions,
      scales: {
        ...baseOptions.scales,
        y: {
          ...baseOptions.scales?.y,
          ticks: {
            callback: (value: number) => formatNumber(value),
            font: { size: 11 }
          },
          title: {
            display: true,
            text: 'Volume'
          }
        }
      },
      plugins: {
        ...baseOptions.plugins,
        tooltip: {
          callbacks: {
            label: (context: any) => `Volume: ${formatNumber(context.raw)}`
          }
        }
      }
    }
  }

  return baseOptions
})
</script>

<template>
  <div class="bg-white rounded-lg shadow-md p-4 w-full">
    <div class="flex flex-col sm:flex-row justify-between items-center gap-4 mb-4">
      <TimeRangeSelector
        :selected-range="selectedRange"
        @range-selected="handleRangeChange"
      />
      <ChartDataSelector
        :selected-type="selectedDataType"
        @data-type-selected="selectedDataType = $event"
      />
    </div>
    <div class="h-[300px] sm:h-[400px] md:h-[500px] w-full">
      <Line
        :data="chartData"
        :option="chartOptions"
      />
    </div>
  </div>
</template>