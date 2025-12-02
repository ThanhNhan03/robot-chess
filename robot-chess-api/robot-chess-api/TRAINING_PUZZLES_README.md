# Training Puzzles - Hướng dẫn sử dụng

## Tổng quan
Hệ thống hỗ trợ 2 chế độ chơi:
1. **normal_game** - Chơi cờ bình thường từ vị trí khởi đầu
2. **training_puzzle** - Giải các thế cờ đã được thiết lập sẵn

## Cài đặt ban đầu

### 1. Khởi tạo 10 thế cờ hardcode vào database
```bash
POST /api/trainingpuzzles/initialize
```

Hệ thống sẽ tạo 10 thế cờ với 3 mức độ khó:
- **Easy** (3 puzzles): Các thế cờ chiếu hết đơn giản
- **Medium** (3 puzzles): Các thế cờ tấn công phức tạp hơn
- **Hard** (4 puzzles): Các thế cờ chiến thuật nâng cao

## API Endpoints

### 1. Lấy tất cả puzzles
```bash
GET /api/trainingpuzzles
```

### 2. Lấy puzzles theo độ khó
```bash
GET /api/trainingpuzzles/difficulty/{difficulty}
# difficulty: easy, medium, hard
```

### 3. Lấy puzzle ngẫu nhiên theo độ khó
```bash
GET /api/trainingpuzzles/random/{difficulty}
```

### 4. Lấy puzzle theo ID
```bash
GET /api/trainingpuzzles/{puzzleId}
```

## Cách bắt đầu game với Training Puzzle

### Cách 1: Để hệ thống tự chọn puzzle ngẫu nhiên
```json
POST /api/games/start
{
  "gameTypeCode": "training_puzzle",
  "difficulty": "medium"
  // Không cần truyền puzzleId, hệ thống sẽ tự chọn ngẫu nhiên
}
```

### Cách 2: Chọn puzzle cụ thể
```json
POST /api/games/start
{
  "gameTypeCode": "training_puzzle",
  "difficulty": "medium",
  "puzzleId": "550e8400-e29b-41d4-a716-446655440000"
  // Truyền puzzleId nếu muốn chơi puzzle cụ thể
}
```

### Response
```json
{
  "gameId": "...",
  "requestId": "...",
  "gameTypeCode": "training_puzzle",
  "difficulty": "medium",
  "status": "in_progress",
  "message": "Game started successfully",
  "fenStart": "5r1k/1b2Nppp/8/2R5/4Q3/8/5PPP/6K1 w - - 0 1",
  "puzzleId": "550e8400-e29b-41d4-a716-446655440000"
}
```

## Workflow cho Frontend

### 1. Người chơi chọn chế độ "Training Puzzle"
```javascript
// Frontend hiển thị 2 options:
// - Normal Game
// - Training Puzzle
```

### 2. Nếu chọn Training Puzzle, chọn độ khó
```javascript
// Frontend hiển thị dropdown:
// - Easy
// - Medium  
// - Hard
```

### 3. Gọi API start game
```javascript
const response = await fetch('/api/games/start', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': 'Bearer ' + token
  },
  body: JSON.stringify({
    gameTypeCode: 'training_puzzle',
    difficulty: selectedDifficulty // 'easy', 'medium', or 'hard'
  })
});

const gameData = await response.json();
// gameData.fenStart chứa FEN của puzzle
// gameData.puzzleId chứa ID của puzzle đang chơi
```

### 4. Hiển thị bàn cờ với FEN từ puzzle
```javascript
// Sử dụng gameData.fenStart để setup bàn cờ
chessBoard.position(gameData.fenStart);
```

## Danh sách 10 Puzzles

### Easy (3 puzzles)
1. **Scholar's Mate Pattern**
   - FEN: `r1bqkb1r/pppp1ppp/2n2n2/4p2Q/2B1P3/8/PPPP1PPP/RNB1K1NR w KQkq - 0 1`
   - Solution: `Qxf7#`

2. **Fried Liver Attack**
   - FEN: `r1bqk2r/pppp1ppp/2n2n2/2b1p3/2B1P3/3P1N2/PPP2PPP/RNBQK2R w KQkq - 0 1`
   - Solution: `Bxf7+`

3. **Simple Back Rank Mate**
   - FEN: `r1bqkbnr/pppp1ppp/2n5/4p3/2B1P3/5Q2/PPPP1PPP/RNB1K1NR w KQkq - 0 1`
   - Solution: `Qxf7#`

### Medium (3 puzzles)
4. **Knight and Queen Combo**
   - FEN: `5r1k/1b2Nppp/8/2R5/4Q3/8/5PPP/6K1 w - - 0 1`
   - Solution: `Qh4`

5. **Bishop Sacrifice**
   - FEN: `r1bq1rk1/ppp2ppp/2np1n2/2b1p3/2B1P3/2NP1N2/PPP2PPP/R1BQ1RK1 w - - 0 1`
   - Solution: `Bxf7+`

6. **Knight Fork**
   - FEN: `r2qkb1r/pp2nppp/3p4/2pNN1B1/2BnP3/3P4/PPP2PPP/R2bK2R w KQkq - 0 1`
   - Solution: `Nf6+`

### Hard (4 puzzles)
7. **Complex Tactical Position**
   - FEN: `r1b2rk1/ppp2ppp/8/4N3/1b1pn3/8/PPP2PPP/RNBQR1K1 w - - 0 1`
   - Solution: `Qxd4`

8. **Advanced Knight Maneuver**
   - FEN: `r4rk1/1b3ppp/pq1bpn2/1p6/3NP3/PBN2P2/1P1Q2PP/2R2RK1 w - - 0 1`
   - Solution: `Nf5`

9. **Discovered Attack**
   - FEN: `r2q1rk1/pb2bppp/1p2pn2/3pN3/3P4/P2B1N2/1P3PPP/R2QR1K1 w - - 0 1`
   - Solution: `Nxf7`

10. **Pin Exploitation**
    - FEN: `r1b1r1k1/pp1n1pbp/1qp3p1/3p4/1P1P4/P1N1PN2/4BPPP/R2QR1K1 w - - 0 1`
    - Solution: `Bh5`

## Kiểm tra Solution

Sau khi người chơi thực hiện nước đi, có thể kiểm tra xem họ đã giải đúng puzzle chưa bằng cách:
1. So sánh nước đi của họ với `solutionMove` trong database
2. Hoặc sử dụng chess engine để validate

## Notes
- Mỗi puzzle có một giải pháp tối ưu (`solutionMove`)
- Frontend có thể hiển thị hint hoặc solution sau khi người chơi thử một số lần
- Có thể thêm tính năng đếm số lần thử, thời gian giải puzzle để tính điểm
