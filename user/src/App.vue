<script setup>
import { RouterLink, RouterView } from 'vue-router'
import { useAuthStore } from '@/store/auth'
import { computed } from 'vue'

const authStore = useAuthStore()
// TEMPORARILY SET isAuthenticated TO TRUE FOR UI TESTING
// const isAuthenticated = computed(() => authStore.isAuthenticated)
const isAuthenticated = computed(() => true) // Always show authenticated UI for testing
</script>

<template>
  <div id="app">
    <!-- Header Navigation -->
    <header class="header">
      <div class="container">
        <div class="logo">
          <RouterLink to="/home">♟️ Chess Master</RouterLink>
        </div>
        
        <nav class="navbar">
          <RouterLink to="/home">Home</RouterLink>
          <RouterLink v-if="isAuthenticated" to="/play">Play</RouterLink>
          <RouterLink to="/tutorial">Tutorial</RouterLink>
          <RouterLink to="/practice">Practice</RouterLink>
          <RouterLink to="/leaderboard">Leaderboard</RouterLink>
          <RouterLink v-if="isAuthenticated" to="/history">History</RouterLink>
          <RouterLink v-if="isAuthenticated" to="/profile">Profile</RouterLink>
          <RouterLink v-if="isAuthenticated" to="/settings">Settings</RouterLink>
          
          <div class="auth-links">
            <template v-if="!isAuthenticated">
              <RouterLink to="/login" class="btn-login">Login</RouterLink>
              <RouterLink to="/register" class="btn-register">Register</RouterLink>
            </template>
            <template v-else>
              <RouterLink to="/logout" class="btn-logout">Logout</RouterLink>
            </template>
          </div>
        </nav>
      </div>
    </header>

    <!-- Main Content -->
    <main class="main-content">
      <RouterView />
    </main>

    <!-- Footer -->
    <footer class="footer">
      <div class="container">
        <p>&copy; 2025 Chess Master. All rights reserved.</p>
      </div>
    </footer>
  </div>
</template>

<style scoped>
#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 1rem 0;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo a {
  font-size: 1.5rem;
  font-weight: bold;
  text-decoration: none;
  color: white;
}

.navbar {
  display: flex;
  gap: 1.5rem;
  align-items: center;
  flex-wrap: wrap;
}

.navbar a {
  text-decoration: none;
  color: white;
  font-weight: 500;
  padding: 0.5rem 1rem;
  border-radius: 5px;
  transition: all 0.3s ease;
}

.navbar a:hover {
  background: rgba(255,255,255,0.2);
  transform: translateY(-2px);
}

.navbar a.router-link-active {
  background: rgba(255,255,255,0.3);
}

.auth-links {
  display: flex;
  gap: 0.5rem;
  margin-left: 1rem;
}

.btn-login, .btn-register, .btn-logout {
  padding: 0.5rem 1.5rem !important;
  border-radius: 20px !important;
}

.btn-register {
  background: white !important;
  color: #667eea !important;
}

.btn-logout {
  background: rgba(255,255,255,0.2) !important;
  border: 1px solid white;
}

.main-content {
  flex: 1;
  padding: 2rem 0;
  background: #f5f7fa;
}

.footer {
  background: #2d3748;
  color: white;
  padding: 1.5rem 0;
  text-align: center;
}

@media (max-width: 768px) {
  .container {
    flex-direction: column;
    gap: 1rem;
  }
  
  .navbar {
    justify-content: center;
    width: 100%;
  }
}
</style>