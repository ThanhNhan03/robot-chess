import { ref, type Ref } from 'vue'
import webSocketService from '../services/webSocketService'
import type { FenLogic } from './useFenLogic'

export interface WebSocketLogic {
  isConnected: Ref<boolean>
  initializeWebSocket: (chessBoardLogic?: any) => Promise<void>
  cleanupWebSocket: () => void
}

export function useWebSocketLogic(fenLogic: FenLogic): WebSocketLogic {
  // Game state
  const isConnected = ref(false)
  let chessBoardLogicRef: any = null

  // Handle incoming FEN messages from WebSocket
  const handleWebSocketFen = (data: any) => {
    try {
      if (data.fen_str) {
        console.log(`Received FEN from WebSocket: ${data.fen_str}`)
        
        // If we have chess board logic reference, use the external FEN update method
        if (chessBoardLogicRef && chessBoardLogicRef.updateFromExternalFen) {
          chessBoardLogicRef.updateFromExternalFen(data.fen_str)
        } else {
          // Fallback to direct FEN logic update (old behavior)
          fenLogic.updateFromFen(data.fen_str)
        }
        
        console.log('Board updated from WebSocket FEN')
      }
    } catch (error) {
      console.error('Error handling FEN message:', error, data)
    }
  }

  // Initialize WebSocket connection
  const initializeWebSocket = async (chessBoardLogic?: any) => {
    // Store reference to chess board logic for proper FEN handling
    chessBoardLogicRef = chessBoardLogic
    
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