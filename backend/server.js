const net = require('net');
const WebSocket = require('ws');

// Cấu hình
const TCP_PORT = 8080;
const WS_PORT = 8081;

// Store clients
let webSocketClients = new Set();
let tcpClients = new Set();

// Tạo WebSocket server cho frontend
const wss = new WebSocket.Server({ port: WS_PORT });
console.log(`WebSocket server đang chạy trên port ${WS_PORT}`);

wss.on('connection', (ws) => {
  console.log('Frontend client đã kết nối');
  webSocketClients.add(ws);

  ws.on('close', () => {
    console.log('Frontend client đã ngắt kết nối');
    webSocketClients.delete(ws);
  });

  ws.on('error', (err) => {
    console.error('WebSocket error:', err);
    webSocketClients.delete(ws);
  });
});

// Broadcast FEN đến tất cả WebSocket clients
function broadcastToWebSocketClients(data) {
  const message = JSON.stringify(data);
  webSocketClients.forEach(client => {
    if (client.readyState === WebSocket.OPEN) {
      client.send(message);
    }
  });
}

// Tạo TCP server cho robot/external clients
const tcpServer = net.createServer((socket) => {
  console.log('TCP client đã kết nối từ:', socket.remoteAddress + ':' + socket.remotePort);
  tcpClients.add(socket);

  // Thiết lập encoding
  socket.setEncoding('utf8');

  // Xử lý dữ liệu nhận được
  socket.on('data', (data) => {
    try {
      const message = data.toString().trim();
      console.log('Nhận được:', message);

      // Parse message - có thể là JSON hoặc plain FEN string
      let fenData;
      
      try {
        // Thử parse JSON trước
        const parsed = JSON.parse(message);
        if (parsed.fen_str || parsed.fen) {
          fenData = {
            fen_str: parsed.fen_str || parsed.fen,
            timestamp: new Date().toISOString(),
            source: 'tcp'
          };
        }
      } catch (e) {
        // Nếu không phải JSON, coi như plain FEN string
        if (message.match(/^[rnbqkpRNBQKP1-8\/\s\-]+$/)) {
          fenData = {
            fen_str: message,
            timestamp: new Date().toISOString(),
            source: 'tcp'
          };
        }
      }

      if (fenData) {
        console.log('FEN hợp lệ:', fenData.fen_str);
        
        // Gửi đến tất cả WebSocket clients (frontend)
        broadcastToWebSocketClients(fenData);
        
        // Gửi response lại cho TCP client
        socket.write(JSON.stringify({
          status: 'success',
          message: 'FEN received and broadcasted',
          timestamp: new Date().toISOString()
        }) + '\n');
      } else {
        console.log('Message không phải FEN hợp lệ');
        socket.write(JSON.stringify({
          status: 'error',
          message: 'Invalid FEN format',
          timestamp: new Date().toISOString()
        }) + '\n');
      }

    } catch (error) {
      console.error('Lỗi xử lý data:', error);
      socket.write(JSON.stringify({
        status: 'error',
        message: 'Server processing error',
        timestamp: new Date().toISOString()
      }) + '\n');
    }
  });

  // Xử lý khi client ngắt kết nối
  socket.on('end', () => {
    console.log('TCP client đã ngắt kết nối');
    tcpClients.delete(socket);
  });

  // Xử lý lỗi
  socket.on('error', (err) => {
    console.error('TCP Socket error:', err);
    tcpClients.delete(socket);
  });

  // Gửi welcome message
  socket.write(JSON.stringify({
    status: 'connected',
    message: 'Welcome to Robot Chess TCP Server',
    timestamp: new Date().toISOString(),
    instructions: 'Send FEN string as JSON: {"fen_str": "your_fen"} or plain FEN string'
  }) + '\n');
});

// Xử lý lỗi server
tcpServer.on('error', (err) => {
  console.error('TCP Server error:', err);
});

// Khởi động TCP server
tcpServer.listen(TCP_PORT, () => {
  console.log(`TCP server đang chạy trên port ${TCP_PORT}`);
  console.log(`Để test, kết nối bằng telnet: telnet localhost ${TCP_PORT}`);
  console.log(`Gửi FEN string: {"fen_str": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"}`);
});

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\nĐang shutdown server...');
  tcpServer.close(() => {
    console.log('TCP server đã shutdown');
  });
  wss.close(() => {
    console.log('WebSocket server đã shutdown');
  });
  process.exit(0);
});

// Log status mỗi 30 giây
setInterval(() => {
  console.log(`Status: ${tcpClients.size} TCP clients, ${webSocketClients.size} WebSocket clients`);
}, 30000);