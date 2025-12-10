// Point Package API Service
import { API_BASE_URL } from '../config'

export interface PointPackage {
  id: number
  name: string
  points: number
  price: number
  description?: string
  isActive: boolean
  createdAt: string
}

export interface CreatePointPackageRequest {
  name: string
  points: number
  price: number
  description?: string
}

export interface UpdatePointPackageRequest {
  name?: string
  points?: number
  price?: number
  description?: string
  isActive?: boolean
}

export interface PointTransaction {
  id: string
  userId: string
  amount: number
  transactionType: string
  description?: string
  relatedPaymentId?: string
  createdAt: string
}

class PointPackageService {
  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('authToken')
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    }
  }

  // Admin APIs
  async getAllPackages(): Promise<PointPackage[]> {
    const response = await fetch(`${API_BASE_URL}/PointPackages/admin`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch point packages')
    }

    return response.json()
  }

  async createPackage(data: CreatePointPackageRequest): Promise<PointPackage> {
    const response = await fetch(`${API_BASE_URL}/PointPackages/admin`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(data)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to create point package')
    }

    return response.json()
  }

  async updatePackage(id: number, data: UpdatePointPackageRequest): Promise<PointPackage> {
    const response = await fetch(`${API_BASE_URL}/PointPackages/admin/${id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(data)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to update point package')
    }

    return response.json()
  }

  async deletePackage(id: number): Promise<boolean> {
    const response = await fetch(`${API_BASE_URL}/PointPackages/admin/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to delete point package')
    }

    return true
  }

  // Public APIs
  async getActivePackages(): Promise<PointPackage[]> {
    const response = await fetch(`${API_BASE_URL}/PointPackages`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch active point packages')
    }

    return response.json()
  }

  async purchasePackage(packageId: number, userId: string, transactionId: string): Promise<{ message: string }> {
    const response = await fetch(
      `${API_BASE_URL}/PointPackages/purchase?userId=${userId}&transactionId=${transactionId}`,
      {
        method: 'POST',
        headers: this.getAuthHeaders(),
        body: JSON.stringify({ packageId })
      }
    )

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to purchase point package')
    }

    return response.json()
  }

  async usePoints(userId: string, amount: number, description: string): Promise<{ message: string }> {
    const response = await fetch(`${API_BASE_URL}/PointPackages/use?userId=${userId}`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({ amount, description })
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to use points')
    }

    return response.json()
  }

  async getUserTransactions(userId: string): Promise<PointTransaction[]> {
    const response = await fetch(`${API_BASE_URL}/PointPackages/transactions/${userId}`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch user transactions')
    }

    return response.json()
  }
}

export const pointPackageService = new PointPackageService()
