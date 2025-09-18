#!/bin/bash

echo "🚀 Starting TCP Server Web Interface"
echo "=================================="

# Kiểm tra Node.js
if ! command -v node &> /dev/null; then
    echo "❌ Node.js not found. Please install Node.js first."
    exit 1
fi

# Màu cho output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}📦 Installing web interface dependencies...${NC}"
cd web-interface
npm install

echo -e "${YELLOW}🔍 Checking for TCP Server...${NC}"

# Kiểm tra xem TCP Server có đang chạy không
TCP_RUNNING=$(lsof -ti :8080,8081,8082 2>/dev/null)

if [ -z "$TCP_RUNNING" ]; then
    echo -e "${YELLOW}⚠️ TCP Server not running${NC}"
    echo -e "${BLUE}🚀 Starting TCP Server in background...${NC}"
    
    # Start TCP Server trong background
    cd ..
    npm start &
    TCP_PID=$!
    
    echo -e "${GREEN}✅ TCP Server started (PID: $TCP_PID)${NC}"
    
    # Đợi một chút để server khởi động
    sleep 2
    
    cd web-interface
else
    echo -e "${GREEN}✅ TCP Server already running${NC}"
fi

echo -e "${BLUE}🌐 Starting Web Interface...${NC}"
echo -e "${GREEN}📡 Web UI will be available at: http://localhost:3000${NC}"
echo -e "${YELLOW}📝 Instructions:${NC}"
echo "   1. Open http://localhost:3000 in your browser"
echo "   2. Click 'Connect to TCP Server'"
echo "   3. Start sending messages!"
echo ""
echo -e "${YELLOW}Press Ctrl+C to stop both servers${NC}"
echo ""

# Start web server
npm run web