<template>
  <div class="user-management">
    <div class="panel-header">
      <h2 class="panel-title">
        <span>�</span>
        User Management
      </h2>
      <button class="btn-flat btn-primary" title="Add a new user to the system">
        <span>➕</span> Add New User
      </button>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon">👥</div>
        <div class="stat-content">
          <div class="stat-value">156</div>
          <div class="stat-label">Total Users</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon">🟢</div>
        <div class="stat-content">
          <div class="stat-value">89</div>
          <div class="stat-label">Active Users</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon">👨‍💼</div>
        <div class="stat-content">
          <div class="stat-value">5</div>
          <div class="stat-label">Admins</div>
        </div>
      </div>
      <div class="stat-card stat-primary">
        <div class="stat-icon">🆕</div>
        <div class="stat-content">
          <div class="stat-value">12</div>
          <div class="stat-label">New This Week</div>
        </div>
      </div>
    </div>

    <!-- Search and Filter -->
    <div class="panel-section">
      <div class="search-filter-bar">
        <div class="search-box">
          <input type="text" class="input-flat" placeholder="Search users by name or email..." />
        </div>
        <div class="filter-controls">
          <select class="select-flat">
            <option value="">All Roles</option>
            <option value="admin">Admin</option>
            <option value="player">Player</option>
            <option value="guest">Guest</option>
          </select>
          <select class="select-flat">
            <option value="">All Status</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Users List -->
    <div class="panel-section">
      <h3 class="section-title">Users List</h3>
      <div class="users-table">
        <table class="table-flat">
          <thead>
            <tr>
              <th>Avatar</th>
              <th>Username</th>
              <th>Email</th>
              <th>Full Name</th>
              <th>Role</th>
              <th>Status</th>
              <th>Joined</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <!-- User Row 1 - Admin -->
            <tr>
              <td>
                <div class="user-avatar">👨‍💼</div>
              </td>
              <td><strong>admin_001</strong></td>
              <td>admin@chessrobot.com</td>
              <td>John Smith</td>
              <td><span class="badge-flat badge-danger">ADMIN</span></td>
              <td><span class="badge-flat badge-success">ACTIVE</span></td>
              <td>2024-01-15</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-flat btn-sm btn-primary" title="Edit user details">✏️ Edit</button>
                  <button class="btn-flat btn-sm btn-warning" title="Suspend user account">⏸️ Suspend</button>
                </div>
              </td>
            </tr>

            <!-- User Row 2 - Player -->
            <tr>
              <td>
                <div class="user-avatar">👨</div>
              </td>
              <td><strong>player_123</strong></td>
              <td>player123@email.com</td>
              <td>Michael Johnson</td>
              <td><span class="badge-flat badge-primary">PLAYER</span></td>
              <td><span class="badge-flat badge-success">ACTIVE</span></td>
              <td>2024-03-20</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-flat btn-sm btn-primary" title="Edit user details">✏️ Edit</button>
                  <button class="btn-flat btn-sm btn-warning" title="Suspend user account">⏸️ Suspend</button>
                </div>
              </td>
            </tr>

            <!-- User Row 3 - Player -->
            <tr>
              <td>
                <div class="user-avatar">👩</div>
              </td>
              <td><strong>chess_master</strong></td>
              <td>sarah.chen@email.com</td>
              <td>Sarah Chen</td>
              <td><span class="badge-flat badge-primary">PLAYER</span></td>
              <td><span class="badge-flat badge-success">ACTIVE</span></td>
              <td>2024-05-10</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-flat btn-sm btn-primary" title="Edit user details">✏️ Edit</button>
                  <button class="btn-flat btn-sm btn-warning" title="Suspend user account">⏸️ Suspend</button>
                </div>
              </td>
            </tr>

            <!-- User Row 4 - Guest -->
            <tr>
              <td>
                <div class="user-avatar">🧑</div>
              </td>
              <td><strong>guest_789</strong></td>
              <td>guest789@email.com</td>
              <td>Alex Martinez</td>
              <td><span class="badge-flat badge-info">GUEST</span></td>
              <td><span class="badge-flat badge-success">ACTIVE</span></td>
              <td>2024-10-28</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-flat btn-sm btn-primary" title="Edit user details">✏️ Edit</button>
                  <button class="btn-flat btn-sm btn-warning" title="Suspend user account">⏸️ Suspend</button>
                </div>
              </td>
            </tr>

            <!-- User Row 5 - Inactive Player -->
            <tr class="user-inactive">
              <td>
                <div class="user-avatar">👤</div>
              </td>
              <td><strong>old_player</strong></td>
              <td>old@email.com</td>
              <td>David Wilson</td>
              <td><span class="badge-flat badge-primary">PLAYER</span></td>
              <td><span class="badge-flat badge-danger">INACTIVE</span></td>
              <td>2023-12-01</td>
              <td>
                <div class="action-buttons">
                  <button class="btn-flat btn-sm btn-primary" title="Edit user details">✏️ Edit</button>
                  <button class="btn-flat btn-sm btn-success" title="Activate user account">▶️ Activate</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="pagination">
        <button class="btn-flat btn-sm btn-secondary" disabled title="Go to previous page">⬅️ Previous</button>
        <div class="page-numbers">
          <button class="btn-flat btn-sm btn-primary" title="Current page">1</button>
          <button class="btn-flat btn-sm" title="Go to page 2">2</button>
          <button class="btn-flat btn-sm" title="Go to page 3">3</button>
          <button class="btn-flat btn-sm" title="Go to page 4">4</button>
          <button class="btn-flat btn-sm" title="Go to page 5">5</button>
        </div>
        <button class="btn-flat btn-sm btn-secondary" title="Go to next page">Next ➡️</button>
      </div>
    </div>

    <!-- User Activity Log -->
    <div class="panel-section">
      <h3 class="section-title">Recent User Activity</h3>
      <div class="activity-log">
        <table class="table-flat">
          <thead>
            <tr>
              <th>Time</th>
              <th>User</th>
              <th>Action</th>
              <th>Details</th>
              <th>IP Address</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>11:23:45</td>
              <td>player_123</td>
              <td><span class="badge-flat badge-info">LOGIN</span></td>
              <td>Successful login</td>
              <td>192.168.1.105</td>
            </tr>
            <tr>
              <td>11:15:22</td>
              <td>chess_master</td>
              <td><span class="badge-flat badge-success">GAME_START</span></td>
              <td>Started game vs Robot #1</td>
              <td>192.168.1.110</td>
            </tr>
            <tr>
              <td>11:10:11</td>
              <td>guest_789</td>
              <td><span class="badge-flat badge-warning">PROFILE_UPDATE</span></td>
              <td>Updated profile picture</td>
              <td>192.168.1.115</td>
            </tr>
            <tr>
              <td>11:05:33</td>
              <td>admin_001</td>
              <td><span class="badge-flat badge-danger">USER_SUSPEND</span></td>
              <td>Suspended user: old_player</td>
              <td>192.168.1.100</td>
            </tr>
            <tr>
              <td>10:58:12</td>
              <td>player_123</td>
              <td><span class="badge-flat badge-success">GAME_END</span></td>
              <td>Won against Robot #2</td>
              <td>192.168.1.105</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Component logic will be implemented later
</script>

<style scoped>
@import '../../assets/styles/UserManagement.css';
</style>
