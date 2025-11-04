<template>
  <div class="auth-page">
    <div class="auth-overlay"></div>
    <div class="auth-modal">
      <div class="auth-header">
        <div class="auth-icon logout-icon">
          <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
            <polyline points="16 17 21 12 16 7"></polyline>
            <line x1="21" y1="12" x2="9" y2="12"></line>
          </svg>
        </div>
        <h2>Logging Out...</h2>
        <p>You have been successfully logged out</p>
      </div>

      <div class="logout-message">
        <div class="loading-spinner"></div>
        <p>Redirecting to home page...</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../store/auth'

const router = useRouter()
const authStore = useAuthStore()

onMounted(() => {
  authStore.logout()
  
  setTimeout(() => {
    router.push('/')
  }, 2000)
})
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
  text-align: center;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: slideUp 0.3s ease;
}

.auth-header {
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

.logout-icon {
  background: linear-gradient(135deg, #f56565, #ed8936);
}

.auth-header h2 {
  margin-bottom: 0.5rem;
  color: #2d3748;
}

.auth-header p {
  color: #718096;
  font-size: 0.95rem;
}

.logout-message {
  padding: 2rem 0;
}

.loading-spinner {
  width: 50px;
  height: 50px;
  margin: 0 auto 1rem;
  border: 4px solid #e2e8f0;
  border-radius: 50%;
  border-top-color: #667eea;
  animation: spin 1s ease-in-out infinite;
}

.logout-message p {
  color: #718096;
  font-size: 0.9rem;
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