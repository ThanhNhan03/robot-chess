<template>
  <div class="chess-board">
    <div class="board-container">
      <div class="board-grid">
        <div
          v-for="(row, rowIndex) in 8"
          :key="rowIndex"
          class="board-row"
        >
          <div
            v-for="(col, colIndex) in 8"
            :key="colIndex"
            :class="[
              'chess-square',
              (rowIndex + colIndex) % 2 === 0 ? 'light' : 'dark',
              { 'selected': isSquareSelected(rowIndex, colIndex) }
            ]"
            @click="onSquareClick(rowIndex, colIndex)"
          >
            <img
              v-if="getPieceImage(rowIndex, colIndex)"
              :src="getPieceImage(rowIndex, colIndex)"
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
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

// Initial chess board setup with piece codes
const initialBoard = ref([
  ['br', 'bn', 'bb', 'bq', 'bk', 'bb', 'bn', 'br'],
  ['bp', 'bp', 'bp', 'bp', 'bp', 'bp', 'bp', 'bp'],
  ['', '', '', '', '', '', '', ''],
  ['', '', '', '', '', '', '', ''],
  ['', '', '', '', '', '', '', ''],
  ['', '', '', '', '', '', '', ''],
  ['wp', 'wp', 'wp', 'wp', 'wp', 'wp', 'wp', 'wp'],
  ['wr', 'wn', 'wb', 'wq', 'wk', 'wb', 'wn', 'wr']
])

// Piece name mapping for alt text
const pieceNames: { [key: string]: string } = {
  'wr': 'White Rook',
  'wn': 'White Knight', 
  'wb': 'White Bishop',
  'wq': 'White Queen',
  'wk': 'White King',
  'wp': 'White Pawn',
  'br': 'Black Rook',
  'bn': 'Black Knight',
  'bb': 'Black Bishop', 
  'bq': 'Black Queen',
  'bk': 'Black King',
  'bp': 'Black Pawn'
}

const selectedSquare = ref<{row: number, col: number} | null>(null)

const getPiece = (row: number, col: number) => {
  return initialBoard.value[row][col]
}

const getPieceImage = (row: number, col: number) => {
  const piece = initialBoard.value[row][col]
  if (piece) {
    return new URL(`../assets/${piece}.png`, import.meta.url).href
  }
  return null
}

const getPieceAlt = (row: number, col: number) => {
  const piece = initialBoard.value[row][col]
  return piece ? pieceNames[piece] || piece : ''
}

const onSquareClick = (row: number, col: number) => {
  if (selectedSquare.value) {
    // Move piece if different square is selected
    if (selectedSquare.value.row !== row || selectedSquare.value.col !== col) {
      // Simple move logic (without validation for now)
      const piece = initialBoard.value[selectedSquare.value.row][selectedSquare.value.col]
      initialBoard.value[selectedSquare.value.row][selectedSquare.value.col] = ''
      initialBoard.value[row][col] = piece
    }
    selectedSquare.value = null
  } else {
    // Select square if it has a piece
    if (initialBoard.value[row][col]) {
      selectedSquare.value = { row, col }
    }
  }
}

const isSquareSelected = (row: number, col: number) => {
  return selectedSquare.value?.row === row && selectedSquare.value?.col === col
}
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

.chess-square.selected {
  animation: selectPulse 1.5s ease-in-out infinite;
}

/* Possible move indicators */
.chess-square.possible-move {
  background-color: rgba(76, 175, 80, 0.3) !important;
}

.chess-square.possible-move::after {
  content: '';
  position: absolute;
  width: 30%;
  height: 30%;
  background-color: #4CAF50;
  border-radius: 50%;
  opacity: 0.7;
}

/* Capture move indicators */
.chess-square.possible-capture::after {
  content: '';
  position: absolute;
  width: 100%;
  height: 100%;
  border: 3px solid #F44336;
  border-radius: 50%;
  box-sizing: border-box;
}
</style>
