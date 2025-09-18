import net from 'net';
import { EventEmitter } from 'events';

class TCPServer extends EventEmitter {
  constructor(port = 8080, host = '100.107.161.16') {
    super();
    this.port = port;
    this.host = host;
    this.server = null;
    this.clients = new Map(); // Map để lưu trữ clients với ID
    this.nextClientId = 1; // ID counter cho clients
  }

  /**
   * Tìm port khả dụng
   */
  async findAvailablePort(startPort = this.port) {
    return new Promise((resolve) => {
      const testServer = net.createServer();
      testServer.listen(startPort, this.host, () => {
        const port = testServer.address().port;
        testServer.close(() => resolve(port));
      });
      testServer.on('error', () => {
        // Port đã được sử dụng, thử port tiếp theo
        resolve(this.findAvailablePort(startPort + 1));
      });
    });
  }

  /**
   * Bắt đầu server
   */
  async start() {
    this.server = net.createServer();
    
    // Xử lý khi có client kết nối
    this.server.on('connection', (socket) => {
      this.handleNewConnection(socket);
    });

    // Xử lý lỗi server
    this.server.on('error', async (error) => {
      if (error.code === 'EADDRINUSE') {
        console.log(`⚠️ Port ${this.port} đã được sử dụng, đang tìm port khả dụng...`);
        this.port = await this.findAvailablePort(this.port + 1);
        console.log(`🔄 Thử lại với port ${this.port}`);
        this.start(); // Thử lại với port mới
        return;
      }
      console.error('❌ Server error:', error);
      this.emit('error', error);
    });

    // Bắt đầu lắng nghe
    this.server.listen(this.port, this.host, () => {
      console.log(`🚀 TCP Server đang chạy trên ${this.host}:${this.port}`);
      this.emit('started', { host: this.host, port: this.port });
    });
  }

  /**
   * Xử lý kết nối mới
   */
  handleNewConnection(socket) {
    const clientId = this.nextClientId++;
    const clientInfo = {
      id: clientId,
      socket: socket,
      address: socket.remoteAddress,
      port: socket.remotePort,
      connectedAt: new Date()
    };

    // Thêm client vào danh sách
    this.clients.set(clientId, clientInfo);
    
    console.log(`✅ Client #${clientId} kết nối từ ${clientInfo.address}:${clientInfo.port}`);
    console.log(`📊 Tổng số clients: ${this.clients.size}`);
    
    // Gửi thông báo chào mừng cho client
    this.sendToClient(clientId, {
      type: 'welcome',
      clientId: clientId,
      message: `Chào mừng! Bạn được gán ID: ${clientId}`,
      timestamp: new Date().toISOString()
    });

    // Broadcast thông báo có client mới cho tất cả clients khác
    this.broadcastToOthers(clientId, {
      type: 'client_joined',
      clientId: clientId,
      message: `Client #${clientId} đã tham gia`,
      totalClients: this.clients.size
    });

    // Xử lý dữ liệu từ client
    socket.on('data', (data) => {
      this.handleClientData(clientId, data);
    });

    // Xử lý khi client ngắt kết nối
    socket.on('close', () => {
      this.handleClientDisconnect(clientId);
    });

    // Xử lý lỗi socket
    socket.on('error', (error) => {
      console.error(`❌ Socket error từ client #${clientId}:`, error);
      this.handleClientDisconnect(clientId);
    });

    // Emit event client connected
    this.emit('clientConnected', clientInfo);
  }

  /**
   * Xử lý dữ liệu từ client
   */
  handleClientData(clientId, data) {
    try {
      const rawMessage = data.toString().trim();
      console.log(`📥 Nhận từ client #${clientId}: ${rawMessage}`);

      let message;
      let isJsonInput = false;
      
      // Thử parse JSON, nếu không thành công thì coi như plain text
      try {
        message = JSON.parse(rawMessage);
        isJsonInput = true;
        console.log(`🔧 Detected JSON input from client #${clientId}`);
      } catch (e) {
        message = { type: 'text', content: rawMessage };
        isJsonInput = false;
        console.log(`🔧 Detected TEXT input from client #${clientId}`);
      }

      // Xử lý các loại message khác nhau
      switch (message.type) {
        case 'broadcast':
          this.handleBroadcastMessage(clientId, message, isJsonInput);
          break;
        case 'private':
          this.handlePrivateMessage(clientId, message, isJsonInput);
          break;
        case 'ping':
          this.handlePingMessage(clientId, isJsonInput);
          break;
        case 'list_clients':
          this.sendClientsList(clientId, isJsonInput);
          break;
        default:
          // Echo message trở lại client với format tương ứng
          this.sendEchoResponse(clientId, message, rawMessage, isJsonInput);
      }

      // Emit event data received
      this.emit('dataReceived', { clientId, data: message, isJsonInput });

    } catch (error) {
      console.error(`❌ Lỗi xử lý data từ client #${clientId}:`, error);
      this.sendToClient(clientId, {
        type: 'error',
        message: 'Lỗi xử lý dữ liệu',
        error: error.message
      });
    }
  }

  /**
   * Gửi echo response theo format tương ứng
   */
  sendEchoResponse(clientId, message, rawMessage, isJsonInput) {
    if (isJsonInput) {
      // JSON input -> JSON output
      this.sendToClient(clientId, {
        type: 'echo',
        originalMessage: message,
        timestamp: new Date().toISOString(),
        clientId: clientId,
        format: 'json'
      });
    } else {
      // Text input -> Text output
      const textResponse = `Echo: ${message.content} (from client #${clientId} at ${new Date().toLocaleTimeString()})`;
      this.sendTextToClient(clientId, textResponse);
    }
  }

  /**
   * Xử lý ping message
   */
  handlePingMessage(clientId, isJsonInput) {
    if (isJsonInput) {
      this.sendToClient(clientId, { 
        type: 'pong', 
        timestamp: new Date().toISOString(),
        format: 'json'
      });
    } else {
      this.sendTextToClient(clientId, `Pong! Server time: ${new Date().toLocaleTimeString()}`);
    }
  }
  handleBroadcastMessage(senderId, message, isJsonInput) {
    if (isJsonInput) {
      const broadcastData = {
        type: 'broadcast',
        from: senderId,
        content: message.content,
        timestamp: new Date().toISOString(),
        format: 'json'
      };
      this.broadcastToOthers(senderId, broadcastData);
    } else {
      const textBroadcast = `[Broadcast from #${senderId}] ${message.content}`;
      this.broadcastTextToOthers(senderId, textBroadcast);
    }
    
    console.log(`📢 Client #${senderId} broadcast (${isJsonInput ? 'JSON' : 'TEXT'}): ${message.content}`);
  }

  /**
   * Xử lý private message
   */
  handlePrivateMessage(senderId, message, isJsonInput) {
    const targetId = message.targetId;
    
    if (!this.clients.has(targetId)) {
      if (isJsonInput) {
        this.sendToClient(senderId, {
          type: 'error',
          message: `Client #${targetId} không tồn tại`,
          format: 'json'
        });
      } else {
        this.sendTextToClient(senderId, `Lỗi: Client #${targetId} không tồn tại`);
      }
      return;
    }

    if (isJsonInput) {
      const privateData = {
        type: 'private_message',
        from: senderId,
        content: message.content,
        timestamp: new Date().toISOString(),
        format: 'json'
      };
      this.sendToClient(targetId, privateData);
      
      this.sendToClient(senderId, {
        type: 'message_sent',
        to: targetId,
        content: message.content,
        timestamp: new Date().toISOString(),
        format: 'json'
      });
    } else {
      const textPrivate = `[Private from #${senderId}] ${message.content}`;
      this.sendTextToClient(targetId, textPrivate);
      
      const confirmText = `Private message đã gửi đến #${targetId}: ${message.content}`;
      this.sendTextToClient(senderId, confirmText);
    }

    console.log(`💬 Private message (${isJsonInput ? 'JSON' : 'TEXT'}) từ #${senderId} đến #${targetId}: ${message.content}`);
  }

  /**
   * Gửi danh sách clients
   */
  sendClientsList(requesterId, isJsonInput = true) {
    const clientsList = Array.from(this.clients.values()).map(client => ({
      id: client.id,
      address: client.address,
      port: client.port,
      connectedAt: client.connectedAt,
      isYou: client.id === requesterId
    }));

    if (isJsonInput) {
      this.sendToClient(requesterId, {
        type: 'clients_list',
        clients: clientsList,
        total: clientsList.length,
        format: 'json'
      });
    } else {
      let textList = `Danh sách clients (${clientsList.length} clients):\n`;
      clientsList.forEach(client => {
        const marker = client.isYou ? ' (Bạn)' : '';
        textList += `- Client #${client.id}${marker} - ${client.address}:${client.port}\n`;
      });
      this.sendTextToClient(requesterId, textList.trim());
    }
  }

  /**
   * Xử lý khi client ngắt kết nối
   */
  handleClientDisconnect(clientId) {
    const client = this.clients.get(clientId);
    if (!client) return;

    // Xóa client khỏi danh sách
    this.clients.delete(clientId);
    
    console.log(`❌ Client #${clientId} đã ngắt kết nối (${client.address}:${client.port})`);
    console.log(`📊 Tổng số clients còn lại: ${this.clients.size}`);

    // Thông báo cho các clients khác
    this.broadcast({
      type: 'client_left',
      clientId: clientId,
      message: `Client #${clientId} đã rời khỏi`,
      totalClients: this.clients.size
    });

    // Emit event client disconnected
    this.emit('clientDisconnected', { clientId, client });
  }

  /**
   * Gửi text thuần đến một client cụ thể
   */
  sendTextToClient(clientId, text) {
    const client = this.clients.get(clientId);
    if (!client) {
      console.warn(`⚠️ Không tìm thấy client #${clientId}`);
      return false;
    }

    try {
      // Gửi text thuần, không JSON
      const message = text + '\n';
      client.socket.write(message);
      console.log(`📤 Sent TEXT to #${clientId}: ${text}`);
      return true;
    } catch (error) {
      console.error(`❌ Lỗi gửi text đến client #${clientId}:`, error);
      return false;
    }
  }

  /**
   * Gửi dữ liệu đến một client cụ thể (JSON format)
   */
  sendToClient(clientId, data) {
    const client = this.clients.get(clientId);
    if (!client) {
      console.warn(`⚠️ Không tìm thấy client #${clientId}`);
      return false;
    }

    try {
      const message = JSON.stringify(data) + '\n';
      client.socket.write(message);
      console.log(`📤 Sent JSON to #${clientId}:`, data.type || 'unknown');
      return true;
    } catch (error) {
      console.error(`❌ Lỗi gửi data đến client #${clientId}:`, error);
      return false;
    }
  }

  /**
   * Broadcast text thuần đến tất cả clients trừ sender
   */
  broadcastTextToOthers(senderId, text) {
    let sentCount = 0;
    this.clients.forEach((client, clientId) => {
      if (clientId !== senderId && this.sendTextToClient(clientId, text)) {
        sentCount++;
      }
    });
    console.log(`📡 Broadcast TEXT đến ${sentCount} clients (excluding #${senderId})`);
    return sentCount;
  }

  /**
   * Broadcast dữ liệu đến tất cả clients
   */
  broadcast(data) {
    let sentCount = 0;
    this.clients.forEach((client, clientId) => {
      if (this.sendToClient(clientId, data)) {
        sentCount++;
      }
    });
    console.log(`📡 Broadcast JSON đến ${sentCount}/${this.clients.size} clients`);
    return sentCount;
  }

  /**
   * Broadcast dữ liệu đến tất cả clients trừ sender
   */
  broadcastToOthers(senderId, data) {
    let sentCount = 0;
    this.clients.forEach((client, clientId) => {
      if (clientId !== senderId && this.sendToClient(clientId, data)) {
        sentCount++;
      }
    });
    console.log(`📡 Broadcast JSON đến ${sentCount} clients (excluding #${senderId})`);
    return sentCount;
  }

  /**
   * Lấy thông tin về một client
   */
  getClientInfo(clientId) {
    return this.clients.get(clientId) || null;
  }

  /**
   * Lấy danh sách tất cả clients
   */
  getAllClients() {
    return Array.from(this.clients.values());
  }

  /**
   * Kick một client
   */
  kickClient(clientId, reason = 'Kicked by server') {
    const client = this.clients.get(clientId);
    if (!client) return false;

    this.sendToClient(clientId, {
      type: 'kicked',
      reason: reason,
      timestamp: new Date().toISOString()
    });

    setTimeout(() => {
      client.socket.destroy();
    }, 1000); // Delay để client nhận được message trước khi ngắt kết nối

    return true;
  }

  /**
   * Dừng server
   */
  stop() {
    if (!this.server) return;

    // Thông báo cho tất cả clients
    this.broadcast({
      type: 'server_shutdown',
      message: 'Server đang tắt...',
      timestamp: new Date().toISOString()
    });

    // Đóng tất cả kết nối
    this.clients.forEach((client) => {
      client.socket.destroy();
    });

    // Đóng server
    this.server.close(() => {
      console.log('🛑 TCP Server đã dừng');
      this.emit('stopped');
    });

    this.clients.clear();
  }

  /**
   * Lấy thống kê server
   */
  getStats() {
    return {
      totalClients: this.clients.size,
      serverUptime: process.uptime(),
      host: this.host,
      port: this.port,
      clients: this.getAllClients().map(client => ({
        id: client.id,
        address: client.address,
        port: client.port,
        connectedAt: client.connectedAt
      }))
    };
  }
}

export default TCPServer;