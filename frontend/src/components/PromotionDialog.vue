<template>
  <div v-if="isVisible" class="promotion-overlay" @click="handleOverlayClick">
    <div class="promotion-dialog" @click.stop>
      <h3 class="promotion-title">Choose Promotion Piece</h3>
      <p class="promotion-subtitle">Select the piece to promote your pawn to:</p>
      
      <div class="promotion-options">
        <button
          class="promotion-option"
          @click="selectPiece('q')"
          :title="`${playerColor} Queen`"
        >
          <img 
            :src="getPieceImage('q')" 
            :alt="`${playerColor} Queen`"
            class="promotion-piece-image"
          />
          <span class="promotion-label">Queen</span>
        </button>
        
        <button
          class="promotion-option"
          @click="selectPiece('r')"
          :title="`${playerColor} Rook`"
        >
          <img 
            :src="getPieceImage('r')" 
            :alt="`${playerColor} Rook`"
            class="promotion-piece-image"
          />
          <span class="promotion-label">Rook</span>
        </button>
        
        <button
          class="promotion-option"
          @click="selectPiece('b')"
          :title="`${playerColor} Bishop`"
        >
          <img 
            :src="getPieceImage('b')" 
            :alt="`${playerColor} Bishop`"
            class="promotion-piece-image"
          />
          <span class="promotion-label">Bishop</span>
        </button>
        
        <button
          class="promotion-option"
          @click="selectPiece('n')"
          :title="`${playerColor} Knight`"
        >
          <img 
            :src="getPieceImage('n')" 
            :alt="`${playerColor} Knight`"
            class="promotion-piece-image"
          />
          <span class="promotion-label">Knight</span>
        </button>
      </div>
      
      <div class="promotion-actions">
        <button class="cancel-btn" @click="cancel">Cancel</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  isVisible: boolean
  currentPlayer: 'white' | 'black'
}

interface Emits {
  selectPromotion: [piece: 'q' | 'r' | 'b' | 'n']
  cancel: []
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Computed properties
const playerColor = computed(() => props.currentPlayer === 'white' ? 'White' : 'Black')

// Methods
const getPieceImage = (pieceType: 'q' | 'r' | 'b' | 'n') => {
  const prefix = props.currentPlayer === 'white' ? 'w' : 'b'
  const pieceCode = `${prefix}${pieceType}`
  return new URL(`../assets/${pieceCode}.png`, import.meta.url).href
}

const selectPiece = (piece: 'q' | 'r' | 'b' | 'n') => {
  emit('selectPromotion', piece)
}

const cancel = () => {
  emit('cancel')
}

const handleOverlayClick = () => {
  cancel()
}
</script>

<style scoped>
.promotion-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  animation: fadeIn 0.2s ease-out;
}

.promotion-dialog {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
  max-width: 400px;
  width: 90%;
  animation: scaleIn 0.3s ease-out;
}

.promotion-title {
  margin: 0 0 8px 0;
  text-align: center;
  font-size: 20px;
  font-weight: bold;
  color: #333;
}

.promotion-subtitle {
  margin: 0 0 20px 0;
  text-align: center;
  font-size: 14px;
  color: #666;
}

.promotion-options {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  margin-bottom: 20px;
}

.promotion-option {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 16px 12px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  background: #fafafa;
  cursor: pointer;
  transition: all 0.2s ease;
  min-height: 100px;
}

.promotion-option:hover {
  border-color: #4CAF50;
  background: #f5f5f5;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(76, 175, 80, 0.3);
}

.promotion-option:active {
  transform: translateY(0);
}

.promotion-piece-image {
  width: 48px;
  height: 48px;
  object-fit: contain;
  margin-bottom: 8px;
  filter: drop-shadow(2px 2px 4px rgba(0, 0, 0, 0.2));
}

.promotion-label {
  font-size: 12px;
  font-weight: bold;
  color: #333;
  text-align: center;
}

.promotion-actions {
  display: flex;
  justify-content: center;
}

.cancel-btn {
  padding: 8px 16px;
  border: 1px solid #ccc;
  border-radius: 6px;
  background: #f5f5f5;
  color: #666;
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 14px;
}

.cancel-btn:hover {
  background: #e0e0e0;
  border-color: #999;
}

/* Animations */
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes scaleIn {
  from { 
    opacity: 0;
    transform: scale(0.8);
  }
  to { 
    opacity: 1;
    transform: scale(1);
  }
}

/* Responsive design */
@media (max-width: 480px) {
  .promotion-dialog {
    padding: 16px;
    margin: 16px;
  }
  
  .promotion-options {
    grid-template-columns: repeat(4, 1fr);
    gap: 8px;
  }
  
  .promotion-option {
    padding: 12px 8px;
    min-height: 80px;
  }
  
  .promotion-piece-image {
    width: 36px;
    height: 36px;
  }
  
  .promotion-label {
    font-size: 10px;
  }
}
</style>