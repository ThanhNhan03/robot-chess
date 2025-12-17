<template>
  <div class="robot-management">
    <div class="panel-header">
      <h2 class="panel-title">
        <Bot :size="24" />
        Robot Management
      </h2>
      <div style="display: flex; gap: 12px;">
        <button 
          class="btn-flat btn-success" 
          @click="handleRefresh"
          :disabled="isLoading"
        >
          <RefreshCw :size="18" :class="{ 'spin': isLoading }" /> 
          {{ isLoading ? 'Refreshing...' : 'Refresh' }}
        </button>
        <button class="btn-flat btn-primary">
          <Plus :size="18" /> Add New Robot
        </button>
      </div>
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
            <div style="display: flex; gap: 8px; align-items: center;">
              <span v-if="getTempConfig(robot.id).hasChanges" class="badge-flat badge-warning">
                <Activity :size="12" /> UNSAVED
              </span>
              <span class="badge-flat" :class="robot.isOnline ? 'badge-success' : 'badge-danger'">
                {{ robot.isOnline ? 'ACTIVE' : 'INACTIVE' }}
              </span>
            </div>
          </div>
          <div class="config-body">
            <div class="config-row">
              <div class="config-slider-group">
                <div class="config-label-row">
                  <span class="config-label">Speed</span>
                  <span class="config-value" :class="{ 'value-changed': getTempConfig(robot.id).speed !== (robot.config?.speed || 50) }">
                    {{ getTempConfig(robot.id).speed }} <span class="config-unit">mm/s</span>
                  </span>
                </div>
                <input 
                  type="range" 
                  class="config-slider"
                  :value="getTempConfig(robot.id).speed"
                  @input="(e) => updateTempConfig(robot.id, 'speed', Number((e.target as HTMLInputElement).value))"
                  min="10" 
                  max="100" 
                  step="5"
                  :disabled="!robot.isOnline"
                  title="Adjust robot movement speed"
                />
                <div class="slider-labels">
                  <span>Slow (10)</span>
                  <span>Fast (100)</span>
                </div>
              </div>
            </div>
            <div class="config-row">
              <div class="config-slider-group">
                <div class="config-label-row">
                  <span class="config-label">Gripper Force</span>
                  <span class="config-value" :class="{ 'value-changed': getTempConfig(robot.id).gripperForce !== (robot.config?.gripperForce || 50) }">
                    {{ getTempConfig(robot.id).gripperForce }} <span class="config-unit">%</span>
                  </span>
                </div>
                <input 
                  type="range" 
                  class="config-slider"
                  :value="getTempConfig(robot.id).gripperForce"
                  @input="(e) => updateTempConfig(robot.id, 'gripperForce', Number((e.target as HTMLInputElement).value))"
                  min="0" 
                  max="100" 
                  step="5"
                  :disabled="!robot.isOnline"
                  title="Gripper grip force"
                />
                <div class="slider-labels">
                  <span>Gentle (0)</span>
                  <span>Strong (100)</span>
                </div>
              </div>
            </div>
            <div class="config-row">
              <div class="config-slider-group">
                <div class="config-label-row">
                  <span class="config-label">Gripper Speed</span>
                  <span class="config-value" :class="{ 'value-changed': getTempConfig(robot.id).gripperSpeed !== (robot.config?.gripperSpeed || 50) }">
                    {{ getTempConfig(robot.id).gripperSpeed }} <span class="config-unit">mm/s</span>
                  </span>
                </div>
                <input 
                  type="range" 
                  class="config-slider"
                  :value="getTempConfig(robot.id).gripperSpeed"
                  @input="(e) => updateTempConfig(robot.id, 'gripperSpeed', Number((e.target as HTMLInputElement).value))"
                  min="10" 
                  max="100" 
                  step="5"
                  :disabled="!robot.isOnline"
                  title="Gripper open/close speed"
                />
                <div class="slider-labels">
                  <span>Slow (10)</span>
                  <span>Fast (100)</span>
                </div>
              </div>
            </div>
          </div>
          <div class="config-footer">
            <span class="config-updated">Last updated: {{ formatDate(robot.config?.updatedAt) }}</span>
            <div class="config-actions">
              <button 
                v-if="getTempConfig(robot.id).hasChanges"
                class="btn-flat btn-sm btn-secondary" 
                @click="handleDiscardChanges(robot.id)"
                :disabled="!robot.isOnline || savingConfigs[robot.id]"
              >
                Discard
              </button>
              <button 
                class="btn-flat btn-sm" 
                :class="getTempConfig(robot.id).hasChanges ? 'btn-success' : 'btn-warning'"
                @click="getTempConfig(robot.id).hasChanges ? handleSaveConfig(robot.id) : handleResetToDefault(robot.id)"
                :disabled="!robot.isOnline || savingConfigs[robot.id]"
              >
                <Zap v-if="savingConfigs[robot.id]" :size="14" class="spin" />
                {{ savingConfigs[robot.id] ? 'Saving...' : getTempConfig(robot.id).hasChanges ? 'Save Changes' : 'Reset to Default' }}
              </button>
            </div>
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
import { robotService, type Robot, type RobotStats, type UpdateRobotConfigRequest } from '../../services/robotService'

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
const isLoading = ref(false)
const error = ref<string | null>(null)

// Temporary config state for editing (before saving)
const tempConfigs = ref<Record<string, {
  speed: number
  maxSpeed: number
  gripperForce: number
  gripperSpeed: number
  hasChanges: boolean
}>>({})

// Saving state for each robot
const savingConfigs = ref<Record<string, boolean>>({})

// Helper functions for temp config
const getTempConfig = (robotId: string) => {
  if (!tempConfigs.value[robotId]) {
    const robot = robots.value.find(r => r.id === robotId)
    tempConfigs.value[robotId] = {
      speed: robot?.config?.speed || 50,
      maxSpeed: robot?.config?.maxSpeed || 100,
      gripperForce: robot?.config?.gripperForce || 50,
      gripperSpeed: robot?.config?.gripperSpeed || 50,
      hasChanges: false
    }
  }
  return tempConfigs.value[robotId]
}

const updateTempConfig = (robotId: string, field: string, value: number) => {
  const temp = getTempConfig(robotId)
  const robot = robots.value.find(r => r.id === robotId)
  
  // Update the field
  ;(temp as any)[field] = value
  
  // Check if there are changes
  temp.hasChanges = 
    temp.speed !== (robot?.config?.speed || 50) ||
    temp.maxSpeed !== (robot?.config?.maxSpeed || 100) ||
    temp.gripperForce !== (robot?.config?.gripperForce || 50) ||
    temp.gripperSpeed !== (robot?.config?.gripperSpeed || 50)
}

const handleDiscardChanges = (robotId: string) => {
  const robot = robots.value.find(r => r.id === robotId)
  if (robot) {
    tempConfigs.value[robotId] = {
      speed: robot.config?.speed || 50,
      maxSpeed: robot.config?.maxSpeed || 100,
      gripperForce: robot.config?.gripperForce || 50,
      gripperSpeed: robot.config?.gripperSpeed || 50,
      hasChanges: false
    }
  }
}

const handleSaveConfig = async (robotId: string) => {
  const temp = getTempConfig(robotId)
  
  try {
    savingConfigs.value[robotId] = true
    
    const config: UpdateRobotConfigRequest = {
      speed: temp.speed,
      maxSpeed: temp.maxSpeed,
      gripperForce: temp.gripperForce,
      gripperSpeed: temp.gripperSpeed,
    }
    
    await robotService.updateRobotConfig(robotId, config)
    
    // Update robot config in local state without full reload
    const robot = robots.value.find(r => r.id === robotId)
    if (robot) {
      // Initialize config if it doesn't exist
      if (!robot.config) {
        robot.config = {
          speed: temp.speed,
          maxSpeed: temp.maxSpeed,
          gripperForce: temp.gripperForce,
          gripperSpeed: temp.gripperSpeed,
          emergencyStop: false,
          updatedAt: new Date().toISOString()
        } as any
      } else {
        // Update existing config
        robot.config.speed = temp.speed
        robot.config.maxSpeed = temp.maxSpeed
        robot.config.gripperForce = temp.gripperForce
        robot.config.gripperSpeed = temp.gripperSpeed
        robot.config.updatedAt = new Date().toISOString()
      }
    }
    
    // Mark as saved (no changes)
    temp.hasChanges = false
    
    alert('Configuration saved successfully!')
  } catch (err) {
    alert('Failed to save configuration')
    console.error(err)
  } finally {
    savingConfigs.value[robotId] = false
  }
}

// Load data
const loadRobots = async () => {
  try {
    isLoading.value = true
    error.value = null
    robots.value = await robotService.getAllRobots()
    robotStats.value = await robotService.getRobotStats()
    
    // Only reset temp configs that don't have unsaved changes
    for (const robotId in tempConfigs.value) {
      const config = tempConfigs.value[robotId]
      if (config && !config.hasChanges) {
        delete tempConfigs.value[robotId]
      }
    }
  } catch (err) {
    error.value = 'Failed to load robots'
    console.error('Error loading robots:', err)
  } finally {
    isLoading.value = false
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
const handleToggleEmergencyStop = async (robotId: string) => {
  console.log('Toggle emergency stop:', robotId)
  // TODO: Implement emergency stop toggle
}

const handleResetToDefault = async (robotId: string) => {
  if (confirm('Are you sure you want to reset this robot config to default?')) {
    try {
      savingConfigs.value[robotId] = true
      
      const defaultConfig: UpdateRobotConfigRequest = {
        speed: 50,
        maxSpeed: 100,
        gripperForce: 50,
        gripperSpeed: 50,
      }
      await robotService.updateRobotConfig(robotId, defaultConfig)
      
      // Update local state without reload
      const robot = robots.value.find(r => r.id === robotId)
      if (robot) {
        if (!robot.config) {
          robot.config = {
            speed: 50,
            maxSpeed: 100,
            gripperForce: 50,
            gripperSpeed: 50,
            emergencyStop: false,
            updatedAt: new Date().toISOString()
          } as any
        } else {
          robot.config.speed = 50
          robot.config.maxSpeed = 100
          robot.config.gripperForce = 50
          robot.config.gripperSpeed = 50
          robot.config.updatedAt = new Date().toISOString()
        }
      }
      
      // Update temp config to default values
      if (tempConfigs.value[robotId]) {
        tempConfigs.value[robotId] = {
          speed: 50,
          maxSpeed: 100,
          gripperForce: 50,
          gripperSpeed: 50,
          hasChanges: false
        }
      }
      
      alert('Config reset to default successfully')
    } catch (err) {
      alert('Failed to reset config')
      console.error(err)
    } finally {
      savingConfigs.value[robotId] = false
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

// Manual refresh function
const handleRefresh = async () => {
  await loadRobots()
}

// Mount - only load once on initial mount
onMounted(async () => {
  await loadRobots()
  // No auto-refresh - user must click refresh button manually
})
</script>

<style scoped>
@import '../../assets/styles/RobotManagement.css';
</style>