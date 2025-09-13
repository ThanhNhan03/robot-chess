<template>
  <div class="chess-board">
    <TheChessboard
      :board-config="boardConfig"
      @board-created="handleBoardCreated"
      @move="handleMove"
      :style="{ width: '500px', height: '500px', maxWidth: '500px', maxHeight: '500px' }"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { TheChessboard } from 'vue3-chessboard'
import 'vue3-chessboard/style.css'
import type { BoardApi, BoardConfig, MoveEvent } from 'vue3-chessboard'
import mqttService from '../services/mqttService'

// Board API and configuration
let boardAPI: BoardApi
const boardConfig: BoardConfig = {
  coordinates: true,
  movable: {
    free: false,
    color: 'both',
    showDests: true,
    rookCastle: true
  },
  drawable: {
    enabled: false
  },
  highlight: {
    lastMove: true,
    check: true
  },
  animation: {
    enabled: true,
    duration: 200
  },
  premovable: {
    enabled: false
  }
}

// Game state
const isConnected = ref(false)
const currentFen = ref('rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1')

// Handle board creation
const handleBoardCreated = (api: BoardApi) => {
  boardAPI = api
}

// Handle move events
const handleMove = (move: MoveEvent) => {
  console.log('♟️ Move made:', move)
  // Get current FEN from the board API after the move
  if (boardAPI) {
    currentFen.value = boardAPI.getFen()
  }
}

// Handle incoming FEN messages
const handleMqttFen = (data: any) => {
  try {
    if (data.fen_str) {
      currentFen.value = data.fen_str
      // Update the board with the new FEN position
      if (boardAPI) {
        boardAPI.setPosition(data.fen_str)
      }
      
      console.log(`📥 Received FEN: ${data.fen_str}`)
      console.log('📋 Board updated from FEN')
    }
  } catch (error) {
    console.error('❌ Error handling FEN message:', error, data)
  }
}

// Initialize MQTT connection
onMounted(async () => {
  try {
    isConnected.value = await mqttService.connect()
    if (isConnected.value) {
      console.log('✅ MQTT connected successfully')
      
      // Subscribe to FEN topic
      const topics = mqttService.getTopics()
      mqttService.subscribe(topics.CHESS_FEN, handleMqttFen)
    } else {
      console.error('❌ Failed to connect to MQTT')
    }
  } catch (error) {
    console.error('❌ MQTT connection error:', error)
  }
})

// Cleanup on component unmount
onBeforeUnmount(() => {
  if (isConnected.value) {
    const topics = mqttService.getTopics()
    mqttService.unsubscribe(topics.CHESS_FEN, handleMqttFen)
  }
})

// Expose methods for external use
defineExpose({
  getBoard: () => currentFen.value,
  getBoardAPI: () => boardAPI,
  isConnected: () => isConnected.value
})
</script>

<style scoped>
/* Force override vue3-chessboard default styles */
:deep(.vue3-chessboard .cg-wrap),
:deep(.cg-wrap) {
  width: min(500px, calc(100vw - 60px)) !important;
  height: min(500px, calc(100vw - 60px)) !important;
  max-width: 500px !important;
  max-height: 500px !important;
}

.chess-board {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  height: 100%;
  padding: 10px;
  min-height: 520px;
}

/* Override vue3-chessboard styles for better centering and sizing */
:deep(.cg-wrap) {
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  border: 2px solid #8B4513;
  background: #DEB887;
  padding: 6px;
  margin: auto;
}

/* Force the chessboard container size */
:deep(.cg-container) {
  width: 100% !important;
  height: 100% !important;
}

/* Force the board element size */
:deep(.cg-board) {
  width: 100% !important;
  height: 100% !important;
}

/* Style the chessboard container */
:deep(.cg-wrap cg-container) {
  border-radius: 8px;
  overflow: hidden;
}

/* Enhance coordinates styling */
:deep(.cg-wrap coords) {
  font-family: 'Segoe UI', sans-serif;
  font-weight: 600;
  color: #654321;
}

/* Responsive adjustments for better balance */
@media (max-width: 1400px) {
  .chess-board {
    padding: 12px;
    min-height: 480px;
  }
  
  :deep(.cg-wrap) {
    width: min(460px, calc(100vw - 80px)) !important;
    height: min(460px, calc(100vw - 80px)) !important;
    max-width: 460px !important;
    max-height: 460px !important;
  }
}

@media (max-width: 1024px) {
  .chess-board {
    padding: 10px;
    min-height: 450px;
  }
  
  :deep(.cg-wrap) {
    width: min(440px, calc(100vw - 60px)) !important;
    height: min(440px, calc(100vw - 60px)) !important;
    max-width: 440px !important;
    max-height: 440px !important;
  }
}

@media (max-width: 768px) {
  .chess-board {
    padding: 8px;
    min-height: 400px;
  }
  
  :deep(.cg-wrap) {
    width: min(380px, calc(100vw - 40px)) !important;
    height: min(380px, calc(100vw - 40px)) !important;
    max-width: 380px !important;
    max-height: 380px !important;
    border-width: 2px;
    padding: 6px;
  }
}

@media (max-width: 480px) {
  .chess-board {
    padding: 8px;
    min-height: 300px;
  }
  
  :deep(.cg-wrap) {
    width: min(280px, calc(100vw - 40px)) !important;
    height: min(280px, calc(100vw - 40px)) !important;
    max-width: 280px !important;
    max-height: 280px !important;
    border-width: 2px;
    padding: 4px;
  }
}

/* Style for piece movement animations */
:deep(.cg-wrap piece) {
  transition: transform 0.2s ease;
}

/* Custom highlighting for selected squares */
:deep(.cg-wrap square.selected) {
  background: rgba(255, 255, 0, 0.4) !important;
}

:deep(.cg-wrap square.move-dest) {
  background: rgba(20, 85, 30, 0.5) !important;
}

/* Improve coordinate styling */
:deep(.cg-wrap coords) {
  font-family: 'Segoe UI', sans-serif;
  font-weight: 600;
  color: #654321;
  font-size: 12px;
}

/* Ensure the board maintains its aspect ratio */
:deep(.cg-wrap cg-container) {
  position: relative;
  width: 100% !important;
  height: 100% !important;
}

/* For larger screens, allow even bigger board */
@media (min-width: 1600px) {
  :deep(.cg-wrap) {
    width: min(550px, calc(100vw - 40px)) !important;
    height: min(550px, calc(100vw - 40px)) !important;
    max-width: 550px !important;
    max-height: 550px !important;
  }
}
</style>
