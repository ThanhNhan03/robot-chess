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

        if data.get("Type") == "ai_request" and data.get("Command") == "start_game":
            payload = data.get("Payload") or {}
            st = payload.get("status")

            if st in ("start", "resume", "end"):
                status["state"] = st
                print("updated state:", status["state"])
                if status["state"] == "end":
                    break

    return

async def playChess(cam, cornersH, hand, piece, fen, stockfish, tcp_client):
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
        await fen.updateFEN(FEN_new, stockfish, tcp_client)
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
    
    status = {"state": "resume"}  # default: waiting

    recv_task = asyncio.create_task(receiveStatus(tcp_client, status))

    # # wait until server sends "start" or "end"
    while True:
        current = status.get("state")
        print("current state:", current)

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
            await playChess(cam, cornersH, hand, piece, fen, stockfish, tcp_client)
            
        await asyncio.sleep(0.1)


if __name__ == "__main__":
    asyncio.run(main())

