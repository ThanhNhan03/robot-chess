<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import CameraView from '../../components/CameraView.vue'
import ChessBoard from '../../components/ChessBoard.vue'
import GameStatus from '../../components/GameStatus.vue'
import MoveHistory from '../../components/MoveHistory.vue'
import RobotCommand from '../../components/RobotCommand.vue'

const router = useRouter()

// Ref to access ChessBoard methods and data
const chessBoardRef = ref<InstanceType<typeof ChessBoard> | null>(null)

// Handle events from MoveHistory
const handleUndoMove = () => {
  if (chessBoardRef.value) {
    chessBoardRef.value.undoLastMove()
  }
}

const handleNewGame = () => {
  if (chessBoardRef.value) {
    chessBoardRef.value.resetGame()
  }
}

const handleClearHistory = () => {
  if (chessBoardRef.value) {
    chessBoardRef.value.resetGame()
  }
}

const handleExportPGN = () => {
  if (chessBoardRef.value) {
    const moveHistory = chessBoardRef.value.moveHistory()
    
    let pgn = '[Event "Robot Chess Game"]\n'
    pgn += '[Date "' + new Date().toISOString().split('T')[0] + '"]\n'
    pgn += '[White "Player"]\n'
    pgn += '[Black "Robot"]\n\n'
    
    for (let i = 0; i < moveHistory.length; i += 2) {
      const moveNumber = Math.floor(i / 2) + 1
      pgn += `${moveNumber}. ${moveHistory[i]}`
      
      if (i + 1 < moveHistory.length) {
        pgn += ` ${moveHistory[i + 1]}`
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
}

// Handle modal events
const handleModalNewGame = () => {
  if (chessBoardRef.value) {
    chessBoardRef.value.resetGame()
  }
}

const handleModalClose = () => {
  // Modal will automatically close when game state changes
  console.log('Modal closed')
}
</script>

<template>
  <div class="chess-robot-screen">
    <!-- Admin Button -->
    <button class="admin-button" @click="router.push('/admin')" title="Admin Dashboard">
      ⚙️ ADMIN
    </button>

    <main class="app-main">
      <div class="main-content">
        <!-- Camera Section -->
        <div class="camera-section">
          <CameraView />
        </div>

        <!-- Chess Board Section -->
        <div class="board-section">
          <ChessBoard ref="chessBoardRef" />
        </div>

        <!-- Right Sidebar with History and Robot Control -->
        <aside class="right-sidebar">
          <div class="sidebar-section">
            <MoveHistory 
              :move-history="chessBoardRef?.moveHistory() || []"
              :current-turn="chessBoardRef?.currentPlayer() || 'white'"
              :is-receiving-external-move="chessBoardRef?.isReceivingExternalMove() || false"
              :is-check="chessBoardRef?.isCheck() || false"
              :is-game-over="chessBoardRef?.isGameOver() || false"
              :is-checkmate="chessBoardRef?.isCheckmate() || false"
              :is-stalemate="chessBoardRef?.isStalemate() || false"
              @undo-move="handleUndoMove"
              @new-game="handleNewGame"
              @clear-history="handleClearHistory"
              @export-pgn="handleExportPGN"
            />
          </div>
          <div class="sidebar-section">
            <RobotCommand />
          </div>
        </aside>
      </div>
    </main>
    
    <!-- Game Status Modal -->
    <GameStatus 
      :current-player="chessBoardRef?.currentPlayer() || 'white'"
      :is-receiving-external-move="chessBoardRef?.isReceivingExternalMove() || false"
      :is-check="chessBoardRef?.isCheck() || false"
      :is-game-over="chessBoardRef?.isGameOver() || false"
      :is-checkmate="chessBoardRef?.isCheckmate() || false"
      :is-stalemate="chessBoardRef?.isStalemate() || false"
      @new-game="handleModalNewGame"
      @close="handleModalClose"
    />
  </div>
</template>

<style scoped src="../../assets/styles/ChessRobotScreen.css">
</style>