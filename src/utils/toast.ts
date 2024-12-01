import { Component, VNode } from 'vue'
import { useToast } from 'vue-toastification'

interface ToastOptions {
    component?: VNode | Component
    message?: string
    timeout?: number
  }

  export const showWarningToast = (options: ToastOptions | string, duration = 5000): string | number => {
    const toast = useToast()
    const defaultOptions = {
      timeout: duration,
      closeOnClick: true,
      pauseOnFocusLoss: true,
      pauseOnHover: true,
      draggable: true
    }
  
    if (typeof options === 'string') {
      return toast.warning(options, defaultOptions)
    }
  
    const { component, message, timeout = duration } = options
    return toast.warning(component || message || '', {
      ...defaultOptions,
      timeout
    })
  }

  export const showErrorToast = (message: string, duration = 5000): string | number => {
    const toast = useToast()
    return toast.error(message, {
      timeout: duration,
      closeOnClick: true,
      pauseOnFocusLoss: true,
      pauseOnHover: true,
      draggable: true
    })
  }

  export const showSuccessToast = (message: string, duration = 5000): string | number => {
    const toast = useToast()
    return toast.success(message, {
      timeout: duration,
      closeOnClick: true,
      pauseOnFocusLoss: true,
      pauseOnHover: true,
      draggable: true
    })
  }

export const dismissToast = (toastId: string | number): void => {
    const toast = useToast()
    toast.dismiss(toastId)
  }

  export const dismissAllToasts = (): void => {
    const toast = useToast()
    toast.clear()
  }