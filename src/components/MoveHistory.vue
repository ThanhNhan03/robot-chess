<template>
  <div class="move-history">
    <div class="history-header">
      <h3>Lịch sử nước đi</h3>
      <div class="game-info">
        <span class="move-count">Nước: {{ moveHistory.length }}</span>
        <span class="current-turn">Lượt: {{ gameState.turn === 'w' ? 'Trắng' : 'Đen' }}</span>
      </div>
    </div>
    
    <div class="history-content">
      <div v-if="moveHistory.length === 0" class="no-moves">
        <p>Chưa có nước đi nào</p>
      </div>
      
      <div v-else class="moves-list">
        <div
          v-for="(move, index) in moveHistory"
          :key="index"
          :class="['move-item', { 'active': index === selectedMoveIndex }]"
          @click="selectMove(index)"
        >
          <div class="move-number">
            <span v-if="index % 2 === 0">{{ Math.floor(index / 2) + 1 }}.</span>
          </div>
          <div class="move-notation">{{ move.san }}</div>
          <div class="move-details">
            <span class="move-from-to">{{ move.from }}-{{ move.to }}</span>
          </div>
          <div v-if="move.captured" class="capture-indicator">×</div>
          <div v-if="move.flags.includes('+')" class="check-indicator">+</div>
          <div v-if="move.flags.includes('#')" class="checkmate-indicator">#</div>
        </div>
      </div>
    </div>
    
    <div class="game-status-info" v-if="gameState">
      <div class="status-row" v-if="gameState.isCheck">
        <span class="status-label">🔥 Chiếu bí!</span>
      </div>
      <div class="status-row" v-if="gameState.isCheckmate">
        <span class="status-label">👑 Chiếu hết!</span>
      </div>
      <div class="status-row" v-if="gameState.isStalemate">
        <span class="status-label">🤝 Hòa - Bí quân</span>
      </div>
      <div class="status-row" v-if="gameState.isDraw">
        <span class="status-label">🤝 Hòa</span>
      </div>
    </div>
    
    <div class="history-controls">
      <button @click="$emit('undo-move')" :disabled="moveHistory.length === 0" class="control-btn">
        ↶ Hoàn tác
      </button>
      <button @click="$emit('reset-game')" :disabled="moveHistory.length === 0" class="control-btn danger">
        � Reset
      </button>
      <button @click="exportPGN" :disabled="moveHistory.length === 0" class="control-btn">
        📄 Xuất PGN
      </button>
    </div>
    
    <div class="game-stats">
      <div class="stat-item">
        <span class="stat-label">Tổng nước đi:</span>
        <span class="stat-value">{{ moveHistory.length }}</span>
      </div>
      <div class="stat-item">
        <span class="stat-label">Số lần bắt quân:</span>
        <span class="stat-value">{{ captureCount }}</span>
      </div>
      <div class="stat-item">
        <span class="stat-label">Nước đi khả thi:</span>
        <span class="stat-value">{{ gameState?.moves.length || 0 }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { GameMove, GameState } from '../chess/ChessGame'

// Props
const props = defineProps<{
  moveHistory: GameMove[]
  gameState: GameState
  pgn: string
}>()

// Emits
const emit = defineEmits<{
  'undo-move': []
  'reset-game': []
}>()

const selectedMoveIndex = ref(-1)

const captureCount = computed(() => {
  return props.moveHistory.filter(move => move.captured).length
})

const selectMove = (index: number) => {
  selectedMoveIndex.value = index
}

const exportPGN = () => {
  if (!props.pgn) {
    alert('Không có dữ liệu PGN để xuất!')
    return
  }

  let pgn = '[Event "Robot Chess Game"]\n'
  pgn += '[Site "Robot Chess App"]\n'
  pgn += '[Date "' + new Date().toISOString().split('T')[0] + '"]\n'
  pgn += '[Round "1"]\n'
  pgn += '[White "Player"]\n'
  pgn += '[Black "Robot"]\n'
  pgn += '[Result "*"]\n\n'
  
  pgn += props.pgn
  
  // Add game result
  if (props.gameState.isCheckmate) {
    const winner = props.gameState.turn === 'w' ? '0-1' : '1-0'
    pgn += ` ${winner}`
  } else if (props.gameState.isDraw || props.gameState.isStalemate) {
    pgn += ' 1/2-1/2'
  } else {
    pgn += ' *'
  }
  
  const blob = new Blob([pgn], { type: 'text/plain' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `chess-game-${new Date().getTime()}.pgn`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}
</script>

<style scoped>
.move-history {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  overflow: hidden;
  height: 100%;
  display: flex;
  flex-direction: column;
  min-height: 400px;
}

.history-header {
  background: #34495e;
  color: white;
  padding: 12px 15px;
}

.history-header h3 {
  margin: 0 0 8px 0;
  font-size: clamp(16px, 3vw, 18px);
}

.game-info {
  display: flex;
  gap: 15px;
  font-size: clamp(12px, 2.5vw, 14px);
  color: #bdc3c7;
  flex-wrap: wrap;
}

.history-content {
  flex: 1;
  overflow-y: auto;
  max-height: calc(100vh - 400px);
  min-height: 200px;
}

.no-moves {
  text-align: center;
  padding: 30px 20px;
  color: #7f8c8d;
}

.moves-list {
  padding: 8px 0;
}

.move-item {
  display: grid;
  grid-template-columns: 35px 1fr 60px auto auto auto;
  gap: 6px;
  padding: 6px 15px;
  cursor: pointer;
  transition: background 0.2s;
  align-items: center;
  font-size: clamp(12px, 2.5vw, 14px);
}

.move-item:hover {
  background: #f8f9fa;
}

.move-item.active {
  background: #e3f2fd;
  border-left: 4px solid #2196f3;
}

.move-number {
  font-weight: bold;
  color: #7f8c8d;
  font-size: clamp(11px, 2vw, 14px);
}

.move-notation {
  font-family: 'Courier New', monospace;
  font-weight: bold;
  font-size: clamp(13px, 2.5vw, 16px);
}

.move-details {
  font-size: clamp(10px, 2vw, 12px);
  color: #7f8c8d;
}

.move-from-to {
  font-family: 'Courier New', monospace;
}

.game-status-info {
  background: #fff3cd;
  border: 1px solid #ffeaa7;
  padding: 8px 15px;
  margin: 0;
}

.status-row {
  margin: 2px 0;
}

.status-label {
  font-weight: bold;
  color: #856404;
}

.capture-indicator {
  color: #e74c3c;
  font-weight: bold;
}

.check-indicator {
  color: #f39c12;
  font-weight: bold;
}

.checkmate-indicator {
  color: #e74c3c;
  font-weight: bold;
  font-size: clamp(14px, 3vw, 18px);
}

.history-controls {
  background: #ecf0f1;
  padding: 10px 15px;
  display: flex;
  gap: 6px;
  justify-content: space-between;
  flex-wrap: wrap;
}

.control-btn {
  background: #3498db;
  color: white;
  border: none;
  padding: 6px 8px;
  border-radius: 6px;
  cursor: pointer;
  font-size: clamp(10px, 2vw, 12px);
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 3px;
  min-width: 60px;
  justify-content: center;
}

.control-btn:hover:not(:disabled) {
  background: #2980b9;
  transform: translateY(-1px);
}

.control-btn:disabled {
  background: #bdc3c7;
  cursor: not-allowed;
  transform: none;
}

.control-btn.danger {
  background: #e74c3c;
}

.control-btn.danger:hover:not(:disabled) {
  background: #c0392b;
}

.game-stats {
  background: #f8f9fa;
  padding: 10px 15px;
  border-top: 1px solid #dee2e6;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  margin-bottom: 6px;
  font-size: clamp(12px, 2.5vw, 14px);
  flex-wrap: wrap;
  gap: 5px;
}

.stat-item:last-child {
  margin-bottom: 0;
}

.stat-label {
  color: #7f8c8d;
}

.stat-value {
  font-weight: bold;
  color: #2c3e50;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .move-item {
    grid-template-columns: 30px 1fr 45px auto auto auto;
    gap: 4px;
    padding: 5px 12px;
  }
  
  .history-controls {
    padding: 8px 12px;
    gap: 4px;
  }
  
  .control-btn {
    padding: 5px 6px;
    min-width: 50px;
  }
  
  .game-stats {
    padding: 8px 12px;
  }
}

@media (max-width: 480px) {
  .history-header {
    padding: 10px 12px;
  }
  
  .game-info {
    gap: 10px;
  }
  
  .move-item {
    grid-template-columns: 25px 1fr 40px auto;
    padding: 4px 10px;
  }
  
  .capture-indicator,
  .check-indicator,
  .checkmate-indicator {
    display: none;
  }
  
  .history-controls {
    flex-direction: column;
    gap: 8px;
  }
  
  .control-btn {
    width: 100%;
    min-width: auto;
  }
}
</style>
