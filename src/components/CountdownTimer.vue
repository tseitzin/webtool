<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'

const props = defineProps<{
  duration: number
  onComplete?: () => void
}>()

const timeLeft = ref(Math.ceil(props.duration / 1000))
const intervalId = ref<number | null>(null)

const updateTimer = () => {
  if (timeLeft.value > 0) {
    timeLeft.value--
  } else {
    if (intervalId.value) {
      clearInterval(intervalId.value)
    }
    if (props.onComplete) {
      props.onComplete()
    }
  }
}

onMounted(() => {
  intervalId.value = window.setInterval(updateTimer, 1000)
})

onBeforeUnmount(() => {
  if (intervalId.value) {
    clearInterval(intervalId.value)
  }
})
</script>

<template>
  <span class="font-bold">{{ timeLeft }}s</span>
</template>