<template>
  <div class="notification-management">
    <div class="panel-header">
      <h2 class="panel-title">
        <span class="material-icons">notifications</span>
        Notification Management
      </h2>
      <button class="btn-flat btn-primary" @click="showCreateModal = true">
        <span class="material-icons">add</span> Create Notification
      </button>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon"><span class="material-icons">mail</span></div>
        <div class="stat-content">
          <div class="stat-value">156</div>
          <div class="stat-label">Total Sent</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon"><span class="material-icons">schedule</span></div>
        <div class="stat-content">
          <div class="stat-value">8</div>
          <div class="stat-label">Scheduled</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon"><span class="material-icons">drafts</span></div>
        <div class="stat-content">
          <div class="stat-value">3</div>
          <div class="stat-label">Draft</div>
        </div>
      </div>
      <div class="stat-card stat-primary">
        <div class="stat-icon"><span class="material-icons">people</span></div>
        <div class="stat-content">
          <div class="stat-value">1,234</div>
          <div class="stat-label">Total Recipients</div>
        </div>
      </div>
    </div>

    <!-- Filter Bar -->
    <div class="panel-section">
      <div class="filter-bar">
        <div class="search-box">
          <input type="text" class="input-flat" placeholder="Search notifications..." />
        </div>
        <div class="filter-controls">
          <select class="select-flat">
            <option value="">All Types</option>
            <option value="info">Info</option>
            <option value="warning">Warning</option>
            <option value="success">Success</option>
            <option value="error">Error</option>
          </select>
          <select class="select-flat">
            <option value="">All Status</option>
            <option value="sent">Sent</option>
            <option value="scheduled">Scheduled</option>
            <option value="draft">Draft</option>
          </select>
          <select class="select-flat">
            <option value="">All Audiences</option>
            <option value="all">All Users</option>
            <option value="admin">Admins</option>
            <option value="player">Players</option>
            <option value="viewer">Viewers</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Notifications Table -->
    <div class="panel-section">
      <h3 class="section-title">Notifications</h3>
      <div class="notifications-table">
        <table class="table-flat">
          <thead>
            <tr>
              <th>Type</th>
              <th>Title</th>
              <th>Message</th>
              <th>Audience</th>
              <th>Status</th>
              <th>Scheduled/Sent</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <!-- Notification 1 - Sent -->
            <tr>
              <td>
                <span class="type-badge type-info"><span class="material-icons-outlined">info</span> INFO</span>
              </td>
              <td><strong>System Maintenance</strong></td>
              <td class="message-cell">Scheduled maintenance on Nov 30, 2024 from 2:00 AM to 4:00 AM</td>
              <td>
                <span class="badge-flat badge-primary">ALL USERS</span>
              </td>
              <td>
                <span class="badge-flat badge-success">SENT</span>
              </td>
              <td>2024-11-28 10:00</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-flat btn-sm btn-primary"><span class="material-icons">edit</span> Edit</button>
                  <button class="btn-flat btn-sm btn-danger"><span class="material-icons">delete</span> Delete</button>
                </div>
              </td>
            </tr>

            <!-- Notification 2 - Scheduled -->
            <tr>
              <td>
                <span class="type-badge type-warning"><span class="material-icons-outlined">warning</span> WARNING</span>
              </td>
              <td><strong>Session Expiry</strong></td>
              <td class="message-cell">Your session will expire in 5 minutes. Please save your work.</td>
              <td>
                <span class="badge-flat badge-info">PLAYERS</span>
              </td>
              <td>
                <span class="badge-flat badge-warning">SCHEDULED</span>
              </td>
              <td>2024-11-30 14:00</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-flat btn-sm btn-primary"><span class="material-icons">edit</span> Edit</button>
                  <button class="btn-flat btn-sm btn-danger"><span class="material-icons">delete</span> Delete</button>
                  <button class="btn-flat btn-sm btn-success"><span class="material-icons">send</span> Send Now</button>
                </div>
              </td>
            </tr>

            <!-- Notification 3 - Sent -->
            <tr>
              <td>
                <span class="type-badge type-success"><span class="material-icons-outlined">check_circle</span> SUCCESS</span>
              </td>
              <td><strong>Profile Updated</strong></td>
              <td class="message-cell">Your profile has been successfully updated.</td>
              <td>
                <span class="badge-flat badge-secondary">SPECIFIC</span>
              </td>
              <td>
                <span class="badge-flat badge-success">SENT</span>
              </td>
              <td>2024-11-29 09:30</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-flat btn-sm btn-primary"><span class="material-icons">edit</span> Edit</button>
                  <button class="btn-flat btn-sm btn-danger"><span class="material-icons">delete</span> Delete</button>
                </div>
              </td>
            </tr>

            <!-- Notification 4 - Sent -->
            <tr>
              <td>
                <span class="type-badge type-error"><span class="material-icons-outlined">error</span> ERROR</span>
              </td>
              <td><strong>Connection Lost</strong></td>
              <td class="message-cell">Connection to robot ROBOT_004 has been lost. Please check the robot status.</td>
              <td>
                <span class="badge-flat badge-danger">ADMINS</span>
              </td>
              <td>
                <span class="badge-flat badge-success">SENT</span>
              </td>
              <td>2024-11-29 14:45</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-flat btn-sm btn-primary"><span class="material-icons">edit</span> Edit</button>
                  <button class="btn-flat btn-sm btn-danger"><span class="material-icons">delete</span> Delete</button>
                </div>
              </td>
            </tr>

            <!-- Notification 5 - Draft -->
            <tr class="notification-draft">
              <td>
                <span class="type-badge type-info"><span class="material-icons-outlined">info</span> INFO</span>
              </td>
              <td><strong>New Feature Release</strong></td>
              <td class="message-cell">We've added new AI difficulty levels! Try them out in your next game.</td>
              <td>
                <span class="badge-flat badge-primary">ALL USERS</span>
              </td>
              <td>
                <span class="badge-flat badge-secondary">DRAFT</span>
              </td>
              <td>-</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-flat btn-sm btn-primary"><span class="material-icons">edit</span> Edit</button>
                  <button class="btn-flat btn-sm btn-danger"><span class="material-icons">delete</span> Delete</button>
                  <button class="btn-flat btn-sm btn-success"><span class="material-icons">send</span> Send</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="pagination">
        <button class="btn-flat btn-sm btn-secondary" disabled><span class="material-icons">arrow_back</span> Previous</button>
        <div class="page-numbers">
          <button class="btn-flat btn-sm btn-primary">1</button>
          <button class="btn-flat btn-sm">2</button>
          <button class="btn-flat btn-sm">3</button>
        </div>
        <button class="btn-flat btn-sm btn-secondary">Next <span class="material-icons">arrow_forward</span></button>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showCreateModal" class="modal-overlay" @click="showCreateModal = false">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>Create Notification</h3>
          <button class="modal-close" @click="showCreateModal = false"><span class="material-icons">close</span></button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label>Title</label>
            <input type="text" class="input-flat" placeholder="Enter notification title..." />
          </div>
          <div class="form-group">
            <label>Message</label>
            <textarea class="input-flat" rows="4" placeholder="Enter notification message..."></textarea>
          </div>
          <div class="form-group">
            <label>Type</label>
            <select class="select-flat">
              <option value="info">Info</option>
              <option value="warning">Warning</option>
              <option value="success">Success</option>
              <option value="error">Error</option>
            </select>
          </div>
          <div class="form-group">
            <label>Target Audience</label>
            <select class="select-flat">
              <option value="all">All Users</option>
              <option value="admin">Admins Only</option>
              <option value="player">Players Only</option>
              <option value="viewer">Viewers Only</option>
              <option value="specific">Specific Users</option>
            </select>
          </div>
          <div class="form-group">
            <label>Schedule (Optional)</label>
            <input type="datetime-local" class="input-flat" />
          </div>
          <div class="form-group">
            <label>Expires At (Optional)</label>
            <input type="datetime-local" class="input-flat" />
          </div>
          <div class="form-group">
            <label>Status</label>
            <select class="select-flat">
              <option value="draft">Save as Draft</option>
              <option value="scheduled">Schedule</option>
              <option value="send">Send Immediately</option>
            </select>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn-flat btn-secondary" @click="showCreateModal = false">Cancel</button>
          <button class="btn-flat btn-primary">Create Notification</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const showCreateModal = ref(false)
</script>

<style scoped>
@import '../../assets/styles/NotificationManagement.css';
</style>
