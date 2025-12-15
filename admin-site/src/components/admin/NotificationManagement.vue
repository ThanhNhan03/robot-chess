<template>
  <div class="notification-component">
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
                Specific Users (Select from list)
              </label>
            </div>
            
            <!-- Email Selection with Tags -->
            <div v-if="recipientType === 'specific'" class="email-selector">
              <!-- Selected Emails as Tags -->
              <div class="selected-emails">
                <span 
                  v-for="email in selectedEmails" 
                  :key="email" 
                  class="email-tag"
                >
                  {{ email }}
                  <button type="button" @click="removeEmail(email)" class="remove-tag">×</button>
                </span>
              </div>

              <!-- Search Input -->
              <div class="search-input-wrapper">
                <input
                  v-model="emailSearchQuery"
                  @input="searchUsers"
                  @focus="showDropdown = true"
                  type="text"
                  placeholder="Search users by email or name..."
                  class="email-search-input"
                />
              </div>

              <!-- Dropdown List -->
              <div v-if="showDropdown && filteredUsers.length > 0" class="user-dropdown">
                <div
                  v-for="user in filteredUsers"
                  :key="user.id"
                  @click="addEmail(user.email)"
                  class="user-item"
                  :class="{ selected: selectedEmails.includes(user.email) }"
                >
                  <div class="user-info">
                    <span class="user-avatar">{{ getUserInitial(user) }}</span>
                    <div class="user-details">
                      <div class="user-name">{{ user.fullName || user.username }}</div>
                      <div class="user-email">{{ user.email }}</div>
                    </div>
                  </div>
                  <span v-if="selectedEmails.includes(user.email)" class="check-icon">✓</span>
                </div>
              </div>

              <!-- No Results -->
              <div v-if="showDropdown && emailSearchQuery && filteredUsers.length === 0" class="no-results">
                No users found
              </div>

              <!-- Selected Count -->
              <div v-if="selectedEmails.length > 0" class="selected-count">
                {{ selectedEmails.length }} user(s) selected
              </div>
            </div>
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
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import notificationService, { type CreateNotificationRequest } from '../../services/notificationService';
import { userService, type User } from '../../services/userService';

const form = ref<CreateNotificationRequest>({
  title: '',
  message: '',
  type: 'info',
  sendEmail: true
});

const recipientType = ref<'all' | 'specific'>('all');
const selectedEmails = ref<string[]>([]);
const emailSearchQuery = ref('');
const showDropdown = ref(false);
const allUsers = ref<User[]>([]);
const loading = ref(false);
const successMessage = ref('');
const errorMessage = ref('');
const notificationHistory = ref<any[]>([]);

const STORAGE_KEY = 'admin_notification_history';

const filteredUsers = computed(() => {
  if (!emailSearchQuery.value.trim()) {
    return allUsers.value.filter(u => u.role === 'player' && u.isActive).slice(0, 10);
  }
  
  const query = emailSearchQuery.value.toLowerCase();
  return allUsers.value
    .filter(u => 
      u.role === 'player' && 
      u.isActive &&
      (u.email.toLowerCase().includes(query) ||
       u.username.toLowerCase().includes(query) ||
       (u.fullName && u.fullName.toLowerCase().includes(query)))
    )
    .slice(0, 10);
});

onMounted(async () => {
  loadNotificationHistory();
  await loadUsers();
  
  // Click outside to close dropdown
  document.addEventListener('click', handleClickOutside);
});

const loadUsers = async () => {
  try {
    allUsers.value = await userService.getAllUsers(false);
  } catch (error) {
    console.error('Failed to load users:', error);
  }
};

const handleClickOutside = (event: MouseEvent) => {
  const target = event.target as HTMLElement;
  if (!target.closest('.email-selector')) {
    showDropdown.value = false;
  }
};

const searchUsers = () => {
  showDropdown.value = true;
};

const addEmail = (email: string) => {
  if (!selectedEmails.value.includes(email)) {
    selectedEmails.value.push(email);
  }
  emailSearchQuery.value = '';
  showDropdown.value = false;
};

const removeEmail = (email: string) => {
  selectedEmails.value = selectedEmails.value.filter(e => e !== email);
};

const getUserInitial = (user: User): string => {
  if (user.fullName) {
    return user.fullName.charAt(0).toUpperCase();
  }
  return user.username.charAt(0).toUpperCase();
};


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

    // Use selected emails if specific recipients selected
    if (recipientType.value === 'specific') {
      if (selectedEmails.value.length === 0) {
        errorMessage.value = 'Please select at least one recipient';
        return;
      }
      
      // Basic email validation
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      const invalidEmails = selectedEmails.value.filter(email => !emailRegex.test(email));
      if (invalidEmails.length > 0) {
        errorMessage.value = `Invalid email(s): ${invalidEmails.join(', ')}`;
        return;
      }
      
      data.userEmails = selectedEmails.value;
    }

    const response = await notificationService.sendNotification(data);
    
    // Save to localStorage
    saveNotificationToHistory(response.notification, response.stats);
    
    // Broadcast notification to web app localStorage
    broadcastNotificationToWebApp(response.notification);
    
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
    selectedEmails.value = [];
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

const broadcastNotificationToWebApp = (notification: any) => {
  // This will be picked up by the web app through localStorage sync
  const webAppNotifications = localStorage.getItem('chess_robot_notifications');
  let notifications = [];
  
  try {
    if (webAppNotifications) {
      notifications = JSON.parse(webAppNotifications);
    }
  } catch (error) {
    console.error('Error parsing web app notifications:', error);
  }
  
  notifications.unshift(notification);
  
  // Keep only last 20 notifications
  if (notifications.length > 20) {
    notifications = notifications.slice(0, 20);
  }
  
  localStorage.setItem('chess_robot_notifications', JSON.stringify(notifications));
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
.notification-component {
  width: 100%;
  height: 100%;
  overflow-y: auto;
}

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

.user-emails-input {
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

/* Email Selector Dropdown Styles */
.email-selector {
  position: relative;
  width: 100%;
}

.selected-emails {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 12px;
  min-height: 36px;
}

.email-tag {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 500;
  animation: fadeIn 0.2s ease;
}

.remove-tag {
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  transition: background 0.2s;
}

.remove-tag:hover {
  background: rgba(255, 255, 255, 0.3);
}

.search-input-wrapper {
  position: relative;
}

.search-input-wrapper input {
  width: 100%;
  padding: 12px;
  border: 2px solid #ddd;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.3s;
}

.search-input-wrapper input:focus {
  outline: none;
  border-color: #667eea;
}

.user-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  max-height: 300px;
  overflow-y: auto;
  background: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  margin-top: 4px;
  z-index: 100;
  animation: slideDown 0.2s ease;
}

.user-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  cursor: pointer;
  transition: background 0.2s;
  border-bottom: 1px solid #f0f0f0;
}

.user-item:hover {
  background: #f8f9fa;
}

.user-item:last-child {
  border-bottom: none;
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
  flex-shrink: 0;
}

.user-details {
  flex: 1;
  min-width: 0;
}

.user-details strong {
  display: block;
  color: #333;
  font-size: 14px;
  margin-bottom: 2px;
}

.user-details small {
  color: #666;
  font-size: 12px;
  display: block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.check-icon {
  color: #4caf50;
  font-size: 18px;
  flex-shrink: 0;
}

.no-results {
  padding: 20px;
  text-align: center;
  color: #999;
  font-size: 14px;
}

.selected-count {
  margin-top: 8px;
  font-size: 13px;
  color: #666;
  font-weight: 500;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: scale(0.9);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
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
  
  .user-dropdown {
    max-height: 250px;
  }
}
</style>
