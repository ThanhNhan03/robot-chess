# 🌐 TCP Server Web Interface

Giao diện web đẹp mắt để tương tác với TCP Server thông qua trình duyệt.

## ✨ Tính năng

- ✅ **Real-time messaging** - Gửi/nhận messages qua WebSocket
- ✅ **Beautiful UI** - Giao diện chat hiện đại với animations
- ✅ **Multiple message types** - Text, JSON, Preset messages
- ✅ **Auto TCP discovery** - Tự động tìm TCP Server
- ✅ **Live connection status** - Hiển thị trạng thái kết nối real-time
- ✅ **Message history** - Lưu trữ và hiển thị lịch sử chat
- ✅ **JSON syntax support** - Editor JSON với examples
- ✅ **Responsive design** - Hoạt động tốt trên mobile

## 🚀 Cách sử dụng

### Option 1: Tự động (Khuyến nghị)
```bash
cd tcp-server
./start-web.sh
```

### Option 2: Manual

1. **Start TCP Server** (Terminal 1):
```bash
cd tcp-server
npm start
```

2. **Start Web Interface** (Terminal 2):
```bash
cd tcp-server/web-interface
npm install
npm run web
```

3. **Mở trình duyệt**:
   - Vào: http://localhost:3000
   - Click "Connect to TCP Server"
   - Bắt đầu gửi messages!

## 🎮 Cách sử dụng Interface

### 📱 **Connection Status**
- **Green dot**: Connected to TCP Server
- **Red dot**: Disconnected
- Port hiển thị bên phải

### 💬 **Gửi Messages**

#### **1. Text Tab**
- Nhập text thông thường
- Press Enter để gửi

#### **2. JSON Tab**
- Nhập JSON messages
- Click examples để auto-fill
- Ctrl+Enter để gửi

#### **3. Preset Tab**
- Click buttons để gửi messages có sẵn:
  - 🏓 Ping Server
  - 👥 List Clients  
  - 📢 Broadcast Hello
  - 💬 Test Message
  - 🧪 Custom Data

### 📨 **Message Display**
- **Blue (right)**: Messages bạn gửi
- **Gray (left)**: Messages nhận từ server
- **Green (center)**: System messages
- **Red (center)**: Error messages

## 🎯 **Message Types Support**

### Text Messages
```
Hello server!
```

### JSON Messages
```json
{
  "type": "broadcast",
  "content": "Hello everyone! 👋"
}
```

```json
{
  "type": "private",
  "targetId": 2,
  "content": "Secret message"
}
```

```json
{
  "type": "custom_data",
  "payload": {
    "user": "WebUser",
    "action": "login",
    "data": {"level": 5, "score": 1250}
  }
}
```

## 🔧 **Architecture**

```
Browser (WebSocket) ↔ Express Server ↔ TCP Server
```

- **Frontend**: HTML/CSS/JS với Socket.IO client
- **Backend**: Express + Socket.IO server  
- **Bridge**: TCP client manager kết nối tới TCP Server
- **Real-time**: WebSocket cho instant messaging

## 📊 **Features Detail**

### **Auto TCP Discovery**
- Tự động tìm TCP Server trên ports 8080-8084
- Hiển thị status và port number
- Retry connection khi mất kết nối

### **Message Management**
- Parse JSON tự động
- Format messages đẹp
- Scroll to bottom auto
- Message counter
- Clear messages function

### **Error Handling**
- Invalid JSON detection
- Connection error display
- Graceful fallbacks
- User-friendly error messages

### **Responsive Design**
- Mobile-friendly layout
- Touch-optimized buttons
- Collapsible panels
- Fluid typography

## 🎨 **UI Components**

- **Header**: Status và connection info
- **Messages Panel**: Chat-like message display
- **Send Panel**: Tabbed input interface
- **Controls**: Connection management buttons
- **Footer**: Stats và timestamps

## ⚙️ **Configuration**

### Ports
- **Web Interface**: 3000 (configurable via PORT env)
- **TCP Server**: Auto-discovered (8080-8084)

### Environment Variables
```bash
PORT=3000          # Web server port
TCP_HOST=localhost # TCP server host
```

## 🔍 **Troubleshooting**

### Web interface không load
```bash
# Kiểm tra port 3000
lsof -i :3000
# Hoặc thử port khác
PORT=3001 npm run web
```

### Không kết nối được TCP Server
1. Kiểm tra TCP Server đang chạy:
```bash
lsof -i :8080
```

2. Start TCP Server:
```bash
cd tcp-server
npm start
```

3. Refresh web page

### Messages không hiển thị
- Kiểm tra browser console (F12)
- Refresh page
- Check network tab for WebSocket connection

## 📸 **Screenshots**

### Main Interface
- Modern chat-like design
- Real-time status indicators
- Tabbed message input

### Message Types
- Formatted JSON display
- Color-coded message types
- Timestamp và metadata

## 🚀 **Advanced Usage**

### Custom Messages
```javascript
// Trong browser console
tcpWebInterface.sendMessage({
  type: "custom",
  data: {your: "data"},
  timestamp: Date.now()
});
```

### API Endpoint
```bash
# Get server status
curl http://localhost:3000/status
```

## 📝 **Development**

### Watch mode
```bash
npm run dev  # Auto-restart on changes
```

### File structure
```
web-interface/
├── package.json       # Dependencies
├── web-server.js      # Express + Socket.IO server
└── public/
    ├── index.html     # Main UI
    ├── style.css      # Styling
    └── app.js         # Frontend logic
```

Giao diện này cung cấp trải nghiệm hoàn chỉnh để tương tác với TCP Server! 🎉