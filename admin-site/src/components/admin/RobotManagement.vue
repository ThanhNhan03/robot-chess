<template>
  <div class="robot-management">
    <div class="panel-header">
      <h2 class="panel-title"><Bot :size="32" class="icon-title" /> Robot Management</h2>
      <button class="btn-flat btn-primary">
        <Plus :size="20" /> Add New Robot
      </button>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon"><CheckCircle :size="40" /></div>
        <div class="stat-content">
          <div class="stat-value">3</div>
          <div class="stat-label">Connected Robots</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon"><AlertTriangle :size="40" /></div>
        <div class="stat-content">
          <div class="stat-value">1</div>
          <div class="stat-label">Offline Robots</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon"><BarChart3 :size="40" /></div>
        <div class="stat-content">
          <div class="stat-value">127</div>
          <div class="stat-label">Total Moves</div>
        </div>
      </div>
      <div class="stat-card stat-primary">
        <div class="stat-icon"><Timer :size="40" /></div>
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
              <span class="robot-id">Code: ROBOT_001</span>
              <span class="robot-ip">IP: 192.168.1.100</span>
              <span class="robot-location"><MapPin :size="16" /> Lab Room A</span>
            </div>
            <div class="robot-meta">
              <span class="badge-flat badge-success">IDLE</span>
              <span class="badge-flat badge-info">ONLINE</span>
              <span class="robot-uptime">Last seen: 2024-11-29 15:15</span>
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
              <span class="robot-id">Code: ROBOT_002</span>
              <span class="robot-ip">IP: 192.168.1.101</span>
              <span class="robot-location"><MapPin :size="16" /> Lab Room B</span>
            </div>
            <div class="robot-meta">
              <span class="badge-flat badge-warning">BUSY</span>
              <span class="badge-flat badge-info">ONLINE</span>
              <span class="badge-flat badge-primary"><RotateCw :size="14" /> MOVING</span>
              <span class="robot-uptime">Last seen: 2024-11-29 15:18</span>
            </div>
            <div class="robot-game">
              <Gamepad2 :size="16" /> Playing game: abc123-def456-ghi789
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
              <span class="robot-id">Code: ROBOT_003</span>
              <span class="robot-ip">IP: 192.168.1.102</span>
              <span class="robot-location"><MapPin :size="16" /> Testing Area</span>
            </div>
            <div class="robot-meta">
              <span class="badge-flat badge-info">CALIBRATING</span>
              <span class="badge-flat badge-info">ONLINE</span>
              <span class="robot-uptime">Last seen: 2024-11-29 15:19</span>
            </div>
          </div>
          <div class="robot-actions">
            <button class="btn-flat btn-primary btn-sm">Control</button>
            <button class="btn-flat btn-warning btn-sm">Calibrate</button>
            <button class="btn-flat btn-danger btn-sm">Disconnect</button>
          </div>
        </div>

        <!-- Robot Card 4 - Offline with Error -->
        <div class="robot-card robot-offline">
          <div class="robot-status status-offline"></div>
          <div class="robot-info">
            <h4 class="robot-name">Chess Robot #4</h4>
            <div class="robot-details">
              <span class="robot-id">Code: ROBOT_004</span>
              <span class="robot-ip">IP: 192.168.1.103</span>
              <span class="robot-location"><MapPin :size="16" /> Storage</span>
            </div>
            <div class="robot-meta">
              <span class="badge-flat badge-secondary">DISCONNECTED</span>
              <span class="badge-flat badge-danger">OFFLINE</span>
              <span class="badge-flat badge-danger"><AlertTriangle :size="14" /> ERROR</span>
              <span class="robot-uptime">Last seen: 2024-11-29 14:45</span>
            </div>
            <div class="robot-error">
              <AlertTriangle :size="16" /> Connection timeout - Unable to reach robot
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
              <th>Sent At</th>
              <th>Robot</th>
              <th>Command Type</th>
              <th>Status</th>
              <th>Execution Time</th>
              <th>Executed By</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>2024-11-29 10:34:22</td>
              <td>ROBOT_001</td>
              <td>MOVE_PIECE</td>
              <td><span class="badge-flat badge-success">COMPLETED</span></td>
              <td>1,234ms</td>
              <td>admin_001</td>
              <td><button class="btn-flat btn-sm">Details</button></td>
            </tr>
            <tr>
              <td>2024-11-29 10:34:45</td>
              <td>ROBOT_002</td>
              <td>MOVE_PIECE</td>
              <td><span class="badge-flat badge-warning">PENDING</span></td>
              <td>-</td>
              <td>player_123</td>
              <td><button class="btn-flat btn-sm">Cancel</button></td>
            </tr>
            <tr>
              <td>2024-11-29 10:35:01</td>
              <td>ROBOT_003</td>
              <td>CALIBRATE</td>
              <td><span class="badge-flat badge-primary">IN_PROGRESS</span></td>
              <td>5,678ms</td>
              <td>admin_001</td>
              <td><button class="btn-flat btn-sm">Monitor</button></td>
            </tr>
            <tr>
              <td>2024-11-29 10:33:15</td>
              <td>ROBOT_004</td>
              <td>MOVE_PIECE</td>
              <td><span class="badge-flat badge-danger">FAILED</span></td>
              <td>892ms</td>
              <td>player_456</td>
              <td><button class="btn-flat btn-sm">Retry</button></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Robot Configuration -->
    <div class="panel-section">
      <h3 class="section-title">Robot Configuration</h3>
      <div class="config-grid">
        <!-- Config Card 1 -->
        <div class="config-card">
          <div class="config-header">
            <h4 class="config-robot-name"><Bot :size="24" /> Chess Robot #1</h4>
            <span class="badge-flat badge-success">ACTIVE</span>
          </div>
          <div class="config-body">
            <div class="config-row">
              <span class="config-label">Speed</span>
              <div class="config-value-group">
                <span class="config-value">75</span>
                <span class="config-unit">mm/s</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Max Speed</span>
              <div class="config-value-group">
                <span class="config-value">150</span>
                <span class="config-unit">mm/s</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Gripper Force</span>
              <div class="config-value-group">
                <span class="config-value">60</span>
                <span class="config-unit">%</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Gripper Speed</span>
              <div class="config-value-group">
                <span class="config-value">50</span>
                <span class="config-unit">mm/s</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Emergency Stop</span>
              <div class="config-value-group">
                <span class="badge-flat badge-success">DISABLED</span>
                <button class="btn-flat btn-sm btn-danger">Enable</button>
              </div>
            </div>
          </div>
          <div class="config-footer">
            <span class="config-updated">Last updated: 2024-11-29 14:30</span>
            <button class="btn-flat btn-sm btn-warning">Reset to Default</button>
          </div>
        </div>

        <!-- Config Card 2 -->
        <div class="config-card">
          <div class="config-header">
            <h4 class="config-robot-name"><Bot :size="24" /> Chess Robot #2</h4>
            <span class="badge-flat badge-success">ACTIVE</span>
          </div>
          <div class="config-body">
            <div class="config-row">
              <span class="config-label">Speed</span>
              <div class="config-value-group">
                <span class="config-value">80</span>
                <span class="config-unit">mm/s</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Max Speed</span>
              <div class="config-value-group">
                <span class="config-value">150</span>
                <span class="config-unit">mm/s</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Gripper Force</span>
              <div class="config-value-group">
                <span class="config-value">65</span>
                <span class="config-unit">%</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Gripper Speed</span>
              <div class="config-value-group">
                <span class="config-value">55</span>
                <span class="config-unit">mm/s</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Emergency Stop</span>
              <div class="config-value-group">
                <span class="badge-flat badge-success">DISABLED</span>
                <button class="btn-flat btn-sm btn-danger">Enable</button>
              </div>
            </div>
          </div>
          <div class="config-footer">
            <span class="config-updated">Last updated: 2024-11-29 15:10</span>
            <button class="btn-flat btn-sm btn-warning">Reset to Default</button>
          </div>
        </div>

        <!-- Config Card 3 -->
        <div class="config-card">
          <div class="config-header">
            <h4 class="config-robot-name"><Bot :size="24" /> Chess Robot #3</h4>
            <span class="badge-flat badge-success">ACTIVE</span>
          </div>
          <div class="config-body">
            <div class="config-row">
              <span class="config-label">Speed</span>
              <div class="config-value-group">
                <span class="config-value">70</span>
                <span class="config-unit">mm/s</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Max Speed</span>
              <div class="config-value-group">
                <span class="config-value">150</span>
                <span class="config-unit">mm/s</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Gripper Force</span>
              <div class="config-value-group">
                <span class="config-value">70</span>
                <span class="config-unit">%</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Gripper Speed</span>
              <div class="config-value-group">
                <span class="config-value">60</span>
                <span class="config-unit">mm/s</span>
                <button class="btn-flat btn-sm btn-primary">Adjust</button>
              </div>
            </div>
            <div class="config-row">
              <span class="config-label">Emergency Stop</span>
              <div class="config-value-group">
                <span class="badge-flat badge-success">DISABLED</span>
                <button class="btn-flat btn-sm btn-danger">Enable</button>
              </div>
            </div>
          </div>
          <div class="config-footer">
            <span class="config-updated">Last updated: 2024-11-29 15:19</span>
            <button class="btn-flat btn-sm btn-warning">Reset to Default</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {
  Bot,
  Plus,
  CheckCircle,
  AlertTriangle,
  BarChart3,
  Timer,
  MapPin,
  RotateCw,
  Gamepad2
} from 'lucide-vue-next';

// Component logic will be implemented later
</script>

<style scoped>
@import '../../assets/styles/RobotManagement.css';
</style>
