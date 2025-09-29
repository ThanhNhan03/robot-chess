// Interface cho FEN message từ server
export interface ChessFenMessage {
  fen_str: string
  timestamp?: string
  source?: string
}

// Interface cho robot command message
export interface RobotCommandMessage {
  goal_id: string
  header: {
    timestamp: string
  }
  move: {
    type: 'move' | 'attack'
    from: string
    to: string
    from_piece: string
    to_piece: string | null
    notation: string
    results_in_check: boolean
  }
}

// Interface cho server response
export interface ServerResponse {
  type: string
  success?: boolean
  goal_id?: string
  response?: any
  error?: string
  message?: string
  timestamp?: string
  connected_robots?: number
  capabilities?: string[]
}