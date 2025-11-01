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

