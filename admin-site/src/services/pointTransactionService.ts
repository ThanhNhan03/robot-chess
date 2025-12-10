// Point Transaction API Service
import { API_BASE_URL } from '../config'

export interface PointTransaction {
  id: string
  userId: string
  amount: number
  transactionType: string // 'deposit', 'service_usage', 'adjustment'
  description?: string
  relatedPaymentId?: string
  createdAt: string
  user?: {
    id: string
    username: string
    email: string
  }
}

export interface AdjustPointsRequest {
  userId: string
  amount: number // Positive = add, Negative = subtract
  reason: string
}

export const pointTransactionService = {
  // Get all point transactions with optional filters (Admin only)
  async getAllTransactions(params?: {
    startDate?: string
    endDate?: string
    transactionType?: string
  }): Promise<PointTransaction[]> {
    const token = localStorage.getItem('authToken')
    const queryParams = new URLSearchParams()

    if (params?.startDate) queryParams.append('startDate', params.startDate)
    if (params?.endDate) queryParams.append('endDate', params.endDate)
    if (params?.transactionType) queryParams.append('transactionType', params.transactionType)

    const url = `${API_BASE_URL}/PointTransactions/admin/all${queryParams.toString() ? `?${queryParams.toString()}` : ''}`

    const response = await fetch(url, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })

    if (!response.ok) {
      throw new Error(`Failed to fetch transactions: ${response.statusText}`)
    }

    return response.json()
  },

  // Get transactions by user ID (Admin only)
  async getTransactionsByUser(userId: string): Promise<PointTransaction[]> {
    const token = localStorage.getItem('authToken')

    const response = await fetch(`${API_BASE_URL}/PointTransactions/admin/user/${userId}`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })

    if (!response.ok) {
      throw new Error(`Failed to fetch user transactions: ${response.statusText}`)
    }

    return response.json()
  },

  // Get transaction statistics (Admin only)
  async getStatistics(params?: {
    startDate?: string
    endDate?: string
  }): Promise<Record<string, number>> {
    const token = localStorage.getItem('authToken')
    const queryParams = new URLSearchParams()

    if (params?.startDate) queryParams.append('startDate', params.startDate)
    if (params?.endDate) queryParams.append('endDate', params.endDate)

    const url = `${API_BASE_URL}/PointTransactions/admin/statistics${queryParams.toString() ? `?${queryParams.toString()}` : ''}`

    const response = await fetch(url, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })

    if (!response.ok) {
      throw new Error(`Failed to fetch statistics: ${response.statusText}`)
    }

    return response.json()
  },

  // Manually adjust user points (Admin only)
  async adjustPoints(request: AdjustPointsRequest): Promise<{
    transaction: PointTransaction
    newBalance: number
  }> {
    const token = localStorage.getItem('authToken')

    const response = await fetch(`${API_BASE_URL}/PointTransactions/admin/adjust`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to adjust points')
    }

    return response.json()
  }
}
