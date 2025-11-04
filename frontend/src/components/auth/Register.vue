<template>
  <div class="auth-page">
    <div class="auth-overlay" @click="goBack"></div>
    <div class="auth-modal">
      <button class="close-btn" @click="goBack">&times;</button>
      
      <div class="auth-header">
        <div class="auth-icon">
          <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
            <circle cx="8.5" cy="7" r="4"></circle>
            <line x1="20" y1="8" x2="20" y2="14"></line>
            <line x1="23" y1="11" x2="17" y2="11"></line>
          </svg>
        </div>
        <h2>Create Account</h2>
        <p>Join Chess Master today</p>
      </div>

      <form @submit.prevent="onRegister" class="auth-form">
        <div class="form-group">
          <label class="form-label">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
              <circle cx="12" cy="7" r="4"></circle>
            </svg>
            Username
          </label>
          <input 
            v-model="form.username" 
            type="text"
            placeholder="Enter your username" 
            class="form-control"
            required 
            :disabled="loading"
          />
        </div>

        <div class="form-group">
          <label class="form-label">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path>
              <polyline points="22,6 12,13 2,6"></polyline>
            </svg>
            Email
          </label>
          <input 
            v-model="form.email" 
            type="email"
            placeholder="Enter your email" 
            class="form-control"
            required 
            :disabled="loading"
          />
        </div>

        <div class="form-group">
          <label class="form-label">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
              <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
            </svg>
            Password
          </label>
          <input 
            v-model="form.password" 
            type="password"
            placeholder="Enter your password" 
            class="form-control"
            required 
            :disabled="loading"
          />
        </div>

        <div class="form-group">
          <label class="form-label">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
              <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
            </svg>
            Confirm Password
          </label>
          <input 
            v-model="form.confirmPassword" 
            type="password"
            placeholder="Confirm your password" 
            class="form-control"
            required 
            :disabled="loading"
          />
        </div>

        <div v-if="error" class="alert alert-error">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="15" y1="9" x2="9" y2="15"></line>
            <line x1="9" y1="9" x2="15" y2="15"></line>
          </svg>
          {{ error }}
        </div>

        <div v-if="success" class="alert alert-success">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
            <polyline points="22 4 12 14.01 9 11.01"></polyline>
          </svg>
          {{ success }}
        </div>

        <button type="submit" class="btn btn-primary btn-block" :disabled="loading">
          <span v-if="loading" class="loading"></span>
          <span v-else>Create Account</span>
        </button>
      </form>

      <div class="auth-footer">
        <p>Already have an account? <RouterLink to="/login">Sign In</RouterLink></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../store/auth'

const router = useRouter()
const authStore = useAuthStore()

const form = ref({ 
  username: '', 
  email: '', 
  password: '',
  confirmPassword: ''
})
const error = ref(null)
const success = ref(null)
const loading = ref(false)

function goBack() {
  router.push('/')
}

async function onRegister() {
  error.value = null
  success.value = null

  // Validate passwords match
  if (form.value.password !== form.value.confirmPassword) {
    error.value = 'Passwords do not match'
    return
  }

  // Validate password length
  if (form.value.password.length < 6) {
    error.value = 'Password must be at least 6 characters'
    return
  }

  loading.value = true

  try {
    await authStore.register({
      username: form.value.username,
      email: form.value.email,
      password: form.value.password
    })
    
    success.value = 'Registration successful! Redirecting...'
    
    setTimeout(() => {
      router.push('/play')
    }, 1500)
  } catch (e) {
    error.value = e.message || 'Registration failed'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-page {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  animation: fadeIn 0.3s ease;
}

.auth-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(8px);
}

.auth-modal {
  position: relative;
  background: white;
  border-radius: 20px;
  padding: 3rem;
  max-width: 480px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: slideUp 0.3s ease;
}

.close-btn {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: transparent;
  border: none;
  font-size: 2rem;
  color: #718096;
  cursor: pointer;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: all 0.3s ease;
}

.close-btn:hover {
  background: #f5f7fa;
  color: #2d3748;
  transform: rotate(90deg);
}

.auth-header {
  text-align: center;
  margin-bottom: 2rem;
}

.auth-icon {
  width: 80px;
  height: 80px;
  margin: 0 auto 1rem;
  background: linear-gradient(135deg, #667eea, #764ba2);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.auth-header h2 {
  margin-bottom: 0.5rem;
  color: #2d3748;
}

.auth-header p {
  color: #718096;
  font-size: 0.95rem;
}

.auth-form {
  margin-bottom: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #2d3748;
  font-size: 0.9rem;
}

.form-label svg {
  color: #667eea;
}

.form-control {
  width: 100%;
  padding: 0.875rem 1rem;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: #f9fafb;
}

.form-control:focus {
  outline: none;
  border-color: #667eea;
  background: white;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-control:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.alert {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem;
  border-radius: 10px;
  margin-bottom: 1rem;
  font-size: 0.9rem;
}

.alert-error {
  background: #fff5f5;
  color: #c53030;
  border: 1px solid #feb2b2;
}

.alert-success {
  background: #f0fff4;
  color: #276749;
  border: 1px solid #9ae6b4;
}

.btn-block {
  width: 100%;
  padding: 1rem;
  font-size: 1.05rem;
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none !important;
}

.loading {
  display: inline-block;
  width: 18px;
  height: 18px;
  border: 3px solid rgba(255,255,255,.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 1s ease-in-out infinite;
}

.auth-footer {
  text-align: center;
  padding-top: 1.5rem;
  border-top: 1px solid #e2e8f0;
  color: #718096;
}

.auth-footer a {
  color: #667eea;
  font-weight: 600;
  text-decoration: none;
}

.auth-footer a:hover {
  text-decoration: underline;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 768px) {
  .auth-modal {
    padding: 2rem 1.5rem;
    max-width: 95%;
  }
  
  .auth-icon {
    width: 60px;
    height: 60px;
  }
}
</style>