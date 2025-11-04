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
        <!-- Robot Management -->
        <RobotManagement v-if="activeTab === 'robot'" />
        
        <!-- AI Management -->
        <AIManagement v-if="activeTab === 'ai'" />
        
        <!-- User Management -->
        <UserManagement v-if="activeTab === 'user'" />
        
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
import RobotManagement from '../../components/admin/RobotManagement.vue'
import AIManagement from '../../components/admin/AIManagement.vue'
import UserManagement from '../../components/admin/UserManagement.vue'

const activeTab = ref<'robot' | 'ai' | 'user' | 'settings' | 'logs'>('robot')
</script>

<style scoped>
@import '../../assets/styles/AdminScreen.css';
</style>
