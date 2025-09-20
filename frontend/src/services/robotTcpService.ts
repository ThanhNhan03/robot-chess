// TCP Service for Robot Communication
export interface RobotCommandMessage {
  goal_id: string
  header: {
    timestamp: string
  }
  move: {
    type: 'move' | 'attack'
    from: string
    to: string
    from_piece: string
    to_piece?: string
    notation: string
    results_in_check: boolean
  }
}

export interface RobotResponse {
  success: boolean
  message: string
  goal_id?: string
  execution_time?: number
  error?: string
}

class RobotTcpService {
  private socket: WebSocket | null = null
  private isConnected = false
  private subscribers: Map<string, Function[]> = new Map()
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectInterval = 3000

  // Robot TCP configuration
  private robotIP = import.meta.env.VITE_ROBOT_IP || 'localhost'
  private robotPort = import.meta.env.VITE_ROBOT_PORT || '8082'
  private robotTcpEndpoint = import.meta.env.VITE_ROBOT_TCP_ENDPOINT || '/robot/move_piece/goal'

  // Connection status
  getConnectionStatus(): boolean {
    return this.isConnected
  }

  // Connect to robot TCP service (via WebSocket proxy)
  async connect(): Promise<boolean> {
    try {
      console.log(`Connecting to robot at ${this.robotIP}:${this.robotPort}`)
      
      // Create WebSocket connection to robot service
      const wsUrl = `ws://${this.robotIP}:${this.robotPort}`
      this.socket = new WebSocket(wsUrl)

      return new Promise((resolve) => {
        if (!this.socket) {
          resolve(false)
          return
        }

        this.socket.onopen = () => {
          console.log('Connected to robot TCP service')
          this.isConnected = true
          this.reconnectAttempts = 0
          this.notifySubscribers('connection', { connected: true })
          resolve(true)
        }

        this.socket.onclose = (event) => {
          console.log('Robot TCP connection closed:', event.code, event.reason)
          this.isConnected = false
          this.notifySubscribers('connection', { connected: false })
          
          // Auto-reconnect if not manually closed
          if (event.code !== 1000 && this.reconnectAttempts < this.maxReconnectAttempts) {
            this.scheduleReconnect()
          }
          resolve(false)
        }

        this.socket.onerror = (error) => {
          console.error('Robot TCP connection error:', error)
          this.notifySubscribers('error', { error: 'Connection failed' })
          resolve(false)
        }

        this.socket.onmessage = (event) => {
          try {
            const response: RobotResponse = JSON.parse(event.data)
            console.log('Robot response received:', response)
            this.notifySubscribers('response', response)
          } catch (error) {
            console.error('Failed to parse robot response:', error)
            this.notifySubscribers('error', { error: 'Invalid response format' })
          }
        }

        // Connection timeout
        setTimeout(() => {
          if (!this.isConnected) {
            this.socket?.close()
            resolve(false)
          }
        }, 5000)
      })
    } catch (error) {
      console.error('Robot connection error:', error)
      return false
    }
  }

  // Disconnect from robot
  disconnect(): void {
    if (this.socket) {
      this.socket.close(1000, 'Manual disconnect')
      this.socket = null
      this.isConnected = false
    }
  }

  // Send command to robot
  async sendCommand(command: RobotCommandMessage): Promise<boolean> {
    if (!this.isConnected || !this.socket) {
      console.error('Not connected to robot')
      this.notifySubscribers('error', { error: 'Not connected to robot' })
      return false
    }

    try {
      console.log('Sending command to robot:', command)
      
      // Send command via WebSocket
      this.socket.send(JSON.stringify(command))
      
      this.notifySubscribers('commandSent', command)
      return true
    } catch (error) {
      console.error('Failed to send command:', error)
      this.notifySubscribers('error', { error: 'Failed to send command' })
      return false
    }
  }

  // Alternative method for direct TCP (if needed)
  async sendCommandViaTcp(command: RobotCommandMessage): Promise<boolean> {
    try {
      // This would be used with a backend proxy that handles TCP connection
      const response = await fetch('/api/robot/command', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          endpoint: this.robotTcpEndpoint,
          command: command
        })
      })

      if (response.ok) {
        const result = await response.json()
        this.notifySubscribers('response', result)
        return true
      } else {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`)
      }
    } catch (error) {
      console.error('TCP command failed:', error)
      this.notifySubscribers('error', { error: 'TCP command failed' })
      return false
    }
  }

  // Subscribe to events
  subscribe(event: string, callback: Function): void {
    if (!this.subscribers.has(event)) {
      this.subscribers.set(event, [])
    }
    this.subscribers.get(event)!.push(callback)
  }

  // Unsubscribe from events
  unsubscribe(event: string, callback: Function): void {
    const eventSubscribers = this.subscribers.get(event)
    if (eventSubscribers) {
      const index = eventSubscribers.indexOf(callback)
      if (index > -1) {
        eventSubscribers.splice(index, 1)
      }
    }
  }

  // Notify subscribers
  private notifySubscribers(event: string, data: any): void {
    const eventSubscribers = this.subscribers.get(event)
    if (eventSubscribers) {
      eventSubscribers.forEach(callback => {
        try {
          callback(data)
        } catch (error) {
          console.error('Subscriber callback error:', error)
        }
      })
    }
  }

  // Schedule reconnection
  private scheduleReconnect(): void {
    this.reconnectAttempts++
    console.log(`Scheduling reconnect attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts}`)
    
    setTimeout(() => {
      if (!this.isConnected && this.reconnectAttempts <= this.maxReconnectAttempts) {
        this.connect()
      }
    }, this.reconnectInterval)
  }

  // Utility methods
  createCommand(
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
}

// Create singleton instance
const robotTcpService = new RobotTcpService()

export default robotTcpService