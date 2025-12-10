<template>
  <div class="payment-management">
    <div class="header">
      <h1>Payment Management</h1>
    </div>

    <!-- Statistics Cards -->
    <div class="statistics-grid">
      <div class="stat-card">
        <div class="stat-icon"><span class="material-icons">payments</span></div>
        <div class="stat-content">
          <div class="stat-label">Total Revenue</div>
          <div class="stat-value">{{ formatCurrency(statistics.totalRevenue) }}</div>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon"><span class="material-icons">trending_up</span></div>
        <div class="stat-content">
          <div class="stat-label">Today's Revenue</div>
          <div class="stat-value">{{ formatCurrency(statistics.todayRevenue) }}</div>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon"><span class="material-icons">calendar_today</span></div>
        <div class="stat-content">
          <div class="stat-label">This Month's Revenue</div>
          <div class="stat-value">{{ formatCurrency(statistics.thisMonthRevenue) }}</div>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon"><span class="material-icons">check_circle</span></div>
        <div class="stat-content">
          <div class="stat-label">Successful Transactions</div>
          <div class="stat-value">{{ statistics.successfulPayments }}/{{ statistics.totalPayments }}</div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="filters">
      <div class="filter-group">
        <label>From Date:</label>
        <input type="date" v-model="filters.startDate" @change="loadPayments" />
      </div>
      <div class="filter-group">
        <label>To Date:</label>
        <input type="date" v-model="filters.endDate" @change="loadPayments" />
      </div>
      <div class="filter-group">
        <label>Status:</label>
        <select v-model="filters.status" @change="loadPayments">
          <option value="">All</option>
          <option value="success">Success</option>
          <option value="pending">Pending</option>
          <option value="failed">Failed</option>
        </select>
      </div>
      <button class="btn-reset" @click="resetFilters">Reset</button>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading">
      <div class="spinner"></div>
      <p>Loading data...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="error-message">
      {{ error }}
    </div>

    <!-- Payments Table -->
    <div v-else class="table-container">
      <table class="payments-table">
        <thead>
          <tr>
            <th>Transaction ID</th>
            <th>User</th>
            <th>Points Package</th>
            <th>Amount</th>
            <th>Status</th>
            <th>Time</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="payment in payments" :key="payment.id">
            <td>
              <div class="transaction-id">{{ payment.orderCode || payment.transactionId }}</div>
            </td>
            <td>
              <div class="user-info">
                <div class="user-name">{{ payment.user?.username || 'N/A' }}</div>
                <div class="user-email">{{ payment.user?.email || 'N/A' }}</div>
              </div>
            </td>
            <td>
              <div v-if="payment.package" class="package-info">
                <div class="package-name">{{ payment.package.name }}</div>
                <div class="package-points">{{ payment.package.points }} points</div>
              </div>
              <span v-else>-</span>
            </td>
            <td class="amount">{{ formatCurrency(payment.amount) }}</td>
            <td>
              <span class="status-badge" :class="payment.status">
                {{ getStatusText(payment.status) }}
              </span>
            </td>
            <td class="datetime">{{ formatDateTime(payment.createdAt) }}</td>
          </tr>
          <tr v-if="payments.length === 0">
            <td colspan="6" class="no-data">No data available</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { paymentService, type PaymentHistory, type PaymentStatistics } from '../../services/paymentService'

const payments = ref<PaymentHistory[]>([])
const statistics = ref<PaymentStatistics>({
  totalPayments: 0,
  successfulPayments: 0,
  pendingPayments: 0,
  failedPayments: 0,
  totalRevenue: 0,
  todayRevenue: 0,
  thisMonthRevenue: 0
})

const filters = ref({
  startDate: '',
  endDate: '',
  status: ''
})

const loading = ref(false)
const error = ref('')

const loadPayments = async () => {
  loading.value = true
  error.value = ''
  
  try {
    const params = {
      startDate: filters.value.startDate || undefined,
      endDate: filters.value.endDate || undefined,
      status: filters.value.status || undefined
    }
    
    payments.value = await paymentService.getAllPayments(params)
  } catch (err: any) {
    error.value = err.message || 'Unable to load payment data'
    console.error('Error loading payments:', err)
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
    
    statistics.value = await paymentService.getPaymentStatistics(params)
  } catch (err: any) {
    console.error('Error loading statistics:', err)
  }
}

const resetFilters = () => {
  filters.value = {
    startDate: '',
    endDate: '',
    status: ''
  }
  loadPayments()
  loadStatistics()
}

const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount)
}

const formatDateTime = (dateString?: string): string => {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getStatusText = (status?: string): string => {
  const statusMap: Record<string, string> = {
    'success': 'Success',
    'pending': 'Pending',
    'failed': 'Failed'
  }
  return statusMap[status || ''] || status || '-'
}

onMounted(() => {
  loadPayments()
  loadStatistics()
})
</script>

<style scoped>
.payment-management {
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
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
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

.payments-table {
  width: 100%;
  border-collapse: collapse;
}

.payments-table thead {
  background: #f9fafb;
}

.payments-table th {
  padding: 16px;
  text-align: left;
  font-weight: 600;
  font-size: 14px;
  color: #374151;
  border-bottom: 2px solid #e5e7eb;
}

.payments-table td {
  padding: 16px;
  border-bottom: 1px solid #f3f4f6;
  font-size: 14px;
}

.payments-table tbody tr:hover {
  background: #f9fafb;
}

.transaction-id {
  font-family: monospace;
  font-size: 13px;
  color: #6b7280;
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

.package-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.package-name {
  font-weight: 500;
}

.package-points {
  font-size: 12px;
  color: #6b7280;
}

.amount {
  font-weight: 600;
  color: #059669;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
  display: inline-block;
}

.status-badge.success {
  background: #d1fae5;
  color: #065f46;
}

.status-badge.pending {
  background: #fef3c7;
  color: #92400e;
}

.status-badge.failed {
  background: #fee2e2;
  color: #991b1b;
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
