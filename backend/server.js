const net = require('net');
const WebSocket = require('ws');
require('dotenv').config();

// Cấu hình từ environment variables
const PRIMARY_IP = process.env.PRIMARY_IP || '100.73.130.46';
const FALLBACK_IP = process.env.FALLBACK_IP || 'localhost';
const TCP_PORT = process.env.TCP_PORT || 8080;
const WS_PORT = process.env.WS_PORT || 8081;
const CONNECTION_TIMEOUT = process.env.CONNECTION_TIMEOUT || 3000;

// Alternative ports
const ALT_TCP_PORTS = process.env.ALT_TCP_PORTS ? process.env.ALT_TCP_PORTS.split(',').map(p => parseInt(p)) : [8082, 8083, 8084];
const ALT_WS_PORTS = process.env.ALT_WS_PORTS ? process.env.ALT_WS_PORTS.split(',').map(p => parseInt(p)) : [8085, 8086, 8087];

// Store clients
let webSocketClients = new Set();
let tcpClients = new Set();
let currentServerIP = null;

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

// Function để thử bind server với IP cụ thể
function tryBindServer(ip, port, serverType = 'TCP') {
  return new Promise((resolve, reject) => {
    const testServer = net.createServer();
    
    testServer.on('error', (err) => {
      testServer.close();
      reject(err);
    });

    testServer.listen(port, ip, () => {
      console.log(`${serverType} server có thể bind được vào ${ip}:${port}`);
      testServer.close();
      resolve(ip);
    });
  });
}

// Function để tìm port khả dụng
async function findAvailablePort(ip, primaryPort, altPorts, serverType) {
  // Thử port chính trước
  try {
    await tryBindServer(ip, primaryPort, serverType);
    return primaryPort;
  } catch (error) {
    console.log(`Port ${primaryPort} đã bị chiếm, thử ports alternative...`);
  }

  // Thử các ports alternative
  for (const port of altPorts) {
    try {
      await tryBindServer(ip, port, serverType);
      console.log(`Sử dụng alternative port: ${port}`);
      return port;
    } catch (error) {
      console.log(`Port ${port} cũng bị chiếm...`);
    }
  }

  throw new Error(`Không tìm thấy port khả dụng cho ${serverType} trên ${ip}`);
}

// Function để khởi động TCP server với fallback IP
async function startTCPServer() {
  let serverIP = FALLBACK_IP; // Default fallback
  let serverPort = TCP_PORT;
  
  try {
    // Thử bind vào IP chính trước
    console.log(`Thử kết nối TCP server vào IP chính: ${PRIMARY_IP}:${TCP_PORT}`);
    serverPort = await findAvailablePort(PRIMARY_IP, TCP_PORT, ALT_TCP_PORTS, 'TCP');
    serverIP = PRIMARY_IP;
    console.log(`Sử dụng IP chính: ${PRIMARY_IP}:${serverPort}`);
  } catch (error) {
    console.log(`Không thể bind vào IP chính ${PRIMARY_IP}: ${error.message}`);
    console.log(`Fallback sang IP phụ: ${FALLBACK_IP}`);
    
    try {
      serverPort = await findAvailablePort(FALLBACK_IP, TCP_PORT, ALT_TCP_PORTS, 'TCP');
      serverIP = FALLBACK_IP;
      console.log(`Sử dụng IP fallback: ${FALLBACK_IP}:${serverPort}`);
    } catch (fallbackError) {
      console.error(`Không thể bind vào cả hai IP: ${fallbackError.message}`);
      process.exit(1);
    }
  }

  currentServerIP = `${serverIP}:${serverPort}`;

  // Khởi động TCP server với IP và port đã chọn
  tcpServer.listen(serverPort, serverIP, () => {
    console.log(`TCP server đang chạy trên ${serverIP}:${serverPort}`);
    console.log(`Để test, kết nối bằng telnet: telnet ${serverIP} ${serverPort}`);
    console.log(`Gửi FEN string: {"fen_str": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"}`);
  });

  return { ip: serverIP, port: serverPort };
}

// Function để khởi động WebSocket server với fallback IP
async function startWebSocketServer() {
  let wsIP = FALLBACK_IP; // Default fallback
  let wsPort = WS_PORT;
  
  try {
    // Thử bind vào IP chính trước
    console.log(`Thử kết nối WebSocket server vào IP chính: ${PRIMARY_IP}:${WS_PORT}`);
    wsPort = await findAvailablePort(PRIMARY_IP, WS_PORT, ALT_WS_PORTS, 'WebSocket');
    wsIP = PRIMARY_IP;
    console.log(`WebSocket sử dụng IP chính: ${PRIMARY_IP}:${wsPort}`);
  } catch (error) {
    console.log(`WebSocket không thể bind vào IP chính ${PRIMARY_IP}: ${error.message}`);
    console.log(`WebSocket fallback sang IP phụ: ${FALLBACK_IP}`);
    
    try {
      wsPort = await findAvailablePort(FALLBACK_IP, WS_PORT, ALT_WS_PORTS, 'WebSocket');
      wsIP = FALLBACK_IP;
      console.log(`WebSocket sử dụng IP fallback: ${FALLBACK_IP}:${wsPort}`);
    } catch (fallbackError) {
      console.error(`WebSocket không thể bind vào cả hai IP: ${fallbackError.message}`);
      // WebSocket không bắt buộc, có thể tiếp tục mà không có WebSocket
      console.log('Tiếp tục mà không có WebSocket server...');
      return null;
    }
  }

  // Tạo WebSocket server với IP và port đã chọn
  const wss = new WebSocket.Server({ 
    port: wsPort,
    host: wsIP
  });
  
  console.log(`WebSocket server đang chạy trên ${wsIP}:${wsPort}`);

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

  return { wss, ip: wsIP, port: wsPort };
}

// Khởi động servers
async function startServers() {
  console.log('=== Robot Chess TCP Server ===');
  console.log(`IP chính: ${PRIMARY_IP}`);
  console.log(`IP fallback: ${FALLBACK_IP}`);
  console.log(`TCP Port: ${TCP_PORT} (alt: ${ALT_TCP_PORTS.join(', ')})`);
  console.log(`WebSocket Port: ${WS_PORT} (alt: ${ALT_WS_PORTS.join(', ')})`);
  console.log('===============================\n');

  try {
    // Khởi động WebSocket server trước
    const wsResult = await startWebSocketServer();
    
    // Khởi động TCP server
    const tcpResult = await startTCPServer();

    console.log('\n✓ Server đã khởi động thành công!');
    console.log(`✓ TCP: ${tcpResult.ip}:${tcpResult.port}`);
    if (wsResult) {
      console.log(`✓ WebSocket: ${wsResult.ip}:${wsResult.port}`);
    }
    
    // Cập nhật graceful shutdown
    process.on('SIGINT', () => {
      console.log('\nĐang shutdown server...');
      tcpServer.close(() => {
        console.log('TCP server đã shutdown');
      });
      if (wsResult && wsResult.wss) {
        wsResult.wss.close(() => {
          console.log('WebSocket server đã shutdown');
        });
      }
      process.exit(0);
    });

  } catch (error) {
    console.error('Lỗi khởi động servers:', error);
    process.exit(1);
  }
}

// Khởi động tất cả servers
startServers();

// Log status mỗi 30 giây
setInterval(() => {
  const serverInfo = currentServerIP ? `(${currentServerIP})` : '';
  console.log(`Status: ${tcpClients.size} TCP clients, ${webSocketClients.size} WebSocket clients ${serverInfo}`);
}, 30000);