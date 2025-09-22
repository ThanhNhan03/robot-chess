<template>
  <div class="chess-board">
    <div class="board-container">
      <div class="board-grid">
        <!-- Invisible grid overlay for piece positioning and interaction -->
        <div
          v-for="(_row, rowIndex) in 8"
          :key="rowIndex"
          class="board-row"
        >
          <div
            v-for="(_col, colIndex) in 8"
            :key="colIndex"
            :class="[
              'chess-square',
              { 
                'selected': isSquareSelected(rowIndex, colIndex),
                'possible-move': isPossibleMove(rowIndex, colIndex),
                'in-check': isSquareInCheck(rowIndex, colIndex),
                'in-checkmate': isSquareInCheckmate(rowIndex, colIndex)
              }
            ]"
            @click="handleSquareClick(rowIndex, colIndex)"
          >
            <img
              v-if="getPieceImage(rowIndex, colIndex)"
              :src="getPieceImage(rowIndex, colIndex) || ''"
              :alt="getPieceAlt(rowIndex, colIndex)"
              class="chess-piece"
            />
          </div>
        </div>
      </div>
    </div>
    
    <!-- Promotion Dialog -->
    <PromotionDialog
      :is-visible="promotionPending !== null"
      :current-player="currentPlayer"
      @select-promotion="handlePromotionSelect"
      @cancel="handlePromotionCancel"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount } from 'vue'
import { useFenLogic } from '../composables/useFenLogic'
import { useChessBoardLogic } from '../composables/useChessBoardLogic'
import { useWebSocketLogic } from '../composables/useWebSocketLogic'
import PromotionDialog from './PromotionDialog.vue'

// Initialize composables
const fenLogic = useFenLogic()
const chessBoardLogic = useChessBoardLogic(fenLogic)
const webSocketLogic = useWebSocketLogic(fenLogic)

// Destructure what we need from composables
const { currentBoard, initialFen } = fenLogic
const { 
  handleSquareClick, 
  isSquareSelected, 
  isPossibleMove,
  isSquareInCheck,
  isSquareInCheckmate,
  getPieceImage, 
  getPieceAlt,
  isGameOver,
  isCheck,
  isCheckmate,
  isStalemate,
  currentPlayer,
  moveHistory,
  isReceivingExternalMove,
  promotionPending,
  makePromotionMove,
  cancelPromotion,
  resetGame,
  undoLastMove,
  updateFromExternalFen
} = chessBoardLogic
const { isConnected, initializeWebSocket, cleanupWebSocket } = webSocketLogic

// Component lifecycle
onMounted(async () => {
  // Initialize board from FEN
  fenLogic.initializeBoardFromFen()
  
  // Initialize WebSocket with chess board logic reference
  await initializeWebSocket(chessBoardLogic)
})

// Cleanup on component unmount
onBeforeUnmount(() => {
  cleanupWebSocket()
})

// Handle promotion selection
const handlePromotionSelect = (piece: 'q' | 'r' | 'b' | 'n') => {
  makePromotionMove(piece)
}

const handlePromotionCancel = () => {
  cancelPromotion()
}

// Expose methods for external use
defineExpose({
  getBoard: () => currentBoard.value,
  getFen: () => initialFen.value,
  isConnected: () => isConnected.value,
  updateFromFen: fenLogic.updateFromFen,
  // New chess features
  isGameOver: () => isGameOver.value,
  isCheck: () => isCheck.value,
  isCheckmate: () => isCheckmate.value,
  isStalemate: () => isStalemate.value,
  currentPlayer: () => currentPlayer.value,
  moveHistory: () => moveHistory.value,
  isReceivingExternalMove: () => isReceivingExternalMove.value,
  promotionPending: () => promotionPending.value,
  isSquareInCheckmate,
  resetGame,
  undoLastMove,
  updateFromExternalFen,
  makePromotionMove,
  cancelPromotion
})
</script>

<style scoped src="../assets/styles/ChessBoard.css">
</style>