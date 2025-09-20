import { ref, type Ref } from 'vue'
import type { FenLogic } from './useFenLogic'

export interface ChessBoardLogic {
  selectedSquare: Ref<{ row: number, col: number } | null>
  selectedPiece: Ref<string | null>
  handleSquareClick: (row: number, col: number) => void
  movePiece: (fromRow: number, fromCol: number, toRow: number, toCol: number) => void
  isSquareSelected: (row: number, col: number) => boolean
  getSquareNotation: (row: number, col: number) => string
  getPieceImage: (row: number, col: number) => string | null
  getPieceAlt: (row: number, col: number) => string
}

export function useChessBoardLogic(fenLogic: FenLogic): ChessBoardLogic {
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

  const getPieceImage = (row: number, col: number) => {
    const piece = fenLogic.currentBoard.value[row]?.[col]
    if (piece) {
      return new URL(`../assets/${piece}.png`, import.meta.url).href
    }
    return null
  }

  const getPieceAlt = (row: number, col: number) => {
    const piece = fenLogic.currentBoard.value[row]?.[col]
    return piece ? pieceNames[piece] || piece : ''
  }

  // Handle square click for piece selection and movement
  const handleSquareClick = (row: number, col: number) => {
    const clickedPiece = fenLogic.currentBoard.value[row]?.[col]
    
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
    const piece = fenLogic.currentBoard.value[fromRow]?.[fromCol]
    
    if (!piece) {
      console.warn('No piece to move')
      return
    }
    
    // Simple move - just update the board
    fenLogic.currentBoard.value[fromRow][fromCol] = ''
    fenLogic.currentBoard.value[toRow][toCol] = piece
    
    const fromNotation = getSquareNotation(fromRow, fromCol)
    const toNotation = getSquareNotation(toRow, toCol)
    
    console.log(`Moved ${piece} from ${fromNotation} to ${toNotation}`)
    
    // Update FEN string after move
    fenLogic.updateFenFromBoard()
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

  return {
    selectedSquare,
    selectedPiece,
    handleSquareClick,
    movePiece,
    isSquareSelected,
    getSquareNotation,
    getPieceImage,
    getPieceAlt
  }
}