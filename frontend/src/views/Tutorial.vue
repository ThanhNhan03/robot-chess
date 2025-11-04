<template>
  <div class="tutorial">
    <!-- Hero Section -->
    <section class="tutorial-hero">
      <div class="container">
        <div class="hero-content">
          <div class="hero-badge">
            <span class="badge-icon">📚</span>
            <span>Learn Chess</span>
          </div>
          <h1 class="hero-title">Master Chess from Beginner to Pro</h1>
          <p class="hero-subtitle">
            Comprehensive tutorials and lessons to help you become a chess master.
            <br>
            Learn at your own pace with interactive examples and exercises.
          </p>
        </div>
      </div>
    </section>

    <!-- Quick Stats -->
    <section class="quick-stats">
      <div class="container">
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-icon">🎯</div>
            <div class="stat-value">50+</div>
            <div class="stat-label">Lessons</div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">⏱️</div>
            <div class="stat-value">20h</div>
            <div class="stat-label">Content</div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">🏆</div>
            <div class="stat-value">100+</div>
            <div class="stat-label">Exercises</div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">👥</div>
            <div class="stat-value">10K+</div>
            <div class="stat-label">Students</div>
          </div>
        </div>
      </div>
    </section>

    <!-- Tutorial Categories -->
    <section class="tutorial-content">
      <div class="container">
        <div class="section-header">
          <h2 class="section-title">Learning Path</h2>
          <p class="section-subtitle">Choose your level and start learning</p>
        </div>

        <!-- Beginner Level -->
        <div class="level-section">
          <div class="level-header">
            <div class="level-badge beginner">
              <span class="level-icon">🌱</span>
              <span class="level-name">Beginner</span>
            </div>
            <p class="level-description">Perfect for those new to chess</p>
          </div>
          <div class="lessons-grid">
            <div class="lesson-card" v-for="lesson in beginnerLessons" :key="lesson.id">
              <div class="lesson-status" :class="lesson.completed ? 'completed' : 'locked'">
                <span v-if="lesson.completed">✓</span>
                <span v-else>🔒</span>
              </div>
              <div class="lesson-icon">{{ lesson.icon }}</div>
              <h3 class="lesson-title">{{ lesson.title }}</h3>
              <p class="lesson-description">{{ lesson.description }}</p>
              <div class="lesson-meta">
                <span class="lesson-duration">⏱️ {{ lesson.duration }}</span>
                <span class="lesson-exercises">📝 {{ lesson.exercises }} exercises</span>
              </div>
              <button class="lesson-btn" :disabled="lesson.locked">
                {{ lesson.locked ? 'Locked' : lesson.completed ? 'Review' : 'Start Lesson' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Intermediate Level -->
        <div class="level-section">
          <div class="level-header">
            <div class="level-badge intermediate">
              <span class="level-icon">📈</span>
              <span class="level-name">Intermediate</span>
            </div>
            <p class="level-description">Build on your foundations</p>
          </div>
          <div class="lessons-grid">
            <div class="lesson-card" v-for="lesson in intermediateLessons" :key="lesson.id">
              <div class="lesson-status" :class="lesson.completed ? 'completed' : 'locked'">
                <span v-if="lesson.completed">✓</span>
                <span v-else>🔒</span>
              </div>
              <div class="lesson-icon">{{ lesson.icon }}</div>
              <h3 class="lesson-title">{{ lesson.title }}</h3>
              <p class="lesson-description">{{ lesson.description }}</p>
              <div class="lesson-meta">
                <span class="lesson-duration">⏱️ {{ lesson.duration }}</span>
                <span class="lesson-exercises">� {{ lesson.exercises }} exercises</span>
              </div>
              <button class="lesson-btn" :disabled="lesson.locked">
                {{ lesson.locked ? 'Locked' : lesson.completed ? 'Review' : 'Start Lesson' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Advanced Level -->
        <div class="level-section">
          <div class="level-header">
            <div class="level-badge advanced">
              <span class="level-icon">🚀</span>
              <span class="level-name">Advanced</span>
            </div>
            <p class="level-description">Master advanced techniques</p>
          </div>
          <div class="lessons-grid">
            <div class="lesson-card" v-for="lesson in advancedLessons" :key="lesson.id">
              <div class="lesson-status" :class="lesson.completed ? 'completed' : 'locked'">
                <span v-if="lesson.completed">✓</span>
                <span v-else>🔒</span>
              </div>
              <div class="lesson-icon">{{ lesson.icon }}</div>
              <h3 class="lesson-title">{{ lesson.title }}</h3>
              <p class="lesson-description">{{ lesson.description }}</p>
              <div class="lesson-meta">
                <span class="lesson-duration">⏱️ {{ lesson.duration }}</span>
                <span class="lesson-exercises">📝 {{ lesson.exercises }} exercises</span>
              </div>
              <button class="lesson-btn" :disabled="lesson.locked">
                {{ lesson.locked ? 'Locked' : lesson.completed ? 'Review' : 'Start Lesson' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Progress Section -->
    <section class="progress-section">
      <div class="container">
        <div class="progress-card">
          <h3>Your Learning Progress</h3>
          <div class="progress-stats">
            <div class="progress-item">
              <div class="progress-label">Lessons Completed</div>
              <div class="progress-bar-wrapper">
                <div class="progress-bar">
                  <div class="progress-fill" style="width: 35%"></div>
                </div>
                <span class="progress-value">7/20</span>
              </div>
            </div>
            <div class="progress-item">
              <div class="progress-label">Exercises Done</div>
              <div class="progress-bar-wrapper">
                <div class="progress-bar">
                  <div class="progress-fill" style="width: 60%"></div>
                </div>
                <span class="progress-value">30/50</span>
              </div>
            </div>
            <div class="progress-item">
              <div class="progress-label">Overall Progress</div>
              <div class="progress-bar-wrapper">
                <div class="progress-bar">
                  <div class="progress-fill" style="width: 45%"></div>
                </div>
                <span class="progress-value">45%</span>
              </div>
            </div>
          </div>
          <RouterLink to="/practice" class="cta-btn">
            Start Practicing →
          </RouterLink>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const beginnerLessons = ref([
  {
    id: 1,
    title: 'Chess Board Basics',
    description: 'Learn about the chess board, squares, and coordinates',
    icon: '♟️',
    duration: '15 min',
    exercises: 5,
    completed: true,
    locked: false
  },
  {
    id: 2,
    title: 'Piece Movement',
    description: 'How each chess piece moves and captures',
    icon: '♞',
    duration: '20 min',
    exercises: 8,
    completed: true,
    locked: false
  },
  {
    id: 3,
    title: 'Special Moves',
    description: 'Castling, en passant, and pawn promotion',
    icon: '👑',
    duration: '25 min',
    exercises: 10,
    completed: false,
    locked: false
  },
  {
    id: 4,
    title: 'Check & Checkmate',
    description: 'Understanding check, checkmate, and stalemate',
    icon: '🎯',
    duration: '30 min',
    exercises: 12,
    completed: false,
    locked: false
  }
])

const intermediateLessons = ref([
  {
    id: 5,
    title: 'Opening Principles',
    description: 'Key principles for the opening phase',
    icon: '🌅',
    duration: '35 min',
    exercises: 15,
    completed: false,
    locked: false
  },
  {
    id: 6,
    title: 'Tactical Patterns',
    description: 'Forks, pins, skewers, and discovered attacks',
    icon: '⚔️',
    duration: '40 min',
    exercises: 20,
    completed: false,
    locked: false
  },
  {
    id: 7,
    title: 'Middlegame Strategy',
    description: 'Planning and strategic concepts',
    icon: '🎲',
    duration: '45 min',
    exercises: 18,
    completed: false,
    locked: true
  },
  {
    id: 8,
    title: 'Endgame Basics',
    description: 'Essential endgame techniques',
    icon: '🏁',
    duration: '40 min',
    exercises: 16,
    completed: false,
    locked: true
  }
])

const advancedLessons = ref([
  {
    id: 9,
    title: 'Advanced Tactics',
    description: 'Complex tactical combinations',
    icon: '💎',
    duration: '50 min',
    exercises: 25,
    completed: false,
    locked: true
  },
  {
    id: 10,
    title: 'Positional Play',
    description: 'Deep positional understanding',
    icon: '🧠',
    duration: '55 min',
    exercises: 22,
    completed: false,
    locked: true
  },
  {
    id: 11,
    title: 'Endgame Mastery',
    description: 'Advanced endgame techniques',
    icon: '🎖️',
    duration: '60 min',
    exercises: 30,
    completed: false,
    locked: true
  },
  {
    id: 12,
    title: 'Tournament Preparation',
    description: 'Preparing for competitive play',
    icon: '🏆',
    duration: '45 min',
    exercises: 20,
    completed: false,
    locked: true
  }
])
</script>

<style scoped>
.tutorial {
  background: linear-gradient(180deg, #f5f7fa 0%, #ffffff 100%);
}

/* Hero Section */
.tutorial-hero {
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

/* Quick Stats */
.quick-stats {
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

/* Tutorial Content */
.tutorial-content {
  padding: 4rem 0;
}

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

/* Level Section */
.level-section {
  margin-bottom: 4rem;
}

.level-header {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
}

.level-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 2rem;
  border-radius: 50px;
  font-weight: 700;
  font-size: 1.2rem;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
}

.level-badge.beginner {
  background: linear-gradient(135deg, #48bb78, #38a169);
  color: white;
}

.level-badge.intermediate {
  background: linear-gradient(135deg, #ed8936, #dd6b20);
  color: white;
}

.level-badge.advanced {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
}

.level-icon {
  font-size: 1.5rem;
}

.level-description {
  color: var(--text-light);
  font-size: 1.1rem;
}

/* Lessons Grid */
.lessons-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 2rem;
}

.lesson-card {
  background: white;
  border-radius: 20px;
  padding: 2rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  border: 2px solid transparent;
  position: relative;
  overflow: hidden;
}

.lesson-card::before {
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

.lesson-card:hover::before {
  transform: scaleX(1);
}

.lesson-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 40px rgba(102, 126, 234, 0.2);
  border-color: var(--primary-color);
}

.lesson-status {
  position: absolute;
  top: 1rem;
  right: 1rem;
  width: 35px;
  height: 35px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1rem;
  font-weight: bold;
}

.lesson-status.completed {
  background: linear-gradient(135deg, #48bb78, #38a169);
  color: white;
}

.lesson-status.locked {
  background: #e2e8f0;
  color: #a0aec0;
}

.lesson-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.lesson-title {
  font-size: 1.4rem;
  margin-bottom: 0.75rem;
  color: var(--text-color);
}

.lesson-description {
  color: var(--text-light);
  line-height: 1.6;
  margin-bottom: 1.5rem;
  min-height: 48px;
}

.lesson-meta {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border-color);
  font-size: 0.875rem;
  color: var(--text-light);
}

.lesson-btn {
  width: 100%;
  padding: 0.875rem;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  color: white;
}

.lesson-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.3);
}

.lesson-btn:disabled {
  background: #e2e8f0;
  color: #a0aec0;
  cursor: not-allowed;
}

/* Progress Section */
.progress-section {
  padding: 4rem 0;
  background: white;
}

.progress-card {
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.05), rgba(118, 75, 162, 0.05));
  border-radius: 20px;
  padding: 3rem;
  border: 2px solid var(--border-color);
}

.progress-card h3 {
  font-size: 2rem;
  margin-bottom: 2rem;
  color: var(--text-color);
  text-align: center;
}

.progress-stats {
  margin-bottom: 2rem;
}

.progress-item {
  margin-bottom: 2rem;
}

.progress-label {
  font-weight: 600;
  margin-bottom: 0.75rem;
  color: var(--text-color);
}

.progress-bar-wrapper {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.progress-bar {
  flex: 1;
  height: 12px;
  background: var(--border-color);
  border-radius: 10px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--primary-color), var(--secondary-color));
  border-radius: 10px;
  transition: width 0.5s ease;
}

.progress-value {
  font-weight: 700;
  color: var(--primary-color);
  min-width: 60px;
  text-align: right;
}

.cta-btn {
  display: block;
  width: fit-content;
  margin: 0 auto;
  padding: 1.25rem 3rem;
  background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  color: white;
  text-decoration: none;
  border-radius: 50px;
  font-weight: 700;
  font-size: 1.1rem;
  transition: all 0.3s ease;
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.3);
}

.cta-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 30px rgba(102, 126, 234, 0.4);
}

/* Responsive */
@media (max-width: 768px) {
  .hero-title {
    font-size: 2rem;
  }
  
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .lessons-grid {
    grid-template-columns: 1fr;
  }
  
  .level-header {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .progress-card {
    padding: 2rem 1.5rem;
  }
}
</style>
