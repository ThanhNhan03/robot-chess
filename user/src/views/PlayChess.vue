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
            <div class="option-card">
              <div class="option-icon">🤖</div>
              <h3>Play vs AI</h3>
              <p>Challenge chess AI</p>
            </div>
            
            <div class="option-card">
              <div class="option-icon">👥</div>
              <h3>Two Players</h3>
              <p>Play with friend</p>
            </div>
            
            <div class="option-card">
              <div class="option-icon">🎯</div>
              <h3>Practice</h3>
              <p>Improve skills</p>
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
        <div class="game-header">
          <div class="player-info">
            <div class="player-avatar">GH</div>
            <div class="player-details">
              <div class="player-name">Gia Hoang</div>
              <div class="player-rating">⭐ 1650</div>
            </div>
          </div>
          
          <div class="game-controls">
            <button @click="undoMove" class="control-btn" :disabled="!canUndo">
              ↩️ Undo
            </button>
            <button @click="resetGame" class="control-btn">
              🔄 New Game
            </button>
            <button @click="exitGame" class="control-btn exit-btn">
              ❌ Exit
            </button>
          </div>
          
          <div class="opponent-info">
            <div class="player-details">
              <div class="player-name">Opponent</div>
              <div class="player-rating">⭐ 1600</div>
            </div>
            <div class="player-avatar ai">🤖</div>
          </div>
        </div>
        
        <div class="game-content">
          <div class="chess-board-wrapper">
            <div class="game-status-bar">
              <div class="status-message">
                {{ currentPlayer === 'white' ? '⚪ White' : '⚫ Black' }} to move
              </div>
            </div>
            
            <div class="board-notice">
              <p>🎯 Chess Board Integration</p>
              <p class="board-hint">
                The full chess board from 
                <a href="http://localhost:5173/" target="_blank">localhost:5173</a>
                will be integrated here
              </p>
              <div class="board-mockup">
                <div v-for="n in 64" :key="n" class="board-square" 
                     :class="{ dark: Math.floor((n-1)/8) % 2 === (n-1) % 2 }">
                </div>
              </div>
            </div>
          </div>
          
          <div class="game-sidebar">
            <div class="sidebar-card">
              <h3>📜 Move History</h3>
              <div class="move-list">
                <div v-if="moveHistory.length === 0" class="no-moves">
                  No moves yet
                </div>
                <div v-for="(move, index) in moveHistory" :key="index" class="move-item">
                  <span class="move-number">{{ Math.floor(index / 2) + 1 }}.</span>
                  <span class="move-notation">{{ move }}</span>
                </div>
              </div>
            </div>
            
            <div class="sidebar-card">
              <h3>💡 Quick Tips</h3>
              <div class="tips-content">
                <p>• Control the center</p>
                <p>• Develop pieces early</p>
                <p>• Protect your king</p>
                <p>• Think ahead</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const gameStarted = ref(false)
const canUndo = ref(false)
const currentPlayer = ref('white')
const moveHistory = ref([])

const startNewGame = () => {
  // Redirect to the robot chess interface
  window.location.href = 'http://localhost:5173/'
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
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-bottom: 3rem;
}

.option-card {
  padding: 2rem 1.5rem;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 15px;
  transition: all 0.3s ease;
  cursor: pointer;
}

.option-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
}

.option-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
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

.game-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: white;
  padding: 1.5rem 2rem;
  border-radius: 20px;
  margin-bottom: 2rem;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
}

.player-info, .opponent-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.player-avatar {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  font-weight: 700;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.player-avatar.ai {
  background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
  font-size: 2rem;
}

.player-name {
  font-weight: 700;
  font-size: 1.1rem;
  color: #2c3e50;
}

.player-rating {
  font-size: 0.9rem;
  color: #7f8c8d;
  font-weight: 600;
}

.game-controls {
  display: flex;
  gap: 0.75rem;
}

.control-btn {
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
  font-size: 0.95rem;
}

.control-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
}

.control-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.control-btn.exit-btn {
  background: linear-gradient(135deg, #eb3349 0%, #f45c43 100%);
  box-shadow: 0 4px 15px rgba(235, 51, 73, 0.3);
}

.control-btn.exit-btn:hover {
  box-shadow: 0 6px 20px rgba(235, 51, 73, 0.4);
}

.game-content {
  display: grid;
  grid-template-columns: 1fr 350px;
  gap: 2rem;
}

.chess-board-wrapper {
  background: white;
  border-radius: 20px;
  padding: 1.5rem;
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.2);
}

.game-status-bar {
  text-align: center;
  padding: 1rem;
  margin-bottom: 1rem;
  border-radius: 12px;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
}

.status-message {
  font-weight: 700;
  font-size: 1.2rem;
  color: #2c3e50;
}

.board-notice {
  text-align: center;
  padding: 2rem;
}

.board-notice > p:first-child {
  font-size: 1.5rem;
  font-weight: 700;
  color: #667eea;
  margin-bottom: 0.5rem;
}

.board-hint {
  font-size: 1rem;
  color: #7f8c8d;
  margin-bottom: 2rem;
}

.board-hint a {
  color: #667eea;
  font-weight: 600;
  text-decoration: none;
}

.board-hint a:hover {
  text-decoration: underline;
}

.board-mockup {
  display: grid;
  grid-template-columns: repeat(8, 1fr);
  gap: 2px;
  max-width: 400px;
  margin: 0 auto;
  background: #2c3e50;
  padding: 2px;
  border-radius: 8px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
}

.board-square {
  aspect-ratio: 1;
  background: #f0d9b5;
}

.board-square.dark {
  background: #b58863;
}

.game-sidebar {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.sidebar-card {
  background: white;
  border-radius: 20px;
  padding: 1.5rem;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
}

.sidebar-card h3 {
  font-size: 1.25rem;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 3px solid #667eea;
}

.move-list {
  max-height: 400px;
  overflow-y: auto;
}

.no-moves {
  text-align: center;
  padding: 2rem;
  color: #95a5a6;
  font-style: italic;
}

.move-item {
  padding: 0.75rem;
  margin-bottom: 0.5rem;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 8px;
  display: flex;
  gap: 0.75rem;
  transition: all 0.3s ease;
}

.move-item:hover {
  transform: translateX(5px);
}

.move-number {
  font-weight: 700;
  color: #667eea;
}

.move-notation {
  color: #2c3e50;
  font-weight: 600;
}

.tips-content {
  line-height: 2;
}

.tips-content p {
  color: #2c3e50;
  font-weight: 500;
  padding: 0.5rem;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 8px;
  margin-bottom: 0.5rem;
}

@media (max-width: 1024px) {
  .game-content {
    grid-template-columns: 1fr;
  }
  
  .game-header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .game-controls {
    width: 100%;
    justify-content: center;
  }
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
  
  .control-btn {
    padding: 0.625rem 1rem;
    font-size: 0.875rem;
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
