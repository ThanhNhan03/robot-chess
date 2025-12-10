// User API Service
import { API_BASE_URL } from '../config'

export interface User {
  id: string
  email: string
  username: string
  fullName?: string
  avatarUrl?: string
  role: string
  isActive: boolean
  lastLoginAt?: string
  phoneNumber?: string
  pointsBalance?: number
  createdAt?: string
}

export interface CreateUserRequest {
  email: string
  username: string
  password: string
  fullName?: string
  avatarUrl?: string
  role: string
  phoneNumber?: string
}

export interface AdminCreateUserRequest {
  email: string
  username: string
  fullName?: string
  avatarUrl?: string
  role: string
  phoneNumber?: string
}

export interface UpdateUserRequest {
  email?: string
  username?: string
  fullName?: string
  avatarUrl?: string
  role?: string
  phoneNumber?: string
}

export interface UserActivity {
  userId: string
  username: string
  totalGames: number
  gamesWon: number
  gamesLost: number
  gamesDraw: number
  lastLoginAt?: string
  lastGameAt?: string
}

export interface UserStats {
  totalUsers: number
  activeUsers: number
  adminUsers: number
  newUsersThisWeek: number
}

class UserService {
  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('authToken')
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    }
  }

  async getAllUsers(includeInactive: boolean = false): Promise<User[]> {
    const response = await fetch(
      `${API_BASE_URL}/Users?includeInactive=${includeInactive}`,
      {
        method: 'GET',
        headers: this.getAuthHeaders()
      }
    )

    if (!response.ok) {
      throw new Error('Failed to fetch users')
    }

    return response.json()
  }

  async getUserById(id: string): Promise<User> {
    const response = await fetch(`${API_BASE_URL}/Users/${id}`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch user')
    }

    return response.json()
  }

  async createUser(userData: CreateUserRequest): Promise<User> {
    const response = await fetch(`${API_BASE_URL}/Users`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(userData)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to create user')
    }

    return response.json()
  }

  async adminCreateUser(userData: AdminCreateUserRequest): Promise<User> {
    const response = await fetch(`${API_BASE_URL}/Users/admin/create`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(userData)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to create user')
    }

    return response.json()
  }

  async updateUser(id: string, userData: UpdateUserRequest): Promise<User> {
    const response = await fetch(`${API_BASE_URL}/Users/${id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(userData)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to update user')
    }

    return response.json()
  }

  async deleteUser(id: string): Promise<boolean> {
    const response = await fetch(`${API_BASE_URL}/Users/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to delete user')
    }

    return true
  }

  async updateUserStatus(id: string, isActive: boolean): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/Users/${id}/status`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ isActive })
    })

    if (!response.ok) {
      throw new Error('Failed to update user status')
    }
  }

  async getUsersByRole(role: string): Promise<User[]> {
    const response = await fetch(`${API_BASE_URL}/Users/role/${role}`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch users by role')
    }

    return response.json()
  }

  async getUserActivity(id: string): Promise<UserActivity> {
    const response = await fetch(`${API_BASE_URL}/Users/${id}/activity`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch user activity')
    }

    return response.json()
  }

  async getUserStats(): Promise<UserStats> {
    const response = await fetch(`${API_BASE_URL}/Users/stats`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch user stats')
    }

    return response.json()
  }
}

export const userService = new UserService()

// Export convenience function
export const getAllUsers = (includeInactive: boolean = false): Promise<User[]> => {
  return userService.getAllUsers(includeInactive)
}
