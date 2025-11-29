<template>
  <div class="admin-screen">
    <!-- Header -->
    <header class="admin-header">
      <div class="header-content">
        <h1 class="admin-title">ADMIN DASHBOARD</h1>
        <div class="header-actions">
          <button class="btn-flat btn-danger">Logout</button>
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
            <span class="nav-icon">📊</span>
            <span class="nav-label">Overview</span>
            </button>
          <button 
            :class="['nav-item', { active: activeTab === 'robot' }]"
            @click="activeTab = 'robot'"
          >
            <span class="nav-icon">🤖</span>
            <span class="nav-label">Robot Management</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'ai' }]"
            @click="activeTab = 'ai'"
          >
            <span class="nav-icon">🧠</span>
            <span class="nav-label">AI Management</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'user' }]"
            @click="activeTab = 'user'"
          >
            <span class="nav-icon">👥</span>
            <span class="nav-label">User Management</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'faq' }]"
            @click="activeTab = 'faq'"
          >
            <span class="nav-icon">❓</span>
            <span class="nav-label">FAQ Management</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'notification' }]"
            @click="activeTab = 'notification'"
          >
            <span class="nav-icon">🔔</span>
            <span class="nav-label">Notifications</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'settings' }]"
            @click="activeTab = 'settings'"
          >
            <span class="nav-icon">⚙️</span>
            <span class="nav-label">Settings</span>
          </button>
          <button 
            :class="['nav-item', { active: activeTab === 'logs' }]"
            @click="activeTab = 'logs'"
          >
            <span class="nav-icon">📋</span>
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
import { ref } from 'vue'
import OverviewDashboard from '../../components/admin/OverviewDashboard.vue'
import RobotManagement from '../../components/admin/RobotManagement.vue'
import AIManagement from '../../components/admin/AIManagement.vue'
import UserManagement from '../../components/admin/UserManagement.vue'
import FAQManagement from '../../components/admin/FAQManagement.vue'
import NotificationManagement from '../../components/admin/NotificationManagement.vue'

const activeTab = ref<'overview' | 'robot' | 'ai' | 'user' | 'faq' | 'notification' | 'settings' | 'logs'>('overview')
</script>

<style scoped>
@import '../../assets/styles/AdminScreen.css';
</style>
