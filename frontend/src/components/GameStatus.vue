<template>
  <Teleport to="body">
    <Transition name="modal" appear>
      <div v-if="shouldShowStatus" class="modal-overlay" @click="handleOverlayClick">
        <div class="game-status-modal" @click.stop>
          <div v-if="props.isCheckmate" class="status-item status-checkmate">
            <div class="status-icon">👑</div>
            <div class="status-text">
              <div class="checkmate-title">Chiếu hết!</div>
              <div class="winner">{{ props.currentPlayer === 'white' ? 'Đen' : 'Trắng' }} thắng!</div>
              <div class="congratulations">Chúc mừng!</div>
            </div>
          </div>
          
          <div v-else-if="props.isStalemate" class="status-item status-stalemate">
            <div class="status-icon">🤝</div>
            <div class="status-text">
              <div class="stalemate-title">Hòa cờ!</div>
              <div class="draw-reason">Không có nước đi hợp lệ</div>
              <div class="draw-message">Trận đấu kết thúc hòa</div>
            </div>
          </div>

          <div class="modal-actions">
            <button @click="handleNewGame" class="action-btn primary">
              🎯 Ván mới
            </button>
            <button @click="handleClose" class="action-btn secondary">
              ✖️ Đóng
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'

// Props từ parent component
interface Props {
  currentPlayer?: 'white' | 'black'
  isReceivingExternalMove?: boolean
  isCheck?: boolean
  isGameOver?: boolean
  isCheckmate?: boolean
  isStalemate?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  currentPlayer: 'white',
  isReceivingExternalMove: false,
  isCheck: false,
  isGameOver: false,
  isCheckmate: false,
  isStalemate: false
})

// Events emit to parent
const emit = defineEmits<{
  newGame: []
  close: []
}>()

// Chỉ hiển thị status khi game kết thúc (checkmate hoặc stalemate)
const shouldShowStatus = computed(() => {
  return props.isCheckmate || props.isStalemate
})

// Handle functions
const handleNewGame = () => {
  emit('newGame')
}

const handleClose = () => {
  emit('close')
}

const handleOverlayClick = () => {
  emit('close')
}
</script>

<style scoped src="../assets/styles/GameStatus.css">
</style>