import { Chess } from 'chess.js';
import type { Square, PieceSymbol, Move } from 'chess.js';

export type Color = 'w' | 'b';

export interface GameMove {
  from: Square;
  to: Square;
  piece: PieceSymbol;
  captured?: PieceSymbol;
  promotion?: PieceSymbol;
  san: string;
  lan: string;
  flags: string;
}

export interface GameState {
  fen: string;
  turn: Color;
  isCheck: boolean;
  isCheckmate: boolean;
  isStalemate: boolean;
  isDraw: boolean;
  isGameOver: boolean;
  moves: string[];
  history: GameMove[];
}

export class ChessGame {
  private chess: Chess;
  private moveHistory: GameMove[] = [];

  constructor(fen?: string) {
    this.chess = new Chess(fen);
  }

  /**
   * Lấy trạng thái hiện tại của game
   */
  getGameState(): GameState {
    return {
      fen: this.chess.fen(),
      turn: this.chess.turn(),
      isCheck: this.chess.inCheck(),
      isCheckmate: this.chess.isCheckmate(),
      isStalemate: this.chess.isStalemate(),
      isDraw: this.chess.isDraw(),
      isGameOver: this.chess.isGameOver(),
      moves: this.chess.moves(),
      history: this.moveHistory
    };
  }

  /**
   * Lấy tất cả nước đi hợp lệ
   */
  getLegalMoves(square?: Square): string[] {
    if (square) {
      return this.chess.moves({ square, verbose: false });
    }
    return this.chess.moves();
  }

  /**
   * Lấy nước đi hợp lệ chi tiết
   */
  getLegalMovesVerbose(square?: Square): Move[] {
    if (square) {
      return this.chess.moves({ square, verbose: true });
    }
    return this.chess.moves({ verbose: true });
  }

  /**
   * Thực hiện nước đi
   */
  makeMove(move: string | { from: Square; to: Square; promotion?: PieceSymbol }): GameMove | null {
    try {
      const moveResult = this.chess.move(move);
      if (moveResult) {
        const gameMove: GameMove = {
          from: moveResult.from,
          to: moveResult.to,
          piece: moveResult.piece,
          captured: moveResult.captured,
          promotion: moveResult.promotion,
          san: moveResult.san,
          lan: moveResult.lan,
          flags: moveResult.flags
        };
        this.moveHistory.push(gameMove);
        return gameMove;
      }
      return null;
    } catch (error) {
      console.error('Nước đi không hợp lệ:', error);
      return null;
    }
  }

  /**
   * Hoàn tác nước đi cuối
   */
  undoMove(): GameMove | null {
    const undoneMove = this.chess.undo();
    if (undoneMove && this.moveHistory.length > 0) {
      this.moveHistory.pop();
      return {
        from: undoneMove.from,
        to: undoneMove.to,
        piece: undoneMove.piece,
        captured: undoneMove.captured,
        promotion: undoneMove.promotion,
        san: undoneMove.san,
        lan: undoneMove.lan,
        flags: undoneMove.flags
      };
    }
    return null;
  }

  /**
   * Kiểm tra nước đi có hợp lệ không
   */
  isValidMove(move: string | { from: Square; to: Square; promotion?: PieceSymbol }): boolean {
    try {
      // Tạo một bản sao để test
      const tempChess = new Chess(this.chess.fen());
      const result = tempChess.move(move);
      return result !== null;
    } catch {
      return false;
    }
  }

  /**
   * Lấy quân cờ tại ô vuông
   */
  getPiece(square: Square) {
    return this.chess.get(square);
  }

  /**
   * Lấy bàn cờ dưới dạng mảng 2D
   */
  getBoard() {
    return this.chess.board();
  }

  /**
   * Reset game về trạng thái ban đầu
   */
  reset(): void {
    this.chess.reset();
    this.moveHistory = [];
  }

  /**
   * Load game từ FEN string
   */
  loadFen(fen: string): boolean {
    try {
      this.chess.load(fen);
      this.moveHistory = [];
      return true;
    } catch {
      return false;
    }
  }

  /**
   * Load game từ PGN string
   */
  loadPgn(pgn: string): boolean {
    try {
      this.chess.loadPgn(pgn);
      // Rebuild move history from PGN
      this.rebuildMoveHistory();
      return true;
    } catch {
      return false;
    }
  }

  /**
   * Xuất game ra PGN string
   */
  getPgn(): string {
    return this.chess.pgn();
  }

  /**
   * Xuất trạng thái game ra FEN string
   */
  getFen(): string {
    return this.chess.fen();
  }

  /**
   * Kiểm tra nhập thành có hợp lệ không
   */
  canCastle(side: 'k' | 'q' | 'K' | 'Q'): boolean {
    // Simplified castling check using available moves
    const moves = this.chess.moves({ verbose: true });
    
    if (this.chess.turn() === 'w') {
      if (side === 'K') {
        return moves.some(move => move.san === 'O-O');
      } else if (side === 'Q') {
        return moves.some(move => move.san === 'O-O-O');
      }
    } else {
      if (side === 'k') {
        return moves.some(move => move.san === 'O-O');
      } else if (side === 'q') {
        return moves.some(move => move.san === 'O-O-O');
      }
    }
    
    return false;
  }

  /**
   * Lấy các ô bị tấn công bởi một bên
   */
  getAttackedSquares(color: Color): Square[] {
    const squares: Square[] = [];
    
    for (let rank = 0; rank < 8; rank++) {
      for (let file = 0; file < 8; file++) {
        const square = (String.fromCharCode(97 + file) + (8 - rank)) as Square;
        // Note: chess.js might not have isAttacked method in newer versions
        // This is a placeholder implementation
        try {
          if ((this.chess as any).isAttacked && (this.chess as any).isAttacked(square, color)) {
            squares.push(square);
          }
        } catch (e) {
          // Method not available, skip
        }
      }
    }
    
    return squares;
  }

  /**
   * Kiểm tra xem có phải threefold repetition không
   */
  isThreefoldRepetition(): boolean {
    return this.chess.isThreefoldRepetition();
  }

  /**
   * Kiểm tra insufficient material
   */
  isInsufficientMaterial(): boolean {
    return this.chess.isInsufficientMaterial();
  }

  /**
   * Lấy số lượng nước đi từ lần bắt quân hoặc đẩy tốt cuối cùng
   */
  getHalfmoveClock(): number {
    return parseInt(this.chess.fen().split(' ')[4]);
  }

  /**
   * Lấy số lượng nước đi đầy đủ
   */
  getFullmoveNumber(): number {
    return parseInt(this.chess.fen().split(' ')[5]);
  }

  /**
   * Rebuild move history từ current position
   */
  private rebuildMoveHistory(): void {
    this.moveHistory = [];
    const history = this.chess.history({ verbose: true });
    
    for (const move of history) {
      const gameMove: GameMove = {
        from: move.from,
        to: move.to,
        piece: move.piece,
        captured: move.captured,
        promotion: move.promotion,
        san: move.san,
        lan: move.lan,
        flags: move.flags
      };
      this.moveHistory.push(gameMove);
    }
  }

  /**
   * Lấy lịch sử nước đi
   */
  getMoveHistory(): GameMove[] {
    return [...this.moveHistory];
  }

  /**
   * Lấy nước đi cuối cùng
   */
  getLastMove(): GameMove | null {
    return this.moveHistory.length > 0 ? this.moveHistory[this.moveHistory.length - 1] : null;
  }

  /**
   * Kiểm tra game đã kết thúc chưa và lý do
   */
  getGameResult(): { isGameOver: boolean; result?: string; reason?: string } {
    if (!this.chess.isGameOver()) {
      return { isGameOver: false };
    }

    if (this.chess.isCheckmate()) {
      const winner = this.chess.turn() === 'w' ? 'Black' : 'White';
      return {
        isGameOver: true,
        result: winner === 'White' ? '1-0' : '0-1',
        reason: 'Checkmate'
      };
    }

    if (this.chess.isStalemate()) {
      return {
        isGameOver: true,
        result: '1/2-1/2',
        reason: 'Stalemate'
      };
    }

    if (this.chess.isThreefoldRepetition()) {
      return {
        isGameOver: true,
        result: '1/2-1/2',
        reason: 'Threefold repetition'
      };
    }

    if (this.chess.isInsufficientMaterial()) {
      return {
        isGameOver: true,
        result: '1/2-1/2',
        reason: 'Insufficient material'
      };
    }

    // 50-move rule
    if (this.getHalfmoveClock() >= 100) {
      return {
        isGameOver: true,
        result: '1/2-1/2',
        reason: '50-move rule'
      };
    }

    return {
      isGameOver: true,
      result: '1/2-1/2',
      reason: 'Draw'
    };
  }
}