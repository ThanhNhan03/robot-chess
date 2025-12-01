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

def getCorners(cam, corner):
    cornersH = []
    while True:
        ok, frame = cam.read()
        if not ok:
            break
        cornersH = corner.getCorners(frame, 2)
        if cornersH is not None:
            print("Corners found:", cornersH)
            break
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
                    status["game_id"] = game_id
                    status["difficulty"] = difficulty
                    status["game_type"] = game_type
                    status["puzzle_fen"] = puzzle_fen
                    print(f"updated state: {status['state']}, game_id: {game_id}, difficulty: {difficulty}, game_type: {game_type}")
                    if puzzle_fen:
                        print(f"Puzzle FEN: {puzzle_fen}")
                    if status["state"] == "end":
                        break
            
            elif command == "verify_board_setup":
                game_id = payload.get("game_id")
                status["verify_board"] = game_id
                print(f"Received verify board setup request for game: {game_id}")

    return

async def playChess(cam, cornersH, hand, piece, fen, stockfish, tcp_client, game_id=None, difficulty="medium", game_type="normal_game", puzzle_fen=None):
    # Set AI difficulty before starting game
    fen.set_difficulty(difficulty)
    
    # Set initial FEN for puzzle mode
    if game_type == "training_puzzle" and puzzle_fen:
        fen.FEN_last = puzzle_fen
        print(f"[PUZZLE MODE] Starting with FEN: {puzzle_fen}")
    
    board_setup_correct = False
    
    while True:
        ok, frame = cam.read()
        if not ok:
            break  
        # Process Frame
        frame = piece.processFrame(frame)
        # frame = cv2.rotate(frame, cv2.ROTATE_180)
        cv2.imshow("Chess Stream", frame)

        # Get FEN new
        FEN_new = fen.getFEN(frame, cornersH, hand, piece)
        if FEN_new is None:
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
                await asyncio.sleep(1)
            continue
        
        await fen.updateFEN(FEN_new, stockfish, tcp_client, game_id=game_id, check_setup=False)
        renderFen = render_fen(fen.FEN_last)

        if fen.isCheck:
            print("Current state is check")
        if fen.isCheckmate:
            print("Current state is checkmate")
            break
        if fen.isStalemate:
            print("Current state is stalemate")
            break
        if fen.isGameover:
            print("Current state is game over")
            break

        if fen.side == "w":
            print("Robot moves")
        else:
            print("Human moves")

        cv2.imshow("Chess board", renderFen)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            await tcp_client.close()
            break
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

    tcp_client = TCPClient(host="127.0.0.1", port=8080)
    await tcp_client.connect()

    ai_identity = {
       "type": "ai_identify",
       "ai_id": "chess_vision_ai"
    }
    await tcp_client.send(json.dumps(ai_identity) + "\n")    
    print("AI identified with server:", ai_identity)

    cam = cv2.VideoCapture(0)
    #cam = cv2.VideoCapture("test/video1.mp4")
    
    status = {"state": "resume", "difficulty": "medium", "game_type": "normal_game", "puzzle_fen": None}  # default: waiting

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
                    cornersH = getCorners(cam, corner)
                
                if cornersH is not None:
                    FEN_new = fen.getFEN(frame, cornersH, hand, piece)
                    if FEN_new:
                        await fen.send_board_setup_status(tcp_client, FEN_new, game_id=verify_game_id)
            
            # Clear verify request
            status["verify_board"] = None

        if current == "end":
            print("end")
            await tcp_client.close()
            recv_task.cancel()
            cam.release()
            cv2.destroyAllWindows()
            return

        if current == "start":
            ##### Get Corners
            cornersH = getCorners(cam, corner)

            ##### Play Chess
            game_id = status.get("game_id")
            difficulty = status.get("difficulty", "medium")
            game_type = status.get("game_type", "normal_game")
            puzzle_fen = status.get("puzzle_fen")
            await playChess(cam, cornersH, hand, piece, fen, stockfish, tcp_client, game_id=game_id, difficulty=difficulty, game_type=game_type, puzzle_fen=puzzle_fen)
            
        await asyncio.sleep(0.1)


if __name__ == "__main__":
    asyncio.run(main())

