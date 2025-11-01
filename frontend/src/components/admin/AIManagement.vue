<template>
  <div class="ai-management">
    <div class="panel-header">
      <h2 class="panel-title">🧠 AI Management</h2>
      <button class="btn-flat btn-primary">
        <span>➕</span> Register New AI
      </button>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon">🟢</div>
        <div class="stat-content">
          <div class="stat-value">2</div>
          <div class="stat-label">Active AI Instances</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon">🎯</div>
        <div class="stat-content">
          <div class="stat-value">95%</div>
          <div class="stat-label">Move Accuracy</div>
        </div>
      </div>
      <div class="stat-card stat-primary">
        <div class="stat-icon">⚡</div>
        <div class="stat-content">
          <div class="stat-value">1.2s</div>
          <div class="stat-label">Avg Response Time</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon">📈</div>
        <div class="stat-content">
          <div class="stat-value">453</div>
          <div class="stat-label">Total Games</div>
        </div>
      </div>
    </div>

    <!-- AI Instances List -->
    <div class="panel-section">
      <h3 class="section-title">AI Instances</h3>
      <div class="ai-list">
        <!-- AI Card 1 -->
        <div class="ai-card">
          <div class="ai-header">
            <div class="ai-status status-active"></div>
            <div class="ai-info">
              <h4 class="ai-name">Chess Vision AI</h4>
              <div class="ai-details">
                <span class="ai-id">ID: AI_CHESS_VISION_001</span>
                <span class="ai-type">Type: Vision Detection</span>
              </div>
            </div>
            <div class="ai-badge">
              <span class="badge-flat badge-success">ACTIVE</span>
            </div>
          </div>
          
          <div class="ai-body">
            <div class="ai-metrics">
              <div class="metric-item">
                <span class="metric-label">Accuracy</span>
                <div class="progress-flat">
                  <div class="progress-flat-bar progress-success" style="width: 96%"></div>
                </div>
                <span class="metric-value">96%</span>
              </div>
              <div class="metric-item">
                <span class="metric-label">Response Time</span>
                <div class="progress-flat">
                  <div class="progress-flat-bar" style="width: 85%"></div>
                </div>
                <span class="metric-value">1.1s</span>
              </div>
              <div class="metric-item">
                <span class="metric-label">Success Rate</span>
                <div class="progress-flat">
                  <div class="progress-flat-bar progress-success" style="width: 94%"></div>
                </div>
                <span class="metric-value">94%</span>
              </div>
            </div>
            
            <div class="ai-stats-row">
              <div class="stat-mini">
                <span class="stat-mini-label">Games Played</span>
                <span class="stat-mini-value">234</span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Wins</span>
                <span class="stat-mini-value">187</span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Losses</span>
                <span class="stat-mini-value">47</span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Uptime</span>
                <span class="stat-mini-value">12h 45m</span>
              </div>
            </div>
          </div>
          
          <div class="ai-actions">
            <button class="btn-flat btn-primary btn-sm">Configure</button>
            <button class="btn-flat btn-warning btn-sm">Restart</button>
            <button class="btn-flat btn-secondary btn-sm">View Logs</button>
            <button class="btn-flat btn-danger btn-sm">Stop</button>
          </div>
        </div>

        <!-- AI Card 2 -->
        <div class="ai-card">
          <div class="ai-header">
            <div class="ai-status status-active"></div>
            <div class="ai-info">
              <h4 class="ai-name">Stockfish Engine</h4>
              <div class="ai-details">
                <span class="ai-id">ID: AI_STOCKFISH_002</span>
                <span class="ai-type">Type: Chess Engine</span>
              </div>
            </div>
            <div class="ai-badge">
              <span class="badge-flat badge-success">ACTIVE</span>
            </div>
          </div>
          
          <div class="ai-body">
            <div class="ai-metrics">
              <div class="metric-item">
                <span class="metric-label">Accuracy</span>
                <div class="progress-flat">
                  <div class="progress-flat-bar progress-success" style="width: 99%"></div>
                </div>
                <span class="metric-value">99%</span>
              </div>
              <div class="metric-item">
                <span class="metric-label">Response Time</span>
                <div class="progress-flat">
                  <div class="progress-flat-bar" style="width: 92%"></div>
                </div>
                <span class="metric-value">0.8s</span>
              </div>
              <div class="metric-item">
                <span class="metric-label">Success Rate</span>
                <div class="progress-flat">
                  <div class="progress-flat-bar progress-success" style="width: 98%"></div>
                </div>
                <span class="metric-value">98%</span>
              </div>
            </div>
            
            <div class="ai-stats-row">
              <div class="stat-mini">
                <span class="stat-mini-label">Games Played</span>
                <span class="stat-mini-value">219</span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Wins</span>
                <span class="stat-mini-value">198</span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Losses</span>
                <span class="stat-mini-value">21</span>
              </div>
              <div class="stat-mini">
                <span class="stat-mini-label">Uptime</span>
                <span class="stat-mini-value">15h 22m</span>
              </div>
            </div>
          </div>
          
          <div class="ai-actions">
            <button class="btn-flat btn-primary btn-sm">Configure</button>
            <button class="btn-flat btn-warning btn-sm">Restart</button>
            <button class="btn-flat btn-secondary btn-sm">View Logs</button>
            <button class="btn-flat btn-danger btn-sm">Stop</button>
          </div>
        </div>

        <!-- AI Card 3 - Inactive -->
        <div class="ai-card ai-inactive">
          <div class="ai-header">
            <div class="ai-status status-inactive"></div>
            <div class="ai-info">
              <h4 class="ai-name">Neural Network AI</h4>
              <div class="ai-details">
                <span class="ai-id">ID: AI_NEURAL_003</span>
                <span class="ai-type">Type: Deep Learning</span>
              </div>
            </div>
            <div class="ai-badge">
              <span class="badge-flat badge-danger">INACTIVE</span>
            </div>
          </div>
          
          <div class="ai-body">
            <div class="ai-inactive-message">
              <p>This AI instance is currently not running.</p>
              <p class="text-muted">Last active: 2 hours ago</p>
            </div>
          </div>
          
          <div class="ai-actions">
            <button class="btn-flat btn-success btn-sm">Start</button>
            <button class="btn-flat btn-secondary btn-sm">View Logs</button>
            <button class="btn-flat btn-danger btn-sm">Remove</button>
          </div>
        </div>
      </div>
    </div>

    <!-- AI Activity Log -->
    <div class="panel-section">
      <h3 class="section-title">Recent AI Activity</h3>
      <div class="activity-log">
        <table class="table-flat">
          <thead>
            <tr>
              <th>Time</th>
              <th>AI Instance</th>
              <th>Action</th>
              <th>Result</th>
              <th>Details</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>10:45:12</td>
              <td>Chess Vision AI</td>
              <td>Move Detected</td>
              <td><span class="badge-flat badge-success">Success</span></td>
              <td>e2 → e4 (Confidence: 98%)</td>
            </tr>
            <tr>
              <td>10:44:55</td>
              <td>Stockfish Engine</td>
              <td>Best Move Calculated</td>
              <td><span class="badge-flat badge-success">Success</span></td>
              <td>d7 → d5 (Eval: +0.3)</td>
            </tr>
            <tr>
              <td>10:44:38</td>
              <td>Chess Vision AI</td>
              <td>Board Scan</td>
              <td><span class="badge-flat badge-success">Success</span></td>
              <td>FEN validated</td>
            </tr>
            <tr>
              <td>10:44:01</td>
              <td>Neural Network AI</td>
              <td>Instance Stopped</td>
              <td><span class="badge-flat badge-warning">Info</span></td>
              <td>Manual shutdown</td>
            </tr>
            <tr>
              <td>10:43:22</td>
              <td>Stockfish Engine</td>
              <td>Position Analysis</td>
              <td><span class="badge-flat badge-success">Success</span></td>
              <td>Depth: 20, Time: 1.2s</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- AI Configuration Panel -->
    <div class="panel-section">
      <h3 class="section-title">AI Configuration</h3>
      <div class="config-panel">
        <div class="config-group">
          <label class="config-label">Default AI Engine</label>
          <select class="select-flat">
            <option>Chess Vision AI</option>
            <option selected>Stockfish Engine</option>
            <option>Neural Network AI</option>
          </select>
        </div>
        
        <div class="config-group">
          <label class="config-label">AI Difficulty Level</label>
          <select class="select-flat">
            <option>Easy (Depth: 10)</option>
            <option>Medium (Depth: 15)</option>
            <option selected>Hard (Depth: 20)</option>
            <option>Expert (Depth: 25)</option>
          </select>
        </div>
        
        <div class="config-group">
          <label class="config-label">Max Response Time (seconds)</label>
          <input type="number" class="input-flat" value="5" min="1" max="30" />
        </div>
        
        <div class="config-group">
          <label class="config-label">Auto-restart on Failure</label>
          <div class="toggle-switch">
            <input type="checkbox" id="auto-restart" checked />
            <label for="auto-restart">Enabled</label>
          </div>
        </div>
        
        <div class="config-actions">
          <button class="btn-flat btn-primary">Save Configuration</button>
          <button class="btn-flat btn-secondary">Reset to Default</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Component logic will be implemented later
</script>

<style scoped src="../../assets/styles/AIManagement.css">
</style>
