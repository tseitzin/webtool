import { ref } from 'vue'

export class Timer {
  private timer = ref<number | null>(null)

  start(callback: () => void, delay: number) {
    this.stop()
    this.timer.value = window.setTimeout(callback, delay)
  }

  stop() {
    if (this.timer.value) {
      clearTimeout(this.timer.value)
      this.timer.value = null
    }
  }

  isRunning() {
    return this.timer.value !== null
  }
}