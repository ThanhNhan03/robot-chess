// Robot API Service
const API_BASE_URL = 'https://localhost:7096/api'

export interface Robot {
  id: string
  robotCode: string
  name?: string
  location?: string
  ipAddress?: string
  isOnline?: boolean
  lastOnlineAt?: string
  status?: string
  currentGameId?: string
  moveSpeedMs?: number
  createdAt?: string
  updatedAt?: string
  config?: RobotConfig
  latestMonitoring?: RobotMonitoring
}

export interface RobotConfig {
  id: string
  robotId: string
  speed?: number
  gripperForce?: number
  gripperSpeed?: number
  maxSpeed?: number
  emergencyStop?: boolean
  updatedAt?: string
}

export interface RobotMonitoring {
  id: string
  robotId: string
  currentPositionX?: number
  currentPositionY?: number
  currentPositionZ?: number
  currentRotationRx?: number
  currentRotationRy?: number
  currentRotationRz?: number
  gripperState?: string
  gripperPosition?: number
  isMoving?: boolean
  currentSpeed?: number
  hasError?: boolean
  errorMessage?: string
  recordedAt?: string
}

export interface RobotCommandHistory {
  id: string
  robotId: string
  commandType: string
  status?: string
  errorMessage?: string
  sentAt?: string
  startedAt?: string
  completedAt?: string
  executionTimeMs?: number
  executedByUsername?: string
}

export interface CreateRobotRequest {
  robotCode: string
  name?: string
  location?: string
  ipAddress?: string
}

export interface UpdateRobotRequest {
  name?: string
  location?: string
  ipAddress?: string
  status?: string
}

export interface UpdateRobotConfigRequest {
  speed?: number
  gripperForce?: number
  gripperSpeed?: number
  maxSpeed?: number
}

export interface RobotStats {
  totalRobots: number
  onlineRobots: number
  offlineRobots: number
  busyRobots: number
  idleRobots: number
  totalMoves: number
  avgSuccessRate: number
}

class RobotService {
  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('authToken')
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  // Get all robots
  async getAllRobots(): Promise<Robot[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/Robots`, {
        method: 'GET',
        headers: this.getAuthHeaders(),
      })

      if (!response.ok) {
        throw new Error(`Failed to fetch robots: ${response.statusText}`)
      }

      return await response.json()
    } catch (error) {
      console.error('Error fetching robots:', error)
      throw error
    }
  }

  // Get robot by ID
  async getRobotById(id: string): Promise<Robot> {
    try {
      const response = await fetch(`${API_BASE_URL}/Robots/${id}`, {
        method: 'GET',
        headers: this.getAuthHeaders(),
      })

      if (!response.ok) {
        throw new Error(`Failed to fetch robot: ${response.statusText}`)
      }

      return await response.json()
    } catch (error) {
      console.error('Error fetching robot:', error)
      throw error
    }
  }

  // Create new robot
  async createRobot(data: CreateRobotRequest): Promise<Robot> {
    try {
      const response = await fetch(`${API_BASE_URL}/Robots`, {
        method: 'POST',
        headers: this.getAuthHeaders(),
        body: JSON.stringify(data),
      })

      if (!response.ok) {
        throw new Error(`Failed to create robot: ${response.statusText}`)
      }

      return await response.json()
    } catch (error) {
      console.error('Error creating robot:', error)
      throw error
    }
  }

  // Update robot
  async updateRobot(id: string, data: UpdateRobotRequest): Promise<Robot> {
    try {
      const response = await fetch(`${API_BASE_URL}/Robots/${id}`, {
        method: 'PUT',
        headers: this.getAuthHeaders(),
        body: JSON.stringify(data),
      })

      if (!response.ok) {
        throw new Error(`Failed to update robot: ${response.statusText}`)
      }

      return await response.json()
    } catch (error) {
      console.error('Error updating robot:', error)
      throw error
    }
  }

  // Delete robot
  async deleteRobot(id: string): Promise<void> {
    try {
      const response = await fetch(`${API_BASE_URL}/Robots/${id}`, {
        method: 'DELETE',
        headers: this.getAuthHeaders(),
      })

      if (!response.ok) {
        throw new Error(`Failed to delete robot: ${response.statusText}`)
      }
    } catch (error) {
      console.error('Error deleting robot:', error)
      throw error
    }
  }

  // Get robot config
  async getRobotConfig(id: string): Promise<RobotConfig> {
    try {
      const response = await fetch(`${API_BASE_URL}/Robots/${id}/config`, {
        method: 'GET',
        headers: this.getAuthHeaders(),
      })

      if (!response.ok) {
        throw new Error(`Failed to fetch robot config: ${response.statusText}`)
      }

      return await response.json()
    } catch (error) {
      console.error('Error fetching robot config:', error)
      throw error
    }
  }

  // Update robot config
  async updateRobotConfig(id: string, data: UpdateRobotConfigRequest): Promise<RobotConfig> {
    try {
      const response = await fetch(`${API_BASE_URL}/Robots/${id}/config`, {
        method: 'PUT',
        headers: this.getAuthHeaders(),
        body: JSON.stringify(data),
      })

      if (!response.ok) {
        throw new Error(`Failed to update robot config: ${response.statusText}`)
      }

      return await response.json()
    } catch (error) {
      console.error('Error updating robot config:', error)
      throw error
    }
  }

  // Set robot speed
  async setRobotSpeed(id: string, speed: number): Promise<{ commandId: string; message: string }> {
    try {
      const response = await fetch(`${API_BASE_URL}/Robots/${id}/commands/speed`, {
        method: 'POST',
        headers: this.getAuthHeaders(),
        body: JSON.stringify(speed),
      })

      if (!response.ok) {
        throw new Error(`Failed to set robot speed: ${response.statusText}`)
      }

      return await response.json()
    } catch (error) {
      console.error('Error setting robot speed:', error)
      throw error
    }
  }

  // Get robot statistics
  async getRobotStats(): Promise<RobotStats> {
    try {
      const robots = await this.getAllRobots()
      
      const totalRobots = robots.length
      const onlineRobots = robots.filter(r => r.isOnline).length
      const offlineRobots = totalRobots - onlineRobots
      const busyRobots = robots.filter(r => r.status === 'busy').length
      const idleRobots = robots.filter(r => r.status === 'idle').length
      
      // Calculate total moves and success rate from monitoring data
      let totalMoves = 0
      let successfulMoves = 0
      
      robots.forEach(robot => {
        if (robot.latestMonitoring) {
          totalMoves += 1
          if (!robot.latestMonitoring.hasError) {
            successfulMoves += 1
          }
        }
      })
      
      const avgSuccessRate = totalMoves > 0 ? (successfulMoves / totalMoves) * 100 : 0

      return {
        totalRobots,
        onlineRobots,
        offlineRobots,
        busyRobots,
        idleRobots,
        totalMoves,
        avgSuccessRate: Math.round(avgSuccessRate * 100) / 100,
      }
    } catch (error) {
      console.error('Error fetching robot stats:', error)
      throw error
    }
  }

  // Get command history for a robot
  async getCommandHistory(robotId: string, limit: number = 50): Promise<RobotCommandHistory[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/Robots/${robotId}/commands?limit=${limit}`, {
        method: 'GET',
        headers: this.getAuthHeaders(),
      })

      if (!response.ok) {
        throw new Error(`Failed to fetch command history: ${response.statusText}`)
      }

      return await response.json()
    } catch (error) {
      console.error('Error fetching command history:', error)
      throw error
    }
  }
}

export const robotService = new RobotService()
