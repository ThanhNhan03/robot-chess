import { defineStore } from 'pinia'
import axios from 'axios'

// Backend API runs on port 5213 (http) or 7096 (https) - see launchSettings.json
const API_BASE = import.meta.env.VITE_API_BASE || 'http://localhost:5213'

// if a token exists from previous session, set default Authorization header for axios
const _savedToken = localStorage.getItem('token')
if (_savedToken) axios.defaults.headers.common['Authorization'] = `Bearer ${_savedToken}`

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user')) || null,
    token: localStorage.getItem('token') || null,
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
    currentUser: (state) => state.user,
  },
  actions: {
    async register(payload) {
      try {
        // Backend expects: { email, password, username } - case sensitive!
        const requestData = {
          email: payload.email,
          password: payload.password,
          username: payload.username
        }
        
        console.log('🔐 Registering with:', requestData)
        const res = await axios.post(`${API_BASE}/api/auth/signup`, requestData)
        console.log('✅ Registration response:', res.data)
        
        // Backend returns: { success, token, user: { id, email, username, fullName, avatarUrl }, error }
        if (!res.data.success) {
          throw new Error(res.data.error || 'Registration failed')
        }
        
        this.user = res.data.user
        this.token = res.data.token
        
        // set default auth header for subsequent requests
        if (this.token) {
          axios.defaults.headers.common['Authorization'] = `Bearer ${this.token}`
          localStorage.setItem('token', this.token)
        }
        if (this.user) {
          localStorage.setItem('user', JSON.stringify(this.user))
        }
        
        return res.data
      } catch (error) {
        console.error('❌ Registration error:', error.response?.data || error.message)
        if (error.response?.data?.error) {
          throw new Error(error.response.data.error)
        }
        if (error.response?.data) {
          throw new Error(JSON.stringify(error.response.data))
        }
        throw new Error(error.message || 'Registration failed')
      }
    },
    async login(payload) {
      try {
        // Backend expects: { email, password }
        const res = await axios.post(`${API_BASE}/api/auth/login`, payload)
        
        // Backend returns: { success, token, user: { id, email, username, fullName, avatarUrl }, error }
        if (!res.data.success) {
          throw new Error(res.data.error || 'Login failed')
        }
        
        this.user = res.data.user
        this.token = res.data.token
        
        if (this.token) {
          axios.defaults.headers.common['Authorization'] = `Bearer ${this.token}`
          localStorage.setItem('token', this.token)
        }
        if (this.user) {
          localStorage.setItem('user', JSON.stringify(this.user))
        }
        
        return res.data
      } catch (error) {
        if (error.response?.data?.error) {
          throw new Error(error.response.data.error)
        }
        throw new Error(error.message || 'Login failed')
      }
    },
    async logout() {
      try {
        // call backend to sign out (backend expects Authorization header)
        await axios.post(`${API_BASE}/api/auth/logout`, null, {
          headers: { Authorization: `Bearer ${this.token}` }
        })
      } catch (err) {
        // ignore errors - still clear local state
      } finally {
        this.user = null
        this.token = null
        delete axios.defaults.headers.common['Authorization']
        localStorage.removeItem('user')
        localStorage.removeItem('token')
      }
    },
  },
})