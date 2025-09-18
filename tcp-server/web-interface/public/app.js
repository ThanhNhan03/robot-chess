class TCPWebInterface {
    constructor() {
        this.socket = io();
        this.messageCount = 0;
        this.isConnected = false;
        this.tcpConnected = false;
        
        this.initializeElements();
        this.setupEventListeners();
        this.setupSocketEvents();
    }

    initializeElements() {
        // Status elements
        this.statusIndicator = document.getElementById('connection-status');
        this.statusText = document.getElementById('status-text');
        this.clientInfo = document.getElementById('client-info');
        
        // Message elements
        this.messagesContainer = document.getElementById('messages');
        this.messageCountEl = document.getElementById('message-count');
        this.tcpPortEl = document.getElementById('tcp-port');
        this.timestampEl = document.getElementById('timestamp');
        
        // Input elements
        this.textInput = document.getElementById('text-input');
        this.jsonInput = document.getElementById('json-input');
        
        // Button elements
        this.connectBtn = document.getElementById('connect-btn');
        this.disconnectBtn = document.getElementById('disconnect-btn');
        this.sendTextBtn = document.getElementById('send-text');
        this.sendJsonBtn = document.getElementById('send-json');
        this.clearMessagesBtn = document.getElementById('clear-messages');
        this.refreshStatusBtn = document.getElementById('refresh-status');
        
        // Tab elements
        this.tabBtns = document.querySelectorAll('.tab-btn');
        this.tabContents = document.querySelectorAll('.tab-content');
        
        // Example and preset buttons
        this.exampleBtns = document.querySelectorAll('.example-btn');
        this.presetBtns = document.querySelectorAll('.preset-btn');
    }

    setupEventListeners() {
        // Connection buttons
        this.connectBtn.addEventListener('click', () => this.connectToTCP());
        this.disconnectBtn.addEventListener('click', () => this.disconnectFromTCP());
        this.refreshStatusBtn.addEventListener('click', () => this.refreshStatus());
        
        // Send buttons
        this.sendTextBtn.addEventListener('click', () => this.sendTextMessage());
        this.sendJsonBtn.addEventListener('click', () => this.sendJsonMessage());
        
        // Clear messages
        this.clearMessagesBtn.addEventListener('click', () => this.clearMessages());
        
        // Enter key for inputs
        this.textInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.sendTextMessage();
        });
        
        this.jsonInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && e.ctrlKey) this.sendJsonMessage();
        });
        
        // Tab switching
        this.tabBtns.forEach(btn => {
            btn.addEventListener('click', () => this.switchTab(btn.dataset.type));
        });
        
        // Example JSON buttons
        this.exampleBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                this.jsonInput.value = btn.dataset.json;
            });
        });
        
        // Preset message buttons
        this.presetBtns.forEach(btn => {
            btn.addEventListener('click', () => this.sendPresetMessage(btn.dataset.message));
        });
    }

    setupSocketEvents() {
        // Socket connection
        this.socket.on('connect', () => {
            this.isConnected = true;
            this.updateConnectionStatus('Web socket connected', 'info');
            this.addSystemMessage('Connected to web server', 'success');
        });

        this.socket.on('disconnect', () => {
            this.isConnected = false;
            this.tcpConnected = false;
            this.updateConnectionStatus('Disconnected', 'error');
            this.addSystemMessage('Disconnected from web server', 'error');
        });

        // Server status
        this.socket.on('server-status', (data) => {
            if (data.tcpServerFound) {
                this.tcpPortEl.textContent = `TCP Port: ${data.port}`;
                this.addSystemMessage(`TCP Server found on port ${data.port}`, 'info');
            } else {
                this.addSystemMessage('TCP Server not found. Please start TCP Server first.', 'warning');
            }
        });

        // TCP connection events
        this.socket.on('tcp-connected', (data) => {
            this.tcpConnected = true;
            this.updateConnectionStatus('TCP Connected', 'success');
            this.connectBtn.disabled = true;
            this.disconnectBtn.disabled = false;
            this.addSystemMessage(`Connected to TCP Server on port ${data.port}`, 'success');
        });

        this.socket.on('tcp-disconnected', (data) => {
            this.tcpConnected = false;
            this.updateConnectionStatus('TCP Disconnected', 'warning');
            this.connectBtn.disabled = false;
            this.disconnectBtn.disabled = true;
            this.addSystemMessage('TCP connection closed', 'warning');
        });

        this.socket.on('tcp-error', (data) => {
            this.addSystemMessage(`TCP Error: ${data.error}`, 'error');
        });

        // Message events
        this.socket.on('tcp-message', (data) => {
            this.addReceivedMessage(data);
        });

        this.socket.on('message-sent', (data) => {
            this.addSentMessage(data.message);
        });
    }

    switchTab(type) {
        // Update tab buttons
        this.tabBtns.forEach(btn => {
            btn.classList.toggle('active', btn.dataset.type === type);
        });
        
        // Update tab contents
        this.tabContents.forEach(content => {
            content.classList.toggle('active', content.id === `tab-${type}`);
        });
    }

    connectToTCP() {
        if (!this.isConnected) {
            this.addSystemMessage('Please wait for web socket connection first', 'warning');
            return;
        }
        
        this.socket.emit('connect-tcp');
        this.addSystemMessage('Connecting to TCP Server...', 'info');
    }

    disconnectFromTCP() {
        this.socket.disconnect();
        this.tcpConnected = false;
        this.updateConnectionStatus('Disconnected', 'error');
        this.connectBtn.disabled = false;
        this.disconnectBtn.disabled = true;
        this.addSystemMessage('Disconnected from TCP Server', 'info');
    }

    refreshStatus() {
        fetch('/status')
            .then(response => response.json())
            .then(data => {
                this.addSystemMessage(`Status: Web=${data.webServer}, TCP=${data.tcpServer}, Port=${data.tcpPort}, Connections=${data.connections}`, 'info');
            })
            .catch(error => {
                this.addSystemMessage(`Failed to get status: ${error.message}`, 'error');
            });
    }

    sendTextMessage() {
        const text = this.textInput.value.trim();
        if (!text) return;
        
        const message = { type: 'text', content: text };
        this.sendMessage(message);
        this.textInput.value = '';
    }

    sendJsonMessage() {
        const json = this.jsonInput.value.trim();
        if (!json) return;
        
        try {
            const message = JSON.parse(json);
            this.sendMessage(message);
            this.jsonInput.value = '';
        } catch (error) {
            this.addSystemMessage(`Invalid JSON: ${error.message}`, 'error');
        }
    }

    sendPresetMessage(messageJson) {
        try {
            const message = JSON.parse(messageJson);
            
            // Add timestamp to custom_data messages
            if (message.type === 'custom_data' && message.payload) {
                message.payload.timestamp = new Date().toISOString();
            }
            
            this.sendMessage(message);
        } catch (error) {
            this.addSystemMessage(`Failed to send preset message: ${error.message}`, 'error');
        }
    }

    sendMessage(message) {
        if (!this.tcpConnected) {
            this.addSystemMessage('Not connected to TCP Server', 'error');
            return;
        }
        
        this.socket.emit('send-message', { message });
    }

    addSentMessage(message) {
        const messageEl = this.createMessageElement(message, 'sent');
        this.messagesContainer.appendChild(messageEl);
        this.scrollToBottom();
        this.updateMessageCount();
    }

    addReceivedMessage(message) {
        const messageEl = this.createMessageElement(message, 'received');
        this.messagesContainer.appendChild(messageEl);
        this.scrollToBottom();
        this.updateMessageCount();
    }

    addSystemMessage(text, type = 'info') {
        const messageEl = document.createElement('div');
        messageEl.className = `message system ${type}`;
        
        const time = new Date().toLocaleTimeString();
        messageEl.innerHTML = `
            <div class="message-type">${type.toUpperCase()}</div>
            <div>${text}</div>
            <div class="message-time">${time}</div>
        `;
        
        this.messagesContainer.appendChild(messageEl);
        this.scrollToBottom();
        this.updateMessageCount();
        this.updateTimestamp();
    }

    createMessageElement(message, direction) {
        const messageEl = document.createElement('div');
        messageEl.className = `message ${direction}`;
        
        const time = new Date().toLocaleTimeString();
        let content = '';
        let messageType = 'unknown';
        
        if (typeof message === 'object') {
            messageType = message.type || 'object';
            if (message.type === 'welcome') {
                content = message.message;
            } else if (message.type === 'broadcast') {
                content = `[From #${message.from}] ${message.content}`;
            } else if (message.type === 'private_message') {
                content = `[Private from #${message.from}] ${message.content}`;
            } else if (message.type === 'clients_list') {
                content = `Client list: ${message.clients.length} clients\n${message.clients.map(c => `#${c.id}${c.isYou ? ' (You)' : ''}`).join(', ')}`;
            } else if (message.content) {
                content = message.content;
            } else {
                content = JSON.stringify(message, null, 2);
            }
        } else {
            content = String(message);
        }
        
        messageEl.innerHTML = `
            <div class="message-type">${messageType}</div>
            <div>${this.escapeHtml(content)}</div>
            <div class="message-time">${time}</div>
        `;
        
        return messageEl;
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML.replace(/\n/g, '<br>');
    }

    updateConnectionStatus(text, type) {
        this.statusText.textContent = text;
        this.statusIndicator.className = `status-indicator ${type === 'success' ? 'connected' : 'disconnected'}`;
    }

    clearMessages() {
        this.messagesContainer.innerHTML = '';
        this.messageCount = 0;
        this.updateMessageCount();
        this.addSystemMessage('Messages cleared', 'info');
    }

    scrollToBottom() {
        this.messagesContainer.scrollTop = this.messagesContainer.scrollHeight;
    }

    updateMessageCount() {
        this.messageCount = this.messagesContainer.children.length;
        this.messageCountEl.textContent = `Messages: ${this.messageCount}`;
    }

    updateTimestamp() {
        this.timestampEl.textContent = `Last update: ${new Date().toLocaleTimeString()}`;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.tcpWebInterface = new TCPWebInterface();
});