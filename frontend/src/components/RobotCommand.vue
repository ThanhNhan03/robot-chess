<template>
  <div class="robot-command">
    <div class="command-header">
      <h3>Robot Command Control</h3>
      <div class="connection-status" :class="{ connected: isConnected }">
        {{ isConnected ? 'Connected' : 'Disconnected' }}
      </div>
    </div>
    
    <form @submit.prevent="sendCommand" class="command-form">
      <!-- Type Selection -->
      <div class="form-group">
        <label for="moveType">Move Type:</label>
        <select id="moveType" v-model="command.type" required>
          <option value="">Select move type</option>
          <option value="move">Move</option>
          <option value="attack">Attack</option>
        </select>
      </div>

      <!-- Piece Selection -->
      <div class="form-group">
        <label for="piece">Select Piece:</label>
        <select id="piece" v-model="command.piece" required>
          <option value="">Select piece</option>
          <optgroup label="White Pieces">
            <option value="white_king">White King</option>
            <option value="white_queen">White Queen</option>
            <option value="white_rook">White Rook</option>
            <option value="white_bishop">White Bishop</option>
            <option value="white_knight">White Knight</option>
            <option value="white_pawn">White Pawn</option>
          </optgroup>
          <optgroup label="Black Pieces">
            <option value="black_king">Black King</option>
            <option value="black_queen">Black Queen</option>
            <option value="black_rook">Black Rook</option>
            <option value="black_bishop">Black Bishop</option>
            <option value="black_knight">Black Knight</option>
            <option value="black_pawn">Black Pawn</option>
          </optgroup>
        </select>
      </div>

      <!-- From Position -->
      <div class="form-group">
        <label for="fromPosition">From Position:</label>
        <input 
          id="fromPosition"
          type="text" 
          v-model="command.from" 
          placeholder="e.g., d1"
          pattern="[a-h][1-8]"
          title="Enter position like a1, b2, etc."
          required
          maxlength="2"
        />
      </div>

      <!-- To Position -->
      <div class="form-group">
        <label for="toPosition">To Position:</label>
        <input 
          id="toPosition"
          type="text" 
          v-model="command.to" 
          placeholder="e.g., f7"
          pattern="[a-h][1-8]"
          title="Enter position like a1, b2, etc."
          required
          maxlength="2"
        />
      </div>

      <!-- Target Piece (only for attack) -->
      <div class="form-group" v-if="command.type === 'attack'">
        <label for="targetPiece">Target Piece:</label>
        <select id="targetPiece" v-model="command.targetPiece" :required="command.type === 'attack'">
          <option value="">Select target piece</option>
          <optgroup label="White Pieces">
            <option value="white_king">White King</option>
            <option value="white_queen">White Queen</option>
            <option value="white_rook">White Rook</option>
            <option value="white_bishop">White Bishop</option>
            <option value="white_knight">White Knight</option>
            <option value="white_pawn">White Pawn</option>
          </optgroup>
          <optgroup label="Black Pieces">
            <option value="black_king">Black King</option>
            <option value="black_queen">Black Queen</option>
            <option value="black_rook">Black Rook</option>
            <option value="black_bishop">Black Bishop</option>
            <option value="black_knight">Black Knight</option>
            <option value="black_pawn">Black Pawn</option>
          </optgroup>
        </select>
      </div>

      <!-- Additional Options -->
      <div class="form-group">
        <label>
          <input type="checkbox" v-model="command.resultsInCheck" />
          Results in Check
        </label>
      </div>

      <!-- Submit Button -->
      <div class="form-actions">
        <button type="submit" :disabled="!isConnected || isLoading" class="send-button">
          {{ isLoading ? 'Sending...' : 'Send Command to Robot' }}
        </button>
        <button type="button" @click="resetForm" class="reset-button">
          Reset
        </button>
      </div>
    </form>

    <!-- Command Preview -->
    <div class="command-preview" v-if="previewCommand">
      <h4>Command Preview:</h4>
      <pre>{{ previewCommand }}</pre>
    </div>

    <!-- Status Messages -->
    <div class="status-messages" v-if="statusMessage">
      <div :class="['status-message', statusMessage.type]">
        {{ statusMessage.text }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import webSocketService from '../services/webSocketService'

// Types
interface RobotCommand {
  type: 'move' | 'attack' | ''
  piece: string
  from: string
  to: string
  targetPiece?: string
  resultsInCheck: boolean
}

// Reactive data
const command = ref<RobotCommand>({
  type: '',
  piece: '',
  from: '',
  to: '',
  targetPiece: '',
  resultsInCheck: false
})

const isConnected = ref(false)
const isLoading = ref(false)
const statusMessage = ref<{ text: string, type: 'success' | 'error' | 'info' } | null>(null)

// Computed properties
const previewCommand = computed(() => {
  if (!command.value.type || !command.value.piece || !command.value.from || !command.value.to) {
    return null
  }

  const commandData = webSocketService.createRobotCommand(
    command.value.type as 'move' | 'attack',
    command.value.piece,
    command.value.from,
    command.value.to,
    command.value.targetPiece,
    command.value.resultsInCheck
  )

  return JSON.stringify(commandData, null, 2)
})

// Methods
const connectToRobot = async () => {
  try {
    const connected = await webSocketService.connect()
    isConnected.value = connected
    
    if (connected) {
      showStatus('Connected to server', 'success')
    } else {
      showStatus('Failed to connect to server', 'error')
    }
  } catch (error) {
    console.error('Connection error:', error)
    showStatus('Connection failed', 'error')
  }
}

const sendCommand = async () => {
  if (!isConnected.value || !command.value.type || !command.value.piece || !command.value.from || !command.value.to) {
    showStatus('Please fill in all required fields and connect to server', 'error')
    return
  }

  isLoading.value = true
  showStatus('Sending command to robot...', 'info')

  try {
    const robotCommand = webSocketService.createRobotCommand(
      command.value.type as 'move' | 'attack',
      command.value.piece,
      command.value.from,
      command.value.to,
      command.value.targetPiece,
      command.value.resultsInCheck
    )

    const success = webSocketService.sendRobotCommand(robotCommand)
    
    if (success) {
      showStatus('Command sent to server', 'info')
    } else {
      showStatus('Failed to send command', 'error')
      isLoading.value = false
    }
  } catch (error) {
    console.error('Send command error:', error)
    showStatus('Failed to send command', 'error')
    isLoading.value = false
  }
}

const resetForm = () => {
  command.value = {
    type: '',
    piece: '',
    from: '',
    to: '',
    targetPiece: '',
    resultsInCheck: false
  }
  statusMessage.value = null
}

const showStatus = (text: string, type: 'success' | 'error' | 'info') => {
  statusMessage.value = { text, type }
  setTimeout(() => {
    statusMessage.value = null
  }, 5000)
}

const formatPosition = (position: string): string => {
  return position.toLowerCase().replace(/[^a-h1-8]/g, '')
}

// Watchers
watch(() => command.value.from, (newVal) => {
  if (newVal) {
    command.value.from = formatPosition(newVal)
  }
})

watch(() => command.value.to, (newVal) => {
  if (newVal) {
    command.value.to = formatPosition(newVal)
  }
})

// Lifecycle
onMounted(() => {
  // Set up event listeners for WebSocket service
  webSocketService.subscribe('connection', (data: any) => {
    if (data.connected !== undefined) {
      isConnected.value = data.connected
    }
  })

  webSocketService.subscribe('robot_response', (response: any) => {
    console.log('🤖 Robot response:', response)
    if (response.success) {
      showStatus(`Command executed successfully: ${response.response?.message || 'Success'}`, 'success')
    } else {
      showStatus(`Command failed: ${response.error || 'Unknown error'}`, 'error')
    }
    isLoading.value = false
  })

  webSocketService.subscribe('command_sent', (data: any) => {
    console.log('📤 Command sent:', data)
    showStatus(`Command ${data.goal_id} sent to robot`, 'info')
  })

  webSocketService.subscribe('error', (data: { error: string }) => {
    showStatus(data.error, 'error')
    isLoading.value = false
  })

  // Monitor connection status
  const checkConnection = () => {
    isConnected.value = webSocketService.getConnectionStatus()
  }

  // Check connection every 5 seconds
  const connectionChecker = setInterval(checkConnection, 5000)

  // Initial connection
  connectToRobot()

  // Cleanup interval on unmount
  onUnmounted(() => {
    clearInterval(connectionChecker)
    webSocketService.disconnect()
  })
})
</script>

<style scoped>
.robot-command {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  max-width: 500px;
  margin: 0 auto;
}

.command-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 2px solid rgba(102, 126, 234, 0.1);
}

.command-header h3 {
  margin: 0;
  color: #2c3e50;
  font-size: 1.5rem;
  font-weight: 600;
}

.connection-status {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 500;
  background: #e74c3c;
  color: white;
  transition: all 0.3s ease;
}

.connection-status.connected {
  background: #27ae60;
}

.command-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-weight: 500;
  color: #2c3e50;
  font-size: 0.95rem;
}

.form-group select,
.form-group input {
  padding: 12px 16px;
  border: 2px solid rgba(102, 126, 234, 0.2);
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: white;
}

.form-group select:focus,
.form-group input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-group input[type="checkbox"] {
  width: auto;
  margin-right: 8px;
}

.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 8px;
}

.send-button,
.reset-button {
  flex: 1;
  padding: 14px 20px;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.send-button {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.send-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(102, 126, 234, 0.3);
}

.send-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.reset-button {
  background: #ecf0f1;
  color: #2c3e50;
  border: 2px solid rgba(44, 62, 80, 0.1);
}

.reset-button:hover {
  background: #d5dbdb;
  transform: translateY(-1px);
}

.command-preview {
  margin-top: 24px;
  padding: 16px;
  background: #f8f9fa;
  border-radius: 8px;
  border-left: 4px solid #667eea;
}

.command-preview h4 {
  margin: 0 0 12px 0;
  color: #2c3e50;
  font-size: 1rem;
}

.command-preview pre {
  margin: 0;
  font-family: 'Monaco', 'Menlo', 'Consolas', monospace;
  font-size: 0.875rem;
  color: #2c3e50;
  white-space: pre-wrap;
  word-break: break-word;
}

.status-messages {
  margin-top: 16px;
}

.status-message {
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 500;
}

.status-message.success {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.status-message.error {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.status-message.info {
  background: #cce7ff;
  color: #004085;
  border: 1px solid #b8daff;
}

/* Responsive Design */
@media (max-width: 768px) {
  .robot-command {
    padding: 16px;
    margin: 0 8px;
  }
  
  .command-header {
    flex-direction: column;
    gap: 12px;
    align-items: flex-start;
  }
  
  .form-actions {
    flex-direction: column;
  }
}
</style>