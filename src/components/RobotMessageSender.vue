<template>
  <div class="robot-message-sender">
    <div class="card">
      <div class="card-header">
        <h3>Robot Message Sender</h3>
      </div>
      
      <div class="card-body">
        <!-- Message Type Selection -->
        <div class="form-group">
          <label for="messageType">Message Type:</label>
          <select 
            id="messageType" 
            v-model="selectedMessageType" 
            @change="loadTemplate"
            class="form-select"
          >
            <option value="">-- Select Message Type --</option>
            <option value="action_goal">Action Goal (Attack Move)</option>
            <option value="action_feedback">Action Feedback (Progress Update)</option>
            <option value="action_result">Action Result (Completion)</option>
            <option value="custom">Custom Message</option>
          </select>
        </div>

        <!-- Topic Input -->
        <div class="form-group">
          <label for="topic">Topic:</label>
          <input 
            id="topic" 
            v-model="topic" 
            type="text" 
            class="form-input"
            placeholder="e.g., /robot/move_piece/goal"
          />
        </div>

        <!-- JSON Message Input -->
        <div class="form-group">
          <label for="messageJson">JSON Message:</label>
          <textarea 
            id="messageJson"
            v-model="messageJson" 
            class="json-textarea"
            placeholder="Enter your JSON message here..."
            rows="15"
          ></textarea>
        </div>

        <!-- Validation -->
        <div v-if="validationError" class="error-message">
          <strong>JSON Error:</strong> {{ validationError }}
        </div>

        <!-- Send Button -->
        <div class="form-actions">
          <button 
            @click="sendMessage" 
            :disabled="!canSend"
            class="btn-primary"
          >
            Send Message
          </button>
          <button 
            @click="formatJson" 
            class="btn-secondary"
          >
            Format JSON
          </button>
          <button 
            @click="clearMessage" 
            class="btn-secondary"
          >
            Clear
          </button>
        </div>
      </div>
    </div>

    <!-- Send History -->
    <div class="card mt-4">
      <div class="card-header">
        <h4>Send History</h4>
        <button @click="clearHistory" class="btn-small">Clear History</button>
      </div>
      <div class="card-body">
        <div v-if="sendHistory.length === 0" class="no-history">
          No messages sent yet.
        </div>
        <div v-else class="history-list">
          <div 
            v-for="(item, index) in sendHistory" 
            :key="index" 
            class="history-item"
          >
            <div class="history-header">
              <strong>{{ item.timestamp }}</strong>
              <span class="topic-badge">{{ item.topic }}</span>
              <span :class="['status-badge', item.success ? 'success' : 'error']">
                {{ item.success ? 'Sent' : 'Failed' }}
              </span>
            </div>
            <pre class="history-json">{{ JSON.stringify(item.message, null, 2) }}</pre>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { mqttService } from '../services/mqttService'

// Reactive data
const selectedMessageType = ref('')
const topic = ref('')
const messageJson = ref('')
const validationError = ref('')
const sendHistory = ref<Array<{
  timestamp: string
  topic: string
  message: any
  success: boolean
}>>([])

// Message templates
const messageTemplates = {
  action_goal: {
    topic: 'robot/move_piece/goal',
    message: {
      goal_id: "attack_001",
      header: {
        timestamp: "2025-09-08T14:25:08Z"
      },
      move: {
        type: "attack",
        from: "d1",
        to: "f7",
        from_piece: "black_queen",
        to_piece: "white_knight",
        notation: "Qd1xf7+",
        results_in_check: true
      }
    }
  },
  action_feedback: {
    topic: '/robot/move_piece/feedback',
    message: {
      goal_id: "attack_001",
      header: {
        timestamp: "2025-09-08T14:25:12Z"
      },
      current_step: "moving_attacking_piece",
      step_details: "navigating_to_d1",
      progress: 0.6,
      estimated_time_remaining: 6.0,
      current_position: {
        moving_to: "d1",
        purpose: "pickup_attacking_queen"
      }
    }
  },
  action_result: {
    topic: '/robot/move_piece/result',
    message: {
      goal_id: "attack_001",
      header: {
        timestamp: "2025-09-08T14:25:18Z"
      },
      status: "succeeded",
      move_completed: {
        type: "attack",
        from: "d1",
        to: "f7",
        attacking_piece: "queen",
        captured_piece: "knight",
        notation: "Qd1xf7+"
      },
      execution_time: 18.0,
      pieces_moved: 2,
      captured_piece_location: "capture_zone_black",
      board_state_changed: true,
      error_message: null
    }
  }
}

// Computed properties
const canSend = computed(() => {
  return topic.value.trim() !== '' && 
         messageJson.value.trim() !== '' && 
         validationError.value === ''
})

// Methods
function loadTemplate() {
  if (selectedMessageType.value && selectedMessageType.value !== 'custom') {
    const template = messageTemplates[selectedMessageType.value as keyof typeof messageTemplates]
    topic.value = template.topic
    messageJson.value = JSON.stringify(template.message, null, 2)
    validationError.value = ''
  } else if (selectedMessageType.value === 'custom') {
    topic.value = ''
    messageJson.value = ''
    validationError.value = ''
  }
}

function formatJson() {
  try {
    const parsed = JSON.parse(messageJson.value)
    messageJson.value = JSON.stringify(parsed, null, 2)
    validationError.value = ''
  } catch (error) {
    validationError.value = (error as Error).message
  }
}

function clearMessage() {
  selectedMessageType.value = ''
  topic.value = ''
  messageJson.value = ''
  validationError.value = ''
}

function validateJson(): boolean {
  try {
    JSON.parse(messageJson.value)
    validationError.value = ''
    return true
  } catch (error) {
    validationError.value = (error as Error).message
    return false
  }
}

function sendMessage() {
  if (!validateJson()) {
    return
  }

  try {
    const message = JSON.parse(messageJson.value)
    
    // Send via MQTT
    mqttService.publishMessage(topic.value, JSON.stringify(message))
    
    // Add to history
    sendHistory.value.unshift({
      timestamp: new Date().toLocaleString(),
      topic: topic.value,
      message: message,
      success: true
    })
    
    // Keep only last 10 messages
    if (sendHistory.value.length > 10) {
      sendHistory.value = sendHistory.value.slice(0, 10)
    }
    
    console.log('Message sent successfully:', { topic: topic.value, message })
    
  } catch (error) {
    console.error('Failed to send message:', error)
    
    // Add failed attempt to history
    sendHistory.value.unshift({
      timestamp: new Date().toLocaleString(),
      topic: topic.value,
      message: messageJson.value,
      success: false
    })
  }
}

function clearHistory() {
  sendHistory.value = []
}

// Watch for JSON changes to validate
import { watch } from 'vue'
watch(messageJson, () => {
  if (messageJson.value.trim() !== '') {
    validateJson()
  } else {
    validationError.value = ''
  }
})
</script>

<style scoped>
.robot-message-sender {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

.card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
}

.card-header {
  padding: 20px;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h3, .card-header h4 {
  margin: 0;
  color: #333;
}

.card-body {
  padding: 20px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: 600;
  color: #555;
}

.form-select, .form-input {
  width: 100%;
  padding: 10px;
  border: 2px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
  transition: border-color 0.3s;
}

.form-select:focus, .form-input:focus {
  outline: none;
  border-color: #4CAF50;
}

.json-textarea {
  width: 100%;
  padding: 10px;
  border: 2px solid #ddd;
  border-radius: 4px;
  font-family: 'Courier New', monospace;
  font-size: 12px;
  resize: vertical;
  min-height: 300px;
  background-color: #f8f9fa;
  transition: border-color 0.3s;
}

.json-textarea:focus {
  outline: none;
  border-color: #4CAF50;
}

.error-message {
  background-color: #ffebee;
  border: 1px solid #f44336;
  color: #d32f2f;
  padding: 10px;
  border-radius: 4px;
  margin-bottom: 15px;
}

.form-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.btn-primary, .btn-secondary, .btn-small {
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.3s;
}

.btn-primary {
  background-color: #4CAF50;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #45a049;
}

.btn-primary:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #f1f1f1;
  color: #333;
}

.btn-secondary:hover {
  background-color: #e7e7e7;
}

.btn-small {
  padding: 5px 10px;
  font-size: 12px;
  background-color: #f44336;
  color: white;
}

.btn-small:hover {
  background-color: #d32f2f;
}

.no-history {
  text-align: center;
  color: #777;
  padding: 20px;
}

.history-list {
  max-height: 400px;
  overflow-y: auto;
}

.history-item {
  border: 1px solid #eee;
  border-radius: 4px;
  margin-bottom: 15px;
  background-color: #fafafa;
}

.history-header {
  padding: 10px 15px;
  background-color: #f5f5f5;
  border-bottom: 1px solid #eee;
  display: flex;
  gap: 10px;
  align-items: center;
}

.topic-badge {
  background-color: #2196F3;
  color: white;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
}

.status-badge {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: bold;
}

.status-badge.success {
  background-color: #4CAF50;
  color: white;
}

.status-badge.error {
  background-color: #f44336;
  color: white;
}

.history-json {
  padding: 10px 15px;
  margin: 0;
  background-color: white;
  font-family: 'Courier New', monospace;
  font-size: 11px;
  overflow-x: auto;
  white-space: pre-wrap;
  border-radius: 0 0 4px 4px;
}

.mt-4 {
  margin-top: 1.5rem;
}

/* Dark mode support */
@media (prefers-color-scheme: dark) {
  .card {
    background: #2d2d2d;
    color: #fff;
  }
  
  .card-header {
    border-bottom-color: #444;
  }
  
  .form-select, .form-input, .json-textarea {
    background-color: #3d3d3d;
    border-color: #555;
    color: #fff;
  }
  
  .json-textarea {
    background-color: #2a2a2a;
  }
  
  .history-item {
    background-color: #3d3d3d;
    border-color: #555;
  }
  
  .history-header {
    background-color: #4d4d4d;
    border-bottom-color: #555;
  }
  
  .history-json {
    background-color: #2a2a2a;
    color: #fff;
  }
}
</style>
