<script setup lang="ts">
import { ref } from 'vue'
import CameraView from '../../components/CameraView.vue'
import ChessBoard from '../../components/ChessBoard.vue'
import GameStatus from '../../components/GameStatus.vue'
import MoveHistory from '../../components/MoveHistory.vue'
import RobotCommand from '../../components/RobotCommand.vue'

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
    <header class="app-header">
      <h1>Robot Chess</h1>
    </header>

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

<style scoped>
.chess-robot-screen {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.app-header {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 20px;
  text-align: center;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.app-header h1 {
  margin: 0;
  color: #2c3e50;
  font-size: 2.5rem;
  font-weight: 700;
}

.app-main {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 20px;
  max-width: 1600px;
  margin: 0 auto;
}

.main-content {
  display: grid;
  grid-template-columns: 1fr 1fr 400px;
  gap: 20px;
  align-items: start;
}

.camera-section,
.board-section {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: fit-content;
}

.right-sidebar {
  display: flex;
  flex-direction: column;
  gap: 20px;
  min-height: 800px;
  max-height: 100vh;
  overflow-y: auto;
}

.sidebar-section {
  flex: 1;
}

/* Responsive Design */
@media (max-width: 1400px) {
  .main-content {
    grid-template-columns: 1fr 1fr;
    grid-template-rows: auto auto;
  }
  
  .right-sidebar {
    grid-column: 1 / -1;
    min-height: 400px;
    flex-direction: row;
  }
}

@media (max-width: 1024px) {
  .main-content {
    grid-template-columns: 1fr;
    gap: 15px;
  }
  
  .right-sidebar {
    min-height: 350px;
    flex-direction: column;
  }
}

@media (max-width: 768px) {
  .app-header h1 {
    font-size: 2rem;
  }
  
  .app-main {
    padding: 10px;
    gap: 15px;
  }
}

@media (max-width: 480px) {
  .app-header {
    padding: 15px 10px;
  }
  
  .app-header h1 {
    font-size: 1.8rem;
  }
  
  .app-main {
    padding: 8px;
  }
}
</style>