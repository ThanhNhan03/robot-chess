<template>
  <div class="admin-screen">
    <!-- Header -->
    <header class="admin-header">
      <div class="header-content">
        <h1 class="admin-title">ADMIN DASHBOARD</h1>
        <div class="header-actions">
          <div class="user-info">
            <span class="user-avatar">{{ userInitial }}</span>
            <div class="user-details">
              <span class="user-name">{{ userName }}</span>
              <span class="user-role">Administrator</span>
            </div>
          </div>
          <button class="btn-flat btn-danger" @click="handleLogout">
            <span class="material-icons logout-icon"> logout</span>
            Logout
          </button>
        
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <div class="admin-content">
      <!-- Sidebar Navigation -->
      <aside class="admin-sidebar">
        <nav class="admin-nav">
          <button 
            :class="['nav-item', { active: activeTab === 'overview' }]"
            @click="activeTab = 'overview'"
            >
            <span class="material-icons nav-icon">dashboard</span>
            <span class="nav-label">Overview</span>
            </button>
          <button 
            :class="['nav-item', { active: activeTab === 'robot' }]"
            @click="activeTab = 'robot'"
          >
            <span class="material-icons nav-icon">smart_toy</span>
            <span class="nav-label">Robot Management</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'ai' }]"
            @click="activeTab = 'ai'"
          >
            <span class="material-icons nav-icon">psychology</span>
            <span class="nav-label">AI Management</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'user' }]"
            @click="activeTab = 'user'"
          >
            <span class="material-icons nav-icon">people</span>
            <span class="nav-label">User Management</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'points' }]"
            @click="activeTab = 'points'"
          >
            <span class="material-icons nav-icon">monetization_on</span>
            <span class="nav-label">Point Packages</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'payments' }]"
            @click="activeTab = 'payments'"
          >
            <span class="material-icons nav-icon">payment</span>
            <span class="nav-label">Payment History</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'transactions' }]"
            @click="activeTab = 'transactions'"
          >
            <span class="material-icons nav-icon">account_balance_wallet</span>
            <span class="nav-label">Point Transactions</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'faq' }]"
            @click="activeTab = 'faq'"
          >
            <span class="material-icons nav-icon">help</span>
            <span class="nav-label">FAQ Management</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'notification' }]"
            @click="activeTab = 'notification'"
          >
            <span class="material-icons nav-icon">notifications</span>
            <span class="nav-label">Notifications</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'settings' }]"
            @click="activeTab = 'settings'"
          >
            <span class="material-icons nav-icon">settings</span>
            <span class="nav-label">Settings</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'logs' }]"
            @click="activeTab = 'logs'"
          >
            <span class="material-icons nav-icon">description</span>
            <span class="nav-label">System Logs</span>
          </button>
        </nav>
      </aside>

      <!-- Main Panel -->
      <main class="admin-main">
        <!-- Overview Dashboard -->
        <OverviewDashboard v-if="activeTab === 'overview'" />
        
        <!-- Robot Management -->
        <RobotManagement v-if="activeTab === 'robot'" />
        
        <!-- AI Management -->
        <AIManagement v-if="activeTab === 'ai'" />
        
        <!-- User Management -->
        <UserManagement v-if="activeTab === 'user'" />
        
        <!-- Point Package Management -->
        <PointPackageManagement v-if="activeTab === 'points'" />
        
        <!-- Payment History -->
        <PaymentManagement v-if="activeTab === 'payments'" />
        
        <!-- Point Transactions -->
        <PointManagement v-if="activeTab === 'transactions'" />
        
        <!-- FAQ Management -->
        <FAQManagement v-if="activeTab === 'faq'" />
        
        <!-- Notification Management -->
        <NotificationManagement v-if="activeTab === 'notification'" />
        
        <!-- Settings -->
        <div v-if="activeTab === 'settings'" class="panel">
          <h2 class="panel-title">System Settings</h2>
          <p class="panel-placeholder">Settings panel - Coming soon...</p>
        </div>
        
        <!-- Logs -->
        <div v-if="activeTab === 'logs'" class="panel">
          <h2 class="panel-title">System Logs</h2>
          <p class="panel-placeholder">Logs panel - Coming soon...</p>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../store/auth'
import OverviewDashboard from '../../components/admin/OverviewDashboard.vue'
import RobotManagement from '../../components/admin/RobotManagement.vue'
import AIManagement from '../../components/admin/AIManagement.vue'
import UserManagement from '../../components/admin/UserManagement.vue'
import FAQManagement from '../../components/admin/FAQManagement.vue'
import NotificationManagement from '../../components/admin/NotificationManagement.vue'
import PointPackageManagement from '../../components/admin/PointPackageManagement.vue'
import PaymentManagement from './PaymentManagement.vue'
import PointManagement from './PointManagement.vue'

const router = useRouter()
const authStore = useAuthStore()

const activeTab = ref<'overview' | 'robot' | 'ai' | 'user' | 'points' | 'payments' | 'transactions' | 'faq' | 'notification' | 'settings' | 'logs'>('overview')

const userName = computed(() => {
  return authStore.user.value?.fullName || authStore.user.value?.username || 'Admin'
})

const userInitial = computed(() => {
  const name = userName.value
  return name.charAt(0).toUpperCase()
})

const handleLogout = async () => {
  if (confirm('Are you sure you want to logout?')) {
    await authStore.logout()
    router.push('/login')
  }
}

onMounted(() => {
  // Verify user is still authenticated
  if (!authStore.isAuthenticated.value || !authStore.isAdmin.value) {
    router.push('/login')
  }
})
</script>

<style scoped>
@import '../../assets/styles/AdminScreen.css';
</style>
