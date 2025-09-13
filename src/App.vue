<script setup lang="ts">
import CameraView from './components/CameraView.vue'
import ChessBoard from './components/ChessBoard.vue'
import MoveHistory from './components/MoveHistory.vue'
import MQTTLog from './components/MQTTLog.vue'
</script>

<template>
  <div class="app">
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
          <ChessBoard />
        </div>

        <!-- Right Sidebar with History and MQTT Log -->
        <aside class="right-sidebar">
          <div class="sidebar-section">
            <MoveHistory />
          </div>
          <div class="sidebar-section">
            <MQTTLog />
          </div>
        </aside>
      </div>
    </main>
  </div>
</template>

<style scoped>
.app {
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
  grid-template-columns: 1fr 1fr 350px;
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
  min-height: 600px;
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
