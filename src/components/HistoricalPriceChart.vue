<script setup lang="ts">
import { ref } from 'vue'
import type { HistoricalDataPoint, TimeRange } from '../types/polygon'

const props = defineProps<{
  data: HistoricalDataPoint[]
  symbol: string
}>()

const selectedRange = ref<TimeRange>('1M')
const ranges: TimeRange[] = ['1W', '1M', '3M', '6M', '1Y']

const emit = defineEmits<{
  (e: 'rangeChange', range: TimeRange): void
}>()

const handleRangeChange = (range: TimeRange) => {
  selectedRange.value = range
  emit('rangeChange', range)
}

</script>

<template>
  <div class="bg-white rounded-lg shadow-md p-4 sm:p-6">
    <div class="flex justify-between items-center">
      <h2 class="text-xl font-semibold">Historical Price Data for {{ props.symbol }}</h2>
      <div class="flex gap-2">
        <button
          v-for="range in ranges"
          :key="range"
          @click="handleRangeChange(range)"
          :class="[
            'px-3 py-1 text-sm rounded-md transition-colors',
            selectedRange === range
              ? 'bg-indigo-600 text-white'
              : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
          ]"
        >
          {{ range }}
        </button>
      </div>
    </div>

    <!-- <div class="overflow-x-auto">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Open</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">High</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Low</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Close</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Volume</th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr v-for="point in data" :key="point.t">
            <td class="px-6 py-4 whitespace-nowrap text-sm">{{ formatTimestamp(point.t) }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">{{ formatCurrency(point.o) }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">{{ formatCurrency(point.h) }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">{{ formatCurrency(point.l) }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">{{ formatCurrency(point.c) }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm">{{ point.v.toLocaleString() }}</td>
          </tr>
        </tbody>
      </table>
    </div> -->
  </div>
</template>
