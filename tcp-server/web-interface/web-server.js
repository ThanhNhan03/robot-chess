import express from 'express';
import { createServer } from 'http';
import { Server } from 'socket.io';
import net from 'net';
import path from 'path';
import { fileURLToPath } from 'url';
import cors from 'cors';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Express app
const app = express();
const httpServer = createServer(app);
const io = new Server(httpServer, {
  cors: {
    origin: "*",
    methods: ["GET", "POST"]
  }
});

// Middleware
app.use(cors());
app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

// TCP Client pool để kết nối với TCP Server
class TCPClientManager {
  constructor() {
    this.tcpClients = new Map(); // socketId -> tcpClient
    this.tcpServerPort = null;
    this.tcpServerHost = '100.107.161.16';
  }

  async findTCPServer() {
    const ports = [8080, 8081, 8082, 8083, 8084];
    
    for (const port of ports) {
      try {
        await this.testPort(port);
        this.tcpServerPort = port;
        console.log(`✅ Found TCP Server on port ${port}`);
        return true;
      } catch (error) {
        console.log(`❌ Port ${port} not available`);
      }
    }
    
    console.log('⚠️ No TCP Server found. Please start TCP Server first.');
    return false;
  }

  testPort(port) {
    return new Promise((resolve, reject) => {
      const client = net.createConnection(port, this.tcpServerHost);
      
      const timeout = setTimeout(() => {
        client.destroy();
        reject(new Error(`Timeout`));
      }, 1000);
      
      client.on('connect', () => {
        clearTimeout(timeout);
        client.end();
        resolve(true);
      });
      
      client.on('error', (error) => {
        clearTimeout(timeout);
        reject(error);
      });
    });
  }

  async connectClient(socketId) {
    if (!this.tcpServerPort) {
      throw new Error('TCP Server not found');
    }

    const tcpClient = net.createConnection(this.tcpServerPort, this.tcpServerHost);
    this.tcpClients.set(socketId, tcpClient);

    tcpClient.on('connect', () => {
      console.log(`🔌 Web client ${socketId} connected to TCP Server`);
      io.to(socketId).emit('tcp-connected', { 
        port: this.tcpServerPort,
        message: 'Connected to TCP Server' 
      });
    });

    tcpClient.on('data', (data) => {
      const messages = data.toString().trim().split('\n');
      messages.forEach(msg => {
        if (msg) {
          try {
            const parsed = JSON.parse(msg);
            io.to(socketId).emit('tcp-message', parsed);
          } catch (e) {
            io.to(socketId).emit('tcp-message', { type: 'raw', content: msg });
          }
        }
      });
    });

    tcpClient.on('close', () => {
      console.log(`🔌 TCP connection closed for web client ${socketId}`);
      io.to(socketId).emit('tcp-disconnected', { message: 'TCP connection closed' });
      this.tcpClients.delete(socketId);
    });

    tcpClient.on('error', (error) => {
      console.error(`❌ TCP error for client ${socketId}:`, error);
      io.to(socketId).emit('tcp-error', { error: error.message });
      this.tcpClients.delete(socketId);
    });

    return tcpClient;
  }

  sendMessage(socketId, message) {
    const tcpClient = this.tcpClients.get(socketId);
    if (!tcpClient) {
      throw new Error('No TCP connection found');
    }

    const jsonMessage = JSON.stringify(message) + '\n';
    tcpClient.write(jsonMessage);
  }

  disconnectClient(socketId) {
    const tcpClient = this.tcpClients.get(socketId);
    if (tcpClient) {
      tcpClient.end();
      this.tcpClients.delete(socketId);
    }
  }
}

const tcpManager = new TCPClientManager();

// Socket.IO connection handling
io.on('connection', async (socket) => {
  console.log(`🌐 Web client connected: ${socket.id}`);

  // Gửi status về TCP Server
  const tcpServerFound = await tcpManager.findTCPServer();
  socket.emit('server-status', { 
    tcpServerFound,
    port: tcpManager.tcpServerPort 
  });

  // Connect to TCP Server
  socket.on('connect-tcp', async () => {
    try {
      await tcpManager.connectClient(socket.id);
    } catch (error) {
      socket.emit('tcp-error', { error: error.message });
    }
  });

  // Send message to TCP Server
  socket.on('send-message', (data) => {
    try {
      tcpManager.sendMessage(socket.id, data.message);
      socket.emit('message-sent', { message: data.message });
    } catch (error) {
      socket.emit('tcp-error', { error: error.message });
    }
  });

  // Disconnect
  socket.on('disconnect', () => {
    console.log(`🌐 Web client disconnected: ${socket.id}`);
    tcpManager.disconnectClient(socket.id);
  });
});

// Routes
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

app.get('/status', async (req, res) => {
  const tcpServerFound = await tcpManager.findTCPServer();
  res.json({
    webServer: true,
    tcpServer: tcpServerFound,
    tcpPort: tcpManager.tcpServerPort,
    connections: tcpManager.tcpClients.size
  });
});

// Start server
const PORT = process.env.PORT || 3000;
httpServer.listen(PORT, () => {
  console.log('🌐 Web Interface started');
  console.log(`📡 Server: http://localhost:${PORT}`);
  console.log('🔌 Looking for TCP Server...');
  
  // Tìm TCP Server
  tcpManager.findTCPServer().then(found => {
    if (found) {
      console.log(`✅ Ready! TCP Server found on port ${tcpManager.tcpServerPort}`);
    } else {
      console.log('⚠️ TCP Server not found. Start TCP Server first with: npm start');
    }
  });
});

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\n🛑 Shutting down web server...');
  httpServer.close();
  process.exit(0);
});