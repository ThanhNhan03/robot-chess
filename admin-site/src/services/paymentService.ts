// Payment API Service
const API_BASE_URL = 'https://localhost:7096/api'

export interface PaymentHistory {
  id: string
  userId?: string
  transactionId?: string
  orderCode?: string
  amount: number
  status?: string
  createdAt?: string
  packageId?: number
  user?: {
    id: string
    username: string
    email: string
  }
  package?: {
    id: number
    name: string
    points: number
    price: number
  }
}

export interface PaymentStatistics {
  totalPayments: number
  successfulPayments: number
  pendingPayments: number
  failedPayments: number
  totalRevenue: number
  todayRevenue: number
  thisMonthRevenue: number
}

export const paymentService = {
  // Get all payments with optional filters (Admin only)
  async getAllPayments(params?: {
    startDate?: string
    endDate?: string
    status?: string
  }): Promise<PaymentHistory[]> {
    const token = localStorage.getItem('authToken')
    const queryParams = new URLSearchParams()
    
    if (params?.startDate) queryParams.append('startDate', params.startDate)
    if (params?.endDate) queryParams.append('endDate', params.endDate)
    if (params?.status) queryParams.append('status', params.status)
    
    const url = `${API_BASE_URL}/Payments/admin/all${queryParams.toString() ? `?${queryParams.toString()}` : ''}`
    
    const response = await fetch(url, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    
    if (!response.ok) {
      const errorText = await response.text()
      console.error('Payment API error:', response.status, errorText)
      throw new Error(`Failed to fetch payments: ${response.statusText}`)
    }
    
    return response.json()
  },

  // Get payment statistics (Admin only)
  async getPaymentStatistics(params?: {
    startDate?: string
    endDate?: string
  }): Promise<PaymentStatistics> {
    const token = localStorage.getItem('authToken')
    const queryParams = new URLSearchParams()
    
    if (params?.startDate) queryParams.append('startDate', params.startDate)
    if (params?.endDate) queryParams.append('endDate', params.endDate)
    
    const url = `${API_BASE_URL}/Payments/admin/statistics${queryParams.toString() ? `?${queryParams.toString()}` : ''}`
    
    const response = await fetch(url, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    
    if (!response.ok) {
      const errorText = await response.text()
      console.error('Statistics API error:', response.status, errorText)
      throw new Error(`Failed to fetch statistics: ${response.statusText}`)
    }
    
    return response.json()
  }
}
