<template>
  <div class="ai-management">
    <div class="panel-header">
      <h2 class="panel-title">🧠 AI Management</h2>
      <div class="header-actions">
        <button class="btn-flat btn-secondary" @click="refreshConnection">
          🔄 Refresh Status
        </button>
        <button class="btn-flat btn-primary">
          📡 Connection Info
        </button>
      </div>
    </div>

    <!-- Connection Status Banner -->
    <div class="connection-banner" :class="{ 'connected': isConnected, 'disconnected': !isConnected }">
      <div class="banner-icon">{{ isConnected ? '✅' : '❌' }}</div>
      <div class="banner-content">
        <h3 class="banner-title">
          {{ isConnected ? 'Connected to Server' : 'Disconnected from Server' }}
        </h3>
        <p class="banner-text">
          {{ isConnected ? `WebSocket: ${wsEndpoint}` : 'Unable to connect to WebSocket server' }}
        </p>
      </div>
      <div class="banner-stats">
        <div class="banner-stat">
          <span class="stat-number">{{ connectedAIs }}</span>
          <span class="stat-text">AI Clients</span>
        </div>
      </div>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon">🟢</div>
        <div class="stat-content">
          <div class="stat-value">{{ connectedAIs }}</div>
          <div class="stat-label">Connected AIs</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon">📨</div>
        <div class="stat-content">
          <div class="stat-value">{{ totalRequests }}</div>
          <div class="stat-label">Total Requests</div>
        </div>
      </div>
      <div class="stat-card stat-primary">
        <div class="stat-icon">📤</div>
        <div class="stat-content">
          <div class="stat-value">{{ totalResponses }}</div>
          <div class="stat-label">Total Responses</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon">⏱️</div>
        <div class="stat-content">
          <div class="stat-value">{{ avgResponseTime }}ms</div>
          <div class="stat-label">Avg Response Time</div>
        </div>
      </div>
    </div>

    <!-- AI Clients Connected to Server -->
    <div class="panel-section">
      <h3 class="section-title">🔌 Connected AI Clients</h3>
      
      <!-- No AIs Connected -->
      <div v-if="aiClients.length === 0" class="empty-state">
        <div class="empty-icon">🤖</div>
        <div class="empty-title">No AI Clients Connected</div>
        <div class="empty-text">
          Waiting for AI clients to connect to the server...
        </div>
        <div class="empty-hint">
          <strong>Connection Info:</strong><br>
          Endpoint: <code>{{ tcpEndpoint }}</code><br>
          Send: <code>{"type": "ai_identify", "ai_id": "your_ai_id"}</code>
        </div>
      </div>

      <!-- AI Clients List -->
      <div v-else class="ai-list">
        <div v-for="ai in aiClients" :key="ai.id" class="ai-card">
          <div class="ai-header">
            <div class="ai-status status-active"></div>
            <div class="ai-info">
              <h4 class="ai-name">{{ ai.name }}</h4>
              <div class="ai-details">
                <span class="ai-id">ID: {{ ai.id }}</span>
                <span class="ai-type">Connected: {{ ai.connectedAt }}</span>
              </div>
            </div>
            <div class="ai-badge">
              <span class="badge-flat badge-success">CONNECTED</span>
            </div>
          </div>
          
          <div class="ai-body">
            <div class="ai-stats-row">
              <div class="stat-mini">
                <span class="stat-mini-label">Requests Sent</span>
                <span class="stat-mini-value">{{ ai.requestsSent || 0 }}</span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Responses Received</span>
                <span class="stat-mini-value">{{ ai.responsesReceived || 0 }}</span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Last Activity</span>
                <span class="stat-mini-value">{{ ai.lastActivity || 'N/A' }}</span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Connection Time</span>
                <span class="stat-mini-value">{{ ai.connectionDuration || '0m' }}</span>
              </div>
            </div>
            
            <!-- Last Response Preview -->
            <div v-if="ai.lastResponse" class="last-response">
              <div class="response-label">Last Response:</div>
              <div class="response-content">
                <div v-if="ai.lastResponse.fen_str" class="response-item">
                  <span class="response-key">FEN:</span>
                  <span class="response-value">{{ ai.lastResponse.fen_str }}</span>
                </div>
                <div v-if="ai.lastResponse.move" class="response-item">
                  <span class="response-key">Move:</span>
                  <span class="response-value">{{ formatMove(ai.lastResponse.move) }}</span>
                </div>
                <div v-if="ai.lastResponse.best_move" class="response-item">
                  <span class="response-key">Best Move:</span>
                  <span class="response-value">{{ ai.lastResponse.best_move }} (Eval: {{ ai.lastResponse.evaluation }})</span>
                </div>
              </div>
            </div>
          </div>
          
          <div class="ai-actions">
            <button class="btn-flat btn-primary btn-sm" @click="sendTestRequest(ai.id)">
              🧪 Test Request
            </button>
            <button class="btn-flat btn-secondary btn-sm" @click="viewAIDetails(ai.id)">
              📊 View Details
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- AI Activity Log -->
    <div class="panel-section">
      <h3 class="section-title">📝 AI Activity Log (Real-time)</h3>
      
      <div class="log-controls">
        <button class="btn-flat btn-sm btn-secondary" @click="clearLogs">
          🗑️ Clear Logs
        </button>
        <button class="btn-flat btn-sm btn-secondary" @click="pauseLogs">
          {{ logsPaused ? '▶️ Resume' : '⏸️ Pause' }}
        </button>
        <span class="log-count">{{ activityLogs.length }} entries</span>
      </div>

      <div v-if="activityLogs.length === 0" class="empty-state">
        <div class="empty-icon">📭</div>
        <div class="empty-title">No Activity Logs Yet</div>
        <div class="empty-text">
          AI activity will appear here when AI clients send responses
        </div>
      </div>

      <div v-else class="activity-log">
        <table class="table-flat">
          <thead>
            <tr>
              <th>Time</th>
              <th>AI ID</th>
              <th>Type</th>
              <th>Details</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(log, index) in displayedLogs" :key="index">
              <td>{{ log.time }}</td>
              <td><strong>{{ log.aiId }}</strong></td>
              <td>
                <span class="badge-flat" :class="getBadgeClass(log.type)">
                  {{ log.type }}
                </span>
              </td>
              <td>
                <div class="log-details">
                  <div v-if="log.fenStr" class="log-item">
                    <strong>FEN:</strong> {{ truncate(log.fenStr, 50) }}
                  </div>
                  <div v-if="log.move" class="log-item">
                    <strong>Move:</strong> {{ formatMove(log.move) }}
                  </div>
                  <div v-if="log.bestMove" class="log-item">
                    <strong>Best Move:</strong> {{ log.bestMove }} (Eval: {{ log.evaluation }})
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Connection Instructions -->
    <div class="panel-section">
      <h3 class="section-title">📡 AI Connection Instructions</h3>
      <div class="instruction-panel">
        <div class="instruction-step">
          <div class="step-number">1</div>
          <div class="step-content">
            <h4>Connect to TCP Server</h4>
            <div class="code-block">
              <code>{{ tcpEndpoint }}</code>
            </div>
          </div>
        </div>

        <div class="instruction-step">
          <div class="step-number">2</div>
          <div class="step-content">
            <h4>Identify as AI Client</h4>
            <div class="code-block">
              <code>{"type": "ai_identify", "ai_id": "your_ai_id"}</code>
            </div>
          </div>
        </div>

        <div class="instruction-step">
          <div class="step-number">3</div>
          <div class="step-content">
            <h4>Send AI Response (Format 1: With Robot Command)</h4>
            <div class="code-block">
              <pre>{
  "fen_str": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
  "move": {
    "type": "move",
    "from": "e2",
    "to": "e4",
    "notation": "e2e4"
  }
}</pre>
            </div>
          </div>
        </div>

        <div class="instruction-step">
          <div class="step-number">4</div>
          <div class="step-content">
            <h4>Send AI Response (Format 2: Legacy Format)</h4>
            <div class="code-block">
              <pre>{
  "best_move": "e2e4",
  "evaluation": 0.3
}</pre>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { WS_ENDPOINT, TCP_ENDPOINT } from '../../config'

// WebSocket connection
const ws = ref<WebSocket | null>(null)
const isConnected = ref(false)
const wsEndpoint = ref(WS_ENDPOINT)
const tcpEndpoint = ref(TCP_ENDPOINT)

// AI clients tracking
const aiClients = ref<any[]>([])
const connectedAIs = ref(0)

// Statistics
const totalRequests = ref(0)
const totalResponses = ref(0)
const avgResponseTime = ref(0)

// Activity logs
const activityLogs = ref<any[]>([])
const logsPaused = ref(false)
const maxLogs = 100

// Computed
const displayedLogs = computed(() => {
  return activityLogs.value.slice(0, 50) // Show latest 50 logs
})

// Connect to WebSocket
const connectWebSocket = () => {
  try {
    ws.value = new WebSocket(wsEndpoint.value)
    
    ws.value.onopen = () => {
      console.log('Connected to WebSocket server')
      isConnected.value = true
    }
    
    ws.value.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data)
        handleWebSocketMessage(data)
      } catch (error) {
        console.error('Error parsing WebSocket message:', error)
      }
    }
    
    ws.value.onerror = (error) => {
      console.error('WebSocket error:', error)
      isConnected.value = false
    }
    
    ws.value.onclose = () => {
      console.log('WebSocket connection closed')
      isConnected.value = false
      // Attempt to reconnect after 5 seconds
      setTimeout(() => {
        if (!isConnected.value) {
          connectWebSocket()
        }
      }, 5000)
    }
  } catch (error) {
    console.error('Failed to connect to WebSocket:', error)
    isConnected.value = false
  }
}

// Handle WebSocket messages
const handleWebSocketMessage = (data: any) => {
  // Handle connection info
  if (data.type === 'connection') {
    connectedAIs.value = data.connected_ais || 0
    return
  }

  // Handle AI responses
  if (data.type === 'ai_response' || data.type === 'ai_move_executed') {
    totalResponses.value++
    
    // Add to activity log
    if (!logsPaused.value) {
      addActivityLog({
        time: new Date().toLocaleTimeString(),
        aiId: data.ai_id || 'Unknown',
        type: data.type === 'ai_move_executed' ? 'MOVE_EXECUTED' : 'AI_RESPONSE',
        bestMove: data.best_move,
        evaluation: data.evaluation,
        move: data.move,
        response: data.response
      })
    }
    
    // Update AI client info
    updateAIClient(data.ai_id, {
      lastResponse: data.response || data,
      lastActivity: new Date().toLocaleTimeString(),
      responsesReceived: (getAIClient(data.ai_id)?.responsesReceived || 0) + 1
    })
  }

  // Handle FEN from AI
  if (data.fen_str && data.source === 'ai') {
    if (!logsPaused.value) {
      addActivityLog({
        time: new Date().toLocaleTimeString(),
        aiId: data.ai_id || 'Unknown',
        type: 'FEN_SENT',
        fenStr: data.fen_str
      })
    }
  }
}

// Add activity log
const addActivityLog = (log: any) => {
  activityLogs.value.unshift(log)
  // Keep only last maxLogs entries
  if (activityLogs.value.length > maxLogs) {
    activityLogs.value = activityLogs.value.slice(0, maxLogs)
  }
}

// Get AI client by ID
const getAIClient = (aiId: string) => {
  return aiClients.value.find(ai => ai.id === aiId)
}

// Update AI client info
const updateAIClient = (aiId: string, updates: any) => {
  const index = aiClients.value.findIndex(ai => ai.id === aiId)
  if (index !== -1) {
    aiClients.value[index] = {
      ...aiClients.value[index],
      ...updates
    }
  } else {
    // Add new AI client
    aiClients.value.push({
      id: aiId,
      name: `AI Client ${aiId}`,
      connectedAt: new Date().toLocaleTimeString(),
      requestsSent: 0,
      responsesReceived: 1,
      ...updates
    })
  }
}

// Methods
const refreshConnection = () => {
  if (ws.value) {
    ws.value.close()
  }
  connectWebSocket()
}

const clearLogs = () => {
  activityLogs.value = []
}

const pauseLogs = () => {
  logsPaused.value = !logsPaused.value
}

const sendTestRequest = (aiId: string) => {
  if (!isConnected.value || !ws.value) {
    alert('Not connected to server')
    return
  }

  const testRequest = {
    type: 'ai_request',
    request_id: `test_${Date.now()}`,
    fen_position: 'rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1',
    ai_id: aiId
  }

  ws.value.send(JSON.stringify(testRequest))
  totalRequests.value++
  
  addActivityLog({
    time: new Date().toLocaleTimeString(),
    aiId: aiId,
    type: 'TEST_REQUEST',
    fenStr: testRequest.fen_position
  })
}

const viewAIDetails = (aiId: string) => {
  console.log('View AI details:', aiId)
  alert(`Viewing details for AI: ${aiId}`)
}

const formatMove = (move: any) => {
  if (!move) return 'N/A'
  if (typeof move === 'string') return move
  return move.notation || `${move.from} → ${move.to}`
}

const truncate = (text: string, length: number) => {
  if (!text) return ''
  return text.length > length ? text.substring(0, length) + '...' : text
}

const getBadgeClass = (type: string) => {
  const classes: any = {
    'FEN_SENT': 'badge-info',
    'MOVE_EXECUTED': 'badge-success',
    'AI_RESPONSE': 'badge-primary',
    'TEST_REQUEST': 'badge-warning'
  }
  return classes[type] || 'badge-secondary'
}

// Lifecycle
onMounted(() => {
  connectWebSocket()
})

onUnmounted(() => {
  if (ws.value) {
    ws.value.close()
  }
})
</script>

<style scoped>
@import '../../assets/styles/AIManagement.css';
</style>
