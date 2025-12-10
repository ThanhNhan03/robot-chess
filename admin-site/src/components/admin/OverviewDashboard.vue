<template>
  <div class="overview-dashboard">
    <!-- Welcome Section -->
    <div class="welcome-banner">
      <div class="welcome-content">
        <h2 class="welcome-title">👋 Welcome Back, Admin!</h2>
        <p class="welcome-subtitle">Here's what's happening with your Chess Robot System</p>
      </div>
      <div class="welcome-time">
        <div class="current-time">{{ currentTime }}</div>
        <div class="current-date">{{ currentDate }}</div>
      </div>
    </div>

    <!-- Connection Status Cards -->
    <div class="connection-status-section">
      <h3 class="section-title">🔌 System Connection Status</h3>
      <div class="connection-grid">
        <!-- WebSocket Server -->
        <div class="connection-card" :class="{ 'connected': websocketStatus.connected }">
          <div class="connection-header">
            <div class="connection-icon">🌐</div>
            <div class="connection-status-indicator" :class="websocketStatus.connected ? 'status-online' : 'status-offline'">
              <span class="status-dot"></span>
              <span class="status-text">{{ websocketStatus.connected ? 'CONNECTED' : 'DISCONNECTED' }}</span>
            </div>
          </div>
          <div class="connection-body">
            <h4 class="connection-name">WebSocket Server</h4>
            <div class="connection-details">
              <div class="detail-row">
                <span class="detail-label">Endpoint:</span>
                <span class="detail-value">{{ websocketStatus.endpoint }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Active Connections:</span>
                <span class="detail-value">{{ websocketStatus.activeConnections }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Uptime:</span>
                <span class="detail-value">{{ websocketStatus.uptime }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Last Ping:</span>
                <span class="detail-value">{{ websocketStatus.lastPing }}</span>
              </div>
            </div>
          </div>
          <div class="connection-footer">
            <button class="btn-flat btn-sm" :class="websocketStatus.connected ? 'btn-danger' : 'btn-success'" @click="toggleWebSocket">
              {{ websocketStatus.connected ? '🔌 Disconnect' : '🔗 Connect' }}
            </button>
            <button class="btn-flat btn-sm btn-secondary" @click="refreshWebSocket">🔄 Refresh</button>
          </div>
        </div>

        <!-- Database Server -->
        <div class="connection-card" :class="{ 'connected': databaseStatus.connected }">
          <div class="connection-header">
            <div class="connection-icon">💾</div>
            <div class="connection-status-indicator" :class="databaseStatus.connected ? 'status-online' : 'status-offline'">
              <span class="status-dot"></span>
              <span class="status-text">{{ databaseStatus.connected ? 'CONNECTED' : 'DISCONNECTED' }}</span>
            </div>
          </div>
          <div class="connection-body">
            <h4 class="connection-name">Database Server</h4>
            <div class="connection-details">
              <div class="detail-row">
                <span class="detail-label">Host:</span>
                <span class="detail-value">{{ databaseStatus.host }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Database:</span>
                <span class="detail-value">{{ databaseStatus.database }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Connections:</span>
                <span class="detail-value">{{ databaseStatus.connections }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Response Time:</span>
                <span class="detail-value">{{ databaseStatus.responseTime }}</span>
              </div>
            </div>
          </div>
          <div class="connection-footer">
            <button class="btn-flat btn-sm btn-secondary" @click="testDatabase">🧪 Test Connection</button>
            <button class="btn-flat btn-sm btn-secondary" @click="refreshDatabase">🔄 Refresh</button>
          </div>
        </div>

        <!-- Robot Hardware -->
        <div class="connection-card" :class="{ 'connected': robotStatus.connected }">
          <div class="connection-header">
            <div class="connection-icon">🤖</div>
            <div class="connection-status-indicator" :class="robotStatus.connected ? 'status-online' : 'status-offline'">
              <span class="status-dot"></span>
              <span class="status-text">{{ robotStatus.connected ? 'ONLINE' : 'OFFLINE' }}</span>
            </div>
          </div>
          <div class="connection-body">
            <h4 class="connection-name">Robot Hardware</h4>
            <div class="connection-details">
              <div class="detail-row">
                <span class="detail-label">Robot ID:</span>
                <span class="detail-value">{{ robotStatus.robotId }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">IP Address:</span>
                <span class="detail-value">{{ robotStatus.ipAddress }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Status:</span>
                <span class="detail-value">{{ robotStatus.status }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Battery:</span>
                <span class="detail-value">{{ robotStatus.battery }}</span>
              </div>
            </div>
          </div>
          <div class="connection-footer">
            <button class="btn-flat btn-sm btn-primary" @click="controlRobot">🎮 Control</button>
            <button class="btn-flat btn-sm btn-secondary" @click="refreshRobot">🔄 Refresh</button>
          </div>
        </div>

        <!-- AI Engine -->
        <div class="connection-card" :class="{ 'connected': aiStatus.connected }">
          <div class="connection-header">
            <div class="connection-icon">🧠</div>
            <div class="connection-status-indicator" :class="aiStatus.connected ? 'status-online' : 'status-offline'">
              <span class="status-dot"></span>
              <span class="status-text">{{ aiStatus.connected ? 'READY' : 'UNAVAILABLE' }}</span>
            </div>
          </div>
          <div class="connection-body">
            <h4 class="connection-name">AI Engine</h4>
            <div class="connection-details">
              <div class="detail-row">
                <span class="detail-label">Engine:</span>
                <span class="detail-value">{{ aiStatus.engine }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Skill Level:</span>
                <span class="detail-value">{{ aiStatus.skillLevel }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">CPU Usage:</span>
                <span class="detail-value">{{ aiStatus.cpuUsage }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Games Played:</span>
                <span class="detail-value">{{ aiStatus.gamesPlayed }}</span>
              </div>
            </div>
          </div>
          <div class="connection-footer">
            <button class="btn-flat btn-sm btn-warning" @click="configureAI">⚙️ Configure</button>
            <button class="btn-flat btn-sm btn-secondary" @click="refreshAI">🔄 Refresh</button>
          </div>
        </div>
      </div>
    </div>

    <!-- System Statistics -->
    <div class="stats-section">
      <h3 class="section-title">📊 System Statistics</h3>
      <div class="stats-grid">
        <div class="stat-card stat-primary">
          <div class="stat-icon">👥</div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.totalUsers }}</div>
            <div class="stat-label">Total Users</div>
            <div class="stat-trend stat-up">↗️ +12 this week</div>
          </div>
        </div>
        
        <div class="stat-card stat-success">
          <div class="stat-icon">♟️</div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.gamesPlayed }}</div>
            <div class="stat-label">Games Played</div>
            <div class="stat-trend stat-up">↗️ +45 today</div>
          </div>
        </div>
        
        <div class="stat-card stat-info">
          <div class="stat-icon">🤖</div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.activeRobots }}</div>
            <div class="stat-label">Active Robots</div>
            <div class="stat-trend">{{ stats.totalRobots }} total</div>
          </div>
        </div>
        
        <div class="stat-card stat-warning">
          <div class="stat-icon">⚡</div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.systemLoad }}%</div>
            <div class="stat-label">System Load</div>
            <div class="stat-trend stat-down">↘️ -5% from avg</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="quick-actions-section">
      <h3 class="section-title">⚡ Quick Actions</h3>
      <div class="quick-actions-grid">
        <button class="quick-action-card" @click="goToUserManagement">
          <div class="qa-icon">👥</div>
          <div class="qa-label">Manage Users</div>
        </button>
        <button class="quick-action-card" @click="goToRobotManagement">
          <div class="qa-icon">🤖</div>
          <div class="qa-label">Manage Robots</div>
        </button>
        <button class="quick-action-card" @click="goToAIManagement">
          <div class="qa-icon">🧠</div>
          <div class="qa-label">Manage AI</div>
        </button>
        <button class="quick-action-card" @click="viewLogs">
          <div class="qa-icon">📋</div>
          <div class="qa-label">View Logs</div>
        </button>
        <button class="quick-action-card" @click="systemSettings">
          <div class="qa-icon">⚙️</div>
          <div class="qa-label">Settings</div>
        </button>
        <button class="quick-action-card" @click="backupSystem">
          <div class="qa-icon">💾</div>
          <div class="qa-label">Backup</div>
        </button>
      </div>
    </div>

    <!-- Recent Activity -->
    <div class="activity-section">
      <h3 class="section-title">📝 Recent System Activity</h3>
      <div class="activity-list">
        <div v-for="activity in recentActivities" :key="activity.id" class="activity-item">
          <div class="activity-icon" :class="`activity-${activity.type}`">{{ activity.icon }}</div>
          <div class="activity-content">
            <div class="activity-title">{{ activity.title }}</div>
            <div class="activity-description">{{ activity.description }}</div>
          </div>
          <div class="activity-time">{{ activity.time }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { WS_ENDPOINT } from '../../config'

// Connection Status
const websocketStatus = ref({
  connected: true,
  endpoint: WS_ENDPOINT,
  activeConnections: 5,
  uptime: '2h 34m',
  lastPing: '< 1s ago'
})

const databaseStatus = ref({
  connected: true,
  host: 'localhost:5432',
  database: 'chess_robot_db',
  connections: 3,
  responseTime: '12ms'
})

const robotStatus = ref({
  connected: true,
  robotId: 'ROBOT-001',
  ipAddress: '192.168.1.100',
  status: 'Idle',
  battery: '95%'
})

const aiStatus = ref({
  connected: true,
  engine: 'Stockfish 16',
  skillLevel: 'Level 5',
  cpuUsage: '23%',
  gamesPlayed: '1,234'
})

// System Stats
const stats = ref({
  totalUsers: 156,
  gamesPlayed: 1247,
  activeRobots: 3,
  totalRobots: 5,
  systemLoad: 45
})

// Current Time and Date
const currentTime = ref('')
const currentDate = ref('')

const updateDateTime = () => {
  const now = new Date()
  currentTime.value = now.toLocaleTimeString('en-US', { 
    hour: '2-digit', 
    minute: '2-digit',
    second: '2-digit'
  })
  currentDate.value = now.toLocaleDateString('en-US', { 
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

// Recent Activities
const recentActivities = ref([
  {
    id: 1,
    icon: '👤',
    type: 'user',
    title: 'New User Registered',
    description: 'User "chess_master" joined the system',
    time: '2 min ago'
  },
  {
    id: 2,
    icon: '♟️',
    type: 'game',
    title: 'Game Completed',
    description: 'Player "john_doe" won against Robot #2',
    time: '5 min ago'
  },
  {
    id: 3,
    icon: '🤖',
    type: 'robot',
    title: 'Robot Status Changed',
    description: 'Robot #1 is now idle',
    time: '10 min ago'
  },
  {
    id: 4,
    icon: '🔧',
    type: 'system',
    title: 'System Update',
    description: 'Database backup completed successfully',
    time: '15 min ago'
  },
  {
    id: 5,
    icon: '⚠️',
    type: 'warning',
    title: 'Warning',
    description: 'Robot #3 battery low (15%)',
    time: '20 min ago'
  }
])

// Methods
const toggleWebSocket = () => {
  websocketStatus.value.connected = !websocketStatus.value.connected
}

const refreshWebSocket = () => {
  console.log('Refreshing WebSocket connection...')
  websocketStatus.value.lastPing = '< 1s ago'
}

const testDatabase = () => {
  console.log('Testing database connection...')
}

const refreshDatabase = () => {
  console.log('Refreshing database status...')
}

const controlRobot = () => {
  console.log('Opening robot control panel...')
}

const refreshRobot = () => {
  console.log('Refreshing robot status...')
}

const configureAI = () => {
  console.log('Opening AI configuration...')
}

const refreshAI = () => {
  console.log('Refreshing AI status...')
}

const goToUserManagement = () => {
  console.log('Navigate to User Management')
}

const goToRobotManagement = () => {
  console.log('Navigate to Robot Management')
}

const goToAIManagement = () => {
  console.log('Navigate to AI Management')
}

const viewLogs = () => {
  console.log('Navigate to System Logs')
}

const systemSettings = () => {
  console.log('Navigate to Settings')
}

const backupSystem = () => {
  console.log('Start system backup')
}

// Lifecycle
let timeInterval: ReturnType<typeof setInterval> | null = null

onMounted(() => {
  updateDateTime()
  timeInterval = setInterval(updateDateTime, 1000)
})

onUnmounted(() => {
  if (timeInterval) {
    clearInterval(timeInterval)
  }
})
</script>

<style scoped>
@import '../../assets/styles/OverviewDashboard.css';
</style>
