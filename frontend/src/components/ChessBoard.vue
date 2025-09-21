<template>
  <div class="chess-board">
    <!-- Game Status -->
    <div class="game-status">
      <div class="current-player">
        Current Player: <span :class="{ 'white': currentPlayer === 'white', 'black': currentPlayer === 'black' }">
          {{ currentPlayer === 'white' ? 'White' : 'Black' }}
        </span>
      </div>
      <div v-if="isReceivingExternalMove" class="status-warning">Receiving robot move...</div>
      <div v-else-if="isCheck && !isGameOver" class="status-warning">Check!</div>
      <div v-if="isCheckmate" class="status-game-over">Checkmate! {{ currentPlayer === 'white' ? 'Black' : 'White' }} wins!</div>
      <div v-if="isStalemate" class="status-game-over">Stalemate! Game is a draw.</div>
    </div>

    <div class="board-container">
      <div class="board-grid">
        <div
          v-for="(_row, rowIndex) in 8"
          :key="rowIndex"
          class="board-row"
        >
          <div
            v-for="(_col, colIndex) in 8"
            :key="colIndex"
            :class="[
              'chess-square',
              (rowIndex + colIndex) % 2 === 0 ? 'light' : 'dark',
              { 
                'selected': isSquareSelected(rowIndex, colIndex),
                'possible-move': isPossibleMove(rowIndex, colIndex),
                'in-check': isSquareInCheck(rowIndex, colIndex)
              }
            ]"
            @click="handleSquareClick(rowIndex, colIndex)"
          >
            <img
              v-if="getPieceImage(rowIndex, colIndex)"
              :src="getPieceImage(rowIndex, colIndex) || ''"
              :alt="getPieceAlt(rowIndex, colIndex)"
              class="chess-piece"
            />
          </div>
        </div>
      </div>
      
      <!-- Coordinates -->
      <div class="coordinates">
        <div class="row-labels">
          <div v-for="n in 8" :key="n" class="row-label">
            {{ 9 - n }}
          </div>
        </div>
        <div class="col-labels">
          <div v-for="(letter, index) in ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h']" :key="index" class="col-label">
            {{ letter }}
          </div>
        </div>
      </div>
    </div>
    
    <!-- Game Controls -->
    <div class="game-controls">
      <button @click="resetGame" class="control-btn reset">New Game</button>
      <button @click="undoLastMove" class="control-btn undo" :disabled="moveHistory.length === 0">Undo</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount } from 'vue'
import { useFenLogic } from '../composables/useFenLogic'
import { useChessBoardLogic } from '../composables/useChessBoardLogic'
import { useWebSocketLogic } from '../composables/useWebSocketLogic'

// Initialize composables
const fenLogic = useFenLogic()
const chessBoardLogic = useChessBoardLogic(fenLogic)
const webSocketLogic = useWebSocketLogic(fenLogic)

// Destructure what we need from composables
const { currentBoard, initialFen } = fenLogic
const { 
  handleSquareClick, 
  isSquareSelected, 
  isPossibleMove,
  isSquareInCheck,
  getPieceImage, 
  getPieceAlt,
  isGameOver,
  isCheck,
  isCheckmate,
  isStalemate,
  currentPlayer,
  moveHistory,
  isReceivingExternalMove,
  resetGame,
  undoLastMove,
  updateFromExternalFen
} = chessBoardLogic
const { isConnected, initializeWebSocket, cleanupWebSocket } = webSocketLogic

// Component lifecycle
onMounted(async () => {
  // Initialize board from FEN
  fenLogic.initializeBoardFromFen()
  
  // Initialize WebSocket with chess board logic reference
  await initializeWebSocket(chessBoardLogic)
})

// Cleanup on component unmount
onBeforeUnmount(() => {
  cleanupWebSocket()
})

// Expose methods for external use
defineExpose({
  getBoard: () => currentBoard.value,
  getFen: () => initialFen.value,
  isConnected: () => isConnected.value,
  updateFromFen: fenLogic.updateFromFen,
  // New chess features
  isGameOver: () => isGameOver.value,
  isCheck: () => isCheck.value,
  isCheckmate: () => isCheckmate.value,
  isStalemate: () => isStalemate.value,
  currentPlayer: () => currentPlayer.value,
  moveHistory: () => moveHistory.value,
  isReceivingExternalMove: () => isReceivingExternalMove.value,
  resetGame,
  undoLastMove,
  updateFromExternalFen
})
</script>

<style scoped>
.chess-board {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  width: 100%;
  gap: 15px;
}

.game-status {
  text-align: center;
  padding: 10px;
  background: rgba(255, 255, 255, 0.9);
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  min-height: 50px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 5px;
}

.current-player {
  font-weight: bold;
  font-size: 16px;
}

.current-player .white {
  color: #333;
}

.current-player .black {
  color: #666;
}

.status-warning {
  color: #ff6b00;
  font-weight: bold;
  font-size: 18px;
  animation: pulse 1.5s ease-in-out infinite;
}

.status-game-over {
  color: #d32f2f;
  font-weight: bold;
  font-size: 18px;
}

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.6; }
  100% { opacity: 1; }
}

.board-container {
  position: relative;
  border: 3px solid #8B4513;
  background: #DEB887;
  padding: 15px;
  border-radius: 12px;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.3);
  width: fit-content;
  max-width: 100%;
}

.board-grid {
  display: grid;
  grid-template-rows: repeat(8, 1fr);
  width: min(400px, calc(100vw - 100px));
  height: min(400px, calc(100vw - 100px));
  border: 3px solid #654321;
  border-radius: 4px;
  aspect-ratio: 1;
  overflow: hidden;
}

.board-row {
  display: grid;
  grid-template-columns: repeat(8, 1fr);
}

.chess-square {
  display: flex;
  justify-content: center;
  align-items: center;
  position: relative;
  aspect-ratio: 1;
  cursor: pointer;
  transition: all 0.2s ease;
}

.chess-square.light {
  background-color: #F0D9B5;
}

.chess-square.dark {
  background-color: #B58863;
}

.chess-square.selected {
  background-color: #FFE135 !important;
  box-shadow: inset 0 0 0 3px #FF6B35;
  animation: selectPulse 1.5s ease-in-out infinite;
}

.chess-square.possible-move {
  position: relative;
}

.chess-square.possible-move::after {
  content: '';
  position: absolute;
  width: 30%;
  height: 30%;
  background-color: #4CAF50;
  border-radius: 50%;
  opacity: 0.8;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

.chess-square.in-check {
  background-color: #ffcdd2 !important;
  box-shadow: inset 0 0 0 2px #f44336;
}

.chess-square:hover {
  filter: brightness(1.1);
}

.chess-piece {
  width: 80%;
  height: 80%;
  object-fit: contain;
  user-select: none;
  pointer-events: none;
  transition: all 0.2s ease;
  filter: drop-shadow(2px 2px 4px rgba(0, 0, 0, 0.3));
}

.chess-square:hover .chess-piece {
  transform: scale(1.05);
  filter: drop-shadow(3px 3px 6px rgba(0, 0, 0, 0.4));
}

.coordinates {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
}

.row-labels {
  position: absolute;
  left: -15px;
  top: 15px;
  height: min(400px, calc(100vw - 100px));
  display: flex;
  flex-direction: column;
  justify-content: space-around;
  align-items: center;
}

.col-labels {
  position: absolute;
  bottom: -15px;
  left: 15px;
  width: min(400px, calc(100vw - 100px));
  display: flex;
  justify-content: space-around;
  align-items: center;
}

.row-label, .col-label {
  font-weight: bold;
  color: #654321;
  font-size: clamp(10px, 2vw, 14px);
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .board-container {
    padding: 10px;
  }
  
  .row-labels {
    left: -12px;
    top: 10px;
  }
  
  .col-labels {
    bottom: -12px;
    left: 10px;
  }
}

@media (max-width: 480px) {
  .board-container {
    padding: 8px;
  }
  
  .row-labels {
    left: -10px;
    top: 8px;
  }
  
  .col-labels {
    bottom: -10px;
    left: 8px;
  }
}

/* Animation keyframes */
@keyframes selectPulse {
  0% { box-shadow: inset 0 0 0 3px #FF6B35; }
  50% { box-shadow: inset 0 0 0 5px #FF6B35; }
  100% { box-shadow: inset 0 0 0 3px #FF6B35; }
}

.game-controls {
  display: flex;
  gap: 10px;
  justify-content: center;
}

.control-btn {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  font-weight: bold;
  cursor: pointer;
  transition: all 0.2s ease;
}

.control-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.control-btn.reset {
  background-color: #4CAF50;
  color: white;
}

.control-btn.reset:hover:not(:disabled) {
  background-color: #45a049;
}

.control-btn.undo {
  background-color: #2196F3;
  color: white;
}

.control-btn.undo:hover:not(:disabled) {
  background-color: #1976D2;
}
</style>
