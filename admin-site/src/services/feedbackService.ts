import { API_BASE_URL } from '../config'

export interface Feedback {
  id: string
  userId?: string
  userEmail?: string
  userFullName?: string
  message?: string
  createdAt?: string
}

export interface CreateFeedbackDto {
  message: string
}

export interface UpdateFeedbackDto {
  message?: string
}

class FeedbackService {
  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('authToken')
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    }
  }

  async getAllFeedbacks(): Promise<Feedback[]> {
    const response = await fetch(`${API_BASE_URL}/feedbacks/admin`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch feedbacks')
    }

    return response.json()
  }

  async getFeedbackById(id: string): Promise<Feedback> {
    const response = await fetch(`${API_BASE_URL}/feedbacks/${id}`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch feedback')
    }

    return response.json()
  }

  async getMyFeedbacks(): Promise<Feedback[]> {
    const response = await fetch(`${API_BASE_URL}/feedbacks/my-feedbacks`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch my feedbacks')
    }

    return response.json()
  }

  async createFeedback(dto: CreateFeedbackDto): Promise<Feedback> {
    const response = await fetch(`${API_BASE_URL}/feedbacks`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(dto)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to create feedback')
    }

    return response.json()
  }

  async updateFeedback(id: string, dto: UpdateFeedbackDto): Promise<Feedback> {
    const response = await fetch(`${API_BASE_URL}/feedbacks/${id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(dto)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to update feedback')
    }

    return response.json()
  }

  async deleteFeedback(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/feedbacks/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to delete feedback')
    }
  }
}

export const feedbackService = new FeedbackService()
