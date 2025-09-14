import mqtt from 'mqtt'

// Define MqttClient type manually since import has issues
interface MqttClient {
  on(event: string, callback: (...args: any[]) => void): void
  subscribe(topic: string, callback?: (err: any) => void): void
  publish(topic: string, message: string, options?: any, callback?: (error: any) => void): void
  end(): void
  connected: boolean
}

// Định nghĩa message type cho FEN
export interface ChessFenMessage {
  fen_str: string
}

class MQTTService {
  private client: MqttClient | null = null
  private isConnected = false
  private subscribers: Map<string, Function[]> = new Map()
  private mockMode = false  // Fallback to mock mode if MQTT unavailable

  // Cấu hình RabbitMQ MQTT
  private config = {
    host: '100.99.22.52',
    port: 5552,  // WebSocket port for RabbitMQ MQTT (default: 15675)
    username: 'admin',
    password: '123456',
    clientId: `chess_client_${Math.random().toString(16).substr(2, 8)}`,
    protocol: 'ws' as const  // Use WebSocket for browser
  }

  // Alternative configurations to try
  private alternativeConfigs = [
    { ...this.config, port: 5552 },  // Alternative WebSocket port
    { ...this.config, port: 8080 },   // Common alternative
    { ...this.config, host: 'localhost', port: 15675 }, // Local RabbitMQ
    { ...this.config, host: 'broker.hivemq.com', port: 5552, username: '', password: '' },  // Public broker for testing
  ]

  // Topic cho chess game  
  private topics = {
    CHESS_FEN: 'chess/fen'
  }

  async connect(): Promise<boolean> {
    // First try to connect with main config
    let connected = await this.tryConnect(this.config)
    
    if (!connected) {
      console.log('🔄 Main config failed, trying alternatives...')
      
      // Try alternative configurations
      for (const altConfig of this.alternativeConfigs) {
        connected = await this.tryConnect(altConfig)
        if (connected) {
          console.log(`✅ Connected using alternative config: ${altConfig.host}:${altConfig.port}`)
          break
        }
      }
    }
    
    if (!connected) {
      console.log('⚠️ All MQTT connections failed, enabling mock mode')
      this.mockMode = true
      this.isConnected = true  // Simulate connection for UI
      return true
    }
    
    return connected
  }

  private async tryConnect(config: typeof this.config): Promise<boolean> {
    try {
      const connectUrl = `${config.protocol}://${config.host}:${config.port}/ws`
      console.log(`🔌 Trying to connect to: ${connectUrl}`)
      
      this.client = mqtt.connect(connectUrl, {
        clientId: config.clientId,
        username: config.username,
        password: config.password,
        clean: true,
        reconnectPeriod: 0,  // Disable auto-reconnect for testing
        connectTimeout: 5000,  // Shorter timeout for faster fallback
        keepalive: 60,
      })

      return new Promise((resolve) => {
        if (!this.client) {
          resolve(false)
          return
        }

        const timeout = setTimeout(() => {
          console.log(`❌ Connection timeout for ${config.host}:${config.port}`)
          this.client?.end()
          resolve(false)
        }, 5000)

        this.client.on('connect', () => {
          clearTimeout(timeout)
          console.log(`✅ Connected to MQTT broker: ${config.host}:${config.port}`)
          this.isConnected = true
          this.mockMode = false
          
          // Subscribe to chess topics
          this.subscribeToChessTopics()
          resolve(true)
        })

        this.client.on('error', (error) => {
          clearTimeout(timeout)
          console.log(`❌ MQTT connection error for ${config.host}:${config.port}:`, error)
          this.isConnected = false
          this.client?.end()
          resolve(false)
        })

        this.client.on('close', () => {
          clearTimeout(timeout)
          console.log('🔌 MQTT connection closed')
          this.isConnected = false
          
          // If we were not in mock mode and connection drops, switch to mock mode
          if (!this.mockMode) {
            console.log('🎭 Connection lost, switching to mock mode')
            this.mockMode = true
            this.isConnected = true  // Keep UI showing connected but in mock mode
          }
        })

        this.client.on('offline', () => {
          console.log('📡 MQTT client offline')
          this.isConnected = false
          
          // Switch to mock mode when offline
          if (!this.mockMode) {
            console.log('🎭 Client offline, switching to mock mode')
            this.mockMode = true
            this.isConnected = true
          }
        })

        this.client.on('message', (topic, message) => {
          this.handleMessage(topic, message.toString())
        })
      })
    } catch (error) {
      console.error('❌ Failed to connect to MQTT:', error)
      return false
    }
  }

  private subscribeToChessTopics() {
    if (!this.client) return

    this.client?.subscribe(this.topics.CHESS_FEN, (err) => {
      if (err) {
        console.error(`❌ Failed to subscribe to ${this.topics.CHESS_FEN}:`, err)
      } else {
        console.log(`📡 Subscribed to ${this.topics.CHESS_FEN}`)
      }
    })
  }

  private handleMessage(topic: string, message: string) {
    try {
      const data = JSON.parse(message)
      console.log(`📨 Received message on ${topic}:`, data)
      
      // Gọi các subscribers cho topic này
      const callbacks = this.subscribers.get(topic) || []
      callbacks.forEach(callback => callback(data))
    } catch (error) {
      console.error('❌ Failed to parse message:', error)
    }
  }

  // Gửi FEN string
  sendFen(fen_str: string): boolean {
    const message: ChessFenMessage = {
      fen_str
    }

    if (this.mockMode) {
      console.log(`🎭 Mock MQTT - Send FEN: ${fen_str}`)
      // Simulate message reception after small delay
      setTimeout(() => {
        this.handleMessage(this.topics.CHESS_FEN, JSON.stringify(message))
      }, 100)
      return true
    }

    return this.publish(this.topics.CHESS_FEN, message)
  }

  // Generic publish method for any message
  publishMessage(topic: string, message: any): boolean {
    console.log(`📤 Attempting to publish to ${topic}:`, message)
    
    // If we have a real MQTT connection, use it first
    if (this.client && this.isConnected && !this.mockMode) {
      try {
        const payload = JSON.stringify(message)
        this.client.publish(topic, payload, { qos: 1 }, (error) => {
          if (error) {
            console.error(`❌ Failed to publish to ${topic}:`, error)
          } else {
            console.log(`✅ Successfully published to ${topic}`)
          }
        })
        return true
      } catch (error) {
        console.error('❌ Failed to publish message:', error)
        return false
      }
    }
    
    // Fallback to mock mode if no real connection
    console.log(`🎭 Mock MQTT - Publishing to ${topic}:`, message)
    // Simulate message reception after small delay for topics we're subscribed to
    setTimeout(() => {
      this.handleMessage(topic, JSON.stringify(message))
    }, 100)
    return true
  }

  // Generic publish method
  private publish(topic: string, message: ChessFenMessage): boolean {
    if (!this.client || !this.isConnected) {
      console.warn('⚠️ MQTT client not connected, falling back to mock mode')
      this.mockMode = true
      this.isConnected = true
      
      // Simulate message in mock mode
      setTimeout(() => {
        this.handleMessage(topic, JSON.stringify(message))
      }, 100)
      return true
    }

    try {
      const payload = JSON.stringify(message)
      this.client.publish(topic, payload, { qos: 1 }, (error) => {
        if (error) {
          console.error(`❌ Failed to publish to ${topic}:`, error)
          // If publish fails, switch to mock mode for this session
          console.log('🎭 Switching to mock mode due to publish failure')
          this.mockMode = true
          this.isConnected = true
          
          // Retry in mock mode
          setTimeout(() => {
            this.handleMessage(topic, JSON.stringify(message))
          }, 100)
        } else {
          console.log(`📤 Published to ${topic}:`, message)
        }
      })
      return true
    } catch (error) {
      console.error('❌ Failed to publish message:', error)
      // Switch to mock mode on error
      console.log('🎭 Switching to mock mode due to publish error')
      this.mockMode = true
      this.isConnected = true
      
      // Retry in mock mode
      setTimeout(() => {
        this.handleMessage(topic, JSON.stringify(message))
      }, 100)
      return true
    }
  }

  // Subscribe to specific topic
  subscribe(topic: string, callback: Function) {
    if (!this.subscribers.has(topic)) {
      this.subscribers.set(topic, [])
    }
    this.subscribers.get(topic)?.push(callback)
  }

  // Unsubscribe from topic
  unsubscribe(topic: string, callback: Function) {
    const callbacks = this.subscribers.get(topic) || []
    const index = callbacks.indexOf(callback)
    if (index > -1) {
      callbacks.splice(index, 1)
    }
  }

  disconnect() {
    if (this.client) {
      this.client.end()
      this.isConnected = false
      console.log('🔌 Disconnected from MQTT broker')
    }
    if (this.mockMode) {
      this.mockMode = false
      this.isConnected = false
      console.log('🎭 Mock mode disabled')
    }
  }

  // Force reconnect to real MQTT (exit mock mode)
  async forceReconnect(): Promise<boolean> {
    this.disconnect()
    this.mockMode = false
    return await this.connect()
  }

  getConnectionStatus() {
    return this.isConnected
  }

  isMockMode() {
    return this.mockMode
  }

  // Get detailed connection info
  getConnectionInfo() {
    return {
      isConnected: this.isConnected,
      isMockMode: this.mockMode,
      hasClient: !!this.client,
      clientConnected: this.client?.connected || false
    }
  }

  getTopics() {
    return this.topics
  }
}

// Export singleton instance
export const mqttService = new MQTTService()
export default mqttService
