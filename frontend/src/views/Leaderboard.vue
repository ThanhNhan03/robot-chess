<template>
  <div class="leaderboard">
    <div class="container">
      <!-- Hero Section -->
      <div class="leaderboard-hero">
        <h1 class="page-title">
          <span class="trophy-icon">🏆</span>
          Global Leaderboard
        </h1>
        <p class="subtitle">Compete with the best chess players worldwide</p>
      </div>
      
      <!-- Filter Tabs -->
      <div class="tabs">
        <button 
          :class="['tab', { active: activeTab === 'global' }]"
          @click="activeTab = 'global'"
        >
          <span class="tab-icon">🌍</span>
          <span>Global</span>
        </button>
        <button 
          :class="['tab', { active: activeTab === 'friends' }]"
          @click="activeTab = 'friends'"
        >
          <span class="tab-icon">👥</span>
          <span>Friends</span>
        </button>
        <button 
          :class="['tab', { active: activeTab === 'region' }]"
          @click="activeTab = 'region'"
        >
          <span class="tab-icon">📍</span>
          <span>Region</span>
        </button>
      </div>

      <!-- Top 3 Podium -->
      <div class="podium">
        <div class="podium-item podium-second" v-if="filteredPlayers[1]">
          <div class="podium-rank">
            <span class="medal-large">🥈</span>
          </div>
          <img :src="filteredPlayers[1].avatar" class="podium-avatar" />
          <h3 class="podium-name">{{ filteredPlayers[1].name }}</h3>
          <div class="podium-elo">{{ filteredPlayers[1].elo }} ELO</div>
          <div class="podium-base rank-2">2nd</div>
        </div>

        <div class="podium-item podium-first" v-if="filteredPlayers[0]">
          <div class="podium-rank">
            <span class="medal-large crown">👑</span>
          </div>
          <img :src="filteredPlayers[0].avatar" class="podium-avatar" />
          <h3 class="podium-name">{{ filteredPlayers[0].name }}</h3>
          <div class="podium-elo">{{ filteredPlayers[0].elo }} ELO</div>
          <div class="podium-base rank-1">1st</div>
        </div>

        <div class="podium-item podium-third" v-if="filteredPlayers[2]">
          <div class="podium-rank">
            <span class="medal-large">🥉</span>
          </div>
          <img :src="filteredPlayers[2].avatar" class="podium-avatar" />
          <h3 class="podium-name">{{ filteredPlayers[2].name }}</h3>
          <div class="podium-elo">{{ filteredPlayers[2].elo }} ELO</div>
          <div class="podium-base rank-3">3rd</div>
        </div>
      </div>

      <!-- Leaderboard Table -->
      <div class="card leaderboard-card">
        <div class="card-header">
          <h2>All Rankings</h2>
        </div>

        <div v-if="loading" class="loading-container">
          <div class="loading"></div>
          <p>Loading leaderboard...</p>
        </div>

        <div v-else class="leaderboard-body">
          <div 
            v-for="(player, index) in filteredPlayers" 
            :key="player.id"
            :class="['leaderboard-row', { 'current-user': player.isCurrentUser, 'top-three': index < 3 }]"
          >
            <span class="rank-col">
              <span v-if="index === 0" class="rank-badge gold">1</span>
              <span v-else-if="index === 1" class="rank-badge silver">2</span>
              <span v-else-if="index === 2" class="rank-badge bronze">3</span>
              <span v-else class="rank-number">{{ index + 1 }}</span>
            </span>
            <span class="player-col">
              <img :src="player.avatar" :alt="player.name" class="avatar" />
              <div class="player-info">
                <span class="player-name">{{ player.name }}</span>
                <span v-if="player.isCurrentUser" class="badge-you">You</span>
              </div>
            </span>
            <span class="elo-col">
              <span class="elo-value">{{ player.elo }}</span>
              <span class="elo-label">ELO</span>
            </span>
            <span class="games-col">
              <span class="games-value">{{ player.gamesPlayed }}</span>
              <span class="games-label">Games</span>
            </span>
            <span class="winrate-col">
              <div class="winrate-display">
                <div class="progress-bar">
                  <div 
                    class="progress-fill" 
                    :style="{ width: player.winRate + '%' }"
                    :class="getWinRateClass(player.winRate)"
                  ></div>
                </div>
                <span class="percentage">{{ player.winRate }}%</span>
              </div>
            </span>
          </div>
        </div>
      </div>

      <!-- User Stats Card -->
      <div class="user-stats-card">
        <div class="stats-header">
          <h3>📊 Your Performance</h3>
        </div>
        <div class="stats-grid">
          <div class="stat-item">
            <div class="stat-icon">🎯</div>
            <div class="stat-content">
              <div class="stat-label">Current Rank</div>
              <div class="stat-value">#{{ userRank }}</div>
            </div>
          </div>
          <div class="stat-item">
            <div class="stat-icon">⭐</div>
            <div class="stat-content">
              <div class="stat-label">ELO Rating</div>
              <div class="stat-value">{{ userElo }}</div>
            </div>
          </div>
          <div class="stat-item">
            <div class="stat-icon">🎮</div>
            <div class="stat-content">
              <div class="stat-label">Games Played</div>
              <div class="stat-value">{{ userGames }}</div>
            </div>
          </div>
          <div class="stat-item">
            <div class="stat-icon">🏅</div>
            <div class="stat-content">
              <div class="stat-label">Win Rate</div>
              <div class="stat-value">{{ userWinRate }}%</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const activeTab = ref('global')
const loading = ref(true)

// Mock data - Replace with API call
const players = ref([
  { id: 1, name: 'GrandMaster_Alex', elo: 2800, gamesPlayed: 1250, winRate: 85, avatar: 'https://i.pravatar.cc/150?img=1', isCurrentUser: false },
  { id: 2, name: 'ChessQueen_Lisa', elo: 2750, gamesPlayed: 980, winRate: 82, avatar: 'https://i.pravatar.cc/150?img=2', isCurrentUser: false },
  { id: 3, name: 'Knight_Rider', elo: 2700, gamesPlayed: 850, winRate: 79, avatar: 'https://i.pravatar.cc/150?img=3', isCurrentUser: false },
  { id: 4, name: 'You', elo: 2650, gamesPlayed: 500, winRate: 75, avatar: 'https://i.pravatar.cc/150?img=4', isCurrentUser: true },
  { id: 5, name: 'Pawn_Star', elo: 2600, gamesPlayed: 750, winRate: 73, avatar: 'https://i.pravatar.cc/150?img=5', isCurrentUser: false },
  { id: 6, name: 'Bishop_Boss', elo: 2550, gamesPlayed: 680, winRate: 71, avatar: 'https://i.pravatar.cc/150?img=6', isCurrentUser: false },
  { id: 7, name: 'Rook_Master', elo: 2500, gamesPlayed: 620, winRate: 68, avatar: 'https://i.pravatar.cc/150?img=7', isCurrentUser: false },
  { id: 8, name: 'King_Kong', elo: 2450, gamesPlayed: 580, winRate: 65, avatar: 'https://i.pravatar.cc/150?img=8', isCurrentUser: false },
])

const filteredPlayers = computed(() => {
  // Filter based on active tab
  return players.value
})

const userRank = computed(() => {
  const index = players.value.findIndex(p => p.isCurrentUser)
  return index + 1
})

const userElo = computed(() => {
  const user = players.value.find(p => p.isCurrentUser)
  return user?.elo || 1200
})

const userGames = computed(() => {
  const user = players.value.find(p => p.isCurrentUser)
  return user?.gamesPlayed || 0
})

const userWinRate = computed(() => {
  const user = players.value.find(p => p.isCurrentUser)
  return user?.winRate || 0
})

const getWinRateClass = (rate) => {
  if (rate >= 80) return 'excellent'
  if (rate >= 60) return 'good'
  return 'average'
}

onMounted(() => {
  // Simulate API call
  setTimeout(() => {
    loading.value = false
  }, 1000)
})
</script>

<style scoped>
.leaderboard {
  padding: 2rem 0;
  min-height: 80vh;
  background: linear-gradient(180deg, #f5f7fa 0%, #ffffff 100%);
}

.leaderboard-hero {
  text-align: center;
  padding: 3rem 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 20px;
  margin-bottom: 3rem;
  color: white;
  box-shadow: 0 10px 40px rgba(102, 126, 234, 0.3);
}

.trophy-icon {
  font-size: 4rem;
  display: block;
  margin-bottom: 1rem;
  animation: bounce 2s infinite;
}

@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-20px); }
}

.page-title {
  font-size: 3rem;
  margin-bottom: 0.5rem;
  text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
}

.subtitle {
  font-size: 1.2rem;
  opacity: 0.95;
}

.tabs {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-bottom: 3rem;
  flex-wrap: wrap;
}

.tab {
  padding: 1rem 2rem;
  border: none;
  background: white;
  border-radius: 12px;
  cursor: pointer;
  font-weight: 600;
  font-size: 1rem;
  transition: all 0.3s ease;
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.tab-icon {
  font-size: 1.2rem;
}

.tab:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 12px rgba(0,0,0,0.15);
}

.tab.active {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
}

/* Podium Styles */
.podium {
  display: flex;
  justify-content: center;
  align-items: flex-end;
  gap: 2rem;
  margin-bottom: 3rem;
  padding: 2rem 0;
}

.podium-item {
  text-align: center;
  position: relative;
  animation: fadeInUp 0.6s ease;
}

.podium-first {
  order: 2;
  animation-delay: 0.2s;
}

.podium-second {
  order: 1;
  animation-delay: 0s;
}

.podium-third {
  order: 3;
  animation-delay: 0.4s;
}

.medal-large {
  font-size: 3rem;
  display: block;
  margin-bottom: 1rem;
}

.crown {
  animation: rotate 3s ease-in-out infinite;
}

@keyframes rotate {
  0%, 100% { transform: rotate(-10deg); }
  50% { transform: rotate(10deg); }
}

.podium-avatar {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  border: 4px solid white;
  box-shadow: 0 8px 16px rgba(0,0,0,0.2);
  margin-bottom: 1rem;
  object-fit: cover;
}

.podium-first .podium-avatar {
  width: 120px;
  height: 120px;
  border-color: #FFD700;
}

.podium-name {
  font-size: 1.1rem;
  margin-bottom: 0.5rem;
  color: var(--text-color);
}

.podium-elo {
  font-weight: 700;
  color: var(--primary-color);
  font-size: 1.2rem;
  margin-bottom: 1rem;
}

.podium-base {
  padding: 1.5rem;
  border-radius: 8px 8px 0 0;
  font-weight: 700;
  font-size: 1.5rem;
  color: white;
}

.rank-1 {
  background: linear-gradient(135deg, #FFD700, #FFA500);
  height: 150px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.rank-2 {
  background: linear-gradient(135deg, #C0C0C0, #808080);
  height: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.rank-3 {
  background: linear-gradient(135deg, #CD7F32, #8B4513);
  height: 100px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Leaderboard Card */
.leaderboard-card {
  overflow: hidden;
  margin-bottom: 2rem;
  box-shadow: 0 10px 30px rgba(0,0,0,0.1);
}

.card-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 1.5rem;
  text-align: center;
}

.card-header h2 {
  margin: 0;
  color: white;
  font-size: 1.8rem;
}

.leaderboard-body {
  padding: 1rem;
}

.leaderboard-row {
  display: grid;
  grid-template-columns: 80px 1fr 120px 120px 180px;
  gap: 1rem;
  padding: 1.5rem 1rem;
  align-items: center;
  border-bottom: 1px solid var(--border-color);
  transition: all 0.3s ease;
  border-radius: 8px;
  margin-bottom: 0.5rem;
}

.leaderboard-row:hover {
  background: linear-gradient(90deg, rgba(102, 126, 234, 0.05), transparent);
  transform: translateX(5px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
}

.leaderboard-row.current-user {
  background: linear-gradient(90deg, rgba(102, 126, 234, 0.15), rgba(118, 75, 162, 0.1));
  border: 2px solid var(--primary-color);
  font-weight: 600;
  box-shadow: 0 4px 16px rgba(102, 126, 234, 0.2);
}

.leaderboard-row.top-three {
  background: linear-gradient(90deg, rgba(255, 215, 0, 0.1), transparent);
}

.rank-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  font-weight: 700;
  font-size: 1.2rem;
  color: white;
  box-shadow: 0 4px 8px rgba(0,0,0,0.2);
}

.rank-badge.gold {
  background: linear-gradient(135deg, #FFD700, #FFA500);
}

.rank-badge.silver {
  background: linear-gradient(135deg, #C0C0C0, #808080);
}

.rank-badge.bronze {
  background: linear-gradient(135deg, #CD7F32, #8B4513);
}

.rank-number {
  font-weight: 600;
  font-size: 1.2rem;
  color: var(--text-light);
}

.player-col {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid var(--border-color);
  transition: all 0.3s ease;
}

.leaderboard-row:hover .avatar {
  border-color: var(--primary-color);
  transform: scale(1.1);
}

.player-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.player-name {
  font-weight: 600;
  font-size: 1.1rem;
}

.badge-you {
  background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 700;
  width: fit-content;
}

.elo-col, .games-col {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.elo-value, .games-value {
  font-weight: 700;
  font-size: 1.3rem;
  color: var(--primary-color);
}

.elo-label, .games-label {
  font-size: 0.75rem;
  color: var(--text-light);
  text-transform: uppercase;
}

.winrate-display {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: 100%;
}

.progress-bar {
  flex: 1;
  height: 12px;
  background: var(--border-color);
  border-radius: 20px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: 20px;
  transition: width 0.5s ease;
}

.progress-fill.excellent {
  background: linear-gradient(90deg, #48bb78, #38a169);
}

.progress-fill.good {
  background: linear-gradient(90deg, var(--primary-color), var(--secondary-color));
}

.progress-fill.average {
  background: linear-gradient(90deg, var(--warning-color), #ed8936);
}

.percentage {
  font-weight: 700;
  font-size: 1.1rem;
  min-width: 50px;
  text-align: right;
}

.loading-container {
  text-align: center;
  padding: 4rem;
}

/* User Stats Card */
.user-stats-card {
  background: white;
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 10px 30px rgba(0,0,0,0.1);
}

.stats-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 1.5rem;
  text-align: center;
}

.stats-header h3 {
  margin: 0;
  color: white;
  font-size: 1.5rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 0;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 2rem;
  border-right: 1px solid var(--border-color);
  border-bottom: 1px solid var(--border-color);
  transition: all 0.3s ease;
}

.stat-item:hover {
  background: var(--light-bg);
  transform: scale(1.05);
  z-index: 1;
}

.stat-icon {
  font-size: 2.5rem;
}

.stat-content {
  flex: 1;
}

.stat-label {
  font-size: 0.875rem;
  color: var(--text-light);
  margin-bottom: 0.25rem;
  text-transform: uppercase;
  font-weight: 600;
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  color: var(--primary-color);
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 1024px) {
  .leaderboard-row {
    grid-template-columns: 60px 1fr 100px 100px 140px;
    padding: 1rem 0.5rem;
  }
  
  .podium {
    flex-direction: column;
    align-items: center;
  }
  
  .podium-first, .podium-second, .podium-third {
    order: 0;
  }
}

@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
  }
  
  .leaderboard-row {
    grid-template-columns: 50px 1fr 80px;
  }
  
  .games-col,
  .winrate-col {
    display: none;
  }
  
  .tabs {
    flex-direction: column;
    padding: 0 1rem;
  }
  
  .tab {
    width: 100%;
  }
  
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .stat-item {
    border-right: none;
  }
  
  .podium-avatar {
    width: 80px;
    height: 80px;
  }
  
  .podium-first .podium-avatar {
    width: 100px;
    height: 100px;
  }
}
</style>
