// Robot TCP Proxy Server
const express = require('express');
const WebSocket = require('ws');
const net = require('net');
const cors = require('cors');

const app = express();
const PORT = process.env.PORT || 8082;
const ROBOT_HOST = process.env.ROBOT_HOST || 'localhost';
const ROBOT_PORT = process.env.ROBOT_PORT || 9090;

app.use(cors());
app.use(express.json());

// WebSocket server for robot communication
const wss = new WebSocket.Server({ port: PORT });

console.log(`Robot TCP Proxy WebSocket Server running on port ${PORT}`);
console.log(`Robot TCP endpoint: ${ROBOT_HOST}:${ROBOT_PORT}`);
var clients = [];
wss.on('connection', (ws) => {
  clients.push(ws)
  console.log('Frontend connected to robot proxy');
  
  ws.on('message', async (message) => {
    try {
      const command = JSON.parse(message.toString());
      console.log('Received command from frontend:', command);
      
      // Send command to robot via TCP
      // const response = await sendTcpCommand(command);
      
      // Send response back to frontend 
      clients.forEach((_ws) => {
        if (_ws != ws) {
          _ws.send(JSON.stringify(command));
        }
      })
      
    } catch (error) {
      console.error('Error processing command:', error);
      // ws.send(JSON.stringify({
      //   success: false,
      //   message: 'Failed to process command',
      //   error: error.message
      // }));
    }
  });

  ws.on('close', () => {
    console.log('Frontend disconnected from robot proxy');
    clients = clients.filter((_ws) => _ws != ws);
    console.log(clients.length);
  });

  ws.on('error', (error) => {
    console.error('WebSocket error:', error);
  });
});

// Function to send TCP command to robot
function sendTcpCommand(command) {
  return new Promise((resolve, reject) => {
    const client = new net.Socket();
    let responseData = '';

    // Set timeout
    const timeout = setTimeout(() => {
      client.destroy();
      reject(new Error('TCP connection timeout'));
    }, 10000);

    client.connect(ROBOT_PORT, ROBOT_HOST, () => {
      console.log(`Connected to robot at ${ROBOT_HOST}:${ROBOT_PORT}`);
      
      // Send command as JSON
      const commandStr = JSON.stringify(command) + '\n';
      client.write(commandStr);
    });

    client.on('data', (data) => {
      responseData += data.toString();
      
      // Check if we have a complete JSON response
      try {
        const response = JSON.parse(responseData);
        clearTimeout(timeout);
        client.destroy();
        resolve(response);
      } catch (e) {
        // Not complete JSON yet, continue reading
      }
    });

    client.on('close', () => {
      clearTimeout(timeout);
      if (responseData) {
        try {
          const response = JSON.parse(responseData);
          resolve(response);
        } catch (e) {
          resolve({
            success: true,
            message: 'Command sent successfully',
            goal_id: command.goal_id
          });
        }
      } else {
        resolve({
          success: true,
          message: 'Command sent successfully',
          goal_id: command.goal_id
        });
      }
    });

    client.on('error', (error) => {
      clearTimeout(timeout);
      console.error('TCP connection error:', error);
      
      // For development, simulate success even if robot is not available
      if (error.code === 'ECONNREFUSED') {
        resolve({
          success: true,
          message: 'Command sent (robot offline - simulated)',
          goal_id: command.goal_id,
          simulated: true
        });
      } else {
        reject(error);
      }
    });
  });
}

// HTTP endpoint for direct API calls (alternative to WebSocket)
// app.post('/api/robot/command', async (req, res) => {
//   try {
//     const { command } = req.body;
//     console.log('Received HTTP command:', command);
    
//     const response = await sendTcpCommand(command);
//     res.json(response);
    
//   } catch (error) {
//     console.error('HTTP command error:', error);
//     res.status(500).json({
//       success: false,
//       message: 'Failed to send command',
//       error: error.message
//     });
//   }
// });

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    robot_endpoint: `${ROBOT_HOST}:${ROBOT_PORT}`
  });
});

// Start HTTP server (for API endpoints)
app.listen(PORT + 1, () => {
  console.log(`Robot TCP Proxy HTTP Server running on port ${PORT + 1}`);
});

// Handle process termination
process.on('SIGINT', () => {
  console.log('\nShutting down robot proxy server...');
  wss.close(() => {
    console.log('WebSocket server closed');
    process.exit(0);
  });
});

process.on('SIGTERM', () => {
  console.log('\nShutting down robot proxy server...');
  wss.close(() => {
    console.log('WebSocket server closed');
    process.exit(0);
  });
});