#!/bin/bash

# Script để kill processes đang sử dụng port 8080
echo "🔍 Checking processes using port 8080..."

PIDS=$(lsof -ti :8080)

if [ -z "$PIDS" ]; then
    echo "✅ Port 8080 is free"
else
    echo "⚠️ Found processes using port 8080:"
    lsof -i :8080
    
    read -p "Do you want to kill these processes? (y/N): " -n 1 -r
    echo
    
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "🔥 Killing processes..."
        echo $PIDS | xargs kill -9
        echo "✅ Processes killed"
        
        # Wait a moment for cleanup
        sleep 1
        
        # Check again
        REMAINING=$(lsof -ti :8080)
        if [ -z "$REMAINING" ]; then
            echo "🎉 Port 8080 is now free"
        else
            echo "⚠️ Some processes might still be running"
        fi
    else
        echo "❌ Skipped killing processes"
    fi
fi