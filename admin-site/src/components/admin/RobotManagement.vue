<template>
  <div class="robot-management">
    <div class="panel-header">
      <h2 class="panel-title">
        <Bot :size="24" />
        Robot Management
      </h2>
      <button class="btn-flat btn-primary">
        <Plus :size="18" /> Add New Robot
      </button>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon"><Wifi :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ robotStats.onlineRobots }}</div>
          <div class="stat-label">Connected Robots</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon"><WifiOff :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ robotStats.offlineRobots }}</div>
          <div class="stat-label">Offline Robots</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon"><BarChart3 :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ robotStats.totalMoves }}</div>
          <div class="stat-label">Total Moves</div>
        </div>
      </div>
      <div class="stat-card stat-primary">
        <div class="stat-icon"><TrendingUp :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ robotStats.avgSuccessRate.toFixed(1) }}%</div>
          <div class="stat-label">Success Rate</div>
        </div>
      </div>
    </div>

    <!-- Robot List -->
    <div class="panel-section">
      <h3 class="section-title">Robot List</h3>
      <div v-if="isLoading" class="loading-message">Loading robots...</div>
      <div v-else-if="error" class="error-message">{{ error }}</div>
      <div v-else-if="robots.length === 0" class="empty-message">No robots found</div>
      <div v-else class="robot-list">
        <div 
          v-for="robot in robots" 
          :key="robot.id" 
          class="robot-card"
          :class="getRobotCardClass(robot.isOnline)"
        >
          <div class="robot-status" :class="getOnlineStatusClass(robot.isOnline)"></div>
          <div class="robot-info">
            <h4 class="robot-name">{{ robot.name || robot.robotCode }}</h4>
            <div class="robot-details">
              <span class="robot-id">Code: {{ robot.robotCode }}</span>
              <span class="robot-ip">IP: {{ robot.ipAddress || 'N/A' }}</span>
              <span class="robot-location"><MapPin :size="14" /> {{ robot.location || 'Unknown' }}</span>
            </div>
            <div class="robot-meta">
              <span class="badge-flat" :class="getStatusBadgeClass(robot.status)">
                {{ robot.status?.toUpperCase() || 'UNKNOWN' }}
              </span>
              <span class="badge-flat" :class="robot.isOnline ? 'badge-info' : 'badge-danger'">
                {{ robot.isOnline ? 'ONLINE' : 'OFFLINE' }}
              </span>
              <span v-if="robot.latestMonitoring?.isMoving" class="badge-flat badge-primary">
                <Activity :size="12" /> MOVING
              </span>
              <span class="robot-uptime">Last seen: {{ formatDate(robot.lastOnlineAt) }}</span>
            </div>
            <div v-if="robot.currentGameId" class="robot-game">
              <Gamepad2 :size="16" /> Playing game: {{ robot.currentGameId }}
            </div>
            <div v-if="robot.latestMonitoring?.hasError" class="robot-error">
              <AlertTriangle :size="16" /> {{ robot.latestMonitoring.errorMessage || 'Unknown error' }}
            </div>
          </div>
          <div class="robot-actions">
            <button 
              class="btn-flat btn-primary btn-sm" 
              @click="handleControl(robot.id)"
              :disabled="!robot.isOnline"
            >
              <PlayCircle :size="14" /> Control
            </button>
            <button 
              class="btn-flat btn-warning btn-sm" 
              @click="handleCalibrate(robot.id)"
              :disabled="!robot.isOnline"
            >
              <Settings :size="14" /> Calibrate
            </button>
            <button 
              v-if="robot.isOnline"
              class="btn-flat btn-danger btn-sm" 
              @click="handleDisconnect(robot.id)"
            >
              <Power :size="14" /> Disconnect
            </button>
            <button 
              v-else
              class="btn-flat btn-success btn-sm" 
              @click="handleReconnect(robot.id)"
            >
              <RefreshCw :size="14" /> Reconnect
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Command Queue -->
    <div class="panel-section">
      <h3 class="section-title">Command Queue</h3>
      <div v-if="isLoadingCommands" class="loading-message">Loading commands...</div>
      <div v-else-if="commandHistory.length === 0" class="empty-message">No command history</div>
      <div v-else class="command-queue">
        <table class="table-flat">
          <thead>
            <tr>
              <th>Sent At</th>
              <th>Robot</th>
              <th>Command Type</th>
              <th>Status</th>
              <th>Execution Time</th>
              <th>Executed By</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="command in commandHistory" :key="command.id">
              <td>{{ formatDate(command.sentAt) }}</td>
              <td>{{ getRobotName(command.robotId) }}</td>
              <td>{{ command.commandType }}</td>
              <td>
                <span class="badge-flat" :class="getStatusBadgeClass(command.status)">
                  {{ command.status?.toUpperCase() || 'UNKNOWN' }}
                </span>
              </td>
              <td>{{ command.executionTimeMs ? `${command.executionTimeMs}ms` : '-' }}</td>
              <td>{{ command.executedByUsername || 'System' }}</td>
              <td>
                <button class="btn-flat btn-sm"><Info :size="14" /> Details</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Robot Configuration -->
    <div class="panel-section">
      <h3 class="section-title">Robot Configuration</h3>
      <div v-if="isLoading" class="loading-message">Loading configurations...</div>
      <div v-else-if="robots.length === 0" class="empty-message">No robot configurations found</div>
      <div v-else class="config-grid">
        <div 
          v-for="robot in robots" 
          :key="robot.id" 
          class="config-card"
        >
          <div class="config-header">
            <h4 class="config-robot-name">
              <Bot :size="20" /> {{ robot.name || robot.robotCode }}
            </h4>
            <span class="badge-flat" :class="robot.isOnline ? 'badge-success' : 'badge-danger'">
              {{ robot.isOnline ? 'ACTIVE' : 'INACTIVE' }}
            </span>
          </div>
          <div class="config-body">
            <div class="config-row">
              <span class="config-label">Speed</span>
              <div class="config-value-group">
                <span class="config-value">{{ robot.config?.speed || 50 }}</span>
                <span class="config-unit">mm/s</span>
                <button 
                  class="btn-flat btn-sm btn-primary" 
                  @click="handleAdjustSpeed(robot.id, robot.config?.speed || 50)"
                  :disabled="!robot.isOnline"
                >
                  <Zap :size="12" /> Adjust
                </button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Max Speed</span>
              <div class="config-value-group">
                <span class="config-value">{{ robot.config?.maxSpeed || 100 }}</span>
                <span class="config-unit">mm/s</span>
                <button 
                  class="btn-flat btn-sm btn-primary" 
                  @click="handleAdjustMaxSpeed(robot.id, robot.config?.maxSpeed || 100)"
                  :disabled="!robot.isOnline"
                >
                  Adjust
                </button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Gripper Force</span>
              <div class="config-value-group">
                <span class="config-value">{{ robot.config?.gripperForce || 50 }}</span>
                <span class="config-unit">%</span>
                <button 
                  class="btn-flat btn-sm btn-primary" 
                  @click="handleAdjustGripperForce(robot.id, robot.config?.gripperForce || 50)"
                  :disabled="!robot.isOnline"
                >
                  Adjust
                </button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Gripper Speed</span>
              <div class="config-value-group">
                <span class="config-value">{{ robot.config?.gripperSpeed || 50 }}</span>
                <span class="config-unit">mm/s</span>
                <button 
                  class="btn-flat btn-sm btn-primary" 
                  @click="handleAdjustGripperSpeed(robot.id, robot.config?.gripperSpeed || 50)"
                  :disabled="!robot.isOnline"
                >
                  Adjust
                </button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Emergency Stop</span>
              <div class="config-value-group">
                <span class="badge-flat" :class="robot.config?.emergencyStop ? 'badge-danger' : 'badge-success'">
                  {{ robot.config?.emergencyStop ? 'ENABLED' : 'DISABLED' }}
                </span>
                <button 
                  class="btn-flat btn-sm" 
                  :class="robot.config?.emergencyStop ? 'btn-success' : 'btn-danger'"
                  @click="handleToggleEmergencyStop(robot.id)"
                  :disabled="!robot.isOnline"
                >
                  {{ robot.config?.emergencyStop ? 'Disable' : 'Enable' }}
                </button>
              </div>
            </div>
          </div>
          <div class="config-footer">
            <span class="config-updated">Last updated: {{ formatDate(robot.config?.updatedAt) }}</span>
            <button 
              class="btn-flat btn-sm btn-warning" 
              @click="handleResetToDefault(robot.id)"
              :disabled="!robot.isOnline"
            >
              Reset to Default
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { 
  Bot, Plus, Wifi, WifiOff, BarChart3, TrendingUp,
  PlayCircle, AlertTriangle, MapPin, Gamepad2, Power,
  Settings, RefreshCw, Zap, Activity, Info
} from 'lucide-vue-next'
import { robotService, type Robot, type RobotStats, type UpdateRobotConfigRequest, type RobotCommandHistory } from '../../services/robotService'

// State
const robots = ref<Robot[]>([])
const robotStats = ref<RobotStats>({
  totalRobots: 0,
  onlineRobots: 0,
  offlineRobots: 0,
  busyRobots: 0,
  idleRobots: 0,
  totalMoves: 0,
  avgSuccessRate: 0,
})
const commandHistory = ref<RobotCommandHistory[]>([])
const isLoading = ref(false)
const isLoadingCommands = ref(false)
const error = ref<string | null>(null)

// Load data
const loadRobots = async () => {
  try {
    isLoading.value = true
    error.value = null
    robots.value = await robotService.getAllRobots()
    robotStats.value = await robotService.getRobotStats()
  } catch (err) {
    error.value = 'Failed to load robots'
    console.error('Error loading robots:', err)
  } finally {
    isLoading.value = false
  }
}

const loadCommandHistory = async () => {
  try {
    isLoadingCommands.value = true
    // Load command history for all robots and merge them
    const allCommands: RobotCommandHistory[] = []
    for (const robot of robots.value) {
      const commands = await robotService.getCommandHistory(robot.id, 10)
      allCommands.push(...commands)
    }
    // Sort by sentAt descending
    commandHistory.value = allCommands.sort((a, b) => {
      const dateA = new Date(a.sentAt || 0).getTime()
      const dateB = new Date(b.sentAt || 0).getTime()
      return dateB - dateA
    }).slice(0, 20) // Take top 20
  } catch (err) {
    console.error('Error loading command history:', err)
  } finally {
    isLoadingCommands.value = false
  }
}

// Robot actions
const handleControl = (robotId: string) => {
  console.log('Control robot:', robotId)
  // TODO: Implement robot control
}

const handleCalibrate = (robotId: string) => {
  console.log('Calibrate robot:', robotId)
  // TODO: Implement calibration
}

const handleDisconnect = (robotId: string) => {
  console.log('Disconnect robot:', robotId)
  // TODO: Implement disconnect
}

const handleReconnect = (robotId: string) => {
  console.log('Reconnect robot:', robotId)
  // TODO: Implement reconnect
}

// Config actions
const handleAdjustSpeed = async (robotId: string, currentSpeed: number) => {
  const newSpeed = prompt(`Enter new speed (10-100):`, currentSpeed?.toString() || '50')
  if (newSpeed && !isNaN(Number(newSpeed))) {
    try {
      const speed = Number(newSpeed)
      if (speed < 10 || speed > 100) {
        alert('Speed must be between 10 and 100')
        return
      }
      await robotService.updateRobotConfig(robotId, { speed })
      await loadRobots()
      alert('Speed updated successfully')
    } catch (err) {
      alert('Failed to update speed')
      console.error(err)
    }
  }
}

const handleAdjustMaxSpeed = async (robotId: string, currentMaxSpeed: number) => {
  const newMaxSpeed = prompt(`Enter new max speed (10-150):`, currentMaxSpeed?.toString() || '100')
  if (newMaxSpeed && !isNaN(Number(newMaxSpeed))) {
    try {
      const maxSpeed = Number(newMaxSpeed)
      if (maxSpeed < 10 || maxSpeed > 150) {
        alert('Max speed must be between 10 and 150')
        return
      }
      await robotService.updateRobotConfig(robotId, { maxSpeed })
      await loadRobots()
      alert('Max speed updated successfully')
    } catch (err) {
      alert('Failed to update max speed')
      console.error(err)
    }
  }
}

const handleAdjustGripperForce = async (robotId: string, currentForce: number) => {
  const newForce = prompt(`Enter new gripper force (0-100):`, currentForce?.toString() || '50')
  if (newForce && !isNaN(Number(newForce))) {
    try {
      const gripperForce = Number(newForce)
      if (gripperForce < 0 || gripperForce > 100) {
        alert('Gripper force must be between 0 and 100')
        return
      }
      await robotService.updateRobotConfig(robotId, { gripperForce })
      await loadRobots()
      alert('Gripper force updated successfully')
    } catch (err) {
      alert('Failed to update gripper force')
      console.error(err)
    }
  }
}

const handleAdjustGripperSpeed = async (robotId: string, currentGripperSpeed: number) => {
  const newGripperSpeed = prompt(`Enter new gripper speed (10-100):`, currentGripperSpeed?.toString() || '50')
  if (newGripperSpeed && !isNaN(Number(newGripperSpeed))) {
    try {
      const gripperSpeed = Number(newGripperSpeed)
      if (gripperSpeed < 10 || gripperSpeed > 100) {
        alert('Gripper speed must be between 10 and 100')
        return
      }
      await robotService.updateRobotConfig(robotId, { gripperSpeed })
      await loadRobots()
      alert('Gripper speed updated successfully')
    } catch (err) {
      alert('Failed to update gripper speed')
      console.error(err)
    }
  }
}

const handleToggleEmergencyStop = async (robotId: string) => {
  console.log('Toggle emergency stop:', robotId)
  // TODO: Implement emergency stop toggle
}

const handleResetToDefault = async (robotId: string) => {
  if (confirm('Are you sure you want to reset this robot config to default?')) {
    try {
      const defaultConfig: UpdateRobotConfigRequest = {
        speed: 50,
        maxSpeed: 100,
        gripperForce: 50,
        gripperSpeed: 50,
      }
      await robotService.updateRobotConfig(robotId, defaultConfig)
      await loadRobots()
      alert('Config reset to default successfully')
    } catch (err) {
      alert('Failed to reset config')
      console.error(err)
    }
  }
}

// Format date
const formatDate = (date?: string) => {
  if (!date) return '-'
  return new Date(date).toLocaleString('vi-VN')
}

// Get status badge class
const getStatusBadgeClass = (status?: string) => {
  switch (status?.toLowerCase()) {
    case 'idle':
      return 'badge-success'
    case 'busy':
      return 'badge-warning'
    case 'calibrating':
      return 'badge-info'
    case 'error':
      return 'badge-danger'
    case 'completed':
      return 'badge-success'
    case 'pending':
      return 'badge-warning'
    case 'in_progress':
      return 'badge-primary'
    case 'failed':
      return 'badge-danger'
    default:
      return 'badge-secondary'
  }
}

// Get online status
const getOnlineStatusClass = (isOnline?: boolean) => {
  return isOnline ? 'status-online' : 'status-offline'
}

// Get robot card class
const getRobotCardClass = (isOnline?: boolean) => {
  return isOnline ? '' : 'robot-offline'
}

// Get robot name by ID
const getRobotName = (robotId: string) => {
  const robot = robots.value.find(r => r.id === robotId)
  return robot?.robotCode || 'Unknown'
}

// Mount
onMounted(async () => {
  await loadRobots()
  await loadCommandHistory()
  // Auto refresh every 30 seconds to avoid rate limit
  setInterval(async () => {
    try {
      await loadRobots()
      await loadCommandHistory()
    } catch (error) {
      console.error('Auto refresh error:', error)
    }
  }, 30000) // Changed from 5s to 30s
})
</script>

<style scoped>
@import '../../assets/styles/RobotManagement.css';
</style>
