<template>
  <div class="login-container">
    <div class="login-card">
      <div class="login-header">
        <div class="logo">
          <span class="logo-icon">♟️</span>
          <h1 class="logo-text">Robot Chess</h1>
        </div>
        <h2 class="login-title">Admin Portal</h2>
        <p class="login-subtitle">Sign in to manage your chess robot system</p>
      </div>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="email" class="form-label">Email</label>
          <div class="input-wrapper">
            <span class="input-icon">📧</span>
            <input
              id="email"
              v-model="formData.email"
              type="email"
              placeholder="admin@robotchess.com"
              class="form-input"
              :class="{ 'input-error': errors.email }"
              required
              autocomplete="email"
            />
          </div>
          <span v-if="errors.email" class="error-text">{{ errors.email }}</span>
        </div>

        <div class="form-group">
          <label for="password" class="form-label">Password</label>
          <div class="input-wrapper">
            <span class="input-icon">🔒</span>
            <input
              id="password"
              v-model="formData.password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Enter your password"
              class="form-input"
              :class="{ 'input-error': errors.password }"
              required
              autocomplete="current-password"
            />
            <button
              type="button"
              @click="showPassword = !showPassword"
              class="password-toggle"
              tabindex="-1"
            >
              {{ showPassword ? '👁️' : '👁️‍🗨️' }}
            </button>
          </div>
          <span v-if="errors.password" class="error-text">{{ errors.password }}</span>
        </div>

        <div v-if="loginError" class="alert alert-error">
          <span class="alert-icon">⚠️</span>
          <span>{{ loginError }}</span>
        </div>

        <button
          type="submit"
          class="btn-login"
          :disabled="isLoading"
          :class="{ 'btn-loading': isLoading }"
        >
          <span v-if="!isLoading">Sign In</span>
          <span v-else class="loading-spinner">
            <span class="spinner"></span>
            Signing in...
          </span>
        </button>
      </form>

      <div class="login-footer">
        <p class="footer-text">
          🔐 Secure admin access only
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const formData = reactive({
  email: '',
  password: ''
})

const errors = reactive({
  email: '',
  password: ''
})

const loginError = ref('')
const isLoading = ref(false)
const showPassword = ref(false)

const validateForm = (): boolean => {
  errors.email = ''
  errors.password = ''
  loginError.value = ''

  if (!formData.email) {
    errors.email = 'Email is required'
    return false
  }

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!emailRegex.test(formData.email)) {
    errors.email = 'Invalid email format'
    return false
  }

  if (!formData.password) {
    errors.password = 'Password is required'
    return false
  }

  if (formData.password.length < 6) {
    errors.password = 'Password must be at least 6 characters'
    return false
  }

  return true
}

const handleLogin = async () => {
  if (!validateForm()) {
    return
  }

  isLoading.value = true
  loginError.value = ''

  try {
    const response = await fetch('https://localhost:7096/api/Auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email: formData.email,
        password: formData.password
      })
    })

    const data = await response.json()

    if (!response.ok || !data.success) {
      throw new Error(data.error || 'Login failed')
    }

    // Check if user is admin
    if (data.user.role !== 'admin') {
      throw new Error('Access denied. Admin role required.')
    }

    // Save auth data to localStorage
    localStorage.setItem('authToken', data.token)
    localStorage.setItem('adminUser', JSON.stringify(data.user))

    // Redirect to admin dashboard
    router.push('/admin')
  } catch (error: any) {
    loginError.value = error.message || 'An error occurred during login'
    console.error('Login error:', error)
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
@import '../../assets/styles/Login.css';
</style>
