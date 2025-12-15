<template>
  <div class="manual-control">
    <div class="panel-header">
      <h2 class="panel-title">
        <Gamepad2 :size="24" />
        Robot Manual Control
      </h2>
      <!-- <button 
        class="btn-flat btn-danger" 
        @click="handleEmergencyStop"
        :disabled="!isConnected"
      >
        <AlertOctagon :size="18" /> Emergency Stop
      </button> -->
    </div>

    <!-- Connection Status -->
    <div class="connection-card" :class="isConnected ? 'status-connected' : 'status-disconnected'">
      <div class="connection-status">
        <div class="status-indicator" :class="isConnected ? 'indicator-online' : 'indicator-offline'"></div>
        <div class="status-text">
          <span class="status-label">WebSocket Server</span>
          <span class="status-value">{{ isConnected ? 'Connected' : 'Disconnected' }}</span>
        </div>
      </div>
      <div class="connection-info">
        <span class="info-item"><Wifi :size="14" /> {{ wsUrl }}</span>
        <span v-if="isConnected" class="info-item"><Clock :size="14" /> Connected {{ connectionDuration }}</span>
      </div>
      <button 
        class="btn-flat btn-sm" 
        :class="isConnected ? 'btn-danger' : 'btn-success'"
        @click="isConnected ? disconnect() : connect()"
        :disabled="isConnecting"
      >
        {{ isConnecting ? 'Connecting...' : isConnected ? 'Disconnect' : 'Connect' }}
      </button>
    </div>

    <!-- Quick Move Commands -->
    <div class="panel-section">
      <h3 class="section-title">Quick Commands</h3>
      <div class="command-grid">
        <button 
          class="command-btn btn-primary"
          @click="sendTestMove"
          :disabled="!isConnected || isSending"
        >
          <Play :size="20" />
          <span>Test Move</span>
          <small>e2 → e4</small>
        </button>
        <button 
          class="command-btn btn-warning"
          @click="sendTestAttack"
          :disabled="!isConnected || isSending"
        >
          <Target :size="20" />
          <span>Test Attack</span>
          <small>e4 × d5</small>
        </button>
      </div>
    </div>

    <!-- Custom Move Builder -->
    <div class="panel-section">
      <h3 class="section-title">Custom Move Builder</h3>
      <div class="move-builder-card">
        <div class="builder-row">
          <div class="builder-field">
            <label>Move Type</label>
            <select v-model="customMove.type" class="input-flat">
              <option value="move">Normal Move</option>
              <option value="attack">Attack/Capture</option>
              <option value="kingside_castle">Kingside Castle (O-O)</option>
              <option value="queenside_castle">Queenside Castle (O-O-O)</option>
            </select>
          </div>
          <div class="builder-field">
            <label>From Square</label>
            <input 
              v-model="customMove.from" 
              type="text" 
              class="input-flat" 
              placeholder="e.g. e2"
              maxlength="2"
              @input="validateSquare('from')"
            />
          </div>
          <div class="builder-field">
            <label>To Square</label>
            <input 
              v-model="customMove.to" 
              type="text" 
              class="input-flat" 
              placeholder="e.g. e4"
              maxlength="2"
              @input="validateSquare('to')"
            />
          </div>
        </div>

        <div class="builder-row">
          <div class="builder-field">
            <label>From Piece</label>
            <select v-model="customMove.fromPiece" class="input-flat">
              <option value="">Select piece</option>
              <option value="white_pawn">White Pawn</option>
              <option value="white_knight">White Knight</option>
              <option value="white_bishop">White Bishop</option>
              <option value="white_rook">White Rook</option>
              <option value="white_queen">White Queen</option>
              <option value="white_king">White King</option>
              <option value="black_pawn">Black Pawn</option>
              <option value="black_knight">Black Knight</option>
              <option value="black_bishop">Black Bishop</option>
              <option value="black_rook">Black Rook</option>
              <option value="black_queen">Black Queen</option>
              <option value="black_king">Black King</option>
            </select>
          </div>
          <div class="builder-field" v-if="customMove.type === 'attack'">
            <label>To Piece (Captured)</label>
            <select v-model="customMove.toPiece" class="input-flat">
              <option value="">No piece (empty square)</option>
              <option value="white_pawn">White Pawn</option>
              <option value="white_knight">White Knight</option>
              <option value="white_bishop">White Bishop</option>
              <option value="white_rook">White Rook</option>
              <option value="white_queen">White Queen</option>
              <option value="black_pawn">Black Pawn</option>
              <option value="black_knight">Black Knight</option>
              <option value="black_bishop">Black Bishop</option>
              <option value="black_rook">Black Rook</option>
              <option value="black_queen">Black Queen</option>
            </select>
          </div>
        </div>

        <div class="builder-actions">
          <button 
            class="btn-flat btn-secondary" 
            @click="resetCustomMove"
          >
            <RotateCcw :size="16" /> Reset
          </button>
          <button 
            class="btn-flat btn-success" 
            @click="sendCustomMove"
            :disabled="!isConnected || isSending || !isCustomMoveValid"
          >
            <Send :size="16" /> {{ isSending ? 'Sending...' : 'Send Command' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Command Log -->
    <div class="panel-section">
      <h3 class="section-title">
        Command Log
        <button class="btn-flat btn-sm btn-secondary" @click="commandLog = []">
          <Trash2 :size="14" /> Clear Log
        </button>
      </h3>
      <div class="command-log">
        <div 
          v-for="(log, index) in commandLog" 
          :key="index"
          class="log-entry"
          :class="`log-${log.type}`"
        >
          <div class="log-time">{{ log.time }}</div>
          <div class="log-icon">
            <ArrowUp v-if="log.direction === 'sent'" :size="14" />
            <ArrowDown v-else :size="14" />
          </div>
          <div class="log-content">
            <div class="log-label">{{ log.label }}</div>
            <div class="log-details">{{ log.details }}</div>
          </div>
        </div>
        <div v-if="commandLog.length === 0" class="empty-log">
          No commands sent yet
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { 
  Gamepad2, Wifi, Clock, Play, Target, Send, RotateCcw,
  AlertOctagon, ArrowUp, ArrowDown, Trash2
} from 'lucide-vue-next'
import { WS_ENDPOINT } from '../../config'

// WebSocket connection
const ws = ref<WebSocket | null>(null)
const isConnected = ref(false)
const isConnecting = ref(false)
const wsUrl = WS_ENDPOINT
const connectionStartTime = ref<number | null>(null)
const connectionDuration = ref('00:00')

// Command state
const isSending = ref(false)
const commandLog = ref<Array<{
  time: string
  type: 'info' | 'success' | 'error' | 'warning'
  direction: 'sent' | 'received'
  label: string
  details: string
}>>([])

// Custom move builder
const customMove = ref({
  type: 'move',
  from: '',
  to: '',
  fromPiece: '',
  toPiece: ''
})

// Computed
const isCustomMoveValid = computed(() => {
  return customMove.value.from && 
         customMove.value.to && 
         customMove.value.fromPiece &&
         /^[a-h][1-8]$/.test(customMove.value.from) &&
         /^[a-h][1-8]$/.test(customMove.value.to)
})

// Connection duration timer
let durationInterval: number | null = null

const updateConnectionDuration = () => {
  if (connectionStartTime.value) {
    const elapsed = Math.floor((Date.now() - connectionStartTime.value) / 1000)
    const minutes = Math.floor(elapsed / 60)
    const seconds = elapsed % 60
    connectionDuration.value = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
  }
}

// WebSocket functions
const connect = () => {
  if (isConnecting.value || isConnected.value) return

  try {
    isConnecting.value = true
    addLog('info', 'sent', 'Connecting', `Attempting to connect to ${wsUrl}`)

    ws.value = new WebSocket(wsUrl)

    ws.value.onopen = () => {
      isConnected.value = true
      isConnecting.value = false
      connectionStartTime.value = Date.now()
      addLog('success', 'received', 'Connected', 'WebSocket connection established')
      
      // Start duration timer
      durationInterval = window.setInterval(updateConnectionDuration, 1000)
    }

    ws.value.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data)
        addLog('info', 'received', `Received: ${data.type || 'Message'}`, JSON.stringify(data, null, 2))
      } catch (e) {
        addLog('warning', 'received', 'Received', event.data)
      }
    }

    ws.value.onerror = (error) => {
      addLog('error', 'received', 'Connection Error', 'WebSocket error occurred')
      console.error('WebSocket error:', error)
    }

    ws.value.onclose = () => {
      isConnected.value = false
      isConnecting.value = false
      connectionStartTime.value = null
      addLog('warning', 'received', 'Disconnected', 'WebSocket connection closed')
      
      if (durationInterval) {
        clearInterval(durationInterval)
        durationInterval = null
      }
    }
  } catch (error) {
    isConnecting.value = false
    addLog('error', 'sent', 'Connection Failed', `Failed to connect: ${error}`)
  }
}

const disconnect = () => {
  if (ws.value) {
    ws.value.close()
    ws.value = null
  }
}

// Send command to robot via WebSocket
const sendRobotCommand = (move: any) => {
  if (!ws.value || !isConnected.value) {
    addLog('error', 'sent', 'Send Failed', 'Not connected to WebSocket server')
    return
  }

  try {
    isSending.value = true

    // Format: Robot nhận theo format từ code-robot.py handle_robot_command()
    // Nhưng qua WebSocket server sẽ forward, nên cần goal_id và move object
    const command = {
      goal_id: `manual_${Date.now()}`,
      header: {
        timestamp: new Date().toISOString(),
        source: 'admin_manual_control'
      },
      move: move
    }

    ws.value.send(JSON.stringify(command))
    
    addLog('success', 'sent', `Command Sent: ${move.type}`, 
      `${move.from_piece} ${move.from} → ${move.to}${move.to_piece ? ` (captures ${move.to_piece})` : ''}`
    )
  } catch (error) {
    addLog('error', 'sent', 'Send Failed', `Error: ${error}`)
  } finally {
    isSending.value = false
  }
}

// Quick command functions
const sendTestMove = () => {
  sendRobotCommand({
    type: 'move',
    from: 'e2',
    to: 'e4',
    from_piece: 'white_pawn',
    to_piece: null,
    notation: 'e4'
  })
}

const sendTestAttack = () => {
  sendRobotCommand({
    type: 'attack',
    from: 'e4',
    to: 'd5',
    from_piece: 'white_pawn',
    to_piece: 'black_pawn',
    notation: 'exd5'
  })
}

const handleEmergencyStop = () => {
  if (!ws.value || !isConnected.value) return

  const command = {
    CommandType: 'emergency_stop',
    Payload: {
      timestamp: new Date().toISOString()
    }
  }

  ws.value.send(JSON.stringify(command))
  addLog('error', 'sent', 'EMERGENCY STOP', 'Emergency stop command sent')
}

// Custom move functions
const sendCustomMove = () => {
  if (!isCustomMoveValid.value) return

  sendRobotCommand({
    type: customMove.value.type,
    from: customMove.value.from.toLowerCase(),
    to: customMove.value.to.toLowerCase(),
    from_piece: customMove.value.fromPiece,
    to_piece: customMove.value.type === 'attack' ? customMove.value.toPiece : null,
    notation: `${customMove.value.from}-${customMove.value.to}`
  })
}

const resetCustomMove = () => {
  customMove.value = {
    type: 'move',
    from: '',
    to: '',
    fromPiece: '',
    toPiece: ''
  }
}

const validateSquare = (field: 'from' | 'to') => {
  const value = customMove.value[field].toLowerCase()
  if (value && !/^[a-h]?[1-8]?$/.test(value)) {
    customMove.value[field] = value.slice(0, -1)
  }
}

// Logging
const addLog = (
  type: 'info' | 'success' | 'error' | 'warning',
  direction: 'sent' | 'received',
  label: string,
  details: string
) => {
  const now = new Date()
  commandLog.value.unshift({
    time: now.toLocaleTimeString(),
    type,
    direction,
    label,
    details
  })

  // Keep only last 50 logs
  if (commandLog.value.length > 50) {
    commandLog.value = commandLog.value.slice(0, 50)
  }
}

// Lifecycle
onMounted(() => {
  // Auto-connect on mount
  connect()
})

onUnmounted(() => {
  disconnect()
  if (durationInterval) {
    clearInterval(durationInterval)
  }
})
</script>

<style scoped>
.manual-control {
  max-width: 1400px;
  margin: 0 auto;
}

/* Connection Card */
.connection-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  margin-bottom: 24px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  border: 2px solid #e5e7eb;
  transition: all 0.3s ease;
}

.connection-card.status-connected {
  border-color: #10b981;
  background: linear-gradient(135deg, #ecfdf5 0%, #ffffff 100%);
}

.connection-card.status-disconnected {
  border-color: #ef4444;
  background: linear-gradient(135deg, #fef2f2 0%, #ffffff 100%);
}

.connection-status {
  display: flex;
  align-items: center;
  gap: 16px;
}

.status-indicator {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  position: relative;
}

.indicator-online {
  background: #10b981;
  animation: pulse 2s infinite;
}

.indicator-offline {
  background: #ef4444;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.status-text {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.status-label {
  font-size: 12px;
  color: #6b7280;
  font-weight: 500;
}

.status-value {
  font-size: 18px;
  font-weight: 700;
  color: #111827;
}

.connection-info {
  display: flex;
  gap: 16px;
  flex: 1;
}

.info-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #6b7280;
}

/* Command Grid */
.command-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-top: 16px;
}

.command-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 24px;
  border-radius: 12px;
  border: 2px solid transparent;
  background: white;
  cursor: pointer;
  transition: all 0.2s ease;
  font-weight: 600;
}

.command-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
}

.command-btn.btn-primary {
  border-color: #3b82f6;
  color: #3b82f6;
}

.command-btn.btn-warning {
  border-color: #f59e0b;
  color: #f59e0b;
}

.command-btn.btn-info {
  border-color: #06b6d4;
  color: #06b6d4;
}

.command-btn.btn-success {
  border-color: #10b981;
  color: #10b981;
}

.command-btn small {
  font-size: 11px;
  font-weight: 400;
  color: #6b7280;
}

/* Move Builder */
.move-builder-card {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-top: 16px;
  border: 1px solid #e5e7eb;
}

.builder-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 16px;
}

.builder-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.builder-field label {
  font-size: 13px;
  font-weight: 600;
  color: #374151;
}

.builder-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
  padding-top: 16px;
  border-top: 1px solid #e5e7eb;
}

/* Command Log */
.command-log {
  background: #f9fafb;
  border-radius: 12px;
  padding: 16px;
  margin-top: 16px;
  max-height: 400px;
  overflow-y: auto;
}

.log-entry {
  display: grid;
  grid-template-columns: 80px 24px 1fr;
  gap: 12px;
  padding: 12px;
  background: white;
  border-radius: 8px;
  margin-bottom: 8px;
  border-left: 4px solid;
}

.log-entry.log-info {
  border-left-color: #3b82f6;
}

.log-entry.log-success {
  border-left-color: #10b981;
}

.log-entry.log-error {
  border-left-color: #ef4444;
}

.log-entry.log-warning {
  border-left-color: #f59e0b;
}

.log-time {
  font-size: 11px;
  color: #6b7280;
  font-family: 'Courier New', monospace;
}

.log-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  color: #9ca3af;
}

.log-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.log-label {
  font-size: 13px;
  font-weight: 600;
  color: #111827;
}

.log-details {
  font-size: 12px;
  color: #6b7280;
  font-family: 'Courier New', monospace;
  white-space: pre-wrap;
}

.empty-log {
  text-align: center;
  padding: 32px;
  color: #9ca3af;
  font-size: 14px;
}

.section-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
</style>
