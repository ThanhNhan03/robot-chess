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

      <!-- Manual Move Controls -->
      <div class="manual-controls" v-if="isConnected">
        <h4>Manual Move</h4>
        <div class="move-inputs">
          <div class="input-group">
            <label>From:</label>
            <input 
              v-model="manualFrom" 
              placeholder="e.g., e2" 
              maxlength="2"
              class="move-input"
            />
          </div>
          <div class="input-group">
            <label>To:</label>
            <input 
              v-model="manualTo" 
              placeholder="e.g., e4" 
              maxlength="2"
              class="move-input"
            />
          </div>
          <div class="input-group">
            <label>Piece:</label>
            <select v-model="manualPiece" class="piece-select">
              <option value="">Select piece</option>
              <option value="wp">White Pawn</option>
              <option value="wr">White Rook</option>
              <option value="wn">White Knight</option>
              <option value="wb">White Bishop</option>
              <option value="wq">White Queen</option>
              <option value="wk">White King</option>
              <option value="bp">Black Pawn</option>
              <option value="br">Black Rook</option>
              <option value="bn">Black Knight</option>
              <option value="bb">Black Bishop</option>
              <option value="bq">Black Queen</option>
              <option value="bk">Black King</option>
            </select>
          </div>
        </div>
        <button 
          @click="sendManualMove" 
          :disabled="!canSendMove"
          class="control-btn send"
        >
          Send Move
        </button>
      </div>

      <!-- Quick Actions -->
      <div class="quick-actions" v-if="isConnected">
        <h4>Quick Actions</h4>
        <div class="action-buttons">
          <button @click="sendReset" class="control-btn reset">
            🔄 Reset Board
          </button>
          <button @click="sendStatus" class="control-btn status">
            📊 Send Status
          </button>
        </div>
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

// Manual move controls
const manualFrom = ref('')
const manualTo = ref('')
const manualPiece = ref('')

const canSendMove = computed(() => {
  return manualFrom.value && manualTo.value && manualPiece.value && isConnected.value
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

const sendManualMove = () => {
  if (!canSendMove.value) return

  const success = mqttService.sendMove(
    manualFrom.value.toLowerCase(),
    manualTo.value.toLowerCase(),
    manualPiece.value
  )

  if (success) {
    addLogMessage('sent', `Move: ${manualPiece.value} from ${manualFrom.value} to ${manualTo.value}`)
    // Clear inputs after successful send
    manualFrom.value = ''
    manualTo.value = ''
    manualPiece.value = ''
  } else {
    addLogMessage('error', 'Failed to send move')
  }
}

const sendReset = () => {
  const success = mqttService.sendReset()
  if (success) {
    addLogMessage('sent', 'Reset board command')
  } else {
    addLogMessage('error', 'Failed to send reset command')
  }
}

const sendStatus = () => {
  // Mock board state for demo
  const mockBoard = [
    ['br', 'bn', 'bb', 'bq', 'bk', 'bb', 'bn', 'br'],
    ['bp', 'bp', 'bp', 'bp', 'bp', 'bp', 'bp', 'bp'],
    ['', '', '', '', '', '', '', ''],
    ['', '', '', '', '', '', '', ''],
    ['', '', '', '', '', '', '', ''],
    ['', '', '', '', '', '', '', ''],
    ['wp', 'wp', 'wp', 'wp', 'wp', 'wp', 'wp', 'wp'],
    ['wr', 'wn', 'wb', 'wq', 'wk', 'wb', 'wn', 'wr']
  ]
  
  const success = mqttService.sendStatus(mockBoard, 'white')
  if (success) {
    addLogMessage('sent', 'Board status')
  } else {
    addLogMessage('error', 'Failed to send status')
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
  
  // Listen to all chess topics
  const handleMessage = (data: any) => {
    addLogMessage('received', `${data.type}: ${JSON.stringify(data)}`)
  }
  
  mqttService.subscribe(topics.CHESS_MOVES, handleMessage)
  mqttService.subscribe(topics.CHESS_STATUS, handleMessage)
  mqttService.subscribe(topics.CHESS_CONTROL, handleMessage)
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

.manual-controls, .quick-actions {
  margin-bottom: 20px;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 8px;
}

.manual-controls h4, .quick-actions h4 {
  margin: 0 0 15px 0;
  color: #2c3e50;
}

.move-inputs {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 10px;
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

.move-input, .piece-select {
  padding: 8px;
  border: 2px solid #ecf0f1;
  border-radius: 4px;
  font-size: 14px;
  transition: border-color 0.3s;
}

.move-input:focus, .piece-select:focus {
  outline: none;
  border-color: #3498db;
}

.action-buttons {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
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
