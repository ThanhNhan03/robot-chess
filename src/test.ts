import { ChessGame } from './chess/ChessGame'

console.log('Testing ChessGame...')

try {
  const game = new ChessGame()
  console.log('ChessGame created successfully')
  
  const state = game.getGameState()
  console.log('Game state:', state)
  
  const moves = game.getLegalMoves()
  console.log('Legal moves:', moves)
  
} catch (error) {
  console.error('Error testing ChessGame:', error)
}
