<template>
  <div class="camera-view">
    <div class="camera-container">
      <div class="camera-header">
        <h3>Robot Camera Feed</h3>
        <div class="camera-status">
          <div :class="['status-indicator', isConnected ? 'connected' : 'disconnected']"></div>
          <span>{{ isConnected ? 'Connected' : 'Disconnected' }}</span>
        </div>
      </div>
      
      <div class="camera-feed">
        <video
          v-if="isConnected && videoStream"
          ref="videoElement"
          autoplay
          playsinline
          muted
          class="video-stream"
        ></video>
        
        <div v-else class="camera-placeholder">
          <div class="placeholder-icon">📷</div>
          <p>Camera not connected</p>
          <button @click="connectCamera" class="connect-btn">
            Connect Camera
          </button>
        </div>
      </div>
      
      <div class="camera-controls">
        <button 
          @click="toggleCamera" 
          :disabled="!isConnected"
          class="control-btn"
        >
          {{ isStreaming ? 'Stop' : 'Start' }}
        </button>
        
        <button 
          @click="captureFrame" 
          :disabled="!isConnected || !isStreaming"
          class="control-btn"
        >
          Capture
        </button>
        
        <button 
          @click="toggleFullscreen" 
          :disabled="!isConnected"
          class="control-btn"
        >
          Fullscreen
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onBeforeUnmount } from 'vue'

const videoElement = ref<HTMLVideoElement | null>(null)
const videoStream = ref<MediaStream | null>(null)
const isConnected = ref(false)
const isStreaming = ref(false)

const connectCamera = async () => {
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ 
      video: { 
        width: { ideal: 1280 },
        height: { ideal: 720 }
      } 
    })
    
    videoStream.value = stream
    isConnected.value = true
    
    if (videoElement.value) {
      videoElement.value.srcObject = stream
      isStreaming.value = true
    }
  } catch (error) {
    console.error('Error accessing camera:', error)
    alert('Unable to access camera. Please check permissions.')
  }
}

const toggleCamera = () => {
  if (isStreaming.value) {
    if (videoStream.value) {
      videoStream.value.getTracks().forEach(track => track.stop())
      videoStream.value = null
    }
    isStreaming.value = false
    isConnected.value = false
  } else {
    connectCamera()
  }
}

const captureFrame = () => {
  if (videoElement.value) {
    const canvas = document.createElement('canvas')
    const context = canvas.getContext('2d')
    
    canvas.width = videoElement.value.videoWidth
    canvas.height = videoElement.value.videoHeight
    
    if (context) {
      context.drawImage(videoElement.value, 0, 0)
      
      // Convert to blob and download
      canvas.toBlob((blob) => {
        if (blob) {
          const url = URL.createObjectURL(blob)
          const a = document.createElement('a')
          a.href = url
          a.download = `chess-capture-${new Date().getTime()}.png`
          a.click()
          URL.revokeObjectURL(url)
        }
      })
    }
  }
}

const toggleFullscreen = () => {
  if (videoElement.value) {
    if (document.fullscreenElement) {
      document.exitFullscreen()
    } else {
      videoElement.value.requestFullscreen()
    }
  }
}

onBeforeUnmount(() => {
  if (videoStream.value) {
    videoStream.value.getTracks().forEach(track => track.stop())
  }
})
</script>

<style scoped>
.camera-view {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
}

.camera-container {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  overflow: hidden;
  width: 100%;
  max-width: 100%;
}

.camera-header {
  background: #2c3e50;
  color: white;
  padding: 12px 15px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
}

.camera-header h3 {
  margin: 0;
  font-size: clamp(14px, 3vw, 18px);
}

.camera-status {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: clamp(12px, 2.5vw, 14px);
}

.status-indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #e74c3c;
}

.status-indicator.connected {
  background: #27ae60;
}

.camera-feed {
  background: #34495e;
  aspect-ratio: 16/9;
  display: flex;
  justify-content: center;
  align-items: center;
  position: relative;
  min-height: 200px;
}

.video-stream {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.camera-placeholder {
  text-align: center;
  color: #bdc3c7;
  padding: 20px;
}

.placeholder-icon {
  font-size: clamp(32px, 8vw, 48px);
  margin-bottom: 16px;
}

.placeholder-icon + p {
  margin: 0 0 20px 0;
  font-size: clamp(14px, 3vw, 16px);
}

.connect-btn {
  background: #3498db;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: clamp(12px, 2.5vw, 14px);
  transition: background 0.3s;
}

.connect-btn:hover {
  background: #2980b9;
}

.camera-controls {
  background: #ecf0f1;
  padding: 12px 15px;
  display: flex;
  gap: 8px;
  justify-content: center;
  flex-wrap: wrap;
}

.control-btn {
  background: #34495e;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  font-size: clamp(11px, 2.5vw, 14px);
  transition: all 0.3s;
  min-width: 60px;
}

.control-btn:hover:not(:disabled) {
  background: #2c3e50;
  transform: translateY(-1px);
}

.control-btn:disabled {
  background: #bdc3c7;
  cursor: not-allowed;
  transform: none;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .camera-header {
    padding: 10px 12px;
  }
  
  .camera-controls {
    padding: 10px 12px;
    gap: 6px;
  }
  
  .control-btn {
    padding: 5px 10px;
    min-width: 50px;
  }
}

@media (max-width: 480px) {
  .camera-header {
    flex-direction: column;
    text-align: center;
    gap: 8px;
  }
  
  .camera-placeholder {
    padding: 15px;
  }
  
  .camera-controls {
    padding: 8px 10px;
  }
}
</style>
