<template>
  <div class="chess-board">
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
              { 'selected': isSquareSelected(rowIndex, colIndex) }
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
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import webSocketService from '../services/webSocketService'

// Initial chess board setup using FEN notation
const initialFen = ref('rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1')
const currentBoard = ref<string[][]>([])

// Game state
const isConnected = ref(false)

// Interaction state
const selectedSquare = ref<{ row: number, col: number } | null>(null)
const selectedPiece = ref<string | null>(null)

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

// Parse FEN string to board array
const parseFenToBoard = (fen: string): string[][] => {
  // FEN format: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
  // We only need the board part (first section before space)
  const boardFen = fen.split(' ')[0]
  const ranks = boardFen.split('/')
  
  const board: string[][] = []
  
  // FEN notation piece mapping
  const fenToPiece: { [key: string]: string } = {
    'r': 'br', 'n': 'bn', 'b': 'bb', 'q': 'bq', 'k': 'bk', 'p': 'bp', // Black pieces
    'R': 'wr', 'N': 'wn', 'B': 'wb', 'Q': 'wq', 'K': 'wk', 'P': 'wp'  // White pieces
  }
  
  for (let rank of ranks) {
    const row: string[] = []
    
    for (let char of rank) {
      if (fenToPiece[char]) {
        // It's a piece
        row.push(fenToPiece[char])
      } else if ('12345678'.includes(char)) {
        // It's a number indicating empty squares
        const emptySquares = parseInt(char)
        for (let i = 0; i < emptySquares; i++) {
          row.push('')
        }
      }
    }
    
    board.push(row)
  }
  
  return board
}

const getPieceImage = (row: number, col: number) => {
  const piece = currentBoard.value[row]?.[col]
  if (piece) {
    return new URL(`../assets/${piece}.png`, import.meta.url).href
  }
  return null
}

const getPieceAlt = (row: number, col: number) => {
  const piece = currentBoard.value[row]?.[col]
  return piece ? pieceNames[piece] || piece : ''
}

// Handle square click for piece selection and movement
const handleSquareClick = (row: number, col: number) => {
  const clickedPiece = currentBoard.value[row]?.[col]
  
  // If no piece is selected
  if (!selectedSquare.value) {
    // Select the clicked piece if it exists
    if (clickedPiece) {
      selectedSquare.value = { row, col }
      selectedPiece.value = clickedPiece
      console.log(`Selected piece: ${clickedPiece} at ${getSquareNotation(row, col)}`)
    }
  } else {
    // A piece is already selected
    const fromRow = selectedSquare.value.row
    const fromCol = selectedSquare.value.col
    
    // If clicking on the same square, deselect
    if (fromRow === row && fromCol === col) {
      selectedSquare.value = null
      selectedPiece.value = null
      console.log('Deselected piece')
      return
    }
    
    // Move the piece to the new position (free movement)
    movePiece(fromRow, fromCol, row, col)
    
    // Clear selection
    selectedSquare.value = null
    selectedPiece.value = null
  }
}

// Move piece from one square to another (no rules validation)
const movePiece = (fromRow: number, fromCol: number, toRow: number, toCol: number) => {
  const piece = currentBoard.value[fromRow]?.[fromCol]
  
  if (!piece) {
    console.warn('No piece to move')
    return
  }
  
  // Simple move - just update the board
  currentBoard.value[fromRow][fromCol] = ''
  currentBoard.value[toRow][toCol] = piece
  
  const fromNotation = getSquareNotation(fromRow, fromCol)
  const toNotation = getSquareNotation(toRow, toCol)
  
  console.log(`Moved ${piece} from ${fromNotation} to ${toNotation}`)
  
  // Update FEN string after move
  updateFenFromBoard()
}

// Convert row, col to chess notation (e.g., 0,0 = a8)
const getSquareNotation = (row: number, col: number): string => {
  const files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h']
  const rank = 8 - row
  const file = files[col]
  return `${file}${rank}`
}

// Check if a square is selected
const isSquareSelected = (row: number, col: number): boolean => {
  return selectedSquare.value?.row === row && selectedSquare.value?.col === col
}

// Convert board array back to FEN string
const updateFenFromBoard = () => {
  const board = currentBoard.value
  
  // Piece mapping from internal format to FEN
  const pieceToFen: { [key: string]: string } = {
    'br': 'r', 'bn': 'n', 'bb': 'b', 'bq': 'q', 'bk': 'k', 'bp': 'p', // Black pieces
    'wr': 'R', 'wn': 'N', 'wb': 'B', 'wq': 'Q', 'wk': 'K', 'wp': 'P'  // White pieces
  }
  
  let fenBoard = ''
  
  for (let row = 0; row < 8; row++) {
    let rankStr = ''
    let emptyCount = 0
    
    for (let col = 0; col < 8; col++) {
      const piece = board[row]?.[col]
      
      if (piece && pieceToFen[piece]) {
        if (emptyCount > 0) {
          rankStr += emptyCount.toString()
          emptyCount = 0
        }
        rankStr += pieceToFen[piece]
      } else {
        emptyCount++
      }
    }
    
    if (emptyCount > 0) {
      rankStr += emptyCount.toString()
    }
    
    fenBoard += rankStr
    if (row < 7) fenBoard += '/'
  }
  
  // Keep other FEN parts from original (active color, castling, en passant, etc.)
  const fenParts = initialFen.value.split(' ')
  const newFen = `${fenBoard} ${fenParts.slice(1).join(' ')}`
  
  initialFen.value = newFen
  console.log('Updated FEN:', newFen)
}

// Initialize board from FEN
const initializeBoardFromFen = () => {
  currentBoard.value = parseFenToBoard(initialFen.value)
}

// Handle incoming FEN messages from WebSocket
const handleWebSocketFen = (data: any) => {
  try {
    if (data.fen_str) {
      const newBoard = parseFenToBoard(data.fen_str)
      currentBoard.value = newBoard
      initialFen.value = data.fen_str
      
      console.log(`Received FEN: ${data.fen_str}`)
      console.log('Board updated from FEN')
    }
  } catch (error) {
    console.error('Error handling FEN message:', error, data)
  }
}

// Initialize WebSocket connection
onMounted(async () => {
  // Initialize board from FEN
  initializeBoardFromFen()
  
  try {
    isConnected.value = await webSocketService.connect()
    if (isConnected.value) {
      console.log('WebSocket connected successfully')
      
      // Subscribe to FEN messages
      webSocketService.subscribe('fen', handleWebSocketFen)
    } else {
      console.error('Failed to connect to WebSocket')
    }
  } catch (error) {
    console.error('WebSocket connection error:', error)
  }
})

// Cleanup on component unmount
onBeforeUnmount(() => {
  if (isConnected.value) {
    webSocketService.unsubscribe('fen', handleWebSocketFen)
    webSocketService.disconnect()
  }
})

// Expose methods for external use
defineExpose({
  getBoard: () => currentBoard.value,
  getFen: () => initialFen.value,
  isConnected: () => isConnected.value,
  updateFromFen: (fen: string) => {
    const newBoard = parseFenToBoard(fen)
    currentBoard.value = newBoard
    initialFen.value = fen
  }
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
