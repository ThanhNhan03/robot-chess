<template>
  <div class="robot-management">
    <div class="panel-header">
      <h2 class="panel-title">🤖 Robot Management</h2>
      <button class="btn-flat btn-primary">
        <span>➕</span> Add New Robot
      </button>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon">✅</div>
        <div class="stat-content">
          <div class="stat-value">3</div>
          <div class="stat-label">Connected Robots</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon">⚠️</div>
        <div class="stat-content">
          <div class="stat-value">1</div>
          <div class="stat-label">Offline Robots</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon">📊</div>
        <div class="stat-content">
          <div class="stat-value">127</div>
          <div class="stat-label">Total Moves</div>
        </div>
      </div>
      <div class="stat-card stat-primary">
        <div class="stat-icon">⏱️</div>
        <div class="stat-content">
          <div class="stat-value">98%</div>
          <div class="stat-label">Success Rate</div>
        </div>
      </div>
    </div>

    <!-- Robot List -->
    <div class="panel-section">
      <h3 class="section-title">Connected Robots</h3>
      <div class="robot-list">
        <!-- Robot Card 1 -->
        <div class="robot-card">
          <div class="robot-status status-online"></div>
          <div class="robot-info">
            <h4 class="robot-name">Chess Robot #1</h4>
            <div class="robot-details">
              <span class="robot-id">ID: ROBOT_001</span>
              <span class="robot-ip">IP: 192.168.1.100</span>
            </div>
            <div class="robot-meta">
              <span class="badge-flat badge-success">ONLINE</span>
              <span class="robot-uptime">Uptime: 2h 34m</span>
            </div>
          </div>
          <div class="robot-actions">
            <button class="btn-flat btn-primary btn-sm">Control</button>
            <button class="btn-flat btn-warning btn-sm">Calibrate</button>
            <button class="btn-flat btn-danger btn-sm">Disconnect</button>
          </div>
        </div>

        <!-- Robot Card 2 -->
        <div class="robot-card">
          <div class="robot-status status-online"></div>
          <div class="robot-info">
            <h4 class="robot-name">Chess Robot #2</h4>
            <div class="robot-details">
              <span class="robot-id">ID: ROBOT_002</span>
              <span class="robot-ip">IP: 192.168.1.101</span>
            </div>
            <div class="robot-meta">
              <span class="badge-flat badge-success">ONLINE</span>
              <span class="robot-uptime">Uptime: 5h 12m</span>
            </div>
          </div>
          <div class="robot-actions">
            <button class="btn-flat btn-primary btn-sm">Control</button>
            <button class="btn-flat btn-warning btn-sm">Calibrate</button>
            <button class="btn-flat btn-danger btn-sm">Disconnect</button>
          </div>
        </div>

        <!-- Robot Card 3 -->
        <div class="robot-card">
          <div class="robot-status status-online"></div>
          <div class="robot-info">
            <h4 class="robot-name">Chess Robot #3</h4>
            <div class="robot-details">
              <span class="robot-id">ID: ROBOT_003</span>
              <span class="robot-ip">IP: 192.168.1.102</span>
            </div>
            <div class="robot-meta">
              <span class="badge-flat badge-success">ONLINE</span>
              <span class="robot-uptime">Uptime: 1h 08m</span>
            </div>
          </div>
          <div class="robot-actions">
            <button class="btn-flat btn-primary btn-sm">Control</button>
            <button class="btn-flat btn-warning btn-sm">Calibrate</button>
            <button class="btn-flat btn-danger btn-sm">Disconnect</button>
          </div>
        </div>

        <!-- Robot Card 4 - Offline -->
        <div class="robot-card robot-offline">
          <div class="robot-status status-offline"></div>
          <div class="robot-info">
            <h4 class="robot-name">Chess Robot #4</h4>
            <div class="robot-details">
              <span class="robot-id">ID: ROBOT_004</span>
              <span class="robot-ip">IP: 192.168.1.103</span>
            </div>
            <div class="robot-meta">
              <span class="badge-flat badge-danger">OFFLINE</span>
              <span class="robot-uptime">Last seen: 30m ago</span>
            </div>
          </div>
          <div class="robot-actions">
            <button class="btn-flat btn-primary btn-sm" disabled>Control</button>
            <button class="btn-flat btn-warning btn-sm" disabled>Calibrate</button>
            <button class="btn-flat btn-success btn-sm">Reconnect</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Command Queue -->
    <div class="panel-section">
      <h3 class="section-title">Command Queue</h3>
      <div class="command-queue">
        <table class="table-flat">
          <thead>
            <tr>
              <th>Time</th>
              <th>Robot</th>
              <th>Command</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>10:34:22</td>
              <td>ROBOT_001</td>
              <td>Move: e2 → e4</td>
              <td><span class="badge-flat badge-success">Completed</span></td>
              <td><button class="btn-flat btn-sm">Details</button></td>
            </tr>
            <tr>
              <td>10:34:45</td>
              <td>ROBOT_002</td>
              <td>Move: d7 → d5</td>
              <td><span class="badge-flat badge-warning">Pending</span></td>
              <td><button class="btn-flat btn-sm">Cancel</button></td>
            </tr>
            <tr>
              <td>10:35:01</td>
              <td>ROBOT_003</td>
              <td>Calibrate</td>
              <td><span class="badge-flat badge-primary">In Progress</span></td>
              <td><button class="btn-flat btn-sm">Monitor</button></td>
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

<style scoped src="../../assets/styles/RobotManagement.css">
</style>
