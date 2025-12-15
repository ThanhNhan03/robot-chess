<template>
  <div class="feedback-management">
    <div class="panel-header">
      <h2 class="panel-title">
        <MessageSquare :size="24" />
        User Feedback Management
      </h2>
      <div style="display: flex; gap: 12px;">
        <button 
          class="btn-flat btn-success" 
          @click="loadFeedbacks"
          :disabled="isLoading"
        >
          <RefreshCw :size="18" :class="{ 'spin': isLoading }" /> 
          {{ isLoading ? 'Refreshing...' : 'Refresh' }}
        </button>
      </div>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-primary">
        <div class="stat-icon"><MessageCircle :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ feedbacks.length }}</div>
          <div class="stat-label">Total Feedbacks</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon"><Users :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ uniqueUsers }}</div>
          <div class="stat-label">Unique Users</div>
        </div>
      </div>
      <div class="stat-card stat-success">
        <div class="stat-icon"><Calendar :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ feedbacksToday }}</div>
          <div class="stat-label">Today's Feedbacks</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon"><Clock :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ feedbacksThisWeek }}</div>
          <div class="stat-label">This Week</div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="filters-section">
      <div class="filter-group">
        <label>Search:</label>
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Search by user or message..."
          class="filter-input"
        />
      </div>
      <div class="filter-group">
        <label>Date From:</label>
        <input 
          type="date" 
          v-model="dateFrom" 
          class="filter-input"
        />
      </div>
      <div class="filter-group">
        <label>Date To:</label>
        <input 
          type="date" 
          v-model="dateTo" 
          class="filter-input"
        />
      </div>
      <button class="btn-flat btn-secondary" @click="resetFilters">
        <X :size="18" /> Clear Filters
      </button>
    </div>

    <!-- Loading/Error States -->
    <div v-if="isLoading" class="loading-message">
      <div class="spinner"></div>
      Loading feedbacks...
    </div>
    <div v-else-if="error" class="error-message">
      <AlertTriangle :size="20" />
      {{ error }}
    </div>

    <!-- Feedbacks List -->
    <div v-else class="feedbacks-section">
      <div v-if="filteredFeedbacks.length === 0" class="empty-message">
        <MessageSquare :size="48" />
        <p>No feedbacks found</p>
      </div>
      <div v-else class="feedbacks-grid">
        <div 
          v-for="feedback in paginatedFeedbacks" 
          :key="feedback.id" 
          class="feedback-card"
        >
          <div class="feedback-header">
            <div class="user-info">
              <div class="user-avatar">
                {{ getUserInitial(feedback.userFullName || feedback.userEmail || 'U') }}
              </div>
              <div class="user-details">
                <div class="user-name">{{ feedback.userFullName || 'Anonymous' }}</div>
                <div class="user-email">{{ feedback.userEmail || 'N/A' }}</div>
              </div>
            </div>
            <div class="feedback-meta">
              <span class="feedback-date">
                <Calendar :size="14" />
                {{ formatDate(feedback.createdAt) }}
              </span>
            </div>
          </div>
          
          <div class="feedback-body">
            <div class="feedback-message">
              {{ feedback.message }}
            </div>
          </div>
          
          <div class="feedback-footer">
            <div class="feedback-id">ID: {{ feedback.id.substring(0, 8) }}...</div>
            <div class="feedback-actions">
              <button 
                class="btn-flat btn-sm btn-primary" 
                @click="viewFeedback(feedback)"
                title="View Details"
              >
                <Eye :size="14" /> View
              </button>
              <button 
                class="btn-flat btn-sm btn-danger" 
                @click="deleteFeedback(feedback.id)"
                title="Delete Feedback"
              >
                <Trash2 :size="14" /> Delete
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="filteredFeedbacks.length > pageSize" class="pagination">
        <button 
          class="btn-flat btn-sm" 
          @click="currentPage--" 
          :disabled="currentPage === 1"
        >
          <ChevronLeft :size="18" /> Previous
        </button>
        <span class="pagination-info">
          Page {{ currentPage }} of {{ totalPages }}
        </span>
        <button 
          class="btn-flat btn-sm" 
          @click="currentPage++" 
          :disabled="currentPage === totalPages"
        >
          Next <ChevronRight :size="18" />
        </button>
      </div>
    </div>

    <!-- View Feedback Modal -->
    <div v-if="selectedFeedback" class="modal-overlay" @click="closeFeedbackModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>Feedback Details</h3>
          <button class="btn-close" @click="closeFeedbackModal">
            <X :size="20" />
          </button>
        </div>
        <div class="modal-body">
          <div class="detail-row">
            <strong>User:</strong>
            <span>{{ selectedFeedback.userFullName || 'Anonymous' }}</span>
          </div>
          <div class="detail-row">
            <strong>Email:</strong>
            <span>{{ selectedFeedback.userEmail || 'N/A' }}</span>
          </div>
          <div class="detail-row">
            <strong>Date:</strong>
            <span>{{ formatDateTime(selectedFeedback.createdAt) }}</span>
          </div>
          <div class="detail-row">
            <strong>Feedback ID:</strong>
            <span class="feedback-id-full">{{ selectedFeedback.id }}</span>
          </div>
          <div class="detail-row full-width">
            <strong>Message:</strong>
            <div class="feedback-message-full">{{ selectedFeedback.message }}</div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn-flat btn-danger" @click="deleteFeedback(selectedFeedback.id)">
            <Trash2 :size="18" /> Delete Feedback
          </button>
          <button class="btn-flat btn-secondary" @click="closeFeedbackModal">
            Close
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { 
  MessageSquare, MessageCircle, Users, Calendar, Clock,
  RefreshCw, Eye, Trash2, AlertTriangle, X, 
  ChevronLeft, ChevronRight
} from 'lucide-vue-next'
import { feedbackService, type Feedback } from '../../services/feedbackService'

// State
const feedbacks = ref<Feedback[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)
const selectedFeedback = ref<Feedback | null>(null)

// Filters
const searchQuery = ref('')
const dateFrom = ref('')
const dateTo = ref('')

// Pagination
const currentPage = ref(1)
const pageSize = 10

// Load feedbacks
const loadFeedbacks = async () => {
  try {
    isLoading.value = true
    error.value = null
    feedbacks.value = await feedbackService.getAllFeedbacks()
  } catch (err: any) {
    error.value = err.response?.data?.Message || 'Failed to load feedbacks'
    console.error('Error loading feedbacks:', err)
  } finally {
    isLoading.value = false
  }
}

// Computed statistics
const uniqueUsers = computed(() => {
  const userIds = new Set(feedbacks.value.map(f => f.userId).filter(Boolean))
  return userIds.size
})

const feedbacksToday = computed(() => {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  return feedbacks.value.filter(f => {
    const feedbackDate = new Date(f.createdAt || '')
    feedbackDate.setHours(0, 0, 0, 0)
    return feedbackDate.getTime() === today.getTime()
  }).length
})

const feedbacksThisWeek = computed(() => {
  const today = new Date()
  const weekAgo = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000)
  return feedbacks.value.filter(f => {
    const feedbackDate = new Date(f.createdAt || '')
    return feedbackDate >= weekAgo
  }).length
})

// Filtered feedbacks
const filteredFeedbacks = computed(() => {
  let result = feedbacks.value

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(f => 
      f.userFullName?.toLowerCase().includes(query) ||
      f.userEmail?.toLowerCase().includes(query) ||
      f.message?.toLowerCase().includes(query)
    )
  }

  // Date from filter
  if (dateFrom.value) {
    const fromDate = new Date(dateFrom.value)
    result = result.filter(f => new Date(f.createdAt || '') >= fromDate)
  }

  // Date to filter
  if (dateTo.value) {
    const toDate = new Date(dateTo.value)
    toDate.setHours(23, 59, 59, 999)
    result = result.filter(f => new Date(f.createdAt || '') <= toDate)
  }

  // Sort by date (newest first)
  return result.sort((a, b) => {
    const dateA = new Date(a.createdAt || 0).getTime()
    const dateB = new Date(b.createdAt || 0).getTime()
    return dateB - dateA
  })
})

// Pagination
const totalPages = computed(() => Math.ceil(filteredFeedbacks.value.length / pageSize))

const paginatedFeedbacks = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  const end = start + pageSize
  return filteredFeedbacks.value.slice(start, end)
})

// Actions
const viewFeedback = (feedback: Feedback) => {
  selectedFeedback.value = feedback
}

const closeFeedbackModal = () => {
  selectedFeedback.value = null
}

const deleteFeedback = async (id: string) => {
  if (!confirm('Are you sure you want to delete this feedback?')) {
    return
  }

  try {
    await feedbackService.deleteFeedback(id)
    feedbacks.value = feedbacks.value.filter(f => f.id !== id)
    
    if (selectedFeedback.value?.id === id) {
      selectedFeedback.value = null
    }
    
    alert('Feedback deleted successfully')
  } catch (err: any) {
    alert(err.response?.data?.Message || 'Failed to delete feedback')
    console.error('Error deleting feedback:', err)
  }
}

const resetFilters = () => {
  searchQuery.value = ''
  dateFrom.value = ''
  dateTo.value = ''
  currentPage.value = 1
}

// Helpers
const formatDate = (date?: string) => {
  if (!date) return '-'
  return new Date(date).toLocaleDateString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}

const formatDateTime = (date?: string) => {
  if (!date) return '-'
  return new Date(date).toLocaleString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getUserInitial = (name: string) => {
  return name.charAt(0).toUpperCase()
}

// Lifecycle
onMounted(() => {
  loadFeedbacks()
})
</script>

<style scoped>
.feedback-management {
  padding: 24px;
  background: #f5f5f5;
  min-height: 100vh;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  background: white;
  padding: 20px 24px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.panel-title {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 24px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}

/* Statistics */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
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
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 12px;
  border-radius: 12px;
}

.stat-primary .stat-icon { background: #dbeafe; color: #1e40af; }
.stat-success .stat-icon { background: #d1fae5; color: #065f46; }
.stat-info .stat-icon { background: #e0f2fe; color: #0369a1; }
.stat-warning .stat-icon { background: #fef3c7; color: #92400e; }

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 28px;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: #6b7280;
}

/* Filters */
.filters-section {
  background: white;
  padding: 20px;
  border-radius: 12px;
  margin-bottom: 24px;
  display: flex;
  gap: 16px;
  align-items: end;
  flex-wrap: wrap;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex: 1;
  min-width: 200px;
}

.filter-group label {
  font-size: 14px;
  font-weight: 500;
  color: #374151;
}

.filter-input {
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
}

.filter-input:focus {
  border-color: #3b82f6;
}

/* Loading/Error */
.loading-message {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-message {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px 20px;
  background: #fee;
  color: #c33;
  border-radius: 8px;
  font-weight: 500;
}

.empty-message {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  background: white;
  border-radius: 12px;
  color: #9ca3af;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.empty-message p {
  margin-top: 16px;
  font-size: 16px;
}

/* Feedbacks Grid */
.feedbacks-section {
  background: white;
  padding: 24px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.feedbacks-grid {
  display: grid;
  gap: 16px;
}

.feedback-card {
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 16px;
  transition: box-shadow 0.2s, transform 0.2s;
}

.feedback-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.feedback-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  padding-bottom: 12px;
  border-bottom: 1px solid #f3f4f6;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 16px;
}

.user-details {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.user-name {
  font-weight: 600;
  color: #1a1a1a;
  font-size: 14px;
}

.user-email {
  font-size: 12px;
  color: #6b7280;
}

.feedback-meta {
  display: flex;
  align-items: center;
  gap: 8px;
}

.feedback-date {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: #6b7280;
}

.feedback-body {
  margin-bottom: 12px;
}

.feedback-message {
  color: #374151;
  font-size: 14px;
  line-height: 1.6;
  white-space: pre-wrap;
}

.feedback-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 12px;
  border-top: 1px solid #f3f4f6;
}

.feedback-id {
  font-size: 12px;
  color: #9ca3af;
  font-family: 'Courier New', monospace;
}

.feedback-actions {
  display: flex;
  gap: 8px;
}

/* Pagination */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 16px;
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid #e5e7eb;
}

.pagination-info {
  font-size: 14px;
  color: #6b7280;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}

.modal-content {
  background: white;
  border-radius: 12px;
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h3 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #1a1a1a;
}

.btn-close {
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px;
  color: #6b7280;
  transition: color 0.2s;
}

.btn-close:hover {
  color: #1a1a1a;
}

.modal-body {
  padding: 24px;
}

.detail-row {
  display: grid;
  grid-template-columns: 120px 1fr;
  gap: 12px;
  margin-bottom: 16px;
  font-size: 14px;
}

.detail-row.full-width {
  grid-template-columns: 1fr;
}

.detail-row strong {
  color: #6b7280;
  font-weight: 500;
}

.feedback-id-full {
  font-family: 'Courier New', monospace;
  font-size: 12px;
  color: #374151;
  word-break: break-all;
}

.feedback-message-full {
  margin-top: 8px;
  padding: 12px;
  background: #f9fafb;
  border-radius: 6px;
  color: #374151;
  line-height: 1.6;
  white-space: pre-wrap;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 20px 24px;
  border-top: 1px solid #e5e7eb;
}

/* Buttons */
.btn-flat {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-flat:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary { background: #3b82f6; color: white; }
.btn-primary:hover:not(:disabled) { background: #2563eb; }

.btn-success { background: #10b981; color: white; }
.btn-success:hover:not(:disabled) { background: #059669; }

.btn-danger { background: #ef4444; color: white; }
.btn-danger:hover:not(:disabled) { background: #dc2626; }

.btn-secondary { background: #6b7280; color: white; }
.btn-secondary:hover:not(:disabled) { background: #4b5563; }

.btn-sm {
  padding: 6px 12px;
  font-size: 13px;
}

.spin {
  animation: spin 1s linear infinite;
}
</style>
