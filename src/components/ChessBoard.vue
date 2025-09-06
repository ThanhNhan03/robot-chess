<template>
  <div class="chess-board">
    <div class="board-container">
      <!-- Game Status -->
      <div class="game-status">
        <div class="turn-indicator">
          <span :class="['turn-dot', gameState.turn]"></span>
          {{ gameState.turn === 'w' ? 'Lượt Trắng' : 'Lượt Đen' }}
          <span v-if="gameState.isCheck" class="check-indicator">CHIẾU!</span>
        </div>
        <div v-if="gameResult.isGameOver" class="game-over">
          <strong>{{ gameResult.reason }}</strong>
          <div>{{ gameResult.result }}</div>
        </div>
      </div>

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
                'possible-capture': isPossibleCapture(rowIndex, colIndex),
                'last-move': isLastMoveSquare(rowIndex, colIndex)
              }
            ]"
            @click="onSquareClick(rowIndex, colIndex)"
          >
            <img
              v-if="getPieceImage(rowIndex, colIndex)"
              :src="getPieceImage(rowIndex, colIndex) || ''"
              :alt="getPieceAlt(rowIndex, colIndex)"
              class="chess-piece"
            />
            <!-- Debug: Show square notation -->
            <span class="debug-square" v-if="!getPieceImage(rowIndex, colIndex)">
              {{ boardToSquare(rowIndex, colIndex) }}
            </span>
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

      <!-- Game Controls -->
      <div class="game-controls">
        <button @click="undoMove" :disabled="moveHistory.length === 0" class="control-btn">
          ↶ Hoàn tác
        </button>
        <button @click="resetGame" class="control-btn">
          🔄 Reset
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, toRef } from 'vue'
import { ChessGame, type Color } from '../chess/ChessGame'
import type { Square, Move } from 'chess.js'

// Props - để nhận shared game instance từ parent
const props = defineProps<{
  sharedGame: ChessGame  // Make required, not optional
}>()

// Emits
const emit = defineEmits<{
  'game-updated': []
}>()

// Use the shared game instance directly
const chessGame = toRef(props, 'sharedGame')
const gameState = ref(chessGame.value.getGameState())
const selectedSquare = ref<{row: number, col: number} | null>(null)
const possibleMoves = ref<Move[]>([])

// Piece name mapping for alt text
const pieceNames: { [key: string]: string } = {
  'r': 'Rook', 'n': 'Knight', 'b': 'Bishop',
  'q': 'Queen', 'k': 'King', 'p': 'Pawn'
}

// Convert board coordinates to chess notation
const boardToSquare = (row: number, col: number): Square => {
  const file = String.fromCharCode(97 + col) // a-h
  const rank = (8 - row).toString() // 8-1
  return (file + rank) as Square
}

const updateGameState = () => {
  gameState.value = chessGame.value.getGameState()
  console.log('Game state updated:', gameState.value)
  console.log('Move history:', chessGame.value.getMoveHistory())
  emit('game-updated')
}

// Watch for changes in shared game to update local state
watch(() => props.sharedGame, (newGame) => {
  if (newGame) {
    updateGameState()
  }
}, { immediate: true })

// Watch for changes in the chess game itself
watch(chessGame, (newGame) => {
  updateGameState()
}, { immediate: true })

// Computed properties
const moveHistory = computed(() => chessGame.value.getMoveHistory())
const gameResult = computed(() => chessGame.value.getGameResult())

const getPieceImage = (row: number, col: number) => {
  const square = boardToSquare(row, col)
  const piece = chessGame.value.getPiece(square)
  
  if (piece) {
    const color = piece.color === 'w' ? 'w' : 'b'
    const type = piece.type
    const pieceCode = color + type
    return new URL(`../assets/${pieceCode}.png`, import.meta.url).href
  }
  return null
}

const getPieceAlt = (row: number, col: number) => {
  const square = boardToSquare(row, col)
  const piece = chessGame.value.getPiece(square)
  
  if (piece) {
    const color = piece.color === 'w' ? 'White' : 'Black'
    const type = pieceNames[piece.type] || piece.type
    return `${color} ${type}`
  }
  return ''
}

const onSquareClick = (row: number, col: number) => {
  const clickedSquare = boardToSquare(row, col)
  
  if (selectedSquare.value) {
    const fromSquare = boardToSquare(selectedSquare.value.row, selectedSquare.value.col)
    
    // Try to make a move
    if (fromSquare !== clickedSquare) {
      const move = chessGame.value.makeMove({
        from: fromSquare,
        to: clickedSquare
      })
      
      if (move) {
        updateGameState()
        selectedSquare.value = null
        possibleMoves.value = []
        return
      }
    }
    
    // Deselect if clicking the same square or invalid move
    selectedSquare.value = null
    possibleMoves.value = []
  }
  
  // Select new square if it has a piece of the current player
  const piece = chessGame.value.getPiece(clickedSquare)
  if (piece && piece.color === gameState.value.turn) {
    selectedSquare.value = { row, col }
    possibleMoves.value = chessGame.value.getLegalMovesVerbose(clickedSquare)
  }
}

const isSquareSelected = (row: number, col: number) => {
  return selectedSquare.value?.row === row && selectedSquare.value?.col === col
}

const isPossibleMove = (row: number, col: number) => {
  if (!selectedSquare.value) return false
  
  const targetSquare = boardToSquare(row, col)
  return possibleMoves.value.some(move => 
    move.to === targetSquare && !move.captured
  )
}

const isPossibleCapture = (row: number, col: number) => {
  if (!selectedSquare.value) return false
  
  const targetSquare = boardToSquare(row, col)
  return possibleMoves.value.some(move => 
    move.to === targetSquare && move.captured
  )
}

const isLastMoveSquare = (row: number, col: number) => {
  const lastMove = chessGame.value.getLastMove()
  if (!lastMove) return false
  
  const square = boardToSquare(row, col)
  return square === lastMove.from || square === lastMove.to
}

const undoMove = () => {
  const undoneMove = chessGame.value.undoMove()
  if (undoneMove) {
    updateGameState()
    selectedSquare.value = null
    possibleMoves.value = []
  }
}

const resetGame = () => {
  chessGame.value.reset()
  updateGameState()
  selectedSquare.value = null
  possibleMoves.value = []
}

// Initialize game state
onMounted(() => {
  console.log('ChessBoard mounted')
  console.log('chessGame:', chessGame.value)
  console.log('gameState:', gameState.value)
  updateGameState()
})
</script>

<style scoped>
.chess-board {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
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

.game-status {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  padding: 10px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  min-height: 40px;
}

.turn-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: bold;
  color: #654321;
}

.turn-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid #654321;
}

.turn-dot.w {
  background-color: white;
}

.turn-dot.b {
  background-color: black;
}

.check-indicator {
  color: #ff4444;
  font-weight: bold;
  animation: pulse 1s infinite;
}

.game-over {
  text-align: right;
  color: #654321;
  font-weight: bold;
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
}

.chess-square.last-move {
  background-color: #FFD700 !important;
  opacity: 0.8;
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
  top: 75px;
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

.game-controls {
  display: flex;
  justify-content: center;
  gap: 10px;
  margin-top: 15px;
}

.control-btn {
  padding: 8px 16px;
  background: #8B4513;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: bold;
  transition: all 0.2s ease;
}

.control-btn:hover:not(:disabled) {
  background: #A0522D;
  transform: translateY(-1px);
}

.control-btn:disabled {
  background: #ccc;
  cursor: not-allowed;
  opacity: 0.6;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .board-container {
    padding: 10px;
  }
  
  .row-labels {
    left: -12px;
    top: 65px;
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
    top: 60px;
  }
  
  .col-labels {
    bottom: -10px;
    left: 8px;
  }

  .game-status {
    flex-direction: column;
    gap: 5px;
    text-align: center;
  }
}

/* Animation keyframes */
@keyframes selectPulse {
  0% { box-shadow: inset 0 0 0 3px #FF6B35; }
  50% { box-shadow: inset 0 0 0 5px #FF6B35; }
  100% { box-shadow: inset 0 0 0 3px #FF6B35; }
}

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.5; }
  100% { opacity: 1; }
}

.chess-square.selected {
  animation: selectPulse 1.5s ease-in-out infinite;
}

/* Possible move indicators */
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
  opacity: 0.7;
  z-index: 1;
}

/* Capture move indicators */
.chess-square.possible-capture {
  position: relative;
}

.chess-square.possible-capture::before {
  content: '';
  position: absolute;
  width: 100%;
  height: 100%;
  border: 4px solid #F44336;
  border-radius: 50%;
  box-sizing: border-box;
  z-index: 1;
  opacity: 0.8;
}

.debug-square {
  font-size: 10px;
  color: #999;
  position: absolute;
  bottom: 2px;
  right: 2px;
}
</style>
