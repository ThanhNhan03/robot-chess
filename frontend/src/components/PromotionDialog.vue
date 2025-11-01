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

<style scoped src="../assets/styles/PromotionDialog.css">
</style>