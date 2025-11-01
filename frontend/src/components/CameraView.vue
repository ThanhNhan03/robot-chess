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

<style scoped src="../assets/styles/CameraView.css">
</style>
