from __future__ import annotations

import os
import cv2
import numpy as np
import re
import asyncio

import chess
import chess.engine
from pathlib import Path
import json
import threading
from typing import Any, Dict, List, Tuple, Optional

from ultralytics import YOLO

from corners import Corner
from piece import Piece
from hand import Hand
from fen import FEN
from network.socket_client import TCPClient
from renderFen2Img import render_fen
from camera_stream import CameraStream, MJPEGStreamServer

async def getCorners(cam, corner, status=None, timeout=30):
    """Get corners with timeout and state checking"""
    cornersH = None
    start_time = asyncio.get_event_loop().time()
    
    while True:
        # Check if game was cancelled
        if status and status.get("state") == "end":
            print("[INFO] Corner detection cancelled - game ended")
            cv2.destroyAllWindows()
            return None
            
        # Check timeout
        if asyncio.get_event_loop().time() - start_time > timeout:
            print("[WARNING] Corner detection timeout")
            return None
            
        ok, frame = cam.read()
        if not ok:
            await asyncio.sleep(0.1)
            continue
            
        cornersH = corner.getCorners(frame, 2)
        if cornersH is not None:
            print("Corners found:", cornersH)
            return cornersH
            
        # Yield control to allow other tasks to run
        await asyncio.sleep(0.05)
    
    return cornersH

def sendOutput(fen, stockfish):
    fen_str = fen.FEN_last
    isCheck = fen.isCheck
    isCheckmate = fen.isCheckmate
    isStalemate = fen.isStalemate
    isGameover = fen.isGameover

    board = chess.Board(fen.FEN_last)
    limit = chess.engine.Limit(time=0.1)
    result = stockfish.analyse(board, limit)

    bestmove = result["pv"][0]
    
    # Determine move type
    if board.is_castling(bestmove):
        mv_type = "castle"
    elif board.is_capture(bestmove) or board.is_en_passant(bestmove):
        mv_type = "attack"
    else:
        mv_type = "move"
    
    from_sq, to_sq = bestmove.from_square, bestmove.to_square
    from_piece = board.piece_at(from_sq)
    to_piece = board.piece_at(to_sq)
    san = board.san(bestmove)

    # Make the move to get the updated position
    board.push(bestmove)
    results_in_check = board.is_check()
    
    def piece_label(p):
        if p is None:
            return None
        color = "white" if p.color == chess.WHITE else "black"
        names = {1: "pawn", 2: "knight", 3: "bishop", 4: "rook", 5: "queen", 6: "king"}
        return f"{color}_{names[p.piece_type]}"
    
    return {
        "fen_str": fen_str,
        "move": {
            "type": mv_type,
            "from": chess.square_name(from_sq),
            "to": chess.square_name(to_sq),
            "from_piece": piece_label(from_piece),
            "to_piece": piece_label(to_piece),
            "notation": san,
            "results_in_check": results_in_check
        }
    }

async def receiveStatus(tcp_client, status):
    while True:
        msg = await tcp_client.receive()
        print("raw msg:", msg)

        if not msg:
            await asyncio.sleep(0.1)
            continue

        if isinstance(msg, (bytes, bytearray)):
            msg = msg.decode()

        try:
            data = json.loads(msg)
        except json.JSONDecodeError:
            # skip invalid JSON
            continue

        if data.get("Type") == "ai_request":
            command = data.get("Command")
            payload = data.get("Payload") or {}
            
            if command == "start_game":
                st = payload.get("status")
                game_id = payload.get("game_id")
                difficulty = payload.get("difficulty", "medium")
                game_type = payload.get("game_type", "normal_game")
                puzzle_fen = payload.get("puzzle_fen")

                if st in ("start", "resume", "end"):
                    status["state"] = st
                    if st == "end":
                        # For end command, just update state - don't update other fields
                        print(f"[RECEIVE] End command received for game: {game_id}")
                    else:
                        # For start/resume, update all fields
                        status["game_id"] = game_id
                        status["difficulty"] = difficulty
                        status["game_type"] = game_type
                        status["puzzle_fen"] = puzzle_fen
                        print(f"updated state: {status['state']}, game_id: {game_id}, difficulty: {difficulty}, game_type: {game_type}")
                        if puzzle_fen:
                            print(f"Puzzle FEN: {puzzle_fen}")
            
            elif command == "verify_board_setup":
                game_id = payload.get("game_id")
                status["verify_board"] = game_id
                print(f"Received verify board setup request for game: {game_id}")

    return

async def playChess(cam, cornersH, hand, piece, fen, stockfish, tcp_client, game_id=None, difficulty="medium", game_type="normal_game", puzzle_fen=None, status=None):
    # Reset FEN to initial position for new game
    if game_type == "training_puzzle" and puzzle_fen:
        fen.FEN_last = puzzle_fen
        print(f"[PUZZLE MODE] Starting with FEN: {puzzle_fen}")
    else:
        # Reset to standard starting position for normal game
        fen.FEN_last = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        print("[NEW GAME] Reset FEN to initial position")
    
    # Reset game state flags
    fen.isCheck = False
    fen.isCheckmate = False
    fen.isStalemate = False
    fen.isGameover = False
    
    # Set AI difficulty before starting game
    fen.set_difficulty(difficulty)
    
    board_setup_correct = False
    
    while True:
        # Check if game should end
        if status and status.get("state") == "end":
            print("[INFO] Game ended - stopping detection")
            break
            
        ok, frame = cam.read()
        if not ok:
            await asyncio.sleep(0.1)
            continue
            
        # Process Frame
        frame = piece.processFrame(frame)
        # frame = cv2.rotate(frame, cv2.ROTATE_180)
        cv2.imshow("Chess Stream", frame)

        # Get FEN new
        FEN_new = fen.getFEN(frame, cornersH, hand, piece)
        if FEN_new is None:
            await asyncio.sleep(0.05)
            continue
        
        # Render current board state (even if setup not correct)
        renderFen = render_fen(FEN_new)
        cv2.imshow("Chess board", renderFen)
        
        # Keep checking board setup until correct
        if not board_setup_correct:
            is_correct = await fen.check_and_send_board_status(tcp_client, FEN_new, game_id)
            if is_correct:
                board_setup_correct = True
                print("[INFO] Board setup verified as correct - starting game")
            else:
                # Wait a bit before checking again
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    await tcp_client.close()
                    break
                await asyncio.sleep(0.5)
            continue
        
        await fen.updateFEN(FEN_new, stockfish, tcp_client, game_id=game_id, check_setup=False)
        renderFen = render_fen(fen.FEN_last)

        # Check for end state again after move processing
        if status and status.get("state") == "end":
            print("[INFO] Game ended during move processing - stopping detection")
            break

        if fen.isCheck:
            print("[CHECK] Check detected")
            # Send check notification
            await fen.send_check_notification(tcp_client, game_id)
        
        # Check for game over conditions
        if fen.isCheckmate:
            print("[GAME OVER] Checkmate detected")
            await fen.send_game_over_notification(tcp_client, game_id)
            break
        if fen.isStalemate:
            print("[GAME OVER] Stalemate detected")
            await fen.send_game_over_notification(tcp_client, game_id)
            break
        if fen.isGameover:
            print("[GAME OVER] Game over detected")
            await fen.send_game_over_notification(tcp_client, game_id)
            break

        if fen.side == "w":
            print("Robot moves")
        else:
            print("Human moves")

        cv2.imshow("Chess board", renderFen)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
            
        # Yield control to allow receiving new messages
        await asyncio.sleep(0.01)
    
    # Cleanup: close all OpenCV windows
    cv2.destroyAllWindows()
    print("[INFO] Exiting playChess - game ended, windows closed")
    return

async def main():
    piece = Piece("models/modelPiece.pt")
    hand = Hand("models/modelHand.pt")
    # size chuẩn 640, 480
    # rate = 640 / width
    corner = Corner("models/modelCorner.pt", 1)

    # 1 - Human cầm White đi trước
    # 2 - Human cầm White đi sau
    # 3 - Human cầm Black đi sau
    # 4 - Human cầm Black đi trước
    fen = FEN(1)

    engine_path = "/usr/games/stockfish"
    stockfish = chess.engine.SimpleEngine.popen_uci(engine_path)

    tcp_client = TCPClient(host="10.17.0.187", port=8080)
    await tcp_client.connect()

    ai_identity = {
       "type": "ai_identify",
       "ai_id": "chess_vision_ai"
    }
    await tcp_client.send(json.dumps(ai_identity) + "\n")    
    print("AI identified with server:", ai_identity)

    cam = cv2.VideoCapture(0)
    #cam = cv2.VideoCapture("test/video1.mp4")
    
    # Start camera streaming server (low resolution for smooth streaming)
    camera_stream = CameraStream(camera_index=0, width=480, height=360, fps=25)
    camera_stream.start_capture(cam=cam)
    
    stream_server = MJPEGStreamServer(camera_stream, host='0.0.0.0', port=8000)
    stream_server.start()
    print("[INFO] Camera stream available at http://10.17.0.187:8000")
    
    status = {"state": "waiting", "difficulty": "medium", "game_type": "normal_game", "puzzle_fen": None}  # default: waiting for start command

    recv_task = asyncio.create_task(receiveStatus(tcp_client, status))

    # # wait until server sends "start" or "end"
    while True:
        current = status.get("state")
        verify_game_id = status.get("verify_board")
        difficulty = status.get("difficulty", "medium")
        print(f"current state: {current}, difficulty: {difficulty}")

        # Handle verify board setup request
        if verify_game_id:
            print(f"Verifying board setup for game: {verify_game_id}")
            ok, frame = cam.read()
            if ok:
                frame = piece.processFrame(frame)
                # Get current corners if not already set
                if 'cornersH' not in locals() or cornersH is None:
                    cornersH = await getCorners(cam, corner, status=status, timeout=10)
                
                if cornersH is not None:
                    FEN_new = fen.getFEN(frame, cornersH, hand, piece)
                    if FEN_new:
                        await fen.send_board_setup_status(tcp_client, FEN_new, game_id=verify_game_id)
            
            # Clear verify request
            status["verify_board"] = None

        if current == "end":
            print("[INFO] Received end command - resetting to waiting state")
            # Close all OpenCV windows and reset status
            cv2.destroyAllWindows()
            # Reset FEN to initial position
            fen.FEN_last = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            fen.isCheck = False
            fen.isCheckmate = False
            fen.isStalemate = False
            fen.isGameover = False
            status["state"] = "waiting"
            status["game_id"] = None
            status["difficulty"] = "medium"
            status["game_type"] = "normal_game"
            status["puzzle_fen"] = None
            print("[INFO] AI is now in waiting state, ready for next game")
            await asyncio.sleep(0.5)
            continue

        if current == "start":
            ##### Get Corners
            cornersH = await getCorners(cam, corner, status=status, timeout=30)
            
            # Check if corner detection was cancelled or timed out
            if cornersH is None:
                print("[INFO] Corner detection failed or cancelled - returning to waiting state")
                cv2.destroyAllWindows()
                status["state"] = "waiting"
                status["game_id"] = None
                continue

            ##### Play Chess
            game_id = status.get("game_id")
            difficulty = status.get("difficulty", "medium")
            game_type = status.get("game_type", "normal_game")
            puzzle_fen = status.get("puzzle_fen")
            
            # Pass status to playChess so it can check for end state
            await playChess(cam, cornersH, hand, piece, fen, stockfish, tcp_client, 
                          game_id=game_id, difficulty=difficulty, game_type=game_type, 
                          puzzle_fen=puzzle_fen, status=status)
            
            # After playChess ends, reset to waiting state
            print("[INFO] Game finished - returning to waiting state")
            cv2.destroyAllWindows()
            # Reset FEN to initial position
            fen.FEN_last = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            fen.isCheck = False
            fen.isCheckmate = False
            fen.isStalemate = False
            fen.isGameover = False
            status["state"] = "waiting"
            status["game_id"] = None
            continue
            
        await asyncio.sleep(0.1)


if __name__ == "__main__":
    asyncio.run(main())

