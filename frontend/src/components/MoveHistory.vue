<template>
  <div class="move-history">
    <div class="history-header">
      <h3>Lịch sử nước đi</h3>
      <div class="game-info">
        <span class="move-count">Nước: {{ moveHistory.length }}</span>
        <span class="current-turn">Lượt: {{ currentTurn === 'white' ? 'Trắng' : 'Đen' }}</span>
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
          <div class="move-number">{{ Math.floor(index / 2) + 1 }}.</div>
          <div class="move-notation">{{ move.notation }}</div>
          <div class="move-time">{{ formatTime(move.timestamp) }}</div>
          <div v-if="move.capture" class="capture-indicator">×</div>
          <div v-if="move.check" class="check-indicator">+</div>
          <div v-if="move.checkmate" class="checkmate-indicator">#</div>
        </div>
      </div>
    </div>
    
    <div class="history-controls">
      <button @click="undoLastMove" :disabled="moveHistory.length === 0" class="control-btn">
        ↶ Hoàn tác
      </button>
      <button @click="clearHistory" :disabled="moveHistory.length === 0" class="control-btn danger">
        🗑️ Xóa hết
      </button>
      <button @click="exportPGN" :disabled="moveHistory.length === 0" class="control-btn">
        📄 Xuất PGN
      </button>
    </div>
    
    <div class="game-stats">
      <div class="stat-item">
        <span class="stat-label">Thời gian trò chơi:</span>
        <span class="stat-value">{{ formatGameTime(gameStartTime) }}</span>
      </div>
      <div class="stat-item">
        <span class="stat-label">Số lần bắt quân:</span>
        <span class="stat-value">{{ captureCount }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

interface ChessMove {
  notation: string
  from: string
  to: string
  piece: string
  capture: boolean
  check: boolean
  checkmate: boolean
  timestamp: Date
  player: 'white' | 'black'
}

const moveHistory = ref<ChessMove[]>([])
const selectedMoveIndex = ref(-1)
const currentTurn = ref<'white' | 'black'>('white')
const gameStartTime = ref<Date>(new Date())

// Sample moves for demonstration
const sampleMoves: ChessMove[] = [
  {
    notation: 'e4',
    from: 'e2',
    to: 'e4',
    piece: 'pawn',
    capture: false,
    check: false,
    checkmate: false,
    timestamp: new Date(Date.now() - 120000),
    player: 'white'
  },
  {
    notation: 'e5',
    from: 'e7',
    to: 'e5',
    piece: 'pawn',
    capture: false,
    check: false,
    checkmate: false,
    timestamp: new Date(Date.now() - 110000),
    player: 'black'
  },
  {
    notation: 'Nf3',
    from: 'g1',
    to: 'f3',
    piece: 'knight',
    capture: false,
    check: false,
    checkmate: false,
    timestamp: new Date(Date.now() - 100000),
    player: 'white'
  },
  {
    notation: 'Nc6',
    from: 'b8',
    to: 'c6',
    piece: 'knight',
    capture: false,
    check: false,
    checkmate: false,
    timestamp: new Date(Date.now() - 90000),
    player: 'black'
  }
]

const captureCount = computed(() => {
  return moveHistory.value.filter(move => move.capture).length
})

const selectMove = (index: number) => {
  selectedMoveIndex.value = index
}

const undoLastMove = () => {
  if (moveHistory.value.length > 0) {
    moveHistory.value.pop()
    currentTurn.value = currentTurn.value === 'white' ? 'black' : 'white'
  }
}

const clearHistory = () => {
  if (confirm('Bạn có chắc chắn muốn xóa toàn bộ lịch sử nước đi?')) {
    moveHistory.value = []
    selectedMoveIndex.value = -1
    currentTurn.value = 'white'
    gameStartTime.value = new Date()
  }
}

const exportPGN = () => {
  let pgn = '[Event "Robot Chess Game"]\n'
  pgn += '[Date "' + new Date().toISOString().split('T')[0] + '"]\n'
  pgn += '[White "Player"]\n'
  pgn += '[Black "Robot"]\n\n'
  
  for (let i = 0; i < moveHistory.value.length; i += 2) {
    const moveNumber = Math.floor(i / 2) + 1
    pgn += `${moveNumber}. ${moveHistory.value[i].notation}`
    
    if (i + 1 < moveHistory.value.length) {
      pgn += ` ${moveHistory.value[i + 1].notation}`
    }
    pgn += ' '
  }
  
  const blob = new Blob([pgn], { type: 'text/plain' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `chess-game-${new Date().getTime()}.pgn`
  a.click()
  URL.revokeObjectURL(url)
}

const formatTime = (timestamp: Date) => {
  return timestamp.toLocaleTimeString('vi-VN', { 
    hour: '2-digit', 
    minute: '2-digit', 
    second: '2-digit' 
  })
}

const formatGameTime = (startTime: Date) => {
  const now = new Date()
  const diff = Math.floor((now.getTime() - startTime.getTime()) / 1000)
  const minutes = Math.floor(diff / 60)
  const seconds = diff % 60
  return `${minutes}:${seconds.toString().padStart(2, '0')}`
}

// Add sample moves on mount
onMounted(() => {
  moveHistory.value = [...sampleMoves]
})
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
  grid-template-columns: 35px 1fr 50px auto auto auto;
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

.move-time {
  font-size: clamp(10px, 2vw, 12px);
  color: #7f8c8d;
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
