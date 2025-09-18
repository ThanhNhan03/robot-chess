import net from 'net';
import readline from 'readline';

class TCPClient {
  constructor(host = '100.107.161.16', port = 8080) {
    this.host = host;
    this.port = port;
    this.socket = null;
    this.clientId = null;
    this.connected = false;
    
    // Tạo readline interface để nhận input từ console
    this.rl = readline.createInterface({
      input: process.stdin,
      output: process.stdout
    });
  }

  connect() {
    console.log(`🔌 Đang kết nối đến ${this.host}:${this.port}...`);
    
    this.socket = net.createConnection(this.port, this.host);

    this.socket.on('connect', () => {
      this.connected = true;
      console.log('✅ Đã kết nối đến server!');
      this.showHelp();
      this.startInputLoop();
    });

    this.socket.on('data', (data) => {
      this.handleServerData(data);
    });

    this.socket.on('close', () => {
      this.connected = false;
      console.log('❌ Kết nối đã bị đóng');
      this.rl.close();
      process.exit(0);
    });

    this.socket.on('error', (error) => {
      console.error('❌ Lỗi kết nối:', error);
      this.rl.close();
      process.exit(1);
    });
  }

  handleServerData(data) {
    const messages = data.toString().trim().split('\n');
    
    messages.forEach(messageStr => {
      if (!messageStr) return;
      
      try {
        const message = JSON.parse(messageStr);
        this.processServerMessage(message);
      } catch (error) {
        console.log('📨 Raw message:', messageStr);
      }
    });
  }

  processServerMessage(message) {
    switch (message.type) {
      case 'welcome':
        this.clientId = message.clientId;
        console.log(`🎉 ${message.message}`);
        break;
        
      case 'client_joined':
        if (message.clientId !== this.clientId) {
          console.log(`👋 ${message.message} (Total: ${message.totalClients})`);
        }
        break;
        
      case 'client_left':
        console.log(`👋 ${message.message} (Total: ${message.totalClients})`);
        break;
        
      case 'broadcast':
        console.log(`📢 [Client #${message.from}] ${message.content}`);
        break;
        
      case 'private_message':
        console.log(`💬 [Private từ #${message.from}] ${message.content}`);
        break;
        
      case 'message_sent':
        console.log(`✅ Đã gửi private message đến #${message.to}: ${message.content}`);
        break;
        
      case 'echo':
        console.log(`🔄 Echo: ${JSON.stringify(message.originalMessage)}`);
        break;
        
      case 'pong':
        console.log(`🏓 Pong từ server`);
        break;
        
      case 'clients_list':
        console.log('\n👥 Danh sách clients:');
        message.clients.forEach(client => {
          const marker = client.isYou ? ' (You)' : '';
          console.log(`   • Client #${client.id}${marker} - ${client.address}:${client.port} (Connected: ${new Date(client.connectedAt).toLocaleTimeString()})`);
        });
        console.log(`📊 Total: ${message.total} clients\n`);
        break;
        
      case 'kicked':
        console.log(`🚫 Bạn đã bị kick: ${message.reason}`);
        break;
        
      case 'server_shutdown':
        console.log(`🛑 ${message.message}`);
        break;
        
      case 'error':
        console.log(`❌ Error: ${message.message}`);
        break;
        
      default:
        console.log('📨 Unhandled message:', message);
    }
  }

  startInputLoop() {
    this.rl.prompt();
    this.rl.on('line', (input) => {
      this.handleUserInput(input.trim());
      this.rl.prompt();
    });
  }

  handleUserInput(input) {
    if (!input) return;

    // Xử lý commands
    if (input.startsWith('/')) {
      this.handleCommand(input);
      return;
    }

    // Gửi text thông thường
    this.sendMessage({
      type: 'text',
      content: input
    });
  }

  handleCommand(command) {
    const [cmd, ...args] = command.slice(1).split(' ');
    
    switch (cmd.toLowerCase()) {
      case 'help':
        this.showHelp();
        break;
        
      case 'broadcast':
      case 'bc':
        if (args.length === 0) {
          console.log('❌ Usage: /broadcast <message>');
          return;
        }
        this.sendMessage({
          type: 'broadcast',
          content: args.join(' ')
        });
        break;
        
      case 'private':
      case 'pm':
        if (args.length < 2) {
          console.log('❌ Usage: /private <clientId> <message>');
          return;
        }
        const targetId = parseInt(args[0]);
        if (isNaN(targetId)) {
          console.log('❌ Client ID phải là số');
          return;
        }
        this.sendMessage({
          type: 'private',
          targetId: targetId,
          content: args.slice(1).join(' ')
        });
        break;
        
      case 'ping':
        this.sendMessage({ type: 'ping' });
        break;
        
      case 'list':
      case 'clients':
        this.sendMessage({ type: 'list_clients' });
        break;
        
      case 'quit':
      case 'exit':
        console.log('👋 Đang ngắt kết nối...');
        this.socket.end();
        break;
        
      default:
        console.log(`❌ Unknown command: ${cmd}`);
        this.showHelp();
    }
  }

  sendMessage(data) {
    if (!this.connected) {
      console.log('❌ Chưa kết nối đến server');
      return;
    }

    try {
      const message = JSON.stringify(data) + '\n';
      this.socket.write(message);
    } catch (error) {
      console.error('❌ Lỗi gửi message:', error);
    }
  }

  showHelp() {
    console.log('\n📋 Available commands:');
    console.log('   /help                     - Hiển thị help');
    console.log('   /broadcast <message>      - Broadcast message đến tất cả clients');
    console.log('   /private <id> <message>   - Gửi private message đến client ID');
    console.log('   /ping                     - Ping server');
    console.log('   /list                     - Liệt kê tất cả clients');
    console.log('   /quit                     - Thoát');
    console.log('   <any text>                - Gửi text message đến server');
    console.log('');
  }
}

// Lấy host và port từ command line arguments
const args = process.argv.slice(2);
const host = args[0] || '100.107.161.16';

// Tạo và kết nối client
const client = new TCPClient(host, port);

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\n👋 Đang thoát...');
  client.socket?.end();
  process.exit(0);
});

// Kết nối
client.connect();