// Interface cho FEN message từ server
export interface ChessFenMessage {
  fen_str: string
  timestamp?: string
  source?: string
}

class WebSocketService {
  private ws: WebSocket | null = null
  private isConnected = false
  private subscribers: Map<string, Function[]> = new Map()
  private reconnectAttempts = 0
  private maxReconnectAttempts = Number(import.meta.env.VITE_WS_RECONNECT_ATTEMPTS) || 5
  private reconnectInterval = Number(import.meta.env.VITE_WS_RECONNECT_INTERVAL) || 3000
  private connectionTimeout = Number(import.meta.env.VITE_WS_CONNECTION_TIMEOUT) || 5000

  // Cấu hình WebSocket servers với fallback
  private primaryIP = import.meta.env.VITE_WS_PRIMARY_IP || '100.73.130.46'
  private fallbackIP = import.meta.env.VITE_WS_FALLBACK_IP || 'localhost'
  private primaryPort = import.meta.env.VITE_WS_PRIMARY_PORT || '8081'
  private fallbackPorts = import.meta.env.VITE_WS_FALLBACK_PORTS 
    ? import.meta.env.VITE_WS_FALLBACK_PORTS.split(',') 
    : ['8085', '8086', '8087']

  private currentConnection: { ip: string, port: string } | null = null

  // Method để thử kết nối với một URL cụ thể
  private tryConnect(url: string): Promise<boolean> {
    return new Promise((resolve) => {
      console.log(`Đang thử kết nối: ${url}`)
      
      const testWs = new WebSocket(url)
      let timeoutId: number

      const cleanup = () => {
        clearTimeout(timeoutId)
        if (testWs.readyState === WebSocket.CONNECTING || testWs.readyState === WebSocket.OPEN) {
          testWs.close()
        }
      }

      timeoutId = window.setTimeout(() => {
        cleanup()
        console.log(`Timeout kết nối: ${url}`)
        resolve(false)
      }, this.connectionTimeout)

      testWs.onopen = () => {
        cleanup()
        console.log(`Kết nối thành công: ${url}`)
        testWs.close()
        resolve(true)
      }

      testWs.onerror = () => {
        cleanup()
        console.log(`Lỗi kết nối: ${url}`)
        resolve(false)
      }
    })
  }

  // Method để tìm URL WebSocket khả dụng
  private async findAvailableWebSocket(): Promise<string | null> {
    // Thử IP chính với port chính
    const primaryUrl = `ws://${this.primaryIP}:${this.primaryPort}`
    if (await this.tryConnect(primaryUrl)) {
      this.currentConnection = { ip: this.primaryIP, port: this.primaryPort }
      return primaryUrl
    }

    console.log(`IP chính ${this.primaryIP} không khả dụng, thử fallback ports...`)
    
    // Thử IP chính với các ports fallback
    for (const port of this.fallbackPorts) {
      const url = `ws://${this.primaryIP}:${port}`
      if (await this.tryConnect(url)) {
        this.currentConnection = { ip: this.primaryIP, port }
        return url
      }
    }

    console.log(`IP chính ${this.primaryIP} hoàn toàn không khả dụng, thử IP fallback...`)

    // Thử IP fallback với port chính
    const fallbackPrimaryUrl = `ws://${this.fallbackIP}:${this.primaryPort}`
    if (await this.tryConnect(fallbackPrimaryUrl)) {
      this.currentConnection = { ip: this.fallbackIP, port: this.primaryPort }
      return fallbackPrimaryUrl
    }

    // Thử IP fallback với các ports fallback
    for (const port of this.fallbackPorts) {
      const url = `ws://${this.fallbackIP}:${port}`
      if (await this.tryConnect(url)) {
        this.currentConnection = { ip: this.fallbackIP, port }
        return url
      }
    }

    console.error('Không tìm thấy WebSocket server khả dụng!')
    return null
  }

  async connect(): Promise<boolean> {
    try {
      console.log('Đang tìm WebSocket server khả dụng...')
      console.log(`IP chính: ${this.primaryIP}:${this.primaryPort}`)
      console.log(`IP fallback: ${this.fallbackIP}:${this.primaryPort}`)
      console.log(`Fallback ports: ${this.fallbackPorts.join(', ')}`)

      // Tìm URL khả dụng
      const availableUrl = await this.findAvailableWebSocket()
      if (!availableUrl) {
        console.error('Không thể tìm thấy WebSocket server khả dụng')
        return false
      }

      console.log(`Kết nối tới: ${availableUrl}`)
      this.ws = new WebSocket(availableUrl)
      
      return new Promise((resolve) => {
        if (!this.ws) {
          resolve(false)
          return
        }

        this.ws.onopen = () => {
          const connInfo = this.currentConnection ? `${this.currentConnection.ip}:${this.currentConnection.port}` : 'unknown'
          console.log(`WebSocket đã kết nối thành công tới: ${connInfo}`)
          this.isConnected = true
          this.reconnectAttempts = 0
          resolve(true)
        }

        this.ws.onmessage = (event) => {
          try {
            const data = JSON.parse(event.data)
            console.log('Nhận được message:', data)
            
            // Gọi tất cả subscribers
            this.notifySubscribers('message', data)
            
            // Gọi subscribers cho loại message cụ thể
            if (data.fen_str) {
              this.notifySubscribers('fen', data)
            }
          } catch (error) {
            console.error('Lỗi parse message:', error)
          }
        }

        this.ws.onclose = () => {
          console.log('WebSocket đã ngắt kết nối')
          this.isConnected = false
          this.handleReconnect()
        }

        this.ws.onerror = (error) => {
          console.error('WebSocket error:', error)
          this.isConnected = false
          resolve(false)
        }

        // Timeout sau connectionTimeout
        setTimeout(() => {
          if (!this.isConnected) {
            console.error('WebSocket connection timeout')
            resolve(false)
          }
        }, this.connectionTimeout)
      })
    } catch (error) {
      console.error('Lỗi kết nối WebSocket:', error)
      return false
    }
  }

  private handleReconnect() {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++
      console.log(`Thử reconnect lần ${this.reconnectAttempts}/${this.maxReconnectAttempts}...`)
      
      setTimeout(() => {
        this.connect()
      }, this.reconnectInterval)
    } else {
      console.error('Đã thử reconnect tối đa, dừng kết nối')
    }
  }

  subscribe(event: string, callback: Function) {
    if (!this.subscribers.has(event)) {
      this.subscribers.set(event, [])
    }
    this.subscribers.get(event)!.push(callback)
    console.log(`Đã subscribe event: ${event}`)
  }

  unsubscribe(event: string, callback: Function) {
    const callbacks = this.subscribers.get(event)
    if (callbacks) {
      const index = callbacks.indexOf(callback)
      if (index > -1) {
        callbacks.splice(index, 1)
        console.log(`Đã unsubscribe event: ${event}`)
      }
    }
  }

  private notifySubscribers(event: string, data: any) {
    const callbacks = this.subscribers.get(event)
    if (callbacks) {
      callbacks.forEach(callback => {
        try {
          callback(data)
        } catch (error) {
          console.error(`Lỗi trong callback cho event ${event}:`, error)
        }
      })
    }
  }

  send(data: any) {
    if (this.ws && this.isConnected) {
      this.ws.send(JSON.stringify(data))
      console.log('Đã gửi:', data)
    } else {
      console.warn('WebSocket chưa kết nối, không thể gửi data')
    }
  }

  disconnect() {
    if (this.ws) {
      this.ws.close()
      this.ws = null
      this.isConnected = false
      this.currentConnection = null
      console.log('Đã ngắt kết nối WebSocket')
    }
  }

  getConnectionStatus(): boolean {
    return this.isConnected
  }

  getConnectionInfo(): { ip: string, port: string } | null {
    return this.currentConnection
  }

  // Phương thức để test kết nối
  async testConnection(): Promise<boolean> {
    return this.connect()
  }
}

// Tạo instance singleton
const webSocketService = new WebSocketService()

export default webSocketService