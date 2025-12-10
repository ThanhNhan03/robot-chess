// FAQ API Service
import { API_BASE_URL } from '../config'

export interface Faq {
  id: string
  question: string
  answer: string
  category?: string
  isPublished: boolean
  displayOrder?: number
  createdAt?: string
  updatedAt?: string
}

export interface CreateFaqRequest {
  question: string
  answer: string
  category?: string
  isPublished: boolean
  displayOrder?: number
}

export interface UpdateFaqRequest {
  question?: string
  answer?: string
  category?: string
  isPublished?: boolean
  displayOrder?: number
}

export interface FaqStats {
  totalFaqs: number
  publishedFaqs: number
  draftFaqs: number
  categoriesCount: number
}

class FaqService {
  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('authToken')
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    }
  }

  async getAllFaqs(includeUnpublished: boolean = true): Promise<Faq[]> {
    const endpoint = includeUnpublished
      ? `${API_BASE_URL}/Faqs/admin`
      : `${API_BASE_URL}/Faqs`

    const response = await fetch(endpoint, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch FAQs')
    }

    return response.json()
  }

  async getFaqsByCategory(category: string): Promise<Faq[]> {
    const response = await fetch(`${API_BASE_URL}/Faqs?category=${category}`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch FAQs by category')
    }

    return response.json()
  }

  async getFaqById(id: string): Promise<Faq> {
    const response = await fetch(`${API_BASE_URL}/Faqs/${id}`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch FAQ')
    }

    return response.json()
  }

  async createFaq(faqData: CreateFaqRequest): Promise<Faq> {
    const response = await fetch(`${API_BASE_URL}/Faqs/admin`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(faqData)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to create FAQ')
    }

    return response.json()
  }

  async updateFaq(id: string, faqData: UpdateFaqRequest): Promise<Faq> {
    const response = await fetch(`${API_BASE_URL}/Faqs/admin/${id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(faqData)
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Failed to update FAQ')
    }

    return response.json()
  }

  async deleteFaq(id: string): Promise<boolean> {
    const response = await fetch(`${API_BASE_URL}/Faqs/admin/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to delete FAQ')
    }

    return true
  }

  async getCategories(): Promise<string[]> {
    const response = await fetch(`${API_BASE_URL}/Faqs/categories`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to fetch categories')
    }

    return response.json()
  }

  async getFaqStats(): Promise<FaqStats> {
    const faqs = await this.getAllFaqs(true)
    const categories = await this.getCategories()

    return {
      totalFaqs: faqs.length,
      publishedFaqs: faqs.filter(f => f.isPublished).length,
      draftFaqs: faqs.filter(f => !f.isPublished).length,
      categoriesCount: categories.length
    }
  }

  async togglePublishStatus(id: string, isPublished: boolean): Promise<Faq> {
    return this.updateFaq(id, { isPublished })
  }
}

export const faqService = new FaqService()
