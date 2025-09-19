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
  private maxReconnectAttempts = 5
  private reconnectInterval = 3000

  // Cấu hình WebSocket server
  private config = {
    url: 'ws://localhost:8081'
  }

  async connect(): Promise<boolean> {
    try {
      console.log('🔄 Đang kết nối WebSocket...')
      
      this.ws = new WebSocket(this.config.url)
      
      return new Promise((resolve) => {
        if (!this.ws) {
          resolve(false)
          return
        }

        this.ws.onopen = () => {
          console.log('✅ WebSocket đã kết nối thành công')
          this.isConnected = true
          this.reconnectAttempts = 0
          resolve(true)
        }

        this.ws.onmessage = (event) => {
          try {
            const data = JSON.parse(event.data)
            console.log('📥 Nhận được message:', data)
            
            // Gọi tất cả subscribers
            this.notifySubscribers('message', data)
            
            // Gọi subscribers cho loại message cụ thể
            if (data.fen_str) {
              this.notifySubscribers('fen', data)
            }
          } catch (error) {
            console.error('❌ Lỗi parse message:', error)
          }
        }

        this.ws.onclose = () => {
          console.log('🔌 WebSocket đã ngắt kết nối')
          this.isConnected = false
          this.handleReconnect()
        }

        this.ws.onerror = (error) => {
          console.error('❌ WebSocket error:', error)
          this.isConnected = false
          resolve(false)
        }

        // Timeout sau 5 giây
        setTimeout(() => {
          if (!this.isConnected) {
            console.error('⏰ WebSocket connection timeout')
            resolve(false)
          }
        }, 5000)
      })
    } catch (error) {
      console.error('❌ Lỗi kết nối WebSocket:', error)
      return false
    }
  }

  private handleReconnect() {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++
      console.log(`🔄 Thử reconnect lần ${this.reconnectAttempts}/${this.maxReconnectAttempts}...`)
      
      setTimeout(() => {
        this.connect()
      }, this.reconnectInterval)
    } else {
      console.error('❌ Đã thử reconnect tối đa, dừng kết nối')
    }
  }

  subscribe(event: string, callback: Function) {
    if (!this.subscribers.has(event)) {
      this.subscribers.set(event, [])
    }
    this.subscribers.get(event)!.push(callback)
    console.log(`📝 Đã subscribe event: ${event}`)
  }

  unsubscribe(event: string, callback: Function) {
    const callbacks = this.subscribers.get(event)
    if (callbacks) {
      const index = callbacks.indexOf(callback)
      if (index > -1) {
        callbacks.splice(index, 1)
        console.log(`🗑️ Đã unsubscribe event: ${event}`)
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
          console.error(`❌ Lỗi trong callback cho event ${event}:`, error)
        }
      })
    }
  }

  send(data: any) {
    if (this.ws && this.isConnected) {
      this.ws.send(JSON.stringify(data))
      console.log('📤 Đã gửi:', data)
    } else {
      console.warn('⚠️ WebSocket chưa kết nối, không thể gửi data')
    }
  }

  disconnect() {
    if (this.ws) {
      this.ws.close()
      this.ws = null
      this.isConnected = false
      console.log('🔌 Đã ngắt kết nối WebSocket')
    }
  }

  getConnectionStatus(): boolean {
    return this.isConnected
  }

  // Phương thức để test kết nối
  async testConnection(): Promise<boolean> {
    return this.connect()
  }
}

// Tạo instance singleton
const webSocketService = new WebSocketService()

export default webSocketService