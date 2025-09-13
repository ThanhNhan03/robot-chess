<template>
  <div class="mqtt-control">
    <div class="control-header">
      <h3>🔌 MQTT Control</h3>
      <div :class="['status-indicator', isConnected ? 'connected' : 'disconnected']">
        {{ getStatusText() }}
      </div>
    </div>

    <div class="control-content">
      <!-- Connection Controls -->
      <div class="connection-section">
        <div class="connection-buttons">
          <button 
            @click="toggleConnection" 
            :class="['control-btn', isConnected ? 'disconnect' : 'connect']"
            :disabled="isConnecting"
          >
            {{ isConnecting ? 'Connecting...' : (isConnected ? 'Disconnect' : 'Connect') }}
          </button>
          
          <button 
            v-if="isConnected && mqttService.isMockMode()"
            @click="retryRealConnection" 
            class="control-btn retry"
            :disabled="isConnecting"
          >
            {{ isConnecting ? 'Retrying...' : '🔄 Retry Real MQTT' }}
          </button>
        </div>
        
        <div class="connection-info" v-if="isConnected">
          <small>
            {{ mqttService.isMockMode() ? 
              '🎭 Running in mock mode - messages are simulated locally' : 
              '🌐 Connected to real MQTT broker' }}
          </small>
        </div>
      </div>

      <!-- FEN Input Controls -->
      <div class="fen-controls" v-if="isConnected">
        <h4>FEN Input</h4>
        <div class="fen-inputs">
          <div class="input-group">
            <label>FEN String:</label>
            <input 
              v-model="fenString" 
              placeholder="e.g., rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"
              class="fen-input"
            />
          </div>
        </div>
        <button 
          @click="sendFenString" 
          :disabled="!canSendFen"
          class="control-btn send"
        >
          Send FEN
        </button>
      </div>

      <!-- Message Log -->
      <div class="message-log">
        <h4>Message Log</h4>
        <div class="log-container">
          <div 
            v-for="(msg, index) in messageLog" 
            :key="index"
            :class="['log-message', msg.type]"
          >
            <span class="timestamp">{{ formatTime(msg.timestamp) }}</span>
            <span class="message">{{ msg.message }}</span>
          </div>
        </div>
        <button @click="clearLog" class="control-btn clear">Clear Log</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import mqttService from '../services/mqttService'

interface LogMessage {
  type: 'sent' | 'received' | 'error' | 'info'
  message: string
  timestamp: Date
}

const isConnected = ref(false)
const isConnecting = ref(false)
const messageLog = ref<LogMessage[]>([])

// FEN input controls
const fenString = ref('')

const canSendFen = computed(() => {
  return fenString.value.trim() && isConnected.value
})

const getStatusText = () => {
  if (!isConnected.value) return 'Disconnected'
  if (mqttService.isMockMode()) return 'Mock Mode'
  return 'Connected'
}

const addLogMessage = (type: LogMessage['type'], message: string) => {
  messageLog.value.push({
    type,
    message,
    timestamp: new Date()
  })
  
  // Keep only last 50 messages
  if (messageLog.value.length > 50) {
    messageLog.value.shift()
  }
}

const toggleConnection = async () => {
  if (isConnected.value) {
    mqttService.disconnect()
    isConnected.value = false
    addLogMessage('info', 'Disconnected from MQTT broker')
  } else {
    isConnecting.value = true
    try {
      const connected = await mqttService.connect()
      isConnected.value = connected
      
      if (connected) {
        addLogMessage('info', mqttService.isMockMode() ? 
          'Connected in mock mode' : 'Connected to MQTT broker')
        setupMqttListeners()
      } else {
        addLogMessage('error', 'Failed to connect to MQTT broker')
      }
    } catch (error) {
      addLogMessage('error', `Connection error: ${error}`)
    } finally {
      isConnecting.value = false
    }
  }
}

const retryRealConnection = async () => {
  isConnecting.value = true
  addLogMessage('info', 'Attempting to reconnect to real MQTT broker...')
  
  try {
    const connected = await mqttService.forceReconnect()
    isConnected.value = connected
    
    if (connected && !mqttService.isMockMode()) {
      addLogMessage('info', 'Successfully connected to real MQTT broker!')
      setupMqttListeners()
    } else {
      addLogMessage('info', 'Real MQTT connection failed, staying in mock mode')
    }
  } catch (error) {
    addLogMessage('error', `Reconnection error: ${error}`)
  } finally {
    isConnecting.value = false
  }
}

const sendFenString = () => {
  if (!canSendFen.value) return

  const success = mqttService.sendFen(fenString.value.trim())

  if (success) {
    addLogMessage('sent', `FEN: ${fenString.value}`)
    // Clear input after successful send
    fenString.value = ''
  } else {
    addLogMessage('error', 'Failed to send FEN string')
  }
}

const clearLog = () => {
  messageLog.value = []
}

const formatTime = (timestamp: Date) => {
  return timestamp.toLocaleTimeString('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}

const setupMqttListeners = () => {
  const topics = mqttService.getTopics()
  
  // Listen to FEN topic
  const handleFenMessage = (data: any) => {
    addLogMessage('received', `FEN: ${data.fen_str || ''}`)
  }
  
  mqttService.subscribe(topics.CHESS_FEN, handleFenMessage)
}

onMounted(() => {
  // Check if already connected
  isConnected.value = mqttService.getConnectionStatus()
  if (isConnected.value) {
    addLogMessage('info', 'Already connected to MQTT')
    setupMqttListeners()
  }
})

onBeforeUnmount(() => {
  // Cleanup listeners if needed
})
</script>

<style scoped>
.mqtt-control {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  overflow: hidden;
}

.control-header {
  background: #2c3e50;
  color: white;
  padding: 15px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.control-header h3 {
  margin: 0;
  font-size: 18px;
}

.status-indicator {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: bold;
}

.status-indicator.connected {
  background: #27ae60;
  color: white;
}

.status-indicator.disconnected {
  background: #e74c3c;
  color: white;
}

.control-content {
  padding: 20px;
}

.connection-section {
  margin-bottom: 20px;
}

.connection-buttons {
  display: flex;
  gap: 10px;
  margin-bottom: 10px;
  flex-wrap: wrap;
}

.connection-info {
  padding: 8px 12px;
  background: #ecf0f1;
  border-radius: 6px;
  color: #7f8c8d;
  font-style: italic;
}

.control-btn {
  padding: 10px 16px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.3s;
  margin-right: 8px;
  margin-bottom: 8px;
}

.control-btn.connect {
  background: #27ae60;
  color: white;
}

.control-btn.disconnect {
  background: #e74c3c;
  color: white;
}

.control-btn.retry {
  background: #f39c12;
  color: white;
}

.control-btn.send {
  background: #3498db;
  color: white;
}

.control-btn.reset {
  background: #f39c12;
  color: white;
}

.control-btn.status {
  background: #9b59b6;
  color: white;
}

.control-btn.clear {
  background: #95a5a6;
  color: white;
  margin-top: 10px;
}

.control-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}

.control-btn:disabled {
  background: #bdc3c7;
  cursor: not-allowed;
  transform: none;
}

.fen-controls {
  margin-bottom: 20px;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 8px;
}

.fen-controls h4 {
  margin: 0 0 15px 0;
  color: #2c3e50;
}

.fen-inputs {
  margin-bottom: 15px;
}

.input-group {
  display: flex;
  flex-direction: column;
}

.input-group label {
  font-size: 12px;
  font-weight: bold;
  color: #7f8c8d;
  margin-bottom: 4px;
}

.fen-input {
  padding: 8px;
  border: 2px solid #ecf0f1;
  border-radius: 4px;
  font-size: 14px;
  transition: border-color 0.3s;
  width: 100%;
}

.fen-input:focus {
  outline: none;
  border-color: #3498db;
}

.message-log h4 {
  margin: 0 0 10px 0;
  color: #2c3e50;
}

.log-container {
  background: #2c3e50;
  border-radius: 6px;
  padding: 10px;
  max-height: 200px;
  overflow-y: auto;
  font-family: 'Courier New', monospace;
  font-size: 12px;
}

.log-message {
  display: flex;
  margin-bottom: 5px;
  word-break: break-all;
}

.timestamp {
  color: #95a5a6;
  margin-right: 8px;
  flex-shrink: 0;
}

.message {
  flex: 1;
}

.log-message.sent .message {
  color: #27ae60;
}

.log-message.received .message {
  color: #3498db;
}

.log-message.error .message {
  color: #e74c3c;
}

.log-message.info .message {
  color: #f39c12;
}

/* Scrollbar styling */
.log-container::-webkit-scrollbar {
  width: 6px;
}

.log-container::-webkit-scrollbar-track {
  background: #34495e;
}

.log-container::-webkit-scrollbar-thumb {
  background: #7f8c8d;
  border-radius: 3px;
}
</style>
