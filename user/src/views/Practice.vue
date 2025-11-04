<template>
  <div class="practice">
    <!-- Hero Section -->
    <section class="practice-hero">
      <div class="container">
        <div class="hero-content">
          <div class="hero-badge">
            <span class="badge-icon">🎯</span>
            <span>Master Chess Through Practice</span>
          </div>
          <h1 class="hero-title">Practice Arena</h1>
          <p class="hero-subtitle">
            Sharpen your skills with puzzles, drills, and opening training.<br>
            Track your progress and climb the rating ladder!
          </p>
        </div>
      </div>
    </section>

    <!-- Stats Overview -->
    <section class="stats-overview">
      <div class="container">
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-icon">🧩</div>
            <div class="stat-value">{{ totalPuzzlesSolved }}</div>
            <div class="stat-label">Puzzles Solved</div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">🔥</div>
            <div class="stat-value">{{ currentStreak }} Days</div>
            <div class="stat-label">Current Streak</div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">⭐</div>
            <div class="stat-value">{{ practiceRating }}</div>
            <div class="stat-label">Practice Rating</div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">🎖️</div>
            <div class="stat-value">{{ accuracy }}%</div>
            <div class="stat-label">Accuracy</div>
          </div>
        </div>
      </div>
    </section>

    <!-- Main Content -->
    <section class="practice-content">
      <div class="container">
        <!-- Tabs Navigation -->
        <div class="tabs-wrapper">
          <div class="tabs">
            <button 
              :class="['tab-btn', { active: activeTab === 'puzzles' }]"
              @click="activeTab = 'puzzles'"
            >
              <span class="tab-icon">🧩</span>
              <span class="tab-text">Tactical Puzzles</span>
            </button>
            <button 
              :class="['tab-btn', { active: activeTab === 'drills' }]"
              @click="activeTab = 'drills'"
            >
              <span class="tab-icon">⚡</span>
              <span class="tab-text">Speed Drills</span>
            </button>
            <button 
              :class="['tab-btn', { active: activeTab === 'openings' }]"
              @click="activeTab = 'openings'"
            >
              <span class="tab-icon">📖</span>
              <span class="tab-text">Opening Trainer</span>
            </button>
          </div>
        </div>

        <!-- Puzzles Section -->
        <div v-if="activeTab === 'puzzles'" class="puzzles-section">
          <div class="section-header">
            <h2 class="section-title">Tactical Puzzles</h2>
            <p class="section-subtitle">Find the best move in these chess positions</p>
          </div>

          <div class="filters">
            <select v-model="difficultyFilter" class="filter-select">
              <option value="all">All Difficulties</option>
              <option value="Easy">Easy</option>
              <option value="Medium">Medium</option>
              <option value="Hard">Hard</option>
              <option value="Expert">Expert</option>
            </select>
            <select v-model="themeFilter" class="filter-select">
              <option value="all">All Themes</option>
              <option value="Checkmate">Checkmate</option>
              <option value="Tactics">Tactics</option>
              <option value="Endgame">Endgame</option>
              <option value="Strategy">Strategy</option>
            </select>
          </div>

          <div class="puzzles-grid">
            <div 
              v-for="puzzle in filteredPuzzles" 
              :key="puzzle.id"
              class="puzzle-card"
            >
              <div class="puzzle-header">
                <div class="puzzle-rating">
                  <span class="rating-icon">⭐</span>
                  <span class="rating-value">{{ puzzle.rating }}</span>
                </div>
                <div :class="['puzzle-difficulty', puzzle.difficulty.toLowerCase()]">
                  {{ puzzle.difficulty }}
                </div>
              </div>
              
              <div class="puzzle-icon-large">{{ puzzle.icon }}</div>
              
              <h3 class="puzzle-title">{{ puzzle.title }}</h3>
              <p class="puzzle-description">{{ puzzle.description }}</p>
              
              <div class="puzzle-theme">
                <span class="theme-badge">{{ puzzle.theme }}</span>
              </div>
              
              <div class="puzzle-stats">
                <div class="stat-item">
                  <span class="stat-icon">✓</span>
                  <span>{{ puzzle.solved }} solved</span>
                </div>
                <div class="stat-item">
                  <span class="stat-icon">📊</span>
                  <span>{{ puzzle.successRate }}% success</span>
                </div>
              </div>
              
              <button class="puzzle-btn" @click="startPuzzle(puzzle.id)">
                {{ puzzle.completed ? '🔄 Try Again' : '▶️ Start Puzzle' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Drills Section -->
        <div v-else-if="activeTab === 'drills'" class="drills-section">
          <div class="section-header">
            <h2 class="section-title">Speed Drills</h2>
            <p class="section-subtitle">Rapid-fire tactical exercises to boost your calculation speed</p>
          </div>

          <div class="drills-grid">
            <div 
              v-for="drill in drills" 
              :key="drill.id"
              class="drill-card"
            >
              <div class="drill-icon">{{ drill.icon }}</div>
              <h3 class="drill-title">{{ drill.title }}</h3>
              <p class="drill-description">{{ drill.description }}</p>
              
              <div class="drill-info">
                <div class="info-item">
                  <div class="info-label">Exercises</div>
                  <div class="info-value">{{ drill.exercises }}</div>
                </div>
                <div class="info-item">
                  <div class="info-label">Time Limit</div>
                  <div class="info-value">{{ drill.timeLimit }}</div>
                </div>
                <div class="info-item">
                  <div class="info-label">Best Score</div>
                  <div class="info-value">{{ drill.bestScore }}%</div>
                </div>
              </div>
              
              <button class="drill-btn" @click="startDrill(drill.id)">
                ⚡ Start Drill
              </button>
            </div>
          </div>
        </div>

        <!-- Openings Section -->
        <div v-else class="openings-section">
          <div class="section-header">
            <h2 class="section-title">Opening Trainer</h2>
            <p class="section-subtitle">Learn and practice essential chess openings</p>
          </div>

          <div class="openings-grid">
            <div 
              v-for="opening in openings" 
              :key="opening.id"
              class="opening-card"
            >
              <div class="opening-color-indicator" :class="opening.side"></div>
              
              <div class="opening-icon">{{ opening.icon }}</div>
              <h3 class="opening-title">{{ opening.name }}</h3>
              <p class="opening-description">{{ opening.description }}</p>
              
              <div class="opening-stats">
                <div class="stat-row">
                  <span class="stat-label">Popularity:</span>
                  <div class="stat-bar">
                    <div class="stat-fill" :style="{ width: opening.popularity + '%' }"></div>
                  </div>
                  <span class="stat-value">{{ opening.popularity }}%</span>
                </div>
                <div class="stat-row">
                  <span class="stat-label">Win Rate:</span>
                  <div class="stat-bar">
                    <div class="stat-fill success" :style="{ width: opening.winRate + '%' }"></div>
                  </div>
                  <span class="stat-value">{{ opening.winRate }}%</span>
                </div>
              </div>
              
              <div class="opening-meta">
                <span class="meta-badge">{{ opening.variations }} variations</span>
                <span class="meta-badge">{{ opening.moves }} moves</span>
              </div>
              
              <button class="opening-btn" @click="trainOpening(opening.id)">
                📖 Study Opening
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Daily Challenge -->
    <section class="daily-challenge">
      <div class="container">
        <div class="challenge-card">
          <div class="challenge-header">
            <div class="challenge-icon">🏆</div>
            <div>
              <h3 class="challenge-title">Daily Challenge</h3>
              <p class="challenge-subtitle">Complete today's special puzzle for bonus points!</p>
            </div>
          </div>
          <div class="challenge-content">
            <div class="challenge-info">
              <div class="info-item">
                <span class="info-icon">⭐</span>
                <span>Rating: 1850</span>
              </div>
              <div class="info-item">
                <span class="info-icon">🎁</span>
                <span>Reward: +50 XP</span>
              </div>
              <div class="info-item">
                <span class="info-icon">⏰</span>
                <span>Expires in: 6h 42m</span>
              </div>
            </div>
            <button class="challenge-btn">Accept Challenge</button>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const activeTab = ref('puzzles')
const difficultyFilter = ref('all')
const themeFilter = ref('all')

// User Stats
const totalPuzzlesSolved = ref(247)
const currentStreak = ref(12)
const practiceRating = ref(1685)
const accuracy = ref(87)

// Puzzles Data
const puzzles = ref([
  {
    id: 1,
    title: 'Mate in 2',
    description: 'Find the quickest checkmate sequence in this tactical position',
    icon: '♔',
    rating: 1400,
    solved: 1250,
    successRate: 78,
    difficulty: 'Easy',
    theme: 'Checkmate',
    completed: true
  },
  {
    id: 2,
    title: 'Knight Fork',
    description: 'Win material with a devastating knight fork',
    icon: '♘',
    rating: 1600,
    solved: 890,
    successRate: 65,
    difficulty: 'Medium',
    theme: 'Tactics',
    completed: false
  },
  {
    id: 3,
    title: 'Pin & Win',
    description: 'Use a pin to gain a decisive advantage',
    icon: '♗',
    rating: 1550,
    solved: 920,
    successRate: 71,
    difficulty: 'Medium',
    theme: 'Tactics',
    completed: true
  },
  {
    id: 4,
    title: 'Brilliant Sacrifice',
    description: 'A stunning sacrifice leads to victory',
    icon: '♕',
    rating: 1900,
    solved: 450,
    successRate: 42,
    difficulty: 'Hard',
    theme: 'Strategy',
    completed: false
  },
  {
    id: 5,
    title: 'Back Rank Mate',
    description: 'Exploit back rank weakness for checkmate',
    icon: '♜',
    rating: 1500,
    solved: 1100,
    successRate: 82,
    difficulty: 'Easy',
    theme: 'Checkmate',
    completed: false
  },
  {
    id: 6,
    title: 'Endgame Mastery',
    description: 'Convert your advantage to a win in the endgame',
    icon: '♚',
    rating: 2000,
    solved: 380,
    successRate: 38,
    difficulty: 'Expert',
    theme: 'Endgame',
    completed: false
  },
  {
    id: 7,
    title: 'Double Attack',
    description: 'Win material through a powerful double attack',
    icon: '⚔️',
    rating: 1650,
    solved: 760,
    successRate: 68,
    difficulty: 'Medium',
    theme: 'Tactics',
    completed: true
  },
  {
    id: 8,
    title: 'Discovered Attack',
    description: 'Master the art of discovered attacks',
    icon: '💥',
    rating: 1750,
    solved: 620,
    successRate: 55,
    difficulty: 'Hard',
    theme: 'Tactics',
    completed: false
  },
])

// Drills Data
const drills = ref([
  {
    id: 1,
    title: 'Pattern Recognition Sprint',
    description: 'Rapidly identify tactical patterns in 20 positions',
    icon: '⚡',
    exercises: 20,
    timeLimit: '5 min',
    bestScore: 85
  },
  {
    id: 2,
    title: 'Calculation Training',
    description: 'Improve your calculation depth and accuracy',
    icon: '🧠',
    exercises: 15,
    timeLimit: '10 min',
    bestScore: 72
  },
  {
    id: 3,
    title: 'Blitz Tactics',
    description: 'Solve tactical puzzles under time pressure',
    icon: '⏱️',
    exercises: 30,
    timeLimit: '3 min',
    bestScore: 91
  },
  {
    id: 4,
    title: 'Endgame Scenarios',
    description: 'Practice essential endgame techniques',
    icon: '🏁',
    exercises: 12,
    timeLimit: '8 min',
    bestScore: 67
  },
])

// Openings Data
const openings = ref([
  {
    id: 1,
    name: 'Italian Game',
    description: 'Classic opening focusing on rapid development and center control',
    icon: '🇮🇹',
    side: 'white',
    popularity: 85,
    winRate: 52,
    variations: 12,
    moves: 8
  },
  {
    id: 2,
    name: 'Sicilian Defense',
    description: 'Aggressive counter-attacking opening for Black',
    icon: '⚔️',
    side: 'black',
    popularity: 92,
    winRate: 48,
    variations: 24,
    moves: 10
  },
  {
    id: 3,
    name: "Queen's Gambit",
    description: 'Solid opening offering central pawn majority',
    icon: '♕',
    side: 'white',
    popularity: 78,
    winRate: 54,
    variations: 16,
    moves: 9
  },
  {
    id: 4,
    name: 'French Defense',
    description: 'Solid defensive setup with counterattacking potential',
    icon: '🇫🇷',
    side: 'black',
    popularity: 65,
    winRate: 47,
    variations: 14,
    moves: 8
  },
  {
    id: 5,
    name: 'Ruy López',
    description: 'One of the oldest and most classical openings',
    icon: '🇪🇸',
    side: 'white',
    popularity: 88,
    winRate: 53,
    variations: 20,
    moves: 11
  },
  {
    id: 6,
    name: 'King\'s Indian Defense',
    description: 'Hypermodern opening with aggressive kingside play',
    icon: '🐅',
    side: 'black',
    popularity: 70,
    winRate: 49,
    variations: 18,
    moves: 12
  },
])

// Computed Properties
const filteredPuzzles = computed(() => {
  return puzzles.value.filter(puzzle => {
    const difficultyMatch = difficultyFilter.value === 'all' || puzzle.difficulty === difficultyFilter.value
    const themeMatch = themeFilter.value === 'all' || puzzle.theme === themeFilter.value
    return difficultyMatch && themeMatch
  })
})

// Methods
const startPuzzle = (puzzleId) => {
  console.log('Starting puzzle:', puzzleId)
  // Navigate to puzzle solver
}

const startDrill = (drillId) => {
  console.log('Starting drill:', drillId)
  // Navigate to drill trainer
}

const trainOpening = (openingId) => {
  console.log('Training opening:', openingId)
  // Navigate to opening trainer
}
</script>

<style scoped>
.practice {
  background: linear-gradient(180deg, #f5f7fa 0%, #ffffff 100%);
}

/* Hero Section */
.practice-hero {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 4rem 0 3rem;
  text-align: center;
}

.hero-content {
  max-width: 800px;
  margin: 0 auto;
}

.hero-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
  padding: 0.75rem 1.5rem;
  border-radius: 50px;
  font-weight: 600;
  margin-bottom: 1.5rem;
  border: 2px solid rgba(255, 255, 255, 0.3);
}

.badge-icon {
  font-size: 1.2rem;
}

.hero-title {
  font-size: 3rem;
  margin-bottom: 1rem;
  text-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
}

.hero-subtitle {
  font-size: 1.2rem;
  opacity: 0.95;
  line-height: 1.8;
}

/* Stats Overview */
.stats-overview {
  padding: 2rem 0;
  margin-top: -3rem;
  position: relative;
  z-index: 10;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
}

.stat-card {
  background: white;
  padding: 2rem;
  border-radius: 15px;
  text-align: center;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 15px 40px rgba(102, 126, 234, 0.2);
}

.stat-icon {
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.stat-value {
  font-size: 2.5rem;
  font-weight: 800;
  color: var(--primary-color);
  margin-bottom: 0.25rem;
}

.stat-label {
  color: var(--text-light);
  font-size: 0.9rem;
  text-transform: uppercase;
  letter-spacing: 1px;
}

/* Practice Content */
.practice-content {
  padding: 4rem 0;
}

/* Tabs */
.tabs-wrapper {
  margin-bottom: 3rem;
}

.tabs {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

.tab-btn {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 2rem;
  border: 2px solid var(--border-color);
  background: white;
  border-radius: 12px;
  cursor: pointer;
  font-weight: 600;
  font-size: 1rem;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.tab-btn:hover {
  border-color: var(--primary-color);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);
}

.tab-btn.active {
  background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  color: white;
  border-color: transparent;
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.3);
}

.tab-icon {
  font-size: 1.5rem;
}

/* Section Header */
.section-header {
  text-align: center;
  margin-bottom: 3rem;
}

.section-title {
  font-size: 2.5rem;
  color: var(--text-color);
  margin-bottom: 0.5rem;
}

.section-subtitle {
  font-size: 1.1rem;
  color: var(--text-light);
}

/* Filters */
.filters {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-bottom: 3rem;
  flex-wrap: wrap;
}

.filter-select {
  padding: 0.75rem 1.5rem;
  border: 2px solid var(--border-color);
  border-radius: 10px;
  font-size: 1rem;
  font-weight: 600;
  background: white;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-select:hover,
.filter-select:focus {
  border-color: var(--primary-color);
  outline: none;
}

/* Puzzles Grid */
.puzzles-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 2rem;
}

.puzzle-card {
  background: white;
  border-radius: 20px;
  padding: 2rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  border: 2px solid transparent;
  position: relative;
  overflow: hidden;
}

.puzzle-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, var(--primary-color), var(--secondary-color));
  transform: scaleX(0);
  transition: transform 0.3s ease;
}

.puzzle-card:hover::before {
  transform: scaleX(1);
}

.puzzle-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 40px rgba(102, 126, 234, 0.2);
  border-color: var(--primary-color);
}

.puzzle-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.puzzle-rating {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: linear-gradient(135deg, #f6ad55, #ed8936);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 10px;
  font-weight: 700;
}

.puzzle-difficulty {
  padding: 0.5rem 1rem;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.875rem;
}

.puzzle-difficulty.easy {
  background: #c6f6d5;
  color: #22543d;
}

.puzzle-difficulty.medium {
  background: #feebc8;
  color: #7c2d12;
}

.puzzle-difficulty.hard {
  background: #fed7d7;
  color: #742a2a;
}

.puzzle-difficulty.expert {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
}

.puzzle-icon-large {
  font-size: 4rem;
  text-align: center;
  margin-bottom: 1rem;
}

.puzzle-title {
  font-size: 1.4rem;
  margin-bottom: 0.75rem;
  color: var(--text-color);
}

.puzzle-description {
  color: var(--text-light);
  line-height: 1.6;
  margin-bottom: 1rem;
}

.puzzle-theme {
  margin-bottom: 1rem;
}

.theme-badge {
  display: inline-block;
  padding: 0.5rem 1rem;
  background: rgba(102, 126, 234, 0.1);
  color: var(--primary-color);
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 600;
}

.puzzle-stats {
  display: flex;
  gap: 1.5rem;
  padding: 1rem 0;
  border-top: 1px solid var(--border-color);
  border-bottom: 1px solid var(--border-color);
  margin-bottom: 1.5rem;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: var(--text-light);
}

.puzzle-btn {
  width: 100%;
  padding: 1rem;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  color: white;
  font-size: 1rem;
}

.puzzle-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.3);
}

/* Drills Grid */
.drills-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 2rem;
}

.drill-card {
  background: white;
  border-radius: 20px;
  padding: 2.5rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  text-align: center;
  border: 2px solid transparent;
}

.drill-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 40px rgba(102, 126, 234, 0.2);
  border-color: var(--primary-color);
}

.drill-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.drill-title {
  font-size: 1.5rem;
  margin-bottom: 0.75rem;
  color: var(--text-color);
}

.drill-description {
  color: var(--text-light);
  line-height: 1.6;
  margin-bottom: 2rem;
}

.drill-info {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-bottom: 2rem;
  padding: 1.5rem;
  background: rgba(102, 126, 234, 0.05);
  border-radius: 12px;
}

.info-item {
  text-align: center;
}

.info-label {
  font-size: 0.875rem;
  color: var(--text-light);
  margin-bottom: 0.5rem;
}

.info-value {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--primary-color);
}

.drill-btn {
  width: 100%;
  padding: 1rem 2rem;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  background: linear-gradient(135deg, #f6ad55, #ed8936);
  color: white;
}

.drill-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(237, 137, 54, 0.3);
}

/* Openings Grid */
.openings-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 2rem;
}

.opening-card {
  background: white;
  border-radius: 20px;
  padding: 2rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  border: 2px solid transparent;
  position: relative;
  overflow: hidden;
}

.opening-color-indicator {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 6px;
}

.opening-color-indicator.white {
  background: linear-gradient(90deg, #f7fafc, #e2e8f0);
}

.opening-color-indicator.black {
  background: linear-gradient(90deg, #2d3748, #1a202c);
}

.opening-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 40px rgba(102, 126, 234, 0.2);
  border-color: var(--primary-color);
}

.opening-icon {
  font-size: 3.5rem;
  text-align: center;
  margin: 1rem 0;
}

.opening-title {
  font-size: 1.5rem;
  margin-bottom: 0.75rem;
  color: var(--text-color);
  text-align: center;
}

.opening-description {
  color: var(--text-light);
  line-height: 1.6;
  margin-bottom: 2rem;
  text-align: center;
}

.opening-stats {
  margin-bottom: 1.5rem;
}

.stat-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.stat-label {
  font-weight: 600;
  color: var(--text-color);
  min-width: 90px;
}

.stat-bar {
  flex: 1;
  height: 10px;
  background: var(--border-color);
  border-radius: 10px;
  overflow: hidden;
}

.stat-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--primary-color), var(--secondary-color));
  border-radius: 10px;
  transition: width 0.5s ease;
}

.stat-fill.success {
  background: linear-gradient(90deg, #48bb78, #38a169);
}

.stat-value {
  font-weight: 700;
  color: var(--primary-color);
  min-width: 50px;
  text-align: right;
}

.opening-meta {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-bottom: 1.5rem;
}

.meta-badge {
  padding: 0.5rem 1rem;
  background: rgba(102, 126, 234, 0.1);
  color: var(--primary-color);
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 600;
}

.opening-btn {
  width: 100%;
  padding: 1rem;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  color: white;
}

.opening-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.3);
}

/* Daily Challenge */
.daily-challenge {
  padding: 4rem 0;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.05), rgba(118, 75, 162, 0.05));
}

.challenge-card {
  background: white;
  border-radius: 20px;
  padding: 2.5rem;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  border: 3px solid transparent;
  background-image: linear-gradient(white, white), 
                    linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  background-origin: border-box;
  background-clip: padding-box, border-box;
}

.challenge-header {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.challenge-icon {
  font-size: 4rem;
  animation: bounce 2s infinite;
}

@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

.challenge-title {
  font-size: 2rem;
  margin-bottom: 0.5rem;
  color: var(--text-color);
}

.challenge-subtitle {
  color: var(--text-light);
  font-size: 1.1rem;
}

.challenge-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 2rem;
  flex-wrap: wrap;
}

.challenge-info {
  display: flex;
  gap: 2rem;
  flex-wrap: wrap;
}

.challenge-info .info-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: rgba(102, 126, 234, 0.05);
  border-radius: 10px;
  font-weight: 600;
}

.info-icon {
  font-size: 1.25rem;
}

.challenge-btn {
  padding: 1rem 3rem;
  border: none;
  border-radius: 50px;
  font-weight: 700;
  font-size: 1.1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  background: linear-gradient(135deg, #f6ad55, #ed8936);
  color: white;
  box-shadow: 0 8px 20px rgba(237, 137, 54, 0.3);
}

.challenge-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 30px rgba(237, 137, 54, 0.4);
}

/* Responsive */
@media (max-width: 768px) {
  .hero-title {
    font-size: 2rem;
  }
  
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .tabs {
    flex-direction: column;
  }
  
  .tab-btn {
    width: 100%;
    justify-content: center;
  }
  
  .filters {
    flex-direction: column;
  }
  
  .filter-select {
    width: 100%;
  }
  
  .puzzles-grid,
  .drills-grid,
  .openings-grid {
    grid-template-columns: 1fr;
  }
  
  .challenge-content {
    flex-direction: column;
  }
  
  .challenge-info {
    width: 100%;
    flex-direction: column;
  }
  
  .challenge-btn {
    width: 100%;
  }
  
  .drill-info {
    grid-template-columns: 1fr;
  }
}
</style>