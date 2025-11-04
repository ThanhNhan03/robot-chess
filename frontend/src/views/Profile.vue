<template>
  <div class="profile">
    <div class="container">
      <h1 class="page-title">👤 My Profile</h1>

      <div class="grid grid-2">
        <!-- Profile Card -->
        <div class="card profile-card">
          <div class="profile-header">
            <img :src="user.avatar" alt="Avatar" class="profile-avatar" />
            <button class="btn-change-avatar btn-secondary">Change Avatar</button>
          </div>
          
          <div class="profile-info">
            <h2>{{ user.name }}</h2>
            <p class="email">{{ user.email }}</p>
            <p class="member-since">Member since {{ user.joinDate }}</p>
          </div>

          <div class="profile-stats">
            <div class="stat">
              <div class="stat-value">{{ user.elo }}</div>
              <div class="stat-label">ELO Rating</div>
            </div>
            <div class="stat">
              <div class="stat-value">{{ user.gamesPlayed }}</div>
              <div class="stat-label">Games Played</div>
            </div>
            <div class="stat">
              <div class="stat-value">{{ user.winRate }}%</div>
              <div class="stat-label">Win Rate</div>
            </div>
          </div>
        </div>

        <!-- Edit Profile Form -->
        <div class="card">
          <h3>Edit Profile</h3>
          <form @submit.prevent="updateProfile">
            <div class="form-group">
              <label class="form-label">Username</label>
              <input 
                v-model="form.username" 
                type="text" 
                class="form-control"
                placeholder="Enter username"
              />
            </div>

            <div class="form-group">
              <label class="form-label">Email</label>
              <input 
                v-model="form.email" 
                type="email" 
                class="form-control"
                placeholder="Enter email"
              />
            </div>

            <div class="form-group">
              <label class="form-label">Bio</label>
              <textarea 
                v-model="form.bio" 
                class="form-control"
                rows="4"
                placeholder="Tell us about yourself..."
              ></textarea>
            </div>

            <div class="form-group">
              <label class="form-label">Country</label>
              <select v-model="form.country" class="form-control">
                <option value="">Select country</option>
                <option value="VN">Vietnam</option>
                <option value="US">United States</option>
                <option value="UK">United Kingdom</option>
                <option value="FR">France</option>
              </select>
            </div>

            <button type="submit" class="btn btn-primary" :disabled="loading">
              <span v-if="loading" class="loading"></span>
              <span v-else>Save Changes</span>
            </button>

            <p v-if="successMessage" class="success mt-2">{{ successMessage }}</p>
            <p v-if="errorMessage" class="error mt-2">{{ errorMessage }}</p>
          </form>
        </div>
      </div>

      <!-- Game Statistics -->
      <div class="card mt-4">
        <h3>Game Statistics</h3>
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-icon">🏆</div>
            <div class="stat-number">{{ user.wins }}</div>
            <div class="stat-label">Wins</div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">🤝</div>
            <div class="stat-number">{{ user.draws }}</div>
            <div class="stat-label">Draws</div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">❌</div>
            <div class="stat-number">{{ user.losses }}</div>
            <div class="stat-label">Losses</div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">⚡</div>
            <div class="stat-number">{{ user.winStreak }}</div>
            <div class="stat-label">Win Streak</div>
          </div>
        </div>
      </div>

      <!-- Recent Games -->
      <div class="card mt-4">
        <h3>Recent Games</h3>
        <div class="recent-games">
          <div 
            v-for="game in recentGames" 
            :key="game.id"
            class="game-item"
          >
            <div class="game-result" :class="game.result">
              {{ game.result === 'win' ? '✓' : game.result === 'loss' ? '✗' : '=' }}
            </div>
            <div class="game-info">
              <div class="game-opponent">vs {{ game.opponent }}</div>
              <div class="game-date">{{ game.date }}</div>
            </div>
            <div class="game-score">
              {{ game.playerScore }} - {{ game.opponentScore }}
            </div>
            <RouterLink :to="`/history/${game.id}`" class="btn-view">View</RouterLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useAuthStore } from '@/store/auth'

const authStore = useAuthStore()
const loading = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

// Mock user data
const user = ref({
  name: 'Chess Player',
  email: 'player@chess.com',
  avatar: 'https://i.pravatar.cc/150?img=4',
  joinDate: 'January 2025',
  elo: 1650,
  gamesPlayed: 250,
  winRate: 65,
  wins: 163,
  draws: 25,
  losses: 62,
  winStreak: 5,
  bio: 'Chess enthusiast',
  country: 'VN',
})

const form = reactive({
  username: user.value.name,
  email: user.value.email,
  bio: user.value.bio,
  country: user.value.country,
})

const recentGames = ref([
  { id: 1, opponent: 'GrandMaster_Alex', result: 'loss', playerScore: 0, opponentScore: 1, date: '2 hours ago' },
  { id: 2, opponent: 'Knight_Rider', result: 'win', playerScore: 1, opponentScore: 0, date: '5 hours ago' },
  { id: 3, opponent: 'Pawn_Star', result: 'draw', playerScore: 0.5, opponentScore: 0.5, date: '1 day ago' },
  { id: 4, opponent: 'Bishop_Boss', result: 'win', playerScore: 1, opponentScore: 0, date: '2 days ago' },
])

async function updateProfile() {
  loading.value = true
  successMessage.value = ''
  errorMessage.value = ''
  
  try {
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    
    // Update user data
    user.value.name = form.username
    user.value.email = form.email
    user.value.bio = form.bio
    user.value.country = form.country
    
    successMessage.value = 'Profile updated successfully!'
  } catch (error) {
    errorMessage.value = 'Failed to update profile'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.profile {
  padding: 2rem 0;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.container {
  max-width: 1200px;
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

.profile-card {
  text-align: center;
  background: white;
  border-radius: 20px;
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.2);
  padding: 2rem;
  animation: fadeInUp 0.6s ease-out 0.1s backwards;
  position: relative;
  overflow: hidden;
}

.profile-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 120px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  z-index: 0;
}

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

.profile-header {
  margin-bottom: 1.5rem;
  position: relative;
  z-index: 1;
  padding-top: 2rem;
}

.profile-avatar {
  width: 150px;
  height: 150px;
  border-radius: 50%;
  object-fit: cover;
  margin-bottom: 1rem;
  border: 5px solid white;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
  transition: all 0.3s ease;
}

.profile-avatar:hover {
  transform: scale(1.05);
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.4);
}

.btn-change-avatar {
  font-size: 0.875rem;
  padding: 0.625rem 1.25rem;
  border-radius: 25px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  color: white;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.btn-change-avatar:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.5);
}

.profile-info {
  position: relative;
  z-index: 1;
}

.profile-info h2 {
  margin-bottom: 0.5rem;
  font-size: 2rem;
  font-weight: 700;
  color: #2c3e50;
}

.email {
  color: #7f8c8d;
  margin-bottom: 0.5rem;
  font-size: 1rem;
}

.member-since {
  color: #95a5a6;
  font-size: 0.875rem;
  margin-bottom: 2rem;
  font-weight: 500;
}

.profile-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;
  padding-top: 2rem;
  border-top: 2px solid #f0f0f0;
  margin-top: 1rem;
}

.stat {
  text-align: center;
  padding: 1rem;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 15px;
  transition: all 0.3s ease;
}

.stat:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.1);
}

.stat-value {
  font-size: 2.5rem;
  font-weight: 800;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin-bottom: 0.5rem;
}

.stat-label {
  font-size: 0.875rem;
  color: #7f8c8d;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.card {
  background: white;
  border-radius: 20px;
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.2);
  padding: 2rem;
  animation: fadeInUp 0.6s ease-out backwards;
}

.grid-2 .card:nth-child(2) {
  animation-delay: 0.2s;
}

.card h3 {
  font-size: 1.5rem;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 3px solid #667eea;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #2c3e50;
  font-size: 0.9rem;
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

textarea.form-control {
  resize: vertical;
  min-height: 100px;
}

.btn-primary {
  width: 100%;
  padding: 1rem 2rem;
  border-radius: 12px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
  font-size: 1rem;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.5);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.loading {
  display: inline-block;
  width: 20px;
  height: 20px;
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
  font-weight: 600;
  text-align: center;
  padding: 0.75rem;
  background: #d5f4e6;
  border-radius: 8px;
}

.error {
  color: #e74c3c;
  font-weight: 600;
  text-align: center;
  padding: 0.75rem;
  background: #fadbd8;
  border-radius: 8px;
}

.mt-2 {
  margin-top: 0.5rem;
}

.mt-4 {
  margin-top: 2rem;
  animation: fadeInUp 0.6s ease-out 0.3s backwards;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1.5rem;
  margin-top: 1.5rem;
}

.stat-card {
  text-align: center;
  padding: 2rem 1.5rem;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 16px;
  transition: all 0.3s ease;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
}

.stat-icon {
  font-size: 3rem;
  margin-bottom: 0.75rem;
  filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.1));
}

.stat-number {
  font-size: 2rem;
  font-weight: 800;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin-bottom: 0.5rem;
}

.recent-games {
  margin-top: 1.5rem;
}

.game-item {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  padding: 1.25rem;
  border-bottom: 2px solid #f0f0f0;
  transition: all 0.3s ease;
  border-radius: 10px;
  margin-bottom: 0.5rem;
}

.game-item:hover {
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-color: transparent;
  transform: translateX(5px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.game-result {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 1.5rem;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
  transition: all 0.3s ease;
}

.game-result:hover {
  transform: scale(1.1);
}

.game-result.win {
  background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
  color: white;
}

.game-result.loss {
  background: linear-gradient(135deg, #eb3349 0%, #f45c43 100%);
  color: white;
}

.game-result.draw {
  background: linear-gradient(135deg, #bdc3c7 0%, #95a5a6 100%);
  color: white;
}

.game-info {
  flex: 1;
}

.game-opponent {
  font-weight: 700;
  margin-bottom: 0.25rem;
  color: #2c3e50;
  font-size: 1.1rem;
}

.game-date {
  font-size: 0.875rem;
  color: #95a5a6;
  font-weight: 500;
}

.game-score {
  font-weight: 800;
  font-size: 1.25rem;
  color: #667eea;
  padding: 0.5rem 1rem;
  background: #f5f7fa;
  border-radius: 10px;
}

.btn-view {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 10px;
  text-decoration: none;
  font-weight: 600;
  transition: all 0.3s ease;
  box-shadow: 0 4px 10px rgba(102, 126, 234, 0.3);
}

.btn-view:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(102, 126, 234, 0.4);
}

@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
    margin-bottom: 2rem;
  }

  .profile-avatar {
    width: 120px;
    height: 120px;
  }

  .profile-stats {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .game-item {
    flex-wrap: wrap;
    gap: 1rem;
  }

  .game-score {
    font-size: 1rem;
  }

  .btn-view {
    width: 100%;
    text-align: center;
  }
}

@media (max-width: 480px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }

  .stat-card {
    padding: 1.5rem 1rem;
  }
}
</style>
