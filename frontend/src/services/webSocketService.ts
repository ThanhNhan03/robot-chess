import type { RobotCommandMessage } from '../interfaces/websocket.interface'

class WebSocketService {
  private ws: WebSocket | null = null
  private isConnected = false
  private subscribers: Map<string, Function[]> = new Map()
  private reconnectAttempts = 0
  private maxReconnectAttempts = Number(import.meta.env.VITE_WS_RECONNECT_ATTEMPTS) || 5
  private reconnectInterval = Number(import.meta.env.VITE_WS_RECONNECT_INTERVAL) || 3000
  private connectionTimeout = Number(import.meta.env.VITE_WS_CONNECTION_TIMEOUT) || 5000

  // Cấu hình WebSocket servers với fallback
  private primaryIP = import.meta.env.VITE_WS_PRIMARY_IP || 'localhost'
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
            console.log('Nhận được message từ server:', data)
            
            // Xử lý các loại message khác nhau
            this.handleServerMessage(data)
            
            // Gọi tất cả subscribers cho 'message'
            this.notifySubscribers('message', data)
            
            // Gọi subscribers cho loại message cụ thể
            if (data.fen_str) {
              this.notifySubscribers('fen', data)
            }
            
            if (data.type === 'robot_response') {
              this.notifySubscribers('robot_response', data)
            }
            
            if (data.type === 'command_sent') {
              this.notifySubscribers('command_sent', data)
            }
            
            if (data.type === 'connection') {
              this.notifySubscribers('connection', data)
            }
            
          } catch (error) {
            console.error('Lỗi parse message:', error)
            this.notifySubscribers('error', { error: 'Failed to parse server message' })
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

  // Xử lý các loại message từ server
  private handleServerMessage(data: any): void {
    try {
      switch (data.type) {
        case 'connection':
          console.log('Server connection established:', data.message)
          if (data.connected_robots !== undefined) {
            console.log(`Connected robots: ${data.connected_robots}`)
          }
          if (data.capabilities) {
            console.log('Server capabilities:', data.capabilities)
          }
          break

        case 'robot_response':
          console.log('Robot response:', data)
          if (data.success) {
            console.log(`Command ${data.goal_id} executed successfully`)
          } else {
            console.log(`Command ${data.goal_id} failed: ${data.error}`)
          }
          break

        case 'command_sent':
          console.log('Command sent to robot:', data.goal_id)
          break

        case 'error':
          console.error('Server error:', data.message)
          break

        default:
          if (data.fen_str) {
            console.log('FEN position update:', data.fen_str)
          } else {
            console.log('Unknown message type:', data.type || 'no type')
          }
      }
    } catch (error) {
      console.error('Error handling server message:', error)
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
      try {
        const message = JSON.stringify(data)
        this.ws.send(message)
        console.log('Đã gửi:', data)
        
        // Log specific message types
        if (data.goal_id && data.move) {
          console.log(`Sending robot command: ${data.move.type} ${data.move.from_piece} ${data.move.from}→${data.move.to}`)
        }
      } catch (error) {
        console.error('Error sending message:', error)
        this.notifySubscribers('error', { error: 'Failed to send message' })
      }
    } else {
      console.warn('WebSocket chưa kết nối, không thể gửi data')
      this.notifySubscribers('error', { error: 'WebSocket not connected' })
    }
  }

  // Gửi robot command
  sendRobotCommand(command: RobotCommandMessage): boolean {
    if (!this.isConnected) {
      console.error('Cannot send robot command: not connected')
      return false
    }
    
    this.send(command)
    return true
  }

  // Kiểm tra có robot nào connected không
  async checkRobotStatus(): Promise<void> {
    if (this.isConnected) {
      this.send({ type: 'robot_status_request' })
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

  // Utility method để tạo robot command
  createRobotCommand(
    type: 'move' | 'attack',
    fromPiece: string,
    from: string,
    to: string,
    toPiece?: string,
    resultsInCheck: boolean = false
  ): RobotCommandMessage {
    const goalId = `${type}_${Date.now().toString().slice(-6)}`
    const notation = this.generateNotation(type, fromPiece, from, to, resultsInCheck)

    const command: RobotCommandMessage = {
      goal_id: goalId,
      header: {
        timestamp: new Date().toISOString()
      },
      move: {
        type,
        from: from.toLowerCase(),
        to: to.toLowerCase(),
        from_piece: fromPiece,
        notation,
        results_in_check: resultsInCheck
      }
    }

    if (type === 'attack' && toPiece) {
      command.move.to_piece = toPiece
    }

    return command
  }

  // Generate chess notation
  private generateNotation(
    type: 'move' | 'attack',
    piece: string,
    from: string,
    to: string,
    check: boolean
  ): string {
    const pieceSymbols: { [key: string]: string } = {
      'white_king': 'K', 'black_king': 'K',
      'white_queen': 'Q', 'black_queen': 'Q',
      'white_rook': 'R', 'black_rook': 'R',
      'white_bishop': 'B', 'black_bishop': 'B',
      'white_knight': 'N', 'black_knight': 'N',
      'white_pawn': '', 'black_pawn': ''
    }

    const pieceSymbol = pieceSymbols[piece] || ''
    const capture = type === 'attack' ? 'x' : ''
    const checkSymbol = check ? '+' : ''
    
    return `${pieceSymbol}${from}${capture}${to}${checkSymbol}`
  }

  // Get server statistics
  getServerStats(): { connected: boolean, connectionInfo: any, subscribers: number } {
    return {
      connected: this.isConnected,
      connectionInfo: this.currentConnection,
      subscribers: Array.from(this.subscribers.keys()).length
    }
  }
}

// Tạo instance singleton
const webSocketService = new WebSocketService()

export default webSocketService