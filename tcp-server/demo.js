import TCPServer from './tcp-server.js';

// Demo script để test các tính năng của TCP Server
console.log('🧪 TCP Server Demo & Test Script');
console.log('================================\n');

// Tạo server instance
const server = new TCPServer(8080, '100.107.161.16');

// Test events
server.on('started', (info) => {
  console.log(`✅ Server started on ${info.host}:${info.port}`);
  
  // Auto tạo một vài test clients sau 2 giây
  setTimeout(() => {
    createTestClients();
  }, 2000);
});

server.on('clientConnected', (clientInfo) => {
  console.log(`🔗 New client: #${clientInfo.id} from ${clientInfo.address}:${clientInfo.port}`);
  
  // Gửi welcome message custom
  setTimeout(() => {
    server.sendToClient(clientInfo.id, {
      type: 'custom_welcome',
      message: `Xin chào Client #${clientInfo.id}! Server đang test đây 🎉`,
      timestamp: new Date().toISOString()
    });
  }, 1000);
});

server.on('clientDisconnected', (info) => {
  console.log(`🔌 Client disconnected: #${info.clientId}`);
});

server.on('dataReceived', (info) => {
  console.log(`📨 Data from #${info.clientId}:`, info.data);
  
  // Auto response cho một số message types
  if (info.data.type === 'test_auto_response') {
    server.sendToClient(info.clientId, {
      type: 'auto_response',
      message: 'Đây là auto response từ server!',
      originalMessage: info.data
    });
  }
});

// Bắt đầu server (async)
server.start().catch(error => {
  console.error('Failed to start server:', error);
  process.exit(1);
});

// Function tạo test clients
function createTestClients() {
  console.log('\n🤖 Creating test clients...');
  
  import('net').then(({ default: net }) => {
    // Lấy port hiện tại của server
    const currentPort = server.port;
    
    // Tạo 3 test clients
    for (let i = 1; i <= 3; i++) {
      setTimeout(() => {
        const client = net.createConnection(currentPort, '100.107.161.16');
        
        client.on('connect', () => {
          console.log(`🤖 Test client ${i} connected`);
          
          // Gửi test messages
          setTimeout(() => {
            client.write(JSON.stringify({
              type: 'test_auto_response',
              content: `Test message từ client ${i}`
            }) + '\n');
          }, 500);
          
          // Test broadcast
          setTimeout(() => {
            client.write(JSON.stringify({
              type: 'broadcast',
              content: `Broadcast từ test client ${i} 📢`
            }) + '\n');
          }, 1500);
          
          // Disconnect after 10 seconds
          setTimeout(() => {
            console.log(`👋 Test client ${i} disconnecting...`);
            client.end();
          }, 8000 + (i * 1000));
        });
        
        client.on('data', (data) => {
          const messages = data.toString().trim().split('\n');
          messages.forEach(msg => {
            if (msg) {
              try {
                const parsed = JSON.parse(msg);
                console.log(`📱 Test client ${i} received:`, parsed.type, parsed.message || parsed);
              } catch (e) {
                console.log(`📱 Test client ${i} received raw:`, msg);
              }
            }
          });
        });
        
      }, i * 1000);
    }
  });
}

// Demo các server methods sau 5 giây
setTimeout(() => {
  console.log('\n🔧 Testing server methods...');
  
  // Lấy stats
  const stats = server.getStats();
  console.log('📊 Server stats:', {
    totalClients: stats.totalClients,
    uptime: Math.floor(stats.serverUptime) + 's'
  });
  
  // Test broadcast
  server.broadcast({
    type: 'server_announcement',
    message: '📢 Đây là announcement từ server!',
    timestamp: new Date().toISOString()
  });
  
  // List all clients
  const allClients = server.getAllClients();
  console.log('👥 All clients:', allClients.map(c => `#${c.id}`));
  
}, 5000);

// Auto shutdown sau 15 giây
setTimeout(() => {
  console.log('\n🛑 Auto shutting down server...');
  server.stop();
  process.exit(0);
}, 15000);

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\n🛑 Shutting down...');
  server.stop();
  process.exit(0);
});