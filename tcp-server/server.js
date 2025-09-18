import TCPServer from './tcp-server.js';

// Tạo TCP Server instance
const server = new TCPServer(8080, '100.107.161.16');

// Lắng nghe events
server.on('started', (info) => {
  console.log(`🎉 Server started on ${info.host}:${info.port}`);
});

server.on('clientConnected', (clientInfo) => {
  console.log(`🔗 Client connected: #${clientInfo.id} from ${clientInfo.address}:${clientInfo.port}`);
});

server.on('clientDisconnected', (info) => {
  console.log(`🔌 Client disconnected: #${info.clientId}`);
});

server.on('dataReceived', (info) => {
  console.log(`📨 Data from #${info.clientId}:`, info.data);
});

server.on('error', (error) => {
  console.error('💥 Server error:', error);
});

// Bắt đầu server (async)
server.start().catch(error => {
  console.error('Failed to start server:', error);
  process.exit(1);
});

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\n🛑 Đang tắt server...');
  server.stop();
  process.exit(0);
});

process.on('SIGTERM', () => {
  console.log('\n🛑 Đang tắt server...');
  server.stop();
  process.exit(0);
});

// Hiển thị thống kê mỗi 30 giây
setInterval(() => {
  const stats = server.getStats();
  console.log('\n📊 Server Stats:');
  console.log(`   - Total clients: ${stats.totalClients}`);
  console.log(`   - Uptime: ${Math.floor(stats.serverUptime)}s`);
  if (stats.totalClients > 0) {
    console.log('   - Connected clients:');
    stats.clients.forEach(client => {
      console.log(`     • #${client.id} from ${client.address}:${client.port}`);
    });
  }
  console.log('');
}, 30000);