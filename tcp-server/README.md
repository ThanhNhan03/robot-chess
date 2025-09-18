# TCP Server với Node.js

TCP Server mạnh mẽ với khả năng xử lý nhiều client đồng thời, quản lý ID client và xử lý các loại message khác nhau.

## 🚀 Tính năng

- ✅ **Multi-client support**: Xử lý nhiều client kết nối đồng thời
- ✅ **Auto ID assignment**: Tự động gán ID duy nhất cho từng client
- ✅ **Client management**: Quản lý danh sách clients và thông tin kết nối
- ✅ **Message types**: Hỗ trợ nhiều loại message (broadcast, private, ping/pong)
- ✅ **Event-driven**: Sử dụng EventEmitter cho xử lý bất đồng bộ
- ✅ **Graceful shutdown**: Xử lý tắt server an toàn
- ✅ **Error handling**: Xử lý lỗi toàn diện
- ✅ **Statistics**: Thống kê server và client real-time

## 📁 Cấu trúc file

```
tcp-server/
├── package.json        # Package configuration
├── server.js          # Main server runner
├── tcp-server.js      # TCP Server class
├── test-client.js     # Test client for testing
└── README.md          # Documentation
```

## 🛠️ Cài đặt và chạy

### 1. Chạy Server

```bash
cd tcp-server
npm start
```

Server sẽ chạy trên `localhost:8080` theo mặc định.

### 2. Chạy Test Client

Mở terminal mới và chạy:

```bash
cd tcp-server
npm test
```

Hoặc với custom host/port:

```bash
node test-client.js localhost 8080
```

### 3. Development mode (auto-restart)

```bash
npm run dev
```

## 📡 API và Message Types

### Client → Server Messages

#### 1. Text Message
```json
{
  "type": "text",
  "content": "Hello server!"
}
```

#### 2. Broadcast Message
```json
{
  "type": "broadcast",
  "content": "Hello everyone!"
}
```

#### 3. Private Message
```json
{
  "type": "private",
  "targetId": 2,
  "content": "Hello client #2"
}
```

#### 4. Ping
```json
{
  "type": "ping"
}
```

#### 5. List Clients
```json
{
  "type": "list_clients"
}
```

### Server → Client Messages

#### 1. Welcome Message
```json
{
  "type": "welcome",
  "clientId": 1,
  "message": "Chào mừng! Bạn được gán ID: 1",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

#### 2. Client Joined/Left
```json
{
  "type": "client_joined",
  "clientId": 2,
  "message": "Client #2 đã tham gia",
  "totalClients": 2
}
```

#### 3. Broadcast Message
```json
{
  "type": "broadcast",
  "from": 1,
  "content": "Hello everyone!",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

#### 4. Private Message
```json
{
  "type": "private_message",
  "from": 1,
  "content": "Hello!",
  "timestamp": "2024-01-01T12:00:00.000Z"
}
```

#### 5. Clients List
```json
{
  "type": "clients_list",
  "clients": [
    {
      "id": 1,
      "address": "127.0.0.1",
      "port": 12345,
      "connectedAt": "2024-01-01T12:00:00.000Z",
      "isYou": true
    }
  ],
  "total": 1
}
```

## 🎮 Test Client Commands

Khi sử dụng test client, bạn có thể sử dụng các commands sau:

- `/help` - Hiển thị help
- `/broadcast <message>` - Broadcast message đến tất cả clients
- `/private <id> <message>` - Gửi private message đến client ID
- `/ping` - Ping server
- `/list` - Liệt kê tất cả clients
- `/quit` - Thoát
- `<any text>` - Gửi text message đến server

## 💻 Sử dụng TCP Server Class

```javascript
import TCPServer from './tcp-server.js';

// Tạo server instance
const server = new TCPServer(8080, 'localhost');

// Lắng nghe events
server.on('started', (info) => {
  console.log(`Server started on ${info.host}:${info.port}`);
});

server.on('clientConnected', (clientInfo) => {
  console.log(`Client #${clientInfo.id} connected`);
});

server.on('dataReceived', (info) => {
  console.log(`Data from #${info.clientId}:`, info.data);
});

// Bắt đầu server
server.start();

// Gửi message đến client cụ thể
server.sendToClient(1, { type: 'custom', data: 'Hello!' });

// Broadcast đến tất cả clients
server.broadcast({ type: 'announcement', message: 'Server notification' });

// Kick client
server.kickClient(1, 'Violation of rules');

// Lấy thống kê
const stats = server.getStats();
console.log('Server stats:', stats);

// Dừng server
server.stop();
```

## 🔧 Events

TCP Server emit các events sau:

- `started` - Server đã bắt đầu
- `clientConnected` - Client mới kết nối
- `clientDisconnected` - Client ngắt kết nối
- `dataReceived` - Nhận được data từ client
- `error` - Có lỗi xảy ra
- `stopped` - Server đã dừng

## 📊 Methods

### Server Control
- `start()` - Bắt đầu server
- `stop()` - Dừng server
- `getStats()` - Lấy thống kê server

### Client Management
- `sendToClient(clientId, data)` - Gửi data đến client cụ thể
- `broadcast(data)` - Broadcast đến tất cả clients
- `broadcastToOthers(senderId, data)` - Broadcast trừ sender
- `kickClient(clientId, reason)` - Kick client
- `getClientInfo(clientId)` - Lấy thông tin client
- `getAllClients()` - Lấy danh sách tất cả clients

## 🧪 Testing

### Test đa client

1. Chạy server:
```bash
npm start
```

2. Mở nhiều terminal và chạy client:
```bash
# Terminal 1
npm test

# Terminal 2  
npm test

# Terminal 3
npm test
```

3. Test các tính năng:
- Gửi broadcast messages
- Gửi private messages
- List clients
- Ping server

### Test programmatically

```javascript
// Tạo nhiều clients để test
import net from 'net';

for (let i = 0; i < 5; i++) {
  const client = net.createConnection(8080, 'localhost');
  client.on('connect', () => {
    console.log(`Client ${i} connected`);
    client.write(JSON.stringify({ type: 'text', content: `Hello from client ${i}` }) + '\n');
  });
}
```

## ⚙️ Configuration

Có thể thay đổi cấu hình server:

```javascript
// Custom port và host
const server = new TCPServer(3000, '0.0.0.0');

// Hoặc trong server.js
const PORT = process.env.PORT || 8080;
const HOST = process.env.HOST || 'localhost';
```

## 🔒 Security Notes

- Server này được thiết kế cho development/testing
- Để production, cần thêm authentication và authorization
- Validate input data từ clients
- Rate limiting cho tránh spam
- Encryption cho sensitive data

## 🐛 Troubleshooting

### Port đã được sử dụng
```bash
# Kiểm tra port nào đang sử dụng 8080
lsof -i :8080

# Kill process
kill -9 <PID>
```

### Client không kết nối được
- Kiểm tra server đã chạy chưa
- Kiểm tra port và host đúng chưa
- Kiểm tra firewall

### Memory leak
- Server tự động cleanup khi client disconnect
- Sử dụng `server.stop()` để cleanup hoàn toàn

## 📝 License

MIT License - Sử dụng tự do cho mọi mục đích.