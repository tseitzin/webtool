import { dismissAllToasts, dismissToast, showSuccessToast, showWarningToast } from './toast'
import { addActivityListeners, removeActivityListeners } from './activityEvent'
import { Timer } from './timer'
import { ref, h } from 'vue'
import CountdownTimer from '../components/CountdownTimer.vue'


const INACTIVITY_TIMEOUT = 15 * 60 * 1000 // 2 minutes
const WARNING_BEFORE_TIMEOUT = 60 * 1000 // 1 minute
const SHOW_ACTIVITY_FEEDBACK = true // Toggle activity feedback

export class ActivityMonitor {
  private activityTimer: Timer
  private warningTimer: Timer
  private timeoutCallback: (() => void) | null = null
  private warningShown = ref(false)
  private currentToastId = ref<string | number | null>(null)


  constructor() {
    this.activityTimer = new Timer()
    this.warningTimer = new Timer()
  }

  private resetTimers = () => {
    this.warningTimer.stop()
    this.activityTimer.stop()

    // If warning was shown and user becomes active, dismiss it and show confirmation
    if (this.warningShown.value) {
      this.warningShown.value = false
      if (this.currentToastId.value !== null) {
        dismissToast(this.currentToastId.value)
        this.currentToastId.value = null
      }
      if (SHOW_ACTIVITY_FEEDBACK) {
        showSuccessToast('Session extended due to activity', 3000)
      }
    }

    this.warningTimer.start(() => {
      this.warningShown.value = true
      this.currentToastId.value = showWarningToast({
        component: h('div', [
          'You will be logged out in ',
          h(CountdownTimer, {
            duration: WARNING_BEFORE_TIMEOUT,
            onComplete: () => {
              if (this.timeoutCallback) {
                this.timeoutCallback()
              }
            }
          }),
          ' due to inactivity. Move your mouse or press a key to stay logged in.'
        ]),
        timeout: WARNING_BEFORE_TIMEOUT
      })
    }, INACTIVITY_TIMEOUT - WARNING_BEFORE_TIMEOUT)

    this.activityTimer.start(() => {
      if (this.timeoutCallback) {
        dismissAllToasts() // Dismiss all toasts before logout
        this.timeoutCallback()
      }
    }, INACTIVITY_TIMEOUT)
  }

  startMonitoring(onTimeout: () => void) {
    this.timeoutCallback = onTimeout
    addActivityListeners(this.resetTimers)
    this.resetTimers()
  }

  stopMonitoring() {
    removeActivityListeners(this.resetTimers)
    this.warningTimer.stop()
    this.activityTimer.stop()
    this.timeoutCallback = null
    this.warningShown.value = false
    if (this.currentToastId.value !== null) {
      dismissToast(this.currentToastId.value)
      this.currentToastId.value = null
    }
    dismissAllToasts() // Dismiss all toasts when stopping monitoring
  }
}
export const activityMonitor = new ActivityMonitor()