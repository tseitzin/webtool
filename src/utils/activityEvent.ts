import { type ThrottleFunction } from './types'

const ACTIVITY_EVENTS = [
  'mousedown',
  'mousemove',
  'keypress',
  'keydown',
  'scroll',
  'touchstart',
  'click',
  'contextmenu',
  'wheel'
] as const

export const addActivityListeners = (handler: () => void): void => {
  const throttledHandler = throttle(handler, 1000)
  ACTIVITY_EVENTS.forEach(event => {
    document.addEventListener(event, throttledHandler)
  })
}

export const removeActivityListeners = (handler: () => void): void => {
  ACTIVITY_EVENTS.forEach(event => {
    document.removeEventListener(event, handler)
  })
}

// Throttle function to prevent excessive timer resets
const throttle: ThrottleFunction = (func, limit) => {
  let inThrottle = false
  return function(this: void, ...args: unknown[]): void {
    if (!inThrottle) {
      func.apply(null, args)
      inThrottle = true
      setTimeout(() => inThrottle = false, limit)
    }
  }
}