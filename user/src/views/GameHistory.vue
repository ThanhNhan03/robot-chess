<template>
  <div class="game-history">
    <div class="container">
      <h1 class="page-title">📜 Game History</h1>

      <div class="filters card">
        <div class="filter-row">
          <select v-model="filterResult" class="form-control">
            <option value="all">All Results</option>
            <option value="win">Wins Only</option>
            <option value="loss">Losses Only</option>
            <option value="draw">Draws Only</option>
          </select>

          <select v-model="filterTime" class="form-control">
            <option value="all">All Time</option>
            <option value="today">Today</option>
            <option value="week">This Week</option>
            <option value="month">This Month</option>
          </select>

          <button class="btn btn-primary">🔍 Search</button>
        </div>
      </div>

      <div class="games-list">
        <div v-for="game in filteredGames" :key="game.id" class="game-card card">
          <div class="game-header">
            <div class="game-result-badge" :class="game.result">
              {{ game.result.toUpperCase() }}
            </div>
            <div class="game-date">{{ game.date }}</div>
          </div>

          <div class="game-details">
            <div class="player-info">
              <strong>You</strong> ({{ game.yourColor }})
              <span class="player-elo">ELO: {{ game.yourElo }}</span>
            </div>
            <div class="vs">VS</div>
            <div class="player-info">
              <strong>{{ game.opponent }}</strong> ({{ game.opponentColor }})
              <span class="player-elo">ELO: {{ game.opponentElo }}</span>
            </div>
          </div>

          <div class="game-stats">
            <span>⏱️ {{ game.duration }}</span>
            <span>🎯 {{ game.moves }} moves</span>
            <span>📊 {{ game.timeControl }}</span>
          </div>

          <div class="game-actions">
            <button class="btn btn-secondary">👁️ View</button>
            <button class="btn btn-secondary">📊 Analyze</button>
            <button class="btn btn-secondary">📥 Download PGN</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const filterResult = ref('all')
const filterTime = ref('all')

const games = ref([
  {
    id: 1,
    result: 'win',
    date: '2 hours ago',
    opponent: 'AI (Medium)',
    yourColor: 'White',
    opponentColor: 'Black',
    yourElo: 1650,
    opponentElo: 1600,
    duration: '15:32',
    moves: 42,
    timeControl: 'Blitz 5+0'
  },
  {
    id: 2,
    result: 'loss',
    date: '1 day ago',
    opponent: 'GrandMaster_Alex',
    yourColor: 'Black',
    opponentColor: 'White',
    yourElo: 1650,
    opponentElo: 2800,
    duration: '25:18',
    moves: 56,
    timeControl: 'Rapid 10+5'
  },
  {
    id: 3,
    result: 'draw',
    date: '2 days ago',
    opponent: 'Knight_Rider',
    yourColor: 'White',
    opponentColor: 'Black',
    yourElo: 1645,
    opponentElo: 1680,
    duration: '32:45',
    moves: 68,
    timeControl: 'Classical 30+0'
  },
])

const filteredGames = computed(() => {
  let result = games.value
  
  if (filterResult.value !== 'all') {
    result = result.filter(g => g.result === filterResult.value)
  }
  
  return result
})
</script>

<style scoped>
.game-history {
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

.filters {
  margin-bottom: 2rem;
  background: white;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
  border: none;
  animation: fadeIn 0.6s ease-out 0.2s backwards;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.filter-row {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  align-items: center;
}

.filter-row .form-control {
  flex: 1;
  min-width: 200px;
  padding: 0.875rem 1rem;
  border: 2px solid #e0e0e0;
  border-radius: 12px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: white;
}

.filter-row .form-control:hover {
  border-color: #667eea;
}

.filter-row .form-control:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.filter-row .btn-primary {
  padding: 0.875rem 2rem;
  border-radius: 12px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.filter-row .btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.5);
}

.games-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.game-card {
  padding: 1.5rem;
  background: white;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
  border: none;
  transition: all 0.3s ease;
  animation: fadeInUp 0.6s ease-out backwards;
  position: relative;
  overflow: hidden;
}

.game-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
}

.game-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.2);
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

.game-card:nth-child(1) { animation-delay: 0.1s; }
.game-card:nth-child(2) { animation-delay: 0.2s; }
.game-card:nth-child(3) { animation-delay: 0.3s; }
.game-card:nth-child(4) { animation-delay: 0.4s; }
.game-card:nth-child(5) { animation-delay: 0.5s; }

.game-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.game-result-badge {
  padding: 0.625rem 1.25rem;
  border-radius: 25px;
  font-weight: 700;
  font-size: 0.875rem;
  text-transform: uppercase;
  letter-spacing: 1px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
  transition: all 0.3s ease;
}

.game-result-badge:hover {
  transform: scale(1.05);
}

.game-result-badge.win {
  background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
  color: white;
}

.game-result-badge.loss {
  background: linear-gradient(135deg, #eb3349 0%, #f45c43 100%);
  color: white;
}

.game-result-badge.draw {
  background: linear-gradient(135deg, #bdc3c7 0%, #95a5a6 100%);
  color: white;
}

.game-date {
  color: #95a5a6;
  font-size: 0.875rem;
  font-weight: 500;
}

.game-details {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding: 1.5rem;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 12px;
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.05);
}

.player-info {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.player-info strong {
  font-size: 1.125rem;
  color: #2c3e50;
}

.player-elo {
  font-size: 0.875rem;
  color: #7f8c8d;
  font-weight: 600;
  background: white;
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  display: inline-block;
  width: fit-content;
}

.vs {
  font-weight: 800;
  font-size: 1.5rem;
  color: #667eea;
  text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.1);
}

.game-stats {
  display: flex;
  gap: 2rem;
  margin-bottom: 1.5rem;
  color: #7f8c8d;
  font-size: 0.875rem;
  font-weight: 600;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 10px;
}

.game-stats span {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.game-actions {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.game-actions .btn-secondary {
  flex: 1;
  min-width: 120px;
  padding: 0.75rem 1.5rem;
  border-radius: 10px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 10px rgba(102, 126, 234, 0.3);
}

.game-actions .btn-secondary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(102, 126, 234, 0.4);
}

.game-actions .btn-secondary:active {
  transform: translateY(0);
}

@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
    margin-bottom: 2rem;
  }

  .filter-row {
    flex-direction: column;
  }

  .filter-row .form-control,
  .filter-row .btn-primary {
    width: 100%;
    min-width: 100%;
  }

  .game-details {
    flex-direction: column;
    gap: 1rem;
  }
  
  .player-info {
    align-items: center;
    text-align: center;
  }

  .game-stats {
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .game-actions {
    flex-direction: column;
  }

  .game-actions .btn-secondary {
    width: 100%;
    min-width: 100%;
  }
}

@media (max-width: 480px) {
  .game-card {
    padding: 1rem;
  }

  .game-header {
    flex-direction: column;
    gap: 0.75rem;
    align-items: flex-start;
  }
}
</style>
