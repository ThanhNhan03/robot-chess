<template>
  <div class="point-management">
    <div class="header">
      <h1>Points Management</h1>
    </div>

    <!-- Statistics -->
    <div class="statistics-grid">
      <div v-for="(count, type) in statistics" :key="type" class="stat-card">
        <div class="stat-icon"><span class="material-icons">{{ getTypeIcon(type) }}</span></div>
        <div class="stat-content">
          <div class="stat-label">{{ getTypeLabel(type) }}</div>
          <div class="stat-value">{{ count }}</div>
        </div>
      </div>
    </div>

    <!-- Manual Adjustment Form -->
    <div class="adjustment-section">
      <h2>Manual Points Adjustment</h2>
      <form @submit.prevent="handleAdjustPoints" class="adjustment-form">
        <div class="form-group">
          <label>Search User *</label>
          <input 
            type="text" 
            v-model="userSearch" 
            @input="searchUsers"
            placeholder="Search by email or username..."
            autocomplete="off"
          />
          <div v-if="searchResults && searchResults.length > 0" class="search-results">
            <div 
              v-for="user in searchResults" 
              :key="user.id"
              @click="selectUser(user)"
              class="search-result-item"
            >
              <div class="user-info">
                <strong>{{ user.username }}</strong>
                <span class="email">{{ user.email }}</span>
              </div>
              <span class="points-badge">{{ user.pointsBalance || 0 }} points</span>
            </div>
          </div>
          <div v-if="selectedUser" class="selected-user">
            ✓ Selected: <strong>{{ selectedUser.username }}</strong> ({{ selectedUser.email }})
            <button type="button" @click="clearSelectedUser" class="btn-clear">×</button>
          </div>
        </div>
        
        <div class="form-group">
          <label>Points Amount *</label>
          <input 
            type="number" 
            v-model.number="adjustForm.amount" 
            placeholder="Positive = add, negative = subtract"
            required
          />
          <small>Enter a positive number to add points, negative to subtract</small>
        </div>
        
        <div class="form-group">
          <label>Reason *</label>
          <textarea 
            v-model="adjustForm.reason" 
            placeholder="Enter adjustment reason..."
            rows="3"
            required
          ></textarea>
        </div>
        
        <div class="form-actions">
          <button type="submit" class="btn-submit" :disabled="adjusting">
            <span v-if="adjusting">Processing...</span>
            <span v-else>Adjust Points</span>
          </button>
        </div>
      </form>
      
      <div v-if="adjustResult" class="result-message" :class="adjustResult.type">
        {{ adjustResult.message }}
      </div>
    </div>

    <!-- Filters -->
    <div class="filters">
      <div class="filter-group">
        <label>From Date:</label>
        <input type="date" v-model="filters.startDate" @change="loadTransactions" />
      </div>
      <div class="filter-group">
        <label>To Date:</label>
        <input type="date" v-model="filters.endDate" @change="loadTransactions" />
      </div>
      <div class="filter-group">
        <label>Transaction Type:</label>
        <select v-model="filters.transactionType" @change="loadTransactions">
          <option value="">All</option>
          <option value="deposit">Deposit</option>
          <option value="service_usage">Service Usage</option>
          <option value="adjustment">Adjustment</option>
        </select>
      </div>
      <button class="btn-reset" @click="resetFilters">Reset</button>
    </div>

    <!-- Loading/Error States -->
    <div v-if="loading" class="loading">
      <div class="spinner"></div>
      <p>Loading data...</p>
    </div>

    <div v-else-if="error" class="error-message">
      {{ error }}
    </div>

    <!-- Transactions Table -->
    <div v-else class="table-container">
      <table class="transactions-table">
        <thead>
          <tr>
            <th>Time</th>
            <th>User</th>
            <th>Transaction Type</th>
            <th>Points</th>
            <th>Description</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="transaction in transactions" :key="transaction.id">
            <td class="datetime">{{ formatDateTime(transaction.createdAt) }}</td>
            <td>
              <div class="user-info">
                <div class="user-name">{{ transaction.user?.username || 'N/A' }}</div>
                <div class="user-email">{{ transaction.user?.email || 'N/A' }}</div>
              </div>
            </td>
            <td>
              <span class="type-badge" :class="transaction.transactionType">
                {{ getTypeLabel(transaction.transactionType) }}
              </span>
            </td>
            <td :class="['amount', transaction.amount > 0 ? 'positive' : 'negative']">
              {{ transaction.amount > 0 ? '+' : '' }}{{ transaction.amount }}
            </td>
            <td class="description">{{ transaction.description || '-' }}</td>
          </tr>
          <tr v-if="transactions.length === 0">
            <td colspan="5" class="no-data">No data available</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { pointTransactionService, type PointTransaction, type AdjustPointsRequest } from '../../services/pointTransactionService'
import { getAllUsers, type User } from '../../services/userService'

const transactions = ref<PointTransaction[]>([])
const statistics = ref<Record<string, number>>({})

const filters = ref({
  startDate: '',
  endDate: '',
  transactionType: ''
})

const adjustForm = ref<AdjustPointsRequest>({
  userId: '',
  amount: 0,
  reason: ''
})

const userSearch = ref('')
const searchResults = ref<User[]>([])
const selectedUser = ref<User | null>(null)
const allUsers = ref<User[]>([])

const loading = ref(false)
const error = ref('')
const adjusting = ref(false)
const adjustResult = ref<{ type: 'success' | 'error', message: string } | null>(null)

const searchUsers = () => {
  if (!userSearch.value || userSearch.value.length < 2) {
    searchResults.value = []
    return
  }

  const query = userSearch.value.toLowerCase()
  searchResults.value = allUsers.value
    .filter(user => 
      user.email.toLowerCase().includes(query) || 
      user.username.toLowerCase().includes(query)
    )
    .slice(0, 5)
}

const selectUser = (user: User) => {
  selectedUser.value = user
  adjustForm.value.userId = user.id
  userSearch.value = user.username
  searchResults.value = []
}

const clearSelectedUser = () => {
  selectedUser.value = null
  adjustForm.value.userId = ''
  userSearch.value = ''
  searchResults.value = []
}

const loadTransactions = async () => {
  loading.value = true
  error.value = ''
  
  try {
    const params = {
      startDate: filters.value.startDate || undefined,
      endDate: filters.value.endDate || undefined,
      transactionType: filters.value.transactionType || undefined
    }
    
    transactions.value = await pointTransactionService.getAllTransactions(params)
  } catch (err: any) {
    error.value = err.message || 'Unable to load transaction data'
    console.error('Error loading transactions:', err)
  } finally {
    loading.value = false
  }
}

const loadStatistics = async () => {
  try {
    const params = {
      startDate: filters.value.startDate || undefined,
      endDate: filters.value.endDate || undefined
    }
    
    statistics.value = await pointTransactionService.getStatistics(params)
  } catch (err: any) {
    console.error('Error loading statistics:', err)
  }
}

const handleAdjustPoints = async () => {
  if (!selectedUser.value) {
    adjustResult.value = {
      type: 'error',
      message: 'Please select a user from the search list'
    }
    return
  }
  
  if (!adjustForm.value.reason || adjustForm.value.reason.trim() === '') {
    adjustResult.value = {
      type: 'error',
      message: 'Please enter adjustment reason'
    }
    return
  }
  
  adjusting.value = true
  adjustResult.value = null
  
  try {
    const result = await pointTransactionService.adjustPoints(adjustForm.value)
    
    adjustResult.value = {
      type: 'success',
      message: `Adjustment successful! New balance: ${result.newBalance} points`
    }
    
    // Reset form
    adjustForm.value = {
      userId: '',
      amount: 0,
      reason: ''
    }
    selectedUser.value = null
    userSearch.value = ''
    
    // Reload data
    await loadTransactions()
    await loadStatistics()
    
    // Clear success message after 5 seconds
    setTimeout(() => {
      adjustResult.value = null
    }, 5000)
    
  } catch (err: any) {
    adjustResult.value = {
      type: 'error',
      message: err.message || 'Unable to adjust points'
    }
  } finally {
    adjusting.value = false
  }
}

const resetFilters = () => {
  filters.value = {
    startDate: '',
    endDate: '',
    transactionType: ''
  }
  loadTransactions()
  loadStatistics()
}

const formatDateTime = (dateString: string): string => {
  return new Date(dateString).toLocaleString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getTypeLabel = (type: string): string => {
  const typeMap: Record<string, string> = {
    'deposit': 'Deposit',
    'service_usage': 'Service Usage',
    'adjustment': 'Adjustment'
  }
  return typeMap[type] || type
}

const getTypeIcon = (type: string): string => {
  const iconMap: Record<string, string> = {
    'deposit': 'account_balance_wallet',
    'service_usage': 'shopping_cart',
    'adjustment': 'tune'
  }
  return iconMap[type] || 'analytics'
}

onMounted(async () => {
  // Load users for search
  try {
    allUsers.value = await getAllUsers()
  } catch (err) {
    console.error('Error loading users:', err)
  }
  
  loadTransactions()
  loadStatistics()
})
</script>

<style scoped>
.point-management {
  padding: 24px;
  background: #f5f5f5;
  min-height: 100vh;
}

.header {
  margin-bottom: 24px;
}

.header h1 {
  font-size: 28px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}

.statistics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  font-size: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-icon .material-icons {
  font-size: 40px;
  color: #667eea;
}

.stat-content {
  flex: 1;
}

.stat-label {
  font-size: 14px;
  color: #666;
  margin-bottom: 4px;
}

.stat-value {
  font-size: 24px;
  font-weight: 600;
  color: #1a1a1a;
}

.adjustment-section {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.adjustment-section h2 {
  font-size: 20px;
  font-weight: 600;
  margin: 0 0 20px 0;
  color: #1a1a1a;
}

.adjustment-form {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 16px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  position: relative;
}

.form-group label {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.form-group input,
.form-group textarea,
.form-group select {
  padding: 10px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 14px;
  font-family: inherit;
}

.search-results {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: white;
  border: 1px solid #ddd;
  border-radius: 6px;
  margin-top: 4px;
  max-height: 200px;
  overflow-y: auto;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 1000;
}

.search-result-item {
  padding: 12px;
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #f0f0f0;
}

.search-result-item:hover {
  background: #f8f9fa;
}

.search-result-item:last-child {
  border-bottom: none;
}

.search-result-item .user-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.search-result-item .user-info strong {
  color: #1a1a1a;
  font-size: 14px;
}

.search-result-item .user-info .email {
  color: #666;
  font-size: 12px;
}

.search-result-item .points-badge {
  background: #e0f2fe;
  color: #0369a1;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
}

.selected-user {
  background: #ecfdf5;
  border: 1px solid #10b981;
  padding: 10px 12px;
  border-radius: 6px;
  font-size: 14px;
  color: #047857;
  display: flex;
  align-items: center;
  gap: 8px;
}

.selected-user strong {
  color: #065f46;
}

.btn-clear {
  margin-left: auto;
  background: none;
  border: none;
  color: #059669;
  font-size: 20px;
  cursor: pointer;
  padding: 0 8px;
  line-height: 1;
}

.btn-clear:hover {
  color: #047857;
}

.form-group small {
  font-size: 12px;
  color: #6b7280;
}

.form-actions {
  display: flex;
  align-items: end;
}

.btn-submit {
  padding: 10px 24px;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: background 0.2s;
}

.btn-submit:hover:not(:disabled) {
  background: #2563eb;
}

.btn-submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.result-message {
  margin-top: 16px;
  padding: 12px;
  border-radius: 6px;
  font-size: 14px;
}

.result-message.success {
  background: #d1fae5;
  color: #065f46;
}

.result-message.error {
  background: #fee2e2;
  color: #991b1b;
}

.filters {
  background: white;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 24px;
  display: flex;
  gap: 16px;
  align-items: end;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.filter-group label {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.filter-group input,
.filter-group select {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 14px;
}

.btn-reset {
  padding: 8px 16px;
  background: #6b7280;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: background 0.2s;
}

.btn-reset:hover {
  background: #4b5563;
}

.loading {
  text-align: center;
  padding: 60px 20px;
  background: white;
  border-radius: 12px;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-message {
  background: #fee;
  color: #c33;
  padding: 16px;
  border-radius: 8px;
  text-align: center;
}

.table-container {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.transactions-table {
  width: 100%;
  border-collapse: collapse;
}

.transactions-table thead {
  background: #f9fafb;
}

.transactions-table th {
  padding: 16px;
  text-align: left;
  font-weight: 600;
  font-size: 14px;
  color: #374151;
  border-bottom: 2px solid #e5e7eb;
}

.transactions-table td {
  padding: 16px;
  border-bottom: 1px solid #f3f4f6;
  font-size: 14px;
}

.transactions-table tbody tr:hover {
  background: #f9fafb;
}

.user-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.user-name {
  font-weight: 500;
  color: #1a1a1a;
}

.user-email {
  font-size: 12px;
  color: #6b7280;
}

.type-badge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
  display: inline-block;
}

.type-badge.deposit {
  background: #d1fae5;
  color: #065f46;
}

.type-badge.service_usage {
  background: #fef3c7;
  color: #92400e;
}

.type-badge.adjustment {
  background: #dbeafe;
  color: #1e40af;
}

.amount {
  font-weight: 600;
  font-size: 16px;
}

.amount.positive {
  color: #059669;
}

.amount.negative {
  color: #dc2626;
}

.description {
  color: #6b7280;
  font-size: 13px;
}

.datetime {
  color: #6b7280;
  font-size: 13px;
}

.no-data {
  text-align: center;
  padding: 40px;
  color: #9ca3af;
}
</style>
