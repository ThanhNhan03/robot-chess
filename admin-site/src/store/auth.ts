import { reactive, computed } from 'vue'
import { API_BASE_URL } from '../config'

interface User {
  id: string
  email: string
  username: string
  fullName?: string
  avatarUrl?: string
  role: string
  isActive: boolean
  lastLoginAt?: string
}

interface AuthState {
  token: string | null
  user: User | null
  isAuthenticated: boolean
}

const state = reactive<AuthState>({
  token: localStorage.getItem('authToken'),
  user: localStorage.getItem('adminUser')
    ? JSON.parse(localStorage.getItem('adminUser')!)
    : null,
  isAuthenticated: !!localStorage.getItem('authToken')
})

export const useAuthStore = () => {
  const login = (token: string, user: User) => {
    state.token = token
    state.user = user
    state.isAuthenticated = true

    localStorage.setItem('authToken', token)
    localStorage.setItem('adminUser', JSON.stringify(user))
  }

  const logout = async () => {
    const token = state.token

    // Call logout API
    if (token) {
      try {
        await fetch(`${API_BASE_URL}/Auth/logout`, {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        })
      } catch (error) {
        console.error('Logout API error:', error)
      }
    }

    // Clear state
    state.token = null
    state.user = null
    state.isAuthenticated = false

    localStorage.removeItem('authToken')
    localStorage.removeItem('adminUser')
  }

  const isAdmin = computed(() => {
    return state.user?.role === 'admin'
  })

  return {
    // State
    token: computed(() => state.token),
    user: computed(() => state.user),
    isAuthenticated: computed(() => state.isAuthenticated),
    isAdmin,

    // Actions
    login,
    logout
  }
}
