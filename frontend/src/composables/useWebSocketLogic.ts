import { ref, type Ref } from 'vue'
import webSocketService from '../services/webSocketService'
import type { FenLogic } from './useFenLogic'

export interface WebSocketLogic {
  isConnected: Ref<boolean>
  initializeWebSocket: () => Promise<void>
  cleanupWebSocket: () => void
}

export function useWebSocketLogic(fenLogic: FenLogic): WebSocketLogic {
  // Game state
  const isConnected = ref(false)

  // Handle incoming FEN messages from WebSocket
  const handleWebSocketFen = (data: any) => {
    try {
      if (data.fen_str) {
        fenLogic.updateFromFen(data.fen_str)
        
        console.log(`Received FEN: ${data.fen_str}`)
        console.log('Board updated from FEN')
      }
    } catch (error) {
      console.error('Error handling FEN message:', error, data)
    }
  }

  // Initialize WebSocket connection
  const initializeWebSocket = async () => {
    try {
      isConnected.value = await webSocketService.connect()
      if (isConnected.value) {
        console.log('WebSocket connected successfully')
        
        // Subscribe to FEN messages
        webSocketService.subscribe('fen', handleWebSocketFen)
      } else {
        console.error('Failed to connect to WebSocket')
      }
    } catch (error) {
      console.error('WebSocket connection error:', error)
    }
  }

  // Cleanup WebSocket connection
  const cleanupWebSocket = () => {
    if (isConnected.value) {
      webSocketService.unsubscribe('fen', handleWebSocketFen)
      webSocketService.disconnect()
    }
  }

  return {
    isConnected,
    initializeWebSocket,
    cleanupWebSocket
  }
}