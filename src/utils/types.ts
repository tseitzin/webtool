export type ThrottleFunction = (
    func: (...args: unknown[]) => void,
    limit: number
  ) => (...args: unknown[]) => void
  
  export interface Timer {
    start: (callback: () => void, delay: number) => void
    stop: () => void
    isRunning: () => boolean
  }
  
  export interface ActivityMonitor {
    startMonitoring: (onTimeout: () => void) => void
    stopMonitoring: () => void
  }