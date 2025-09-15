<template>
  <div class="move-command">
    <div class="card">
      <div class="card-header">
        <h3>🎯 Move Command</h3>
        <div class="connection-status" :class="{ connected: isConnected, mock: isMockMode }">
          {{ isMockMode ? 'Mock Mode' : (isConnected ? 'Connected' : 'Disconnected') }}
        </div>
      </div>

      <div class="card-body">
        <form @submit.prevent="sendMoveCommand" class="move-form">
          <!-- Move Type Selection -->
          <div class="form-group">
            <label for="moveType">Move Type:</label>
            <select 
              id="moveType" 
              v-model="moveData.type" 
              class="form-control"
              required
            >
              <option value="">Select move type</option>
              <option value="move">Normal Move</option>
              <option value="attack">Attack/Capture</option>
              <option value="castle">Castling</option>
              <option value="en_passant">En Passant</option>
              <option value="promotion">Promotion</option>
            </select>
          </div>

          <!-- From Square -->
          <div class="form-group">
            <label for="fromSquare">From Square:</label>
            <input
              id="fromSquare"
              v-model="moveData.from"
              type="text"
              class="form-control"
              placeholder="e.g., d1"
              pattern="[a-h][1-8]"
              maxlength="2"
              required
            />
          </div>

          <!-- To Square -->
          <div class="form-group">
            <label for="toSquare">To Square:</label>
            <input
              id="toSquare"
              v-model="moveData.to"
              type="text"
              class="form-control"
              placeholder="e.g., f7"
              pattern="[a-h][1-8]"
              maxlength="2"
              required
            />
          </div>

          <!-- From Piece -->
          <div class="form-group">
            <label for="fromPiece">From Piece:</label>
            <select 
              id="fromPiece" 
              v-model="moveData.from_piece" 
              class="form-control"
              required
            >
              <option value="">Select piece</option>
              <optgroup label="White Pieces">
                <option value="white_pawn">White Pawn</option>
                <option value="white_rook">White Rook</option>
                <option value="white_knight">White Knight</option>
                <option value="white_bishop">White Bishop</option>
                <option value="white_queen">White Queen</option>
                <option value="white_king">White King</option>
              </optgroup>
              <optgroup label="Black Pieces">
                <option value="black_pawn">Black Pawn</option>
                <option value="black_rook">Black Rook</option>
                <option value="black_knight">Black Knight</option>
                <option value="black_bishop">Black Bishop</option>
                <option value="black_queen">Black Queen</option>
                <option value="black_king">Black King</option>
              </optgroup>
            </select>
          </div>

          <!-- To Piece (Optional for captures) -->
          <div class="form-group">
            <label for="toPiece">To Piece (if capturing):</label>
            <select 
              id="toPiece" 
              v-model="moveData.to_piece" 
              class="form-control"
            >
              <option value="">None (empty square)</option>
              <optgroup label="White Pieces">
                <option value="white_pawn">White Pawn</option>
                <option value="white_rook">White Rook</option>
                <option value="white_knight">White Knight</option>
                <option value="white_bishop">White Bishop</option>
                <option value="white_queen">White Queen</option>
                <option value="white_king">White King</option>
              </optgroup>
              <optgroup label="Black Pieces">
                <option value="black_pawn">Black Pawn</option>
                <option value="black_rook">Black Rook</option>
                <option value="black_knight">Black Knight</option>
                <option value="black_bishop">Black Bishop</option>
                <option value="black_queen">Black Queen</option>
                <option value="black_king">Black King</option>
              </optgroup>
            </select>
          </div>

          <!-- Additional Options -->
          <div class="form-group">
            <label>
              <input
                type="checkbox"
                v-model="moveData.results_in_check"
                class="checkbox"
              />
              Results in Check
            </label>
          </div>

          <!-- Auto-generated fields (read-only, hidden) -->
          <div class="form-group" style="display: none;">
            <label for="goalId">Goal ID:</label>
            <input
              id="goalId"
              v-model="goalId"
              type="text"
              class="form-control"
              readonly
            />
          </div>

          <div class="form-group" style="display: none;">
            <label for="notation">Notation (auto-generated):</label>
            <input
              id="notation"
              v-model="generatedNotation"
              type="text"
              class="form-control"
              readonly
            />
          </div>

          <!-- Submit Button -->
          <div class="form-actions">
            <button 
              type="submit" 
              class="btn btn-primary"
              :disabled="!isConnected || isLoading"
            >
              <span v-if="isLoading">⏳ Sending...</span>
              <span v-else>🚀 Send Move Command</span>
            </button>
            
            <button 
              type="button" 
              @click="clearForm" 
              class="btn btn-secondary"
            >
              🗑️ Clear
            </button>
          </div>
        </form>

        <!-- Last Sent Message Preview -->
        <div v-if="lastSentMessage" class="message-preview">
          <h4>📤 Last Sent Message:</h4>
          <pre>{{ lastSentMessage }}</pre>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { mqttService } from '../services/mqttService'

// Reactive data
const moveData = ref({
  type: '',
  from: '',
  to: '',
  from_piece: '',
  to_piece: '',
  results_in_check: false
})

const goalId = ref('')
const isLoading = ref(false)
const isConnected = ref(false)
const isMockMode = ref(false)
const lastSentMessage = ref('')

// Generate goal ID
const generateGoalId = () => {
  const timestamp = Date.now()
  const random = Math.random().toString(36).substr(2, 4)
  return `${moveData.value.type}_${timestamp}_${random}`
}

// Generate chess notation (basic implementation)
const generatedNotation = computed(() => {
  if (!moveData.value.from || !moveData.value.to || !moveData.value.from_piece) {
    return ''
  }

  const piece = moveData.value.from_piece
  const from = moveData.value.from
  const to = moveData.value.to
  const isCapture = moveData.value.to_piece && moveData.value.to_piece !== ''
  const isCheck = moveData.value.results_in_check

  // Get piece notation (first letter, uppercase for pieces, empty for pawns)
  let pieceNotation = ''
  if (piece.includes('queen')) pieceNotation = 'Q'
  else if (piece.includes('rook')) pieceNotation = 'R'
  else if (piece.includes('bishop')) pieceNotation = 'B'
  else if (piece.includes('knight')) pieceNotation = 'N'
  else if (piece.includes('king')) pieceNotation = 'K'
  // Pawn moves don't have piece notation

  // Build notation
  let notation = pieceNotation
  
  // For pawn captures, include the from file
  if (!pieceNotation && isCapture) {
    notation += from[0]
  }
  
  // Add capture symbol
  if (isCapture) {
    notation += 'x'
  }
  
  // Add destination square
  notation += to
  
  // Add check symbol
  if (isCheck) {
    notation += '+'
  }

  return notation
})

// Update connection status
const updateConnectionStatus = () => {
  const info = mqttService.getConnectionInfo()
  isConnected.value = info.isConnected
  isMockMode.value = info.isMockMode
}

// Send move command
const sendMoveCommand = async () => {
  if (!isConnected.value) {
    alert('Not connected to MQTT broker!')
    return
  }

  isLoading.value = true
  goalId.value = generateGoalId()

  try {
    const message = {
      goal_id: goalId.value,
      header: {
        timestamp: new Date().toISOString()
      },
      move: {
        type: moveData.value.type,
        from: moveData.value.from,
        to: moveData.value.to,
        from_piece: moveData.value.from_piece,
        to_piece: moveData.value.to_piece || null,
        notation: generatedNotation.value,
        results_in_check: moveData.value.results_in_check
      }
    }

    // Publish message to MQTT
    const success = await publishMoveMessage(message)
    
    if (success) {
      lastSentMessage.value = JSON.stringify(message, null, 2)
      console.log('✅ Move command sent successfully:', message)
    } else {
      alert('Failed to send move command!')
    }
  } catch (error) {
    console.error('❌ Error sending move command:', error)
    alert('Error sending move command: ' + error)
  } finally {
    isLoading.value = false
  }
}

// Publish message via MQTT service
const publishMoveMessage = async (message: any): Promise<boolean> => {
  try {
    const topic = 'robot/move_piece/goal'
    console.log(`📤 Sending move command to ${topic}:`, message)
    
    // Use the publishMessage method from MQTT service
    const success = mqttService.publishMessage(topic, message)
    
    if (success) {
      console.log('✅ Move command sent successfully')
      return true
    } else {
      console.error('❌ Failed to send move command')
      return false
    }
  } catch (error) {
    console.error('❌ Error publishing move message:', error)
    return false
  }
}

// Clear form
const clearForm = () => {
  moveData.value = {
    type: '',
    from: '',
    to: '',
    from_piece: '',
    to_piece: '',
    results_in_check: false
  }
  goalId.value = ''
  lastSentMessage.value = ''
}

// Lifecycle
onMounted(async () => {
  // Connect to MQTT if not connected
  if (!mqttService.getConnectionStatus()) {
    await mqttService.connect()
  }
  updateConnectionStatus()
  
  // Update connection status periodically
  const interval = setInterval(updateConnectionStatus, 1000)
  
  onUnmounted(() => {
    clearInterval(interval)
  })
})
</script>

<style scoped>
.move-command {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.card {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  height: 100%;
}

.card-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 15px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h3 {
  margin: 0;
  font-size: 1.2rem;
  font-weight: 600;
}

.connection-status {
  background: rgba(255, 255, 255, 0.2);
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 500;
}

.connection-status.connected {
  background: rgba(76, 175, 80, 0.8);
}

.connection-status.mock {
  background: rgba(255, 152, 0, 0.8);
}

.card-body {
  padding: 20px;
  flex: 1;
  overflow-y: auto;
}

.move-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.form-group label {
  font-weight: 500;
  color: #2c3e50;
  font-size: 0.9rem;
}

.form-control {
  padding: 8px 12px;
  border: 2px solid #e1e5e9;
  border-radius: 6px;
  font-size: 0.9rem;
  transition: border-color 0.3s ease;
  background: white;
}

.form-control:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-control:read-only {
  background: #f8f9fa;
  color: #6c757d;
}

.checkbox {
  margin-right: 8px;
  transform: scale(1.1);
}

.form-actions {
  display: flex;
  gap: 10px;
  margin-top: 10px;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  font-weight: 500;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s ease;
  flex: 1;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover {
  background: #5a6268;
  transform: translateY(-1px);
}

.message-preview {
  margin-top: 20px;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 6px;
  border-left: 4px solid #667eea;
}

.message-preview h4 {
  margin: 0 0 10px 0;
  color: #2c3e50;
  font-size: 1rem;
}

.message-preview pre {
  margin: 0;
  background: #2d3748;
  color: #e2e8f0;
  padding: 12px;
  border-radius: 4px;
  font-size: 0.8rem;
  overflow-x: auto;
  white-space: pre-wrap;
  word-break: break-all;
}

/* Responsive */
@media (max-width: 768px) {
  .card-header {
    padding: 12px 15px;
    flex-direction: column;
    gap: 8px;
    text-align: center;
  }
  
  .card-body {
    padding: 15px;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .message-preview pre {
    font-size: 0.75rem;
  }
}
</style>
