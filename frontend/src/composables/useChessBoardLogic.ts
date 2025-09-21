import { ref, type Ref } from 'vue'
import { Chess, type Square } from 'chess.js'
import type { FenLogic } from './useFenLogic'

export interface ChessBoardLogic {
  selectedSquare: Ref<{ row: number, col: number } | null>
  selectedPiece: Ref<string | null>
  possibleMoves: Ref<string[]>
  isGameOver: Ref<boolean>
  isCheck: Ref<boolean>
  isCheckmate: Ref<boolean>
  isStalemate: Ref<boolean>
  currentPlayer: Ref<'white' | 'black'>
  moveHistory: Ref<string[]>
  isReceivingExternalMove: Ref<boolean>
  promotionPending: Ref<{ from: string, to: string } | null>
  handleSquareClick: (row: number, col: number) => void
  movePiece: (fromRow: number, fromCol: number, toRow: number, toCol: number) => boolean
  makePromotionMove: (promotionPiece: 'q' | 'r' | 'b' | 'n') => boolean
  cancelPromotion: () => void
  isSquareSelected: (row: number, col: number) => boolean
  isPossibleMove: (row: number, col: number) => boolean
  isSquareInCheck: (row: number, col: number) => boolean
  isSquareInCheckmate: (row: number, col: number) => boolean
  getSquareNotation: (row: number, col: number) => string
  getPieceImage: (row: number, col: number) => string | null
  getPieceAlt: (row: number, col: number) => string
  resetGame: () => void
  undoLastMove: () => void
  updateFromExternalFen: (fen: string) => void
}

export function useChessBoardLogic(fenLogic: FenLogic): ChessBoardLogic {
  // Initialize chess.js instance
  const chess = new Chess()
  
  // Interaction state
  const selectedSquare = ref<{ row: number, col: number } | null>(null)
  const selectedPiece = ref<string | null>(null)
  const possibleMoves = ref<string[]>([])
  
  // Game state
  const isGameOver = ref(false)
  const isCheck = ref(false)
  const isCheckmate = ref(false)
  const isStalemate = ref(false)
  const currentPlayer = ref<'white' | 'black'>('white')
  const moveHistory = ref<string[]>([])
  const isReceivingExternalMove = ref(false)
  const promotionPending = ref<{ from: string, to: string } | null>(null)

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

  // Initialize chess game from FEN
  const initializeChessFromFen = () => {
    try {
      chess.load(fenLogic.initialFen.value)
      updateGameState()
      syncBoardFromChess()
    } catch (error) {
      console.error('Invalid FEN:', error)
      // Fallback to starting position
      chess.reset()
      updateGameState()
      syncBoardFromChess()
    }
  }

  // Update game state from chess.js
  const updateGameState = () => {
    isCheck.value = chess.inCheck()
    isCheckmate.value = chess.isCheckmate()
    isStalemate.value = chess.isStalemate()
    isGameOver.value = chess.isGameOver()
    currentPlayer.value = chess.turn() === 'w' ? 'white' : 'black'
    moveHistory.value = chess.history()
  }

  // Sync board from chess.js to fenLogic
  const syncBoardFromChess = () => {
    const newFen = chess.fen()
    fenLogic.updateFromFen(newFen)
  }

  // Convert row,col to square notation (e.g., 0,0 = a8)
  const getSquareNotation = (row: number, col: number): string => {
    const files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h']
    const rank = 8 - row
    const file = files[col]
    return `${file}${rank}`
  }

  // Check if a move is a pawn promotion
  const isPawnPromotion = (fromSquare: string, toSquare: string): boolean => {
    const piece = chess.get(fromSquare as Square)
    if (!piece || piece.type !== 'p') return false
    
    const toRank = parseInt(toSquare[1])
    return (piece.color === 'w' && toRank === 8) || (piece.color === 'b' && toRank === 1)
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

  // Get possible moves for selected piece
  const getPossibleMoves = (row: number, col: number): string[] => {
    const square = getSquareNotation(row, col) as Square
    const moves = chess.moves({ square, verbose: true })
    return moves.map(move => move.to)
  }

  // Check if square is a possible move
  const isPossibleMove = (row: number, col: number): boolean => {
    const square = getSquareNotation(row, col)
    return possibleMoves.value.includes(square)
  }

  // Check if square contains king in check
  const isSquareInCheck = (row: number, col: number): boolean => {
    const piece = fenLogic.currentBoard.value[row]?.[col]
    if (!piece) return false
    
    const isKing = piece.endsWith('k')
    if (!isKing) return false
    
    const isWhiteKing = piece === 'wk'
    const isCurrentPlayerKing = (isWhiteKing && currentPlayer.value === 'white') || 
                               (!isWhiteKing && currentPlayer.value === 'black')
    
    return isCurrentPlayerKing && isCheck.value
  }

  // Check if square contains king in checkmate
  const isSquareInCheckmate = (row: number, col: number): boolean => {
    const piece = fenLogic.currentBoard.value[row]?.[col]
    if (!piece) return false
    
    const isKing = piece.endsWith('k')
    if (!isKing) return false
    
    const isWhiteKing = piece === 'wk'
    const isCurrentPlayerKing = (isWhiteKing && currentPlayer.value === 'white') || 
                               (!isWhiteKing && currentPlayer.value === 'black')
    
    return isCurrentPlayerKing && isCheckmate.value
  }

  // Handle square click for piece selection and movement
  const handleSquareClick = (row: number, col: number) => {
    const clickedPiece = fenLogic.currentBoard.value[row]?.[col]
    const square = getSquareNotation(row, col)
    
    // If game is over or receiving external move, don't allow manual moves
    if (isGameOver.value || isReceivingExternalMove.value) {
      if (isReceivingExternalMove.value) {
        console.log('Currently receiving external move, manual interaction disabled')
      } else {
        console.log('Game is over!')
      }
      return
    }
    
    // If no piece is selected
    if (!selectedSquare.value) {
      // Select the clicked piece if it belongs to current player
      if (clickedPiece) {
        const isWhitePiece = clickedPiece.startsWith('w')
        const canSelect = (isWhitePiece && currentPlayer.value === 'white') || 
                         (!isWhitePiece && currentPlayer.value === 'black')
        
        if (canSelect) {
          selectedSquare.value = { row, col }
          selectedPiece.value = clickedPiece
          possibleMoves.value = getPossibleMoves(row, col)
          console.log(`Selected ${clickedPiece} at ${square}`)
          console.log('Possible moves:', possibleMoves.value)
        } else {
          console.log(`It's ${currentPlayer.value}'s turn`)
        }
      }
    } else {
      // A piece is already selected
      const fromRow = selectedSquare.value.row
      const fromCol = selectedSquare.value.col
      
      // If clicking on the same square, deselect
      if (fromRow === row && fromCol === col) {
        selectedSquare.value = null
        selectedPiece.value = null
        possibleMoves.value = []
        console.log('Deselected piece')
        return
      }
      
      // Try to move the piece
      const success = movePiece(fromRow, fromCol, row, col)
      
      if (success) {
        // Clear selection after successful move
        selectedSquare.value = null
        selectedPiece.value = null
        possibleMoves.value = []
      } else {
        // If move failed, check if clicking on own piece to select it
        if (clickedPiece) {
          const isWhitePiece = clickedPiece.startsWith('w')
          const canSelect = (isWhitePiece && currentPlayer.value === 'white') || 
                           (!isWhitePiece && currentPlayer.value === 'black')
          
          if (canSelect) {
            selectedSquare.value = { row, col }
            selectedPiece.value = clickedPiece
            possibleMoves.value = getPossibleMoves(row, col)
            console.log(`Selected ${clickedPiece} at ${square}`)
          }
        } else {
          // Clear selection if clicking empty square and move failed
          selectedSquare.value = null
          selectedPiece.value = null
          possibleMoves.value = []
        }
      }
    }
  }

  // Move piece using chess.js validation
  const movePiece = (fromRow: number, fromCol: number, toRow: number, toCol: number): boolean => {
    const fromSquare = getSquareNotation(fromRow, fromCol) as Square
    const toSquare = getSquareNotation(toRow, toCol) as Square
    
    // Check if this is a pawn promotion
    if (isPawnPromotion(fromSquare, toSquare)) {
      // Store the pending promotion move
      promotionPending.value = { from: fromSquare, to: toSquare }
      console.log(`Pawn promotion pending: ${fromSquare} to ${toSquare}`)
      return true // Return true to clear selection, but don't make the move yet
    }
    
    try {
      // Attempt the move (non-promotion)
      const move = chess.move({
        from: fromSquare,
        to: toSquare
      })
      
      if (move) {
        console.log(`Legal move: ${move.san}`)
        
        // Update game state
        updateGameState()
        
        // Sync board
        syncBoardFromChess()
        
        // Check for special conditions
        if (isCheckmate.value) {
          console.log(`Checkmate! ${currentPlayer.value === 'white' ? 'Black' : 'White'} wins!`)
        } else if (isStalemate.value) {
          console.log('Stalemate! Game is a draw.')
        } else if (isCheck.value) {
          console.log(`${currentPlayer.value} is in check!`)
        }
        
        return true
      }
    } catch (error) {
      console.log('Illegal move:', error)
    }
    
    return false
  }

  // Check if a square is selected
  const isSquareSelected = (row: number, col: number): boolean => {
    return selectedSquare.value?.row === row && selectedSquare.value?.col === col
  }

  // Reset game to starting position
  const resetGame = () => {
    chess.reset()
    selectedSquare.value = null
    selectedPiece.value = null
    possibleMoves.value = []
    updateGameState()
    syncBoardFromChess()
    console.log('Game reset to starting position')
  }

  // Undo last move
  const undoLastMove = () => {
    const undone = chess.undo()
    if (undone) {
      selectedSquare.value = null
      selectedPiece.value = null
      possibleMoves.value = []
      updateGameState()
      syncBoardFromChess()
      console.log('Undid last move:', undone.san)
    } else {
      console.log('No move to undo')
    }
  }

  // Update from external FEN (from WebSocket)
  const updateFromExternalFen = (fen: string) => {
    try {
      // Set flag to prevent manual interaction during external update
      isReceivingExternalMove.value = true
      
      console.log('Updating from external FEN:', fen)
      
      // Try to load the FEN into chess.js
      chess.load(fen)
      
      // Update game state
      updateGameState()
      
      // Sync board from chess.js
      syncBoardFromChess()
      
      // Clear any current selection and promotion
      selectedSquare.value = null
      selectedPiece.value = null
      possibleMoves.value = []
      promotionPending.value = null
      
      console.log('Successfully updated board from external FEN')
      
      // Allow manual interaction again after a short delay
      setTimeout(() => {
        isReceivingExternalMove.value = false
      }, 500)
      
    } catch (error) {
      console.error('Error updating from external FEN:', error)
      isReceivingExternalMove.value = false
    }
  }

  // Make promotion move with selected piece
  const makePromotionMove = (promotionPiece: 'q' | 'r' | 'b' | 'n'): boolean => {
    if (!promotionPending.value) {
      console.error('No promotion pending')
      return false
    }
    
    try {
      const move = chess.move({
        from: promotionPending.value.from as Square,
        to: promotionPending.value.to as Square,
        promotion: promotionPiece
      })
      
      if (move) {
        console.log(`Promotion move: ${move.san}`)
        
        // Clear promotion pending
        promotionPending.value = null
        
        // Update game state
        updateGameState()
        
        // Sync board
        syncBoardFromChess()
        
        // Check for special conditions
        if (isCheckmate.value) {
          console.log(`Checkmate! ${currentPlayer.value === 'white' ? 'Black' : 'White'} wins!`)
        } else if (isStalemate.value) {
          console.log('Stalemate! Game is a draw.')
        } else if (isCheck.value) {
          console.log(`${currentPlayer.value} is in check!`)
        }
        
        return true
      }
    } catch (error) {
      console.error('Error making promotion move:', error)
    }
    
    promotionPending.value = null
    return false
  }

  // Cancel promotion and restore piece to original position
  const cancelPromotion = () => {
    console.log('Promotion cancelled')
    promotionPending.value = null
    selectedSquare.value = null
    selectedPiece.value = null
    possibleMoves.value = []
  }

  // Initialize on creation
  initializeChessFromFen()

  return {
    selectedSquare,
    selectedPiece,
    possibleMoves,
    isGameOver,
    isCheck,
    isCheckmate,
    isStalemate,
    currentPlayer,
    moveHistory,
    isReceivingExternalMove,
    promotionPending,
    handleSquareClick,
    movePiece,
    makePromotionMove,
    cancelPromotion,
    isSquareSelected,
    isPossibleMove,
    isSquareInCheck,
    isSquareInCheckmate,
    getSquareNotation,
    getPieceImage,
    getPieceAlt,
    resetGame,
    undoLastMove,
    updateFromExternalFen
  }
}