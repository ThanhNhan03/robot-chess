<template>
  <div class="notification-management">
    <div class="header">
      <h1>📢 Notification Management</h1>
      <p>Send notifications to players via email and in-app alerts</p>
    </div>

    <!-- Create Notification Form -->
    <div class="notification-form-card">
      <h2>Create New Notification</h2>
      
      <form @submit.prevent="handleSendNotification">
        <div class="form-group">
          <label for="title">Title *</label>
          <input
            id="title"
            v-model="form.title"
            type="text"
            placeholder="Enter notification title"
            required
          />
        </div>

        <div class="form-group">
          <label for="message">Message *</label>
          <textarea
            id="message"
            v-model="form.message"
            rows="5"
            placeholder="Enter notification message"
            required
          ></textarea>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label for="type">Type *</label>
            <select id="type" v-model="form.type" required>
              <option value="info">ℹ️ Info</option>
              <option value="warning">⚠️ Warning</option>
              <option value="maintenance">🔧 Maintenance</option>
              <option value="success">✅ Success</option>
              <option value="error">❌ Error</option>
            </select>
          </div>

          <div class="form-group">
            <label>
              <input type="checkbox" v-model="form.sendEmail" />
              Send Email to Users
            </label>
          </div>
        </div>

        <div class="form-group">
          <label>Recipients</label>
          <div class="recipient-options">
            <label>
              <input type="radio" v-model="recipientType" value="all" />
              All Active Players
            </label>
            <label>
              <input type="radio" v-model="recipientType" value="specific" />
              Specific Users (Enter User IDs)
            </label>
          </div>
          
          <textarea
            v-if="recipientType === 'specific'"
            v-model="userIdsText"
            rows="3"
            placeholder="Enter user IDs separated by commas or new lines"
            class="user-ids-input"
          ></textarea>
        </div>

        <div class="form-actions">
          <button type="button" class="btn-secondary" @click="handleTestNotification">
            🧪 Send Test
          </button>
          <button type="submit" class="btn-primary" :disabled="loading">
            {{ loading ? 'Sending...' : '📤 Send Notification' }}
          </button>
        </div>
      </form>
    </div>

    <!-- Notification History (from localStorage) -->
    <div class="notification-history-card">
      <div class="history-header">
        <h2>Recent Notifications</h2>
        <button class="btn-danger" @click="clearHistory">
          🗑️ Clear History
        </button>
      </div>

      <div v-if="notificationHistory.length === 0" class="empty-state">
        <p>No notifications sent yet</p>
      </div>

      <div v-else class="notification-list">
        <div
          v-for="notification in notificationHistory"
          :key="notification.id"
          class="notification-item"
          :class="`type-${notification.type}`"
        >
          <div class="notification-header">
            <span class="notification-type">{{ getTypeIcon(notification.type) }} {{ notification.type }}</span>
            <span class="notification-date">{{ formatDate(notification.createdAt) }}</span>
          </div>
          <h3>{{ notification.title }}</h3>
          <p>{{ notification.message }}</p>
          <div class="notification-stats">
            <span>👥 {{ notification.recipientCount }} recipients</span>
            <span v-if="notification.emailSent">📧 {{ notification.stats?.successCount || 0 }} emails sent</span>
            <span v-if="notification.stats?.failedCount > 0" class="failed">
              ⚠️ {{ notification.stats.failedCount }} failed
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Success/Error Messages -->
    <div v-if="successMessage" class="alert alert-success">
      {{ successMessage }}
    </div>
    <div v-if="errorMessage" class="alert alert-error">
      {{ errorMessage }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import notificationService, { type CreateNotificationRequest } from '../../services/notificationService';

const form = ref<CreateNotificationRequest>({
  title: '',
  message: '',
  type: 'info',
  sendEmail: true
});

const recipientType = ref<'all' | 'specific'>('all');
const userIdsText = ref('');
const loading = ref(false);
const successMessage = ref('');
const errorMessage = ref('');
const notificationHistory = ref<any[]>([]);

const STORAGE_KEY = 'admin_notification_history';

onMounted(() => {
  loadNotificationHistory();
});

const loadNotificationHistory = () => {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      notificationHistory.value = JSON.parse(stored);
    }
  } catch (error) {
    console.error('Error loading notification history:', error);
  }
};

const saveNotificationToHistory = (notification: any, stats: any) => {
  try {
    const historyItem = {
      ...notification,
      stats
    };
    notificationHistory.value.unshift(historyItem);
    
    // Keep only last 50 notifications
    if (notificationHistory.value.length > 50) {
      notificationHistory.value = notificationHistory.value.slice(0, 50);
    }
    
    localStorage.setItem(STORAGE_KEY, JSON.stringify(notificationHistory.value));
  } catch (error) {
    console.error('Error saving notification history:', error);
  }
};

const handleSendNotification = async () => {
  try {
    loading.value = true;
    errorMessage.value = '';
    successMessage.value = '';

    const data: CreateNotificationRequest = {
      title: form.value.title,
      message: form.value.message,
      type: form.value.type,
      sendEmail: form.value.sendEmail
    };

    // Parse user IDs if specific recipients selected
    if (recipientType.value === 'specific' && userIdsText.value.trim()) {
      const ids = userIdsText.value
        .split(/[\n,]+/)
        .map(id => id.trim())
        .filter(id => id.length > 0);
      
      if (ids.length === 0) {
        errorMessage.value = 'Please enter at least one user ID';
        return;
      }
      
      data.userIds = ids;
    }

    const response = await notificationService.sendNotification(data);
    
    // Save to localStorage
    saveNotificationToHistory(response.notification, response.stats);
    
    successMessage.value = `✅ Notification sent successfully! ${response.stats.successCount} emails sent to ${response.notification.recipientCount} users.`;
    
    if (response.stats.failedCount > 0) {
      successMessage.value += ` ⚠️ ${response.stats.failedCount} emails failed.`;
    }

    // Reset form
    form.value = {
      title: '',
      message: '',
      type: 'info',
      sendEmail: true
    };
    userIdsText.value = '';
    recipientType.value = 'all';

    setTimeout(() => {
      successMessage.value = '';
    }, 5000);
  } catch (error: any) {
    errorMessage.value = error.response?.data?.message || 'Failed to send notification';
    setTimeout(() => {
      errorMessage.value = '';
    }, 5000);
  } finally {
    loading.value = false;
  }
};

const handleTestNotification = async () => {
  try {
    loading.value = true;
    errorMessage.value = '';
    successMessage.value = '';

    const response = await notificationService.sendTestNotification(
      undefined,
      form.value.title || 'Test Notification',
      form.value.message || 'This is a test notification',
      form.value.type
    );

    successMessage.value = '✅ Test notification sent successfully!';
    saveNotificationToHistory(response.notification, response.stats);

    setTimeout(() => {
      successMessage.value = '';
    }, 5000);
  } catch (error: any) {
    errorMessage.value = error.response?.data?.message || 'Failed to send test notification';
    setTimeout(() => {
      errorMessage.value = '';
    }, 5000);
  } finally {
    loading.value = false;
  }
};

const clearHistory = () => {
  if (confirm('Are you sure you want to clear notification history?')) {
    localStorage.removeItem(STORAGE_KEY);
    notificationHistory.value = [];
  }
};

const getTypeIcon = (type: string) => {
  const icons: Record<string, string> = {
    info: 'ℹ️',
    warning: '⚠️',
    maintenance: '🔧',
    success: '✅',
    error: '❌'
  };
  return icons[type] || 'ℹ️';
};

const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  return date.toLocaleString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
};
</script>

<style scoped>
.notification-management {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.header {
  margin-bottom: 30px;
}

.header h1 {
  font-size: 32px;
  margin-bottom: 10px;
  color: #333;
}

.header p {
  color: #666;
  font-size: 16px;
}

.notification-form-card,
.notification-history-card {
  background: white;
  border-radius: 12px;
  padding: 30px;
  margin-bottom: 30px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.notification-form-card h2,
.notification-history-card h2 {
  font-size: 24px;
  margin-bottom: 20px;
  color: #333;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #555;
}

.form-group input[type="text"],
.form-group textarea,
.form-group select {
  width: 100%;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 14px;
  font-family: inherit;
}

.form-group textarea {
  resize: vertical;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.recipient-options {
  display: flex;
  gap: 20px;
  margin-bottom: 10px;
}

.recipient-options label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: normal;
}

.user-ids-input {
  margin-top: 10px;
}

.form-actions {
  display: flex;
  gap: 15px;
  justify-content: flex-end;
  margin-top: 30px;
}

.btn-primary,
.btn-secondary,
.btn-danger {
  padding: 12px 24px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background: #f0f0f0;
  color: #333;
}

.btn-secondary:hover {
  background: #e0e0e0;
}

.btn-danger {
  background: #f44336;
  color: white;
}

.btn-danger:hover {
  background: #d32f2f;
}

.history-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.empty-state {
  text-align: center;
  padding: 40px;
  color: #999;
}

.notification-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.notification-item {
  padding: 20px;
  border-left: 4px solid #ccc;
  border-radius: 8px;
  background: #f9f9f9;
}

.notification-item.type-info {
  border-left-color: #2196f3;
}

.notification-item.type-warning {
  border-left-color: #ff9800;
}

.notification-item.type-maintenance {
  border-left-color: #00bcd4;
}

.notification-item.type-success {
  border-left-color: #4caf50;
}

.notification-item.type-error {
  border-left-color: #f44336;
}

.notification-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.notification-type {
  font-weight: 600;
  text-transform: uppercase;
  font-size: 12px;
}

.notification-date {
  color: #999;
  font-size: 12px;
}

.notification-item h3 {
  font-size: 18px;
  margin-bottom: 8px;
  color: #333;
}

.notification-item p {
  color: #666;
  margin-bottom: 12px;
  line-height: 1.5;
}

.notification-stats {
  display: flex;
  gap: 15px;
  font-size: 13px;
  color: #777;
}

.notification-stats .failed {
  color: #f44336;
  font-weight: 600;
}

.alert {
  position: fixed;
  top: 20px;
  right: 20px;
  padding: 16px 24px;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 1000;
  max-width: 400px;
  animation: slideIn 0.3s ease;
}

.alert-success {
  background: #4caf50;
  color: white;
}

.alert-error {
  background: #f44336;
  color: white;
}

@keyframes slideIn {
  from {
    transform: translateX(400px);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

@media (max-width: 768px) {
  .form-row {
    grid-template-columns: 1fr;
  }

  .form-actions {
    flex-direction: column;
  }

  .btn-primary,
  .btn-secondary {
    width: 100%;
  }
}
</style>
