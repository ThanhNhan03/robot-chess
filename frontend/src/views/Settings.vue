<template>
  <div class="settings">
    <div class="container container-sm">
      <h1 class="page-title">⚙️ Settings</h1>

      <!-- Theme Settings -->
      <div class="card">
        <h3>🎨 Appearance</h3>
        
        <div class="form-group">
          <label class="form-label">Chess Board Theme</label>
          <div class="theme-grid">
            <div 
              v-for="theme in boardThemes" 
              :key="theme.id"
              :class="['theme-option', { active: selectedTheme === theme.id }]"
              @click="selectedTheme = theme.id"
            >
              <div class="theme-preview" :style="{ background: theme.gradient }"></div>
              <span>{{ theme.name }}</span>
            </div>
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Piece Style</label>
          <select v-model="pieceStyle" class="form-control">
            <option value="classic">Classic</option>
            <option value="modern">Modern</option>
            <option value="minimal">Minimal</option>
            <option value="3d">3D</option>
          </select>
        </div>

        <div class="form-group">
          <label class="form-label">Interface Mode</label>
          <div class="radio-group">
            <label class="radio-option">
              <input type="radio" v-model="interfaceMode" value="light" />
              <span>☀️ Light Mode</span>
            </label>
            <label class="radio-option">
              <input type="radio" v-model="interfaceMode" value="dark" />
              <span>🌙 Dark Mode</span>
            </label>
            <label class="radio-option">
              <input type="radio" v-model="interfaceMode" value="auto" />
              <span>🔄 Auto</span>
            </label>
          </div>
        </div>
      </div>

      <!-- Game Settings -->
      <div class="card mt-4">
        <h3>🎮 Game Settings</h3>

        <div class="form-group">
          <label class="form-label">AI Difficulty</label>
          <select v-model="aiDifficulty" class="form-control">
            <option value="beginner">🟢 Beginner (ELO 800-1000)</option>
            <option value="intermediate">🟡 Intermediate (ELO 1200-1400)</option>
            <option value="advanced">🟠 Advanced (ELO 1600-1800)</option>
            <option value="expert">🔴 Expert (ELO 2000-2200)</option>
            <option value="master">⚫ Master (ELO 2400+)</option>
          </select>
        </div>

        <div class="form-group">
          <label class="form-label">AI Playing Style</label>
          <select v-model="aiStyle" class="form-control">
            <option value="balanced">⚖️ Balanced</option>
            <option value="aggressive">⚔️ Aggressive</option>
            <option value="defensive">🛡️ Defensive</option>
            <option value="positional">📐 Positional</option>
          </select>
        </div>

        <div class="form-group">
          <label class="form-label">Time Control</label>
          <select v-model="timeControl" class="form-control">
            <option value="bullet">⚡ Bullet (1-2 min)</option>
            <option value="blitz">🔥 Blitz (3-5 min)</option>
            <option value="rapid">⏱️ Rapid (10-15 min)</option>
            <option value="classical">🏛️ Classical (30+ min)</option>
            <option value="unlimited">♾️ Unlimited</option>
          </select>
        </div>

        <div class="toggle-group">
          <label class="toggle-option">
            <input type="checkbox" v-model="showHints" />
            <span>💡 Show Move Hints</span>
          </label>
          <label class="toggle-option">
            <input type="checkbox" v-model="showLegalMoves" />
            <span>✨ Highlight Legal Moves</span>
          </label>
          <label class="toggle-option">
            <input type="checkbox" v-model="enableUndo" />
            <span>↩️ Enable Undo</span>
          </label>
          <label class="toggle-option">
            <input type="checkbox" v-model="autoQueen" />
            <span>👑 Auto-Queen Promotion</span>
          </label>
        </div>
      </div>

      <!-- Sound Settings -->
      <div class="card mt-4">
        <h3>🔊 Sound & Notifications</h3>

        <div class="form-group">
          <label class="form-label">Sound Volume</label>
          <input 
            type="range" 
            v-model="soundVolume" 
            min="0" 
            max="100" 
            class="range-slider"
          />
          <span class="range-value">{{ soundVolume }}%</span>
        </div>

        <div class="toggle-group">
          <label class="toggle-option">
            <input type="checkbox" v-model="moveSounds" />
            <span>🔉 Move Sounds</span>
          </label>
          <label class="toggle-option">
            <input type="checkbox" v-model="captureSounds" />
            <span>💥 Capture Sounds</span>
          </label>
          <label class="toggle-option">
            <input type="checkbox" v-model="checkSounds" />
            <span>⚠️ Check/Checkmate Sounds</span>
          </label>
          <label class="toggle-option">
            <input type="checkbox" v-model="notifications" />
            <span>🔔 Push Notifications</span>
          </label>
        </div>
      </div>

      <!-- Privacy Settings -->
      <div class="card mt-4">
        <h3>🔐 Privacy & Security</h3>

        <div class="toggle-group">
          <label class="toggle-option">
            <input type="checkbox" v-model="showOnline" />
            <span>🟢 Show Online Status</span>
          </label>
          <label class="toggle-option">
            <input type="checkbox" v-model="allowFriendRequests" />
            <span>👥 Allow Friend Requests</span>
          </label>
          <label class="toggle-option">
            <input type="checkbox" v-model="showGameHistory" />
            <span>📊 Public Game History</span>
          </label>
        </div>

        <button class="btn btn-secondary mt-3">Change Password</button>
        <button class="btn btn-danger mt-2">Delete Account</button>
      </div>

      <!-- Save Button -->
      <div class="save-section mt-4">
        <button @click="saveSettings" class="btn btn-primary btn-lg" :disabled="loading">
          <span v-if="loading" class="loading"></span>
          <span v-else>💾 Save Settings</span>
        </button>
        <p v-if="successMessage" class="success mt-2">{{ successMessage }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useGameStore } from '@/store/game'

const gameStore = useGameStore()
const loading = ref(false)
const successMessage = ref('')

// Appearance Settings
const boardThemes = [
  { id: 'classic', name: 'Classic', gradient: 'linear-gradient(135deg, #f0d9b5, #b58863)' },
  { id: 'blue', name: 'Ocean Blue', gradient: 'linear-gradient(135deg, #7dd3fc, #0284c7)' },
  { id: 'green', name: 'Forest Green', gradient: 'linear-gradient(135deg, #86efac, #16a34a)' },
  { id: 'purple', name: 'Royal Purple', gradient: 'linear-gradient(135deg, #c084fc, #7c3aed)' },
]

const selectedTheme = ref('classic')
const pieceStyle = ref('classic')
const interfaceMode = ref('light')

// Game Settings
const aiDifficulty = ref('intermediate')
const aiStyle = ref('balanced')
const timeControl = ref('blitz')
const showHints = ref(true)
const showLegalMoves = ref(true)
const enableUndo = ref(true)
const autoQueen = ref(false)

// Sound Settings
const soundVolume = ref(70)
const moveSounds = ref(true)
const captureSounds = ref(true)
const checkSounds = ref(true)
const notifications = ref(true)

// Privacy Settings
const showOnline = ref(true)
const allowFriendRequests = ref(true)
const showGameHistory = ref(true)

async function saveSettings() {
  loading.value = true
  successMessage.value = ''
  
  try {
    // Save to game store
    gameStore.setTheme(selectedTheme.value)
    gameStore.setAILevel(aiDifficulty.value)
    
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    
    // Save to localStorage
    localStorage.setItem('settings', JSON.stringify({
      selectedTheme: selectedTheme.value,
      pieceStyle: pieceStyle.value,
      interfaceMode: interfaceMode.value,
      aiDifficulty: aiDifficulty.value,
      aiStyle: aiStyle.value,
      timeControl: timeControl.value,
      showHints: showHints.value,
      showLegalMoves: showLegalMoves.value,
      enableUndo: enableUndo.value,
      autoQueen: autoQueen.value,
      soundVolume: soundVolume.value,
      moveSounds: moveSounds.value,
      captureSounds: captureSounds.value,
      checkSounds: checkSounds.value,
      notifications: notifications.value,
      showOnline: showOnline.value,
      allowFriendRequests: allowFriendRequests.value,
      showGameHistory: showGameHistory.value,
    }))
    
    successMessage.value = '✓ Settings saved successfully!'
    
    setTimeout(() => {
      successMessage.value = ''
    }, 3000)
  } catch (error) {
    console.error('Failed to save settings:', error)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.settings {
  padding: 2rem 0;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.container {
  max-width: 900px;
  margin: 0 auto;
  padding: 0 1rem;
}

.page-title {
  text-align: center;
  margin-bottom: 3rem;
  font-size: 3rem;
  font-weight: 800;
  color: white;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.2);
  animation: fadeInDown 0.6s ease-out;
}

@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.card {
  background: white;
  border-radius: 20px;
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.2);
  padding: 2rem;
  animation: fadeInUp 0.6s ease-out backwards;
  margin-bottom: 2rem;
}

.card:nth-child(2) { animation-delay: 0.1s; }
.card:nth-child(3) { animation-delay: 0.2s; }
.card:nth-child(4) { animation-delay: 0.3s; }
.card:nth-child(5) { animation-delay: 0.4s; }

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.card h3 {
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 3px solid #667eea;
  font-size: 1.5rem;
  font-weight: 700;
  color: #2c3e50;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-label {
  display: block;
  margin-bottom: 0.75rem;
  font-weight: 600;
  color: #2c3e50;
  font-size: 1rem;
}

.form-control {
  width: 100%;
  padding: 0.875rem 1rem;
  border: 2px solid #e0e0e0;
  border-radius: 12px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: white;
}

.form-control:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-control:hover {
  border-color: #667eea;
}

.theme-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 1rem;
}

.theme-option {
  cursor: pointer;
  text-align: center;
  padding: 1rem;
  border: 3px solid #e0e0e0;
  border-radius: 15px;
  transition: all 0.3s ease;
  background: white;
}

.theme-option:hover {
  border-color: #667eea;
  transform: translateY(-5px);
  box-shadow: 0 10px 25px rgba(102, 126, 234, 0.3);
}

.theme-option.active {
  border-color: #667eea;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
  box-shadow: 0 10px 25px rgba(102, 126, 234, 0.3);
}

.theme-option span {
  font-weight: 600;
  color: #2c3e50;
  font-size: 0.9rem;
}

.theme-preview {
  width: 100%;
  height: 90px;
  border-radius: 10px;
  margin-bottom: 0.75rem;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
}

.radio-group,
.toggle-group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.radio-option,
.toggle-option {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.25rem;
  border: 2px solid #e0e0e0;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
  background: white;
}

.radio-option:hover,
.toggle-option:hover {
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-color: #667eea;
  transform: translateX(5px);
}

.radio-option input[type="radio"]:checked ~ span,
.toggle-option input[type="checkbox"]:checked ~ span {
  font-weight: 700;
  color: #667eea;
}

.radio-option input,
.toggle-option input {
  width: 22px;
  height: 22px;
  cursor: pointer;
  accent-color: #667eea;
}

.radio-option span,
.toggle-option span {
  font-size: 1rem;
  color: #2c3e50;
  font-weight: 500;
}

.range-slider {
  width: 100%;
  height: 10px;
  border-radius: 5px;
  outline: none;
  background: linear-gradient(to right, #667eea 0%, #764ba2 100%);
  -webkit-appearance: none;
  appearance: none;
}

.range-slider::-webkit-slider-thumb {
  -webkit-appearance: none;
  appearance: none;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: white;
  cursor: pointer;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.3);
  transition: all 0.3s ease;
}

.range-slider::-webkit-slider-thumb:hover {
  transform: scale(1.2);
  box-shadow: 0 6px 15px rgba(102, 126, 234, 0.5);
}

.range-slider::-moz-range-thumb {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: white;
  cursor: pointer;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.3);
  border: none;
  transition: all 0.3s ease;
}

.range-slider::-moz-range-thumb:hover {
  transform: scale(1.2);
  box-shadow: 0 6px 15px rgba(102, 126, 234, 0.5);
}

.range-value {
  display: inline-block;
  margin-top: 0.75rem;
  font-weight: 700;
  font-size: 1.25rem;
  color: #667eea;
  padding: 0.5rem 1rem;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 10px;
}

.mt-2 {
  margin-top: 0.5rem;
}

.mt-3 {
  margin-top: 1rem;
}

.mt-4 {
  margin-top: 2rem;
}

.btn-secondary {
  width: 100%;
  padding: 0.875rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  background: linear-gradient(135deg, #95a5a6 0%, #7f8c8d 100%);
  border: none;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(149, 165, 166, 0.4);
  font-size: 1rem;
}

.btn-secondary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(149, 165, 166, 0.5);
}

.btn-danger {
  width: 100%;
  padding: 0.875rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  background: linear-gradient(135deg, #eb3349 0%, #f45c43 100%);
  border: none;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(235, 51, 73, 0.4);
  font-size: 1rem;
}

.btn-danger:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(235, 51, 73, 0.5);
}

.save-section {
  text-align: center;
  animation: fadeInUp 0.6s ease-out 0.5s backwards;
}

.btn-primary {
  padding: 1rem 3rem;
  border-radius: 15px;
  font-weight: 700;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
  font-size: 1.1rem;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-3px);
  box-shadow: 0 12px 30px rgba(102, 126, 234, 0.5);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-lg {
  padding: 1.25rem 3.5rem;
  font-size: 1.2rem;
}

.loading {
  display: inline-block;
  width: 24px;
  height: 24px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.success {
  color: #27ae60;
  font-weight: 700;
  text-align: center;
  padding: 1rem;
  background: white;
  border-radius: 12px;
  font-size: 1.1rem;
  box-shadow: 0 8px 20px rgba(39, 174, 96, 0.3);
  animation: successPulse 0.5s ease-out;
}

@keyframes successPulse {
  0% {
    transform: scale(0.95);
    opacity: 0;
  }
  50% {
    transform: scale(1.02);
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}

@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
    margin-bottom: 2rem;
  }

  .card {
    padding: 1.5rem;
  }

  .theme-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .btn-lg {
    padding: 1rem 2rem;
    font-size: 1rem;
  }
}

@media (max-width: 480px) {
  .card {
    padding: 1.25rem;
  }

  .card h3 {
    font-size: 1.25rem;
  }

  .theme-grid {
    grid-template-columns: 1fr;
  }
}
</style>
