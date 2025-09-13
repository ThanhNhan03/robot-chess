<template>
  <div class="mqtt-log">
    <div class="log-header">
      <h3>📡 MQTT Messages</h3>
      <div :class="['status-indicator', isConnected ? 'connected' : 'disconnected']">
        {{ getStatusText() }}
      </div>
    </div>

    <div class="log-content">
      <!-- Message Log -->
      <div class="message-log">
        <div class="log-container">
          <div 
            v-for="(msg, index) in messageLog" 
            :key="index"
            :class="['log-message', msg.type]"
          >
            <span class="timestamp">{{ formatTime(msg.timestamp) }}</span>
            <span class="message">{{ msg.message }}</span>
          </div>
          
          <div v-if="messageLog.length === 0" class="empty-log">
            <p>📭 No messages yet</p>
            <small>Messages from MQTTX or other MQTT clients will appear here</small>
          </div>
        </div>
        
        <div class="log-actions">
          <button @click="clearLog" class="control-btn clear">
            🗑️ Clear Log
          </button>
          <button @click="autoConnect" class="control-btn connect" :disabled="isConnecting">
            {{ isConnecting ? 'Connecting...' : '🔄 Auto Connect' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import mqttService from '../services/mqttService'

interface LogMessage {
  type: 'sent' | 'received' | 'error' | 'info'
  message: string
  timestamp: Date
}

const isConnected = ref(false)
const isConnecting = ref(false)
const messageLog = ref<LogMessage[]>([])

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
  
  // Keep only last 100 messages
  if (messageLog.value.length > 100) {
    messageLog.value.shift()
  }
}

const autoConnect = async () => {
  isConnecting.value = true
  try {
    const connected = await mqttService.connect()
    isConnected.value = connected
    
    if (connected) {
      addLogMessage('info', mqttService.isMockMode() ? 
        'Auto-connected in mock mode' : 'Auto-connected to MQTT broker')
      setupMqttListeners()
    } else {
      addLogMessage('error', 'Failed to auto-connect to MQTT broker')
    }
  } catch (error) {
    addLogMessage('error', `Auto-connection error: ${error}`)
  } finally {
    isConnecting.value = false
  }
}

const clearLog = () => {
  messageLog.value = []
  addLogMessage('info', 'Log cleared')
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
  
  // Listen to FEN topic only
  mqttService.subscribe(topics.CHESS_FEN, (data: any) => {
    addLogMessage('received', `FEN: ${data.fen_str || ''}`)
  })
}

onMounted(() => {
  // Auto-connect on startup
  autoConnect()
  
  addLogMessage('info', 'MQTT Log started - listening for FEN messages')
  addLogMessage('info', 'Topic: chess/fen')
})

onBeforeUnmount(() => {
  // Cleanup listeners if needed
})
</script>

<style scoped>
.mqtt-log {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  overflow: hidden;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.log-header {
  background: #2c3e50;
  color: white;
  padding: 15px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
}

.log-header h3 {
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

.log-content {
  padding: 20px;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.message-log {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.log-container {
  background: #2c3e50;
  border-radius: 6px;
  padding: 15px;
  font-family: 'Courier New', monospace;
  font-size: 13px;
  max-height: 300px;
  min-height: 150px;
  overflow-y: auto;
  box-sizing: border-box;
}

.log-message {
  display: flex;
  margin-bottom: 8px;
  word-break: break-word;
  line-height: 1.4;
}

.timestamp {
  color: #95a5a6;
  margin-right: 12px;
  flex-shrink: 0;
  font-weight: 500;
}

.message {
  flex: 1;
}

.log-message.sent .message {
  color: #27ae60;
}

.log-message.received .message {
  color: #3498db;
  font-weight: 500;
}

.log-message.error .message {
  color: #e74c3c;
  font-weight: 500;
}

.log-message.info .message {
  color: #f39c12;
}

.empty-log {
  text-align: center;
  color: #7f8c8d;
  padding: 40px 20px;
}

.empty-log p {
  margin: 0 0 10px 0;
  font-size: 16px;
}

.empty-log small {
  font-size: 12px;
  opacity: 0.8;
}

.log-actions {
  display: flex;
  gap: 10px;
  margin-top: 15px;
  justify-content: space-between;
}

.control-btn {
  padding: 10px 16px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.3s;
}

.control-btn.clear {
  background: #95a5a6;
  color: white;
}

.control-btn.connect {
  background: #27ae60;
  color: white;
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

/* Scrollbar styling */
.log-container::-webkit-scrollbar {
  width: 8px;
}

.log-container::-webkit-scrollbar-track {
  background: #34495e;
}

.log-container::-webkit-scrollbar-thumb {
  background: #7f8c8d;
  border-radius: 4px;
}

.log-container::-webkit-scrollbar-thumb:hover {
  background: #95a5a6;
}
</style>
