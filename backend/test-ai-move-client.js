const net = require('net');

// Cấu hình kết nối
const TCP_HOST = process.env.TCP_HOST || 'localhost';
const TCP_PORT = process.env.TCP_PORT || 8080;

console.log('=== Test AI Move Client ===');
console.log(`Đang kết nối đến TCP server: ${TCP_HOST}:${TCP_PORT}`);

// Tạo TCP client
const client = new net.Socket();

client.connect(TCP_PORT, TCP_HOST, () => {
  console.log('✅ Đã kết nối đến server!');
  
  // Identify as AI client
  const aiIdentity = {
    type: 'ai_identify',
    ai_id: 'test_ai_move_client'
  };
  
  console.log('📤 Gửi AI identity:', JSON.stringify(aiIdentity));
  client.write(JSON.stringify(aiIdentity) + '\n');
  
  // Wait a bit then send AI response (FEN + robot command)
  setTimeout(() => {
    // Example AI response với FEN và robot command
    const aiResponse = {
      fen_str: "rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 2",
      move: {
        type: "attack",
        from: "d1",
        to: "f7", 
        from_piece: "white_queen",
        to_piece: "black_pawn",
        notation: "Qd1xf7+",
        results_in_check: true
      }
    };
    
    console.log('📤 Gửi AI response (FEN + robot command):', JSON.stringify(aiResponse, null, 2));
    client.write(JSON.stringify(aiResponse) + '\n');
    
    // Test thêm một response khác sau 3 giây
    setTimeout(() => {
      const aiResponse2 = {
        fen_str: "rnbqkb1r/pppp1ppp/5n2/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 2 3",
        move: {
          type: "move",
          from: "e2",
          to: "e4",
          from_piece: "white_pawn", 
          to_piece: null,
          notation: "e4",
          results_in_check: false
        }
      };
      
      console.log('📤 Gửi AI response 2 (FEN + robot command):', JSON.stringify(aiResponse2, null, 2));
      client.write(JSON.stringify(aiResponse2) + '\n');
    }, 3000);
    
  }, 1000);
});

// Xử lý dữ liệu nhận được từ server
client.on('data', (data) => {
  try {
    const response = JSON.parse(data.toString().trim());
    console.log('📥 Nhận phản hồi từ server:', JSON.stringify(response, null, 2));
  } catch (error) {
    console.log('📥 Nhận dữ liệu (raw):', data.toString());
  }
});

// Xử lý khi kết nối bị đóng
client.on('close', () => {
  console.log('❌ Kết nối đã bị đóng');
});

// Xử lý lỗi
client.on('error', (err) => {
  console.error('💥 Lỗi kết nối:', err.message);
});

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\n🛑 Đang ngắt kết nối...');
  client.destroy();
  process.exit(0);
});

console.log('ℹ️  Nhấn Ctrl+C để thoát');