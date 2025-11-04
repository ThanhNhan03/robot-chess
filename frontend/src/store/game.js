import { defineStore } from 'pinia'
import { Chess } from 'chess.js'

export const useGameStore = defineStore('game', {
  state: () => ({
    chess: new Chess(),
    gameHistory: [],
    currentGame: null,
    aiLevel: 'medium',
    theme: 'classic',
    playerElo: 1200,
  }),
  getters: {
    fen: (state) => state.chess.fen(),
    isGameOver: (state) => state.chess.isGameOver(),
    isCheck: (state) => state.chess.isCheck(),
    isCheckmate: (state) => state.chess.isCheckmate(),
    isDraw: (state) => state.chess.isDraw(),
    turn: (state) => state.chess.turn(),
    history: (state) => state.chess.history({ verbose: true }),
  },
  actions: {
    makeMove(move) {
      try {
        const result = this.chess.move(move)
        return result
      } catch (error) {
        console.error('Invalid move:', error)
        return null
      }
    },
    resetGame() {
      this.chess.reset()
      this.currentGame = null
    },
    loadGame(fen) {
      this.chess.load(fen)
    },
    saveGame() {
      const game = {
        id: Date.now(),
        fen: this.fen,
        pgn: this.chess.pgn(),
        date: new Date().toISOString(),
        result: this.getGameResult(),
      }
      this.gameHistory.push(game)
      localStorage.setItem('gameHistory', JSON.stringify(this.gameHistory))
    },
    loadGameHistory() {
      const history = localStorage.getItem('gameHistory')
      if (history) {
        this.gameHistory = JSON.parse(history)
      }
    },
    getGameResult() {
      if (this.chess.isCheckmate()) return 'checkmate'
      if (this.chess.isDraw()) return 'draw'
      if (this.chess.isStalemate()) return 'stalemate'
      return 'ongoing'
    },
    setAILevel(level) {
      this.aiLevel = level
    },
    setTheme(theme) {
      this.theme = theme
      localStorage.setItem('chessTheme', theme)
    },
    loadTheme() {
      const theme = localStorage.getItem('chessTheme')
      if (theme) {
        this.theme = theme
      }
    },
  },
})
