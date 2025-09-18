import net from 'net';

// Function để thử kết nối với nhiều ports
async function connectToServer() {
  const ports = [8080, 8081, 8082, 8083, 8084]; // Thử nhiều ports
  
  for (const port of ports) {
    try {
      console.log(`🔌 Trying to connect to 100.107.161.16:${port}...`);
      const client = await tryConnect(port);
      console.log(`✅ Connected to server on port ${port}`);
      return client;
    } catch (error) {
      console.log(`❌ Port ${port} not available`);
    }
  }
  
  throw new Error('No server found on any port. Please start server first with: npm start');
}

function tryConnect(port) {
  return new Promise((resolve, reject) => {
    const client = net.createConnection(port, '100.107.161.16');
    
    const timeout = setTimeout(() => {
      client.destroy();
      reject(new Error(`Timeout connecting to port ${port}`));
    }, 2000);
    
    client.on('connect', () => {
      clearTimeout(timeout);
      resolve(client);
    });
    
    client.on('error', (error) => {
      clearTimeout(timeout);
      reject(error);
    });
  });
}

// Main function
async function runJsonTest() {
  try {
    console.log('🧪 JSON Test Client Starting...');
    console.log('📡 Looking for TCP Server...\n');
    
    const client = await connectToServer();

    
    // Setup event handlers
    client.on('data', (data) => {
      const messages = data.toString().trim().split('\n');
      messages.forEach(msg => {
        if (msg) {
          try {
            const parsed = JSON.parse(msg);
            console.log('📥 Received JSON:', parsed);
          } catch (e) {
            console.log('📥 Received raw:', msg);
          }
        }
      });
    });

    client.on('close', () => {
      console.log('❌ Connection closed');
      process.exit(0);
    });

    client.on('error', (error) => {
      console.error('❌ Connection error:', error);
      process.exit(1);
    });

    // Gửi các JSON messages
    const messages = [
      { type: 'text', content: 'Hello from JSON client!' },
      { type: 'broadcast', content: 'Broadcasting JSON message 📡' },
      { type: 'ping' },
      { type: 'list_clients' },
      { 
        type: 'custom_data', 
        payload: { 
          user: 'TestUser', 
          action: 'login',
          timestamp: new Date().toISOString(),
          data: { level: 5, score: 1250 }
        }
      }
    ];
    
    // Gửi từng message với delay
    messages.forEach((msg, index) => {
      setTimeout(() => {
        console.log('📤 Sending:', msg);
        client.write(JSON.stringify(msg) + '\n');
      }, (index + 1) * 1000);
    });
    
    // Disconnect sau 10 giây
    setTimeout(() => {
      console.log('👋 Disconnecting...');
      client.end();
    }, 10000);

  } catch (error) {
    console.error('❌ Failed to connect to server:', error.message);
    console.log('\n💡 To fix this:');
    console.log('   1. Start the server in another terminal:');
    console.log('      npm start');
    console.log('   2. Then run this JSON test:');
    console.log('      npm run json-test');
    process.exit(1);
  }
}

// Run the test
runJsonTest();