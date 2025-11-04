import { defineStore } from 'pinia'
import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_BASE || 'http://localhost:3000'

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
        const res = await axios.post(`${API_BASE}/api/auth/register`, payload)
        this.user = res.data.user
        this.token = res.data.token
        localStorage.setItem('user', JSON.stringify(res.data.user))
        localStorage.setItem('token', res.data.token)
        return res.data
      } catch (error) {
        throw new Error(error.response?.data?.message || 'Registration failed')
      }
    },
    async login(payload) {
      try {
        const res = await axios.post(`${API_BASE}/api/auth/login`, payload)
        this.user = res.data.user
        this.token = res.data.token
        localStorage.setItem('user', JSON.stringify(res.data.user))
        localStorage.setItem('token', res.data.token)
        return res.data
      } catch (error) {
        throw new Error(error.response?.data?.message || 'Login failed')
      }
    },
    logout() {
      this.user = null
      this.token = null
      localStorage.removeItem('user')
      localStorage.removeItem('token')
    },
  },
})