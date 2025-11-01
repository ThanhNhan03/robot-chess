<template>
  <div class="move-history">
    <div class="history-header">
      <h3>Lịch sử nước đi</h3>
      <div class="current-player-display">
        <span class="player-label">Lượt đi:</span>
        <span :class="['player-indicator', { 'white': currentTurn === 'white', 'black': currentTurn === 'black' }]">
          {{ currentTurn === 'white' ? 'Trắng' : 'Đen' }}
        </span>
      </div>
      <div class="game-info">
        <span class="move-count">Nước: {{ moveHistory.length }}</span>
        <div v-if="isReceivingExternalMove" class="status-receiving">Robot đang di chuyển...</div>
        <div v-else-if="isCheck && !isGameOver" class="status-check">Chiếu!</div>
        <div v-if="isCheckmate" class="status-checkmate">Chiếu hết! {{ currentTurn === 'white' ? 'Đen' : 'Trắng' }} thắng!</div>
        <div v-if="isStalemate" class="status-stalemate">Hòa cờ!</div>
      </div>
    </div>
    
    <div class="history-content">
      <div v-if="moveHistory.length === 0" class="no-moves">
        <p>Chưa có nước đi nào</p>
      </div>
      
      <div v-else class="moves-list">
        <div
          v-for="(move, index) in displayMoves"
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
      <button @click="handleUndoMove" :disabled="moveHistory.length === 0" class="control-btn undo">
        ↶ Hoàn tác
      </button>
      <button @click="handleNewGame" class="control-btn new-game">
        🎯 Ván mới
      </button>
      <button @click="handleClearHistory" :disabled="moveHistory.length === 0" class="control-btn danger">
        🗑️ Xóa hết
      </button>
      <button @click="handleExportPGN" :disabled="moveHistory.length === 0" class="control-btn">
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
import { ref, computed } from 'vue'

// Props từ parent component
interface Props {
  moveHistory?: string[]
  currentTurn?: 'white' | 'black'
  isReceivingExternalMove?: boolean
  isCheck?: boolean
  isGameOver?: boolean
  isCheckmate?: boolean
  isStalemate?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  moveHistory: () => [],
  currentTurn: 'white',
  isReceivingExternalMove: false,
  isCheck: false,
  isGameOver: false,
  isCheckmate: false,
  isStalemate: false
})

// Events emit to parent
const emit = defineEmits<{
  undoMove: []
  clearHistory: []
  exportPGN: []
  newGame: []
}>()

const selectedMoveIndex = ref(-1)
const gameStartTime = ref<Date>(new Date())

// Use props data instead of local data
const moveHistory = computed(() => props.moveHistory || [])
const currentTurn = computed(() => props.currentTurn)

// Convert string moves to display format with additional info
const displayMoves = computed(() => {
  return moveHistory.value.map((move, index) => ({
    notation: move,
    from: '',
    to: '',
    piece: '',
    capture: move.includes('x'),
    check: move.includes('+') && !move.includes('#'),
    checkmate: move.includes('#'),
    timestamp: new Date(Date.now() - (moveHistory.value.length - index) * 30000), // Fake timestamps
    player: index % 2 === 0 ? 'white' : 'black' as 'white' | 'black'
  }))
})

const captureCount = computed(() => {
  return displayMoves.value.filter(move => move.capture).length
})

const selectMove = (index: number) => {
  selectedMoveIndex.value = index
}

const handleUndoMove = () => {
  emit('undoMove')
}

const handleNewGame = () => {
  if (moveHistory.value.length > 0 && confirm('Bạn có chắc chắn muốn bắt đầu ván mới?')) {
    emit('newGame')
    selectedMoveIndex.value = -1
    gameStartTime.value = new Date()
  } else if (moveHistory.value.length === 0) {
    emit('newGame')
  }
}

const handleClearHistory = () => {
  if (confirm('Bạn có chắc chắn muốn xóa toàn bộ lịch sử nước đi?')) {
    emit('clearHistory')
    selectedMoveIndex.value = -1
    gameStartTime.value = new Date()
  }
}

const handleExportPGN = () => {
  emit('exportPGN')
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
</script>

<style scoped src="../assets/styles/MoveHistory.css">
</style>
