<template>
  <div class="play-chess">
    <div class="container">
      <h1 class="page-title">♟️ Play Chess</h1>
      
      <!-- Welcome Screen -->
      <div v-if="!gameStarted" class="welcome-screen">
        <div class="welcome-card">
          <div class="welcome-icon">♟️</div>
          <h2>Welcome, Gia Hoang!</h2>
          <p class="welcome-text">Ready to start a chess game?</p>

          <div class="game-options">
            <div class="option-card" @click="selectDifficulty('easy')">
              <div class="option-icon">😌</div>
              <h3>Easy</h3>
              <p>Relax and enjoy</p>
            </div>

            <div class="option-card" @click="selectDifficulty('medium')">
              <div class="option-icon">🤔</div>
              <h3>Medium</h3>
              <p>A balanced challenge</p>
            </div>

            <div class="option-card" @click="selectDifficulty('hard')">
              <div class="option-icon">😤</div>
              <h3>Hard</h3>
              <p>Test your skills</p>
            </div>
          </div>
          
          <button @click="startNewGame" class="btn btn-start">
            <span class="btn-icon">🎮</span>
            Start New Game
          </button>
          
          <p class="redirect-notice">
            Or visit the 
            <a href="http://localhost:5173/" target="_blank" class="chess-link">
              🤖 Robot Chess Interface
            </a>
          </p>
        </div>
      </div>
      
      <!-- Game Screen -->
      <div v-else class="game-screen">
        <ChessRobotScreen />
      </div>
      
      <!-- Confirmation Dialog -->
      <div v-if="showStartDialog" class="dialog-overlay" @click.self="cancelStartGame">
        <div class="dialog-box">
          <div class="dialog-icon">🎮</div>
          <h2 class="dialog-title">Start New Game</h2>
          <p class="dialog-message">Are you ready to start a chess game against the robot?</p>
          
          <div class="dialog-buttons">
            <button @click="cancelStartGame" class="btn btn-cancel">
              Cancel
            </button>
            <button @click="confirmStartGame" class="btn btn-confirm">
              Let's Play!
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ChessRobotScreen from '../screen/chess-robot/ChessRobotScreen.vue'

const gameStarted = ref(false)
const showStartDialog = ref(false)
const canUndo = ref(false)
const currentPlayer = ref('white')
const moveHistory = ref([])
const selectedDifficulty = ref('medium')

const startNewGame = () => {
  showStartDialog.value = true
}

const confirmStartGame = () => {
  showStartDialog.value = false
  gameStarted.value = true
}

const cancelStartGame = () => {
  showStartDialog.value = false
}

const resetGame = () => {
  if (confirm('Start a new game?')) {
    resetGameState()
  }
}

const resetGameState = () => {
  moveHistory.value = []
  currentPlayer.value = 'white'
  canUndo.value = false
}

const undoMove = () => {
  if (moveHistory.value.length > 0) {
    moveHistory.value.pop()
    canUndo.value = moveHistory.value.length > 0
  }
}

const exitGame = () => {
  if (confirm('Exit game?')) {
    gameStarted.value = false
    resetGameState()
  }
}

const selectDifficulty = (difficulty) => {
  selectedDifficulty.value = difficulty
  console.log(`Selected difficulty: ${difficulty}`)
}
</script>

<style scoped>
.play-chess {
  padding: 2rem 0;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 1rem;
}

.page-title {
  text-align: center;
  margin-bottom: 3rem;
  font-size: 3rem;
  font-weight: 800;
  color: white;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.2);
  animation: fadeInDown 0.6s ease-out;
}

@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Welcome Screen */
.welcome-screen {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 60vh;
}

.welcome-card {
  background: white;
  border-radius: 25px;
  padding: 3rem;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  text-align: center;
  max-width: 800px;
  animation: fadeInUp 0.6s ease-out;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.welcome-icon {
  font-size: 5rem;
  margin-bottom: 1rem;
  animation: bounce 2s ease-in-out infinite;
}

@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

.welcome-card h2 {
  font-size: 2.5rem;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 1rem;
}

.welcome-text {
  font-size: 1.2rem;
  color: #7f8c8d;
  margin-bottom: 3rem;
}

.game-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  margin-bottom: 3rem;
}

.option-card {
  flex: 1;
  padding: 1.5rem;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 15px;
  transition: all 0.3s ease;
  cursor: pointer;
  text-align: center;
}

.option-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
}

.option-icon {
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.option-card h3 {
  font-size: 1.25rem;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 0.5rem;
}

.option-card p {
  font-size: 0.9rem;
  color: #7f8c8d;
}

.btn-start {
  padding: 1.25rem 3.5rem;
  font-size: 1.3rem;
  font-weight: 700;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  color: white;
  border-radius: 15px;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin: 0 auto 2rem;
}

.btn-start:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 35px rgba(102, 126, 234, 0.6);
}

.btn-icon {
  font-size: 1.5rem;
}

.redirect-notice {
  font-size: 1rem;
  color: #7f8c8d;
  margin-top: 1rem;
}

.chess-link {
  color: #667eea;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.3s ease;
}

.chess-link:hover {
  color: #764ba2;
  text-decoration: underline;
}

/* Game Screen */
.game-screen {
  animation: fadeIn 0.6s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

/* Confirmation Dialog */
.dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.75);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  animation: fadeIn 0.3s ease-out;
  backdrop-filter: blur(5px);
}

.dialog-box {
  background: white;
  border-radius: 25px;
  padding: 3rem;
  max-width: 500px;
  width: 90%;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.4);
  animation: slideUp 0.4s ease-out;
  text-align: center;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

.dialog-icon {
  font-size: 4rem;
  margin-bottom: 1.5rem;
  animation: bounce 2s ease-in-out infinite;
}

.dialog-title {
  font-size: 2rem;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 1rem;
}

.dialog-message {
  font-size: 1.1rem;
  color: #7f8c8d;
  margin-bottom: 2.5rem;
  line-height: 1.6;
}

.dialog-buttons {
  display: flex;
  gap: 1rem;
  justify-content: center;
}

.btn-cancel,
.btn-confirm {
  padding: 1rem 2.5rem;
  font-size: 1.1rem;
  font-weight: 600;
  border-radius: 12px;
  border: none;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-cancel {
  background: #e0e0e0;
  color: #2c3e50;
}

.btn-cancel:hover {
  background: #d0d0d0;
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
}

.btn-confirm {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
}

.btn-confirm:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 35px rgba(102, 126, 234, 0.6);
}

@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
  }
  
  .welcome-card {
    padding: 2rem;
  }
  
  .game-options {
    grid-template-columns: 1fr;
  }
  
  .dialog-box {
    padding: 2rem;
  }
  
  .dialog-title {
    font-size: 1.5rem;
  }
  
  .dialog-buttons {
    flex-direction: column;
  }
  
  .btn-cancel,
  .btn-confirm {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .welcome-card {
    padding: 1.5rem;
  }
  
  .welcome-icon {
    font-size: 3rem;
  }
  
  .welcome-card h2 {
    font-size: 1.75rem;
  }
  
  .btn-start {
    padding: 1rem 2rem;
    font-size: 1.1rem;
  }
}
</style>
