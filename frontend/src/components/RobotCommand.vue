<template>
  <div class="robot-command">
    <div class="command-header">
      <h3>Robot AI Command Display</h3>
      <div class="connection-status" :class="{ connected: isConnected }">
        {{ isConnected ? 'Connected' : 'Disconnected' }}
      </div>
    </div>
    
    <!-- AI Move Display -->
    <div class="ai-move-section" v-if="lastAIMove">
      <h4>Latest AI Move:</h4>
      <div class="move-info">
        <div class="move-details">
          <span class="move-type" :class="lastAIMove.move.type">{{ lastAIMove.move.type.toUpperCase() }}</span>
          <span class="move-notation">{{ lastAIMove.move.notation }}</span>
          <span class="move-description">
            {{ lastAIMove.move.from_piece }} from {{ lastAIMove.move.from }} to {{ lastAIMove.move.to }}
            {{ lastAIMove.move.to_piece ? ` (captures ${lastAIMove.move.to_piece})` : '' }}
          </span>
        </div>
        <div class="move-status">
          <span v-if="lastAIMove.move.results_in_check" class="check-indicator">CHECK!</span>
          <span class="timestamp">{{ formatTime(lastAIMove.timestamp) }}</span>
        </div>
      </div>
    </div>

    <!-- Robot Command JSON Display -->
    <div class="command-display" v-if="lastRobotCommand">
      <h4>Robot Command ({{ lastRobotCommand.goal_id.startsWith('ai_cmd') ? 'Server Auto-Sent' : 'Manual Test' }}):</h4>
      <pre class="json-display">{{ formatJSON(lastRobotCommand) }}</pre>
      <div class="command-status">
        <span class="goal-id">Goal ID: {{ lastRobotCommand.goal_id }}</span>
        <span class="command-time">{{ formatTime(lastRobotCommand.header.timestamp) }}</span>
      </div>
    </div>

    <!-- FEN Display -->
    <div class="fen-display" v-if="currentFEN">
      <h4>Current Board Position:</h4>
      <div class="fen-info">
        <code class="fen-string">{{ currentFEN.fen_str }}</code>
        <span class="fen-source">Source: {{ currentFEN.source }}</span>
      </div>
    </div>

    <!-- Robot Response Display -->
    <div class="robot-response" v-if="lastRobotResponse">
      <h4>Robot Response:</h4>
      <div class="response-info" :class="{ success: lastRobotResponse.success, error: !lastRobotResponse.success }">
        <div class="response-status">
          <span class="status-icon">{{ lastRobotResponse.success ? '✅' : '❌' }}</span>
          <span class="status-text">{{ lastRobotResponse.success ? 'SUCCESS' : 'FAILED' }}</span>
        </div>
        <div class="response-details">
          <span class="goal-id">Goal: {{ lastRobotResponse.goal_id }}</span>
          <span class="response-time">{{ formatTime(lastRobotResponse.timestamp) }}</span>
        </div>
        <div class="response-message" v-if="lastRobotResponse.response?.message">
          {{ lastRobotResponse.response.message }}
        </div>
      </div>
    </div>

    <!-- Status Messages -->
    <div class="status-messages" v-if="statusMessage">
      <div :class="['status-message', statusMessage.type]">
        {{ statusMessage.text }}
      </div>
    </div>

    <!-- Manual Send Button (for testing) -->
    <div class="manual-controls" v-if="showManualControls">
      <button 
        @click="toggleManualControls" 
        class="toggle-button"
      >
        Hide Manual Controls
      </button>
      <button 
        @click="testCommand" 
        :disabled="!isConnected" 
        class="test-button"
      >
        Send Test Command
      </button>
    </div>
    <div v-else class="manual-controls">
      <button 
        @click="toggleManualControls" 
        class="toggle-button"
      >
        Show Manual Controls
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import webSocketService from '../services/webSocketService'

// Types
interface AIMove {
  fen_str: string
  move: {
    type: 'move' | 'attack'
    from: string
    to: string
    from_piece: string
    to_piece: string | null
    notation: string
    results_in_check: boolean
  }
  timestamp: string
}

interface RobotCommand {
  goal_id: string
  header: {
    timestamp: string
    source?: string
    ai_id?: string
  }
  move: {
    type: 'move' | 'attack'
    from: string
    to: string
    from_piece: string
    to_piece: string | null
    notation: string
    results_in_check: boolean
  }
}

interface RobotResponse {
  type: string
  success: boolean
  goal_id: string
  response: any
  timestamp: string
}

interface FENData {
  fen_str: string
  source: string
  timestamp: string
}

// Reactive data
const isConnected = ref(false)
const statusMessage = ref<{ text: string, type: 'success' | 'error' | 'info' } | null>(null)
const showManualControls = ref(false)

// AI and Robot data
const lastAIMove = ref<AIMove | null>(null)
const lastRobotCommand = ref<RobotCommand | null>(null)
const lastRobotResponse = ref<RobotResponse | null>(null)
const currentFEN = ref<FENData | null>(null)

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

const showStatus = (text: string, type: 'success' | 'error' | 'info') => {
  statusMessage.value = { text, type }
  setTimeout(() => {
    statusMessage.value = null
  }, 5000)
}

const formatTime = (timestamp: string): string => {
  return new Date(timestamp).toLocaleTimeString()
}

const formatJSON = (obj: any): string => {
  return JSON.stringify(obj, null, 2)
}

const toggleManualControls = () => {
  showManualControls.value = !showManualControls.value
}

const testCommand = () => {
  if (!isConnected.value) return
  
  const testMove = webSocketService.createRobotCommand(
    'move',
    'white_pawn',
    'e2',
    'e4',
    undefined,
    false
  )
  
  // Store test command for display
  lastRobotCommand.value = testMove
  
  webSocketService.sendRobotCommand(testMove)
  showStatus('Manual test command sent', 'info')
}

// Display AI move when received (NO AUTO-SEND - server already sent to robot)
const handleAIMove = (aiMoveData: AIMove) => {
  console.log('🤖 Displaying AI move (server already sent to robot):', aiMoveData)
  
  // Store the AI move for display
  lastAIMove.value = aiMoveData
  
  // Extract the command info that server already sent (for display only)
  // Server created and sent robot command with goal_id like: ai_cmd_123456
  // We just show what was sent
  showStatus(`AI move received: ${aiMoveData.move.notation} (server sent to robot)`, 'info')
}

// Lifecycle
onMounted(() => {
  // Set up event listeners for WebSocket service
  webSocketService.subscribe('connection', (data: any) => {
    if (data.connected !== undefined) {
      isConnected.value = data.connected
    }
  })

  // Listen for AI moves (from server when AI sends fen_str + move)
  webSocketService.subscribe('ai_move_executed', (data: any) => {
    console.log('🤖 AI move executed by server:', data)
    
    // Store the robot command that server already sent
    lastRobotCommand.value = {
      goal_id: data.goal_id,
      header: {
        timestamp: data.timestamp
      },
      move: data.move
    }
    
    // Display AI move info
    const aiMoveData: AIMove = {
      fen_str: '', // Will be updated from FEN message
      move: data.move,
      timestamp: data.timestamp
    }
    handleAIMove(aiMoveData)
  })

  // Listen for FEN updates (board position)
  webSocketService.subscribe('fen', (data: any) => {
    console.log('📋 FEN update:', data)
    currentFEN.value = {
      fen_str: data.fen_str,
      source: data.source || 'unknown',
      timestamp: data.timestamp
    }
    
    // If we have a recent AI move, update its FEN
    if (lastAIMove.value && data.source === 'ai') {
      lastAIMove.value.fen_str = data.fen_str
    }
  })

  // Listen for robot responses
  webSocketService.subscribe('robot_response', (response: any) => {
    console.log('🤖 Robot response:', response)
    lastRobotResponse.value = response
    
    if (response.success) {
      showStatus(`Robot executed command successfully: ${response.goal_id}`, 'success')
    } else {
      showStatus(`Robot failed to execute command: ${response.goal_id}`, 'error')
    }
  })

  webSocketService.subscribe('command_sent', (data: any) => {
    console.log('📤 Command sent acknowledgment:', data)
    // Only update status, don't change lastRobotCommand if it's from AI
    if (!lastRobotCommand.value || lastRobotCommand.value.goal_id !== data.goal_id) {
      showStatus(`Command ${data.goal_id} sent to robot`, 'info')
    }
  })

  webSocketService.subscribe('error', (data: { error: string }) => {
    showStatus(data.error, 'error')
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
  max-width: 600px;
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

/* AI Move Section */
.ai-move-section {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 20px;
  border-radius: 12px;
  margin-bottom: 20px;
}

.ai-move-section h4 {
  margin: 0 0 16px 0;
  font-size: 1.2rem;
  font-weight: 600;
}

.move-info {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.move-details {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  align-items: center;
}

.move-type {
  padding: 4px 12px;
  border-radius: 16px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.move-type.move {
  background: rgba(46, 204, 113, 0.2);
  border: 1px solid rgba(46, 204, 113, 0.4);
}

.move-type.attack {
  background: rgba(231, 76, 60, 0.2);
  border: 1px solid rgba(231, 76, 60, 0.4);
}

.move-notation {
  font-family: 'Monaco', 'Menlo', 'Consolas', monospace;
  font-size: 1.1rem;
  font-weight: bold;
  background: rgba(255, 255, 255, 0.2);
  padding: 4px 8px;
  border-radius: 6px;
}

.move-description {
  font-size: 0.95rem;
  opacity: 0.9;
}

.move-status {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 12px;
}

.check-indicator {
  background: #f39c12;
  color: white;
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: bold;
  animation: pulse 1s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}

.timestamp {
  font-size: 0.875rem;
  opacity: 0.8;
}

/* Command Display */
.command-display {
  background: #f8f9fa;
  border: 2px solid #e9ecef;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 20px;
}

.command-display h4 {
  margin: 0 0 12px 0;
  color: #2c3e50;
  font-size: 1.1rem;
}

.json-display {
  background: #2c3e50;
  color: #ecf0f1;
  padding: 16px;
  border-radius: 8px;
  font-family: 'Monaco', 'Menlo', 'Consolas', monospace;
  font-size: 0.875rem;
  line-height: 1.4;
  white-space: pre-wrap;
  word-break: break-word;
  margin: 0 0 12px 0;
  overflow-x: auto;
}

.command-status {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
  color: #6c757d;
}

.goal-id {
  font-family: 'Monaco', 'Menlo', 'Consolas', monospace;
  background: #e9ecef;
  padding: 2px 6px;
  border-radius: 4px;
}

/* FEN Display */
.fen-display {
  background: #fff3cd;
  border: 2px solid #ffeaa7;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 20px;
}

.fen-display h4 {
  margin: 0 0 12px 0;
  color: #856404;
  font-size: 1.1rem;
}

.fen-info {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.fen-string {
  background: #fff;
  border: 1px solid #ffeaa7;
  padding: 8px 12px;
  border-radius: 6px;
  font-family: 'Monaco', 'Menlo', 'Consolas', monospace;
  font-size: 0.875rem;
  word-break: break-all;
}

.fen-source {
  font-size: 0.875rem;
  color: #856404;
  font-style: italic;
}

/* Robot Response */
.robot-response {
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 20px;
}

.robot-response.success {
  background: #d4edda;
  border: 2px solid #c3e6cb;
}

.robot-response.error {
  background: #f8d7da;
  border: 2px solid #f5c6cb;
}

.robot-response h4 {
  margin: 0 0 16px 0;
  font-size: 1.1rem;
}

.response-info {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.response-status {
  display: flex;
  align-items: center;
  gap: 8px;
}

.status-icon {
  font-size: 1.2rem;
}

.status-text {
  font-weight: bold;
  font-size: 1rem;
}

.response-details {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
  opacity: 0.8;
}

.response-message {
  background: rgba(0, 0, 0, 0.05);
  padding: 8px 12px;
  border-radius: 6px;
  font-style: italic;
}

/* Manual Controls */
.manual-controls {
  display: flex;
  gap: 12px;
  justify-content: center;
  margin-top: 16px;
}

.toggle-button,
.test-button {
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.toggle-button {
  background: #6c757d;
  color: white;
}

.toggle-button:hover {
  background: #545b62;
  transform: translateY(-1px);
}

.test-button {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.test-button:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
}

.test-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

/* Status Messages */
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
  
  .move-details {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .response-details {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .manual-controls {
    flex-direction: column;
  }
  
  .json-display {
    font-size: 0.75rem;
  }
}
</style>