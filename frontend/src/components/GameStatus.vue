<template>
  <Transition name="game-status" appear>
    <div v-if="shouldShowStatus" class="game-status">
      <div v-if="props.isReceivingExternalMove && !props.isGameOver" class="status-item status-receiving">
        <div class="status-icon">🤖</div>
        <div class="status-text">Robot đang di chuyển...</div>
      </div>
      
      <div v-else-if="props.isCheckmate" class="status-item status-checkmate">
        <div class="status-icon">👑</div>
        <div class="status-text">
          <div class="checkmate-title">Chiếu hết!</div>
          <div class="winner">{{ props.currentPlayer === 'white' ? 'Đen' : 'Trắng' }} thắng!</div>
        </div>
      </div>
      
      <div v-else-if="props.isStalemate" class="status-item status-stalemate">
        <div class="status-icon">🤝</div>
        <div class="status-text">
          <div class="stalemate-title">Hòa cờ!</div>
          <div class="draw-reason">Không có nước đi hợp lệ</div>
        </div>
      </div>
    </div>
  </Transition>
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

// Chỉ hiển thị status khi:
// 1. Game kết thúc (checkmate hoặc stalemate)
// 2. Robot đang di chuyển
const shouldShowStatus = computed(() => {
  return props.isCheckmate || props.isStalemate || (props.isReceivingExternalMove && !props.isGameOver)
})
</script>

<style scoped>
.game-status-enter-active,
.game-status-leave-active {
  transition: all 0.5s ease;
}

.game-status-enter-from,
.game-status-leave-to {
  opacity: 0;
  transform: translateY(-20px) scale(0.95);
}

.game-status {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  padding: 15px;
  margin-bottom: 20px;
  min-height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
}

.game-status::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.4), transparent);
  animation: shimmer 3s infinite;
}

@keyframes shimmer {
  0% { left: -100%; }
  100% { left: 100%; }
}

.status-item {
  display: flex;
  align-items: center;
  gap: 12px;
  text-align: center;
  width: 100%;
  z-index: 1;
  position: relative;
}

.status-icon {
  font-size: clamp(24px, 5vw, 28px);
  filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.1));
}

.status-text {
  font-size: clamp(14px, 3vw, 16px);
  font-weight: 600;
  line-height: 1.4;
}

/* Status-specific styles */
.status-receiving {
  color: #f39c12;
}

.status-receiving .status-text {
  color: #e67e22;
}

.status-checkmate {
  color: #8e44ad;
  animation: pulse-victory 1.5s infinite;
}

.status-checkmate .checkmate-title {
  font-size: clamp(18px, 4vw, 22px);
  font-weight: bold;
  color: #8e44ad;
  margin-bottom: 4px;
}

.status-checkmate .winner {
  font-size: clamp(16px, 3.5vw, 20px);
  color: #27ae60;
  font-weight: bold;
}

.status-stalemate {
  color: #7f8c8d;
}

.status-stalemate .stalemate-title {
  font-size: clamp(18px, 4vw, 20px);
  font-weight: bold;
  color: #34495e;
  margin-bottom: 4px;
}

.status-stalemate .draw-reason {
  font-size: clamp(14px, 3vw, 16px);
  color: #7f8c8d;
}

/* Animations */
@keyframes pulse-victory {
  0%, 100% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.08);
    opacity: 0.9;
  }
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .game-status {
    padding: 12px;
    margin-bottom: 15px;
    min-height: 50px;
  }
  
  .status-item {
    gap: 8px;
  }
}

@media (max-width: 480px) {
  .game-status {
    padding: 10px;
    margin-bottom: 12px;
    min-height: 45px;
  }
  
  .status-item {
    gap: 6px;
    flex-direction: column;
  }
  
  .status-icon {
    margin-bottom: -4px;
  }
}
</style>