import { ref, type Ref } from 'vue'

export interface FenLogic {
  initialFen: Ref<string>
  currentBoard: Ref<string[][]>
  parseFenToBoard: (fen: string) => string[][]
  updateFenFromBoard: () => void
  initializeBoardFromFen: () => void
  updateFromFen: (fen: string) => void
}

export function useFenLogic(): FenLogic {
  // Initial chess board setup using FEN notation
  const initialFen = ref('rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1')
  const currentBoard = ref<string[][]>([])

  // Parse FEN string to board array
  const parseFenToBoard = (fen: string): string[][] => {
    const boardFen = fen.split(' ')[0]
    const ranks = boardFen.split('/')
    
    const board: string[][] = []
    
    const fenToPiece: { [key: string]: string } = {
      'r': 'br', 'n': 'bn', 'b': 'bb', 'q': 'bq', 'k': 'bk', 'p': 'bp',
      'R': 'wr', 'N': 'wn', 'B': 'wb', 'Q': 'wq', 'K': 'wk', 'P': 'wp'
    }
    
    for (let rank of ranks) {
      const row: string[] = []
      
      for (let char of rank) {
        if (fenToPiece[char]) {
          row.push(fenToPiece[char])
        } else if ('12345678'.includes(char)) {
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

  // Convert board array back to FEN string
  const updateFenFromBoard = () => {
    const board = currentBoard.value
    
    const pieceToFen: { [key: string]: string } = {
      'br': 'r', 'bn': 'n', 'bb': 'b', 'bq': 'q', 'bk': 'k', 'bp': 'p',
      'wr': 'R', 'wn': 'N', 'wb': 'B', 'wq': 'Q', 'wk': 'K', 'wp': 'P'
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
    
    const fenParts = initialFen.value.split(' ')
    const newFen = `${fenBoard} ${fenParts.slice(1).join(' ')}`
    
    initialFen.value = newFen
  }

  // Initialize board from FEN
  const initializeBoardFromFen = () => {
    currentBoard.value = parseFenToBoard(initialFen.value)
  }

  // Update from external FEN
  const updateFromFen = (fen: string) => {
    const newBoard = parseFenToBoard(fen)
    currentBoard.value = newBoard
    initialFen.value = fen
  }

  return {
    initialFen,
    currentBoard,
    parseFenToBoard,
    updateFenFromBoard,
    initializeBoardFromFen,
    updateFromFen
  }
}
