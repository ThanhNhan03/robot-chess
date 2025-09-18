import net from 'net';

// Test client để demo text vs JSON input/output
class FormatTestClient {
  constructor(host = '100.107.161.16', port = 8080) {
    this.host = host;
    this.port = port;
    this.socket = null;
    this.clientId = null;
  }

  async connect() {
    return new Promise((resolve, reject) => {
      console.log(`🔌 Connecting to ${this.host}:${this.port}...`);
      
      this.socket = net.createConnection(this.port, this.host);

      this.socket.on('connect', () => {
        console.log('✅ Connected to server!');
        this.setupDataHandler();
        resolve(true);
      });

      this.socket.on('error', (error) => {
        console.error('❌ Connection error:', error.message);
        reject(error);
      });
    });
  }

  setupDataHandler() {
    this.socket.on('data', (data) => {
      const messages = data.toString().trim().split('\n');
      
      messages.forEach(messageStr => {
        if (!messageStr) return;
        
        try {
          // Thử parse JSON
          const parsed = JSON.parse(messageStr);
          console.log('📥 Received JSON:', parsed);
          
          if (parsed.type === 'welcome') {
            this.clientId = parsed.clientId;
          }
        } catch (e) {
          // Không phải JSON, là text thuần
          console.log('📥 Received TEXT:', messageStr);
        }
      });
    });
  }

  sendText(text) {
    console.log(`📤 Sending TEXT: "${text}"`);
    this.socket.write(text + '\n');
  }

  sendJSON(obj) {
    const jsonStr = JSON.stringify(obj);
    console.log(`📤 Sending JSON:`, obj);
    this.socket.write(jsonStr + '\n');
  }

  async runTests() {
    try {
      await this.connect();
      
      console.log('\n🧪 Starting Format Tests...\n');
      
      // Test 1: Text input
      console.log('--- Test 1: TEXT Input ---');
      await this.delay(1000);
      this.sendText('Hello server!');
      
      await this.delay(2000);
      
      // Test 2: JSON input
      console.log('\n--- Test 2: JSON Input ---');
      this.sendJSON({ type: 'text', content: 'Hello via JSON!' });
      
      await this.delay(2000);
      
      // Test 3: Text ping
      console.log('\n--- Test 3: TEXT Ping ---');
      this.sendText('ping');
      
      await this.delay(2000);
      
      // Test 4: JSON ping  
      console.log('\n--- Test 4: JSON Ping ---');
      this.sendJSON({ type: 'ping' });
      
      await this.delay(2000);
      
      // Test 5: Text broadcast
      console.log('\n--- Test 5: TEXT Broadcast ---');
      this.sendText('broadcast Hello everyone via text!');
      
      await this.delay(2000);
      
      // Test 6: JSON broadcast
      console.log('\n--- Test 6: JSON Broadcast ---');
      this.sendJSON({ type: 'broadcast', content: 'Hello everyone via JSON!' });
      
      await this.delay(2000);
      
      // Test 7: Text list clients
      console.log('\n--- Test 7: TEXT List Clients ---');
      this.sendText('list');
      
      await this.delay(2000);
      
      // Test 8: JSON list clients
      console.log('\n--- Test 8: JSON List Clients ---');
      this.sendJSON({ type: 'list_clients' });
      
      await this.delay(3000);
      
      console.log('\n✅ All tests completed!');
      this.socket.end();
      
    } catch (error) {
      console.error('❌ Test failed:', error);
      process.exit(1);
    }
  }

  delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}

// Run tests
const testClient = new FormatTestClient();

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\n👋 Shutting down...');
  testClient.socket?.end();
  process.exit(0);
});

console.log('🧪 Format Test Client');
console.log('Testing TEXT vs JSON input/output');
console.log('=====================================\n');

testClient.runTests();