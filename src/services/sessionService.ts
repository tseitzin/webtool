export class SessionService {
    private static instance: SessionService
    private closeHandlers: (() => void)[] = []
  
    private constructor() {
      this.setupEventListeners()
    }
  
    static getInstance(): SessionService {
      if (!SessionService.instance) {
        SessionService.instance = new SessionService()
      }
      return SessionService.instance
    }
  
    private setupEventListeners(): void {
      window.addEventListener('beforeunload', () => {
        this.handleClose()
      })
  
      // Handle visibility change for tab switching/closing
      document.addEventListener('visibilitychange', () => {
        if (document.visibilityState === 'hidden') {
          this.handleClose()
        }
      })
    }
  
    private handleClose(): void {
      this.closeHandlers.forEach(handler => handler())
    }
  
    onClose(handler: () => void): void {
      this.closeHandlers.push(handler)
    }
  
    removeCloseHandler(handler: () => void): void {
      this.closeHandlers = this.closeHandlers.filter(h => h !== handler)
    }
  }