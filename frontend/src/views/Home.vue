<template>
  <div class="home">
    <!-- Hero Section -->
    <section class="hero">
      <div class="hero-background">
        <div class="chess-pieces">
          <span class="piece piece-1">♔</span>
          <span class="piece piece-2">♕</span>
          <span class="piece piece-3">♖</span>
          <span class="piece piece-4">♗</span>
          <span class="piece piece-5">♘</span>
          <span class="piece piece-6">♙</span>
        </div>
      </div>
      <div class="container hero-content">
        <div class="hero-badge">
          <span class="badge-icon">🏆</span>
          <span>World's Best Chess Platform</span>
        </div>
        <h1 class="hero-title">
          Master the Game of 
          <span class="highlight">Chess</span>
        </h1>
        <p class="hero-subtitle">
          Improve your chess skills, play against AI, and compete with players worldwide.
          <br>
          Join millions of chess enthusiasts on their journey to mastery.
        </p>
        <div class="hero-buttons">
          <RouterLink to="/play" class="btn btn-primary btn-lg">
            <span class="btn-icon">🎮</span>
            Start Playing Now
          </RouterLink>
          <RouterLink to="/tutorial" class="btn btn-outline btn-lg">
            <span class="btn-icon">📚</span>
            Learn Chess
          </RouterLink>
        </div>
        <div class="hero-stats">
          <div class="stat-item">
            <div class="stat-number">10K+</div>
            <div class="stat-label">Active Players</div>
          </div>
          <div class="stat-item">
            <div class="stat-number">50K+</div>
            <div class="stat-label">Games Played</div>
          </div>
          <div class="stat-item">
            <div class="stat-number">4.9★</div>
            <div class="stat-label">User Rating</div>
          </div>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section class="features">
      <div class="container">
        <div class="section-header">
          <span class="section-badge">Why Choose Us</span>
          <h2 class="section-title">Powerful Features for Every Player</h2>
          <p class="section-subtitle">Everything you need to become a chess master</p>
        </div>
        
        <div class="features-grid">
          <div class="feature-card" data-aos="fade-up" data-aos-delay="0">
            <div class="feature-icon-wrapper">
              <div class="feature-icon">🎮</div>
            </div>
            <h3>Play Against AI</h3>
            <p>Challenge our advanced AI with multiple difficulty levels from beginner to grandmaster. Perfect your strategy!</p>
            <RouterLink to="/play" class="feature-link">
              Start Playing →
            </RouterLink>
          </div>
          
          <div class="feature-card" data-aos="fade-up" data-aos-delay="100">
            <div class="feature-icon-wrapper">
              <div class="feature-icon">📊</div>
            </div>
            <h3>Track Progress</h3>
            <p>Monitor your ELO rating, analyze your game history, and see your improvement over time with detailed statistics.</p>
            <RouterLink to="/profile" class="feature-link">
              View Stats →
            </RouterLink>
          </div>
          
          <div class="feature-card" data-aos="fade-up" data-aos-delay="200">
            <div class="feature-icon-wrapper">
              <div class="feature-icon">🏆</div>
            </div>
            <h3>Global Leaderboards</h3>
            <p>Compete with players worldwide and climb the rankings. See where you stand among the best chess players.</p>
            <RouterLink to="/leaderboard" class="feature-link">
              View Rankings →
            </RouterLink>
          </div>
          
          <div class="feature-card" data-aos="fade-up" data-aos-delay="0">
            <div class="feature-icon-wrapper">
              <div class="feature-icon">📚</div>
            </div>
            <h3>Learn & Practice</h3>
            <p>Access comprehensive tutorials, practice puzzles, and training exercises to improve your chess skills systematically.</p>
            <RouterLink to="/tutorial" class="feature-link">
              Start Learning →
            </RouterLink>
          </div>
          
          <div class="feature-card" data-aos="fade-up" data-aos-delay="100">
            <div class="feature-icon-wrapper">
              <div class="feature-icon">🔍</div>
            </div>
            <h3>Game Analysis</h3>
            <p>Review your games with AI-powered analysis. Get insights on mistakes, missed opportunities, and optimal moves.</p>
            <RouterLink to="/history" class="feature-link">
              Analyze Games →
            </RouterLink>
          </div>
          
          <div class="feature-card" data-aos="fade-up" data-aos-delay="200">
            <div class="feature-icon-wrapper">
              <div class="feature-icon">🎨</div>
            </div>
            <h3>Fully Customizable</h3>
            <p>Personalize your experience with various board themes, piece styles, and interface settings to match your preference.</p>
            <RouterLink to="/settings" class="feature-link">
              Customize Now →
            </RouterLink>
          </div>
        </div>
      </div>
    </section>

    <!-- How It Works Section -->
    <section class="how-it-works">
      <div class="container">
        <div class="section-header">
          <span class="section-badge">Getting Started</span>
          <h2 class="section-title">How It Works</h2>
          <p class="section-subtitle">Start your chess journey in 3 simple steps</p>
        </div>
        
        <div class="steps-grid">
          <div class="step-card">
            <div class="step-number">1</div>
            <div class="step-icon">📝</div>
            <h3>Create Account</h3>
            <p>Sign up for free in seconds and start your chess journey</p>
          </div>
          
          <div class="step-arrow">→</div>
          
          <div class="step-card">
            <div class="step-number">2</div>
            <div class="step-icon">🎯</div>
            <h3>Choose Mode</h3>
            <p>Select your difficulty level and game mode preferences</p>
          </div>
          
          <div class="step-arrow">→</div>
          
          <div class="step-card">
            <div class="step-number">3</div>
            <div class="step-icon">♟️</div>
            <h3>Start Playing</h3>
            <p>Begin your games and track your progress to mastery</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Testimonials Section -->
    <section class="testimonials">
      <div class="container">
        <div class="section-header">
          <span class="section-badge">Testimonials</span>
          <h2 class="section-title">What Players Say</h2>
          <p class="section-subtitle">Join thousands of satisfied chess enthusiasts</p>
        </div>
        
        <div class="testimonials-grid">
          <div class="testimonial-card">
            <div class="testimonial-rating">⭐⭐⭐⭐⭐</div>
            <p class="testimonial-text">"Best chess platform I've ever used! The AI is challenging and the interface is beautiful."</p>
            <div class="testimonial-author">
              <img src="https://i.pravatar.cc/150?img=11" alt="User" class="author-avatar">
              <div>
                <div class="author-name">Sarah Johnson</div>
                <div class="author-title">ELO 2100</div>
              </div>
            </div>
          </div>
          
          <div class="testimonial-card">
            <div class="testimonial-rating">⭐⭐⭐⭐⭐</div>
            <p class="testimonial-text">"The analysis feature helped me improve my game significantly. Highly recommended!"</p>
            <div class="testimonial-author">
              <img src="https://i.pravatar.cc/150?img=12" alt="User" class="author-avatar">
              <div>
                <div class="author-name">Michael Chen</div>
                <div class="author-title">ELO 2350</div>
              </div>
            </div>
          </div>
          
          <div class="testimonial-card">
            <div class="testimonial-rating">⭐⭐⭐⭐⭐</div>
            <p class="testimonial-text">"Perfect for beginners and experts alike. The tutorials are comprehensive and easy to follow."</p>
            <div class="testimonial-author">
              <img src="https://i.pravatar.cc/150?img=13" alt="User" class="author-avatar">
              <div>
                <div class="author-name">Emma Davis</div>
                <div class="author-title">ELO 1850</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- CTA Section -->
    <section class="cta">
      <div class="container">
        <div class="cta-content">
          <div class="cta-icon">♟️</div>
          <h2 class="cta-title">Ready to Master Chess?</h2>
          <p class="cta-subtitle">Join thousands of players improving their chess skills every day</p>
          <div class="cta-buttons">
            <RouterLink to="/register" class="btn btn-primary btn-xl">
              <span class="btn-icon">🚀</span>
              Create Free Account
            </RouterLink>
            <RouterLink to="/play" class="btn btn-outline-white btn-xl">
              <span class="btn-icon">🎮</span>
              Play as Guest
            </RouterLink>
          </div>
          <p class="cta-note">No credit card required • Free forever • Start in seconds</p>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
// Add any reactive data or methods here if needed
</script>

<style scoped>
.home {
  background: #ffffff;
}

/* Hero Section */
.hero {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  overflow: hidden;
  color: white;
}

.hero-background {
  position: absolute;
  inset: 0;
  opacity: 0.1;
  overflow: hidden;
}

.chess-pieces {
  position: relative;
  width: 100%;
  height: 100%;
}

.piece {
  position: absolute;
  font-size: 8rem;
  opacity: 0.3;
  animation: float 20s infinite ease-in-out;
}

.piece-1 { top: 10%; left: 10%; animation-delay: 0s; }
.piece-2 { top: 60%; left: 80%; animation-delay: 2s; }
.piece-3 { top: 30%; right: 15%; animation-delay: 4s; }
.piece-4 { bottom: 20%; left: 20%; animation-delay: 6s; }
.piece-5 { top: 70%; left: 50%; animation-delay: 8s; }
.piece-6 { bottom: 30%; right: 30%; animation-delay: 10s; }

@keyframes float {
  0%, 100% { transform: translateY(0) rotate(0deg); }
  25% { transform: translateY(-30px) rotate(5deg); }
  50% { transform: translateY(0) rotate(0deg); }
  75% { transform: translateY(30px) rotate(-5deg); }
}

.hero-content {
  position: relative;
  z-index: 1;
  text-align: center;
  padding: 4rem 0;
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
  margin-bottom: 2rem;
  animation: fadeInDown 0.8s ease;
  border: 2px solid rgba(255, 255, 255, 0.3);
}

.badge-icon {
  font-size: 1.2rem;
}

.hero-title {
  font-size: 4.5rem;
  font-weight: 800;
  margin-bottom: 1.5rem;
  line-height: 1.2;
  animation: fadeInUp 0.8s ease 0.2s both;
  text-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
}

.highlight {
  background: linear-gradient(120deg, #ffd700, #ffed4e);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  display: inline-block;
  animation: shine 3s infinite;
}

@keyframes shine {
  0%, 100% { filter: brightness(1); }
  50% { filter: brightness(1.3); }
}

.hero-subtitle {
  font-size: 1.25rem;
  margin-bottom: 3rem;
  opacity: 0.95;
  max-width: 700px;
  margin-left: auto;
  margin-right: auto;
  line-height: 1.8;
  animation: fadeInUp 0.8s ease 0.4s both;
}

.hero-buttons {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
  margin-bottom: 4rem;
  animation: fadeInUp 0.8s ease 0.6s both;
}

.btn-lg {
  padding: 1.25rem 2.5rem;
  font-size: 1.1rem;
  font-weight: 700;
  border-radius: 50px;
  display: inline-flex;
  align-items: center;
  gap: 0.75rem;
  transition: all 0.3s ease;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.2);
}

.btn-xl {
  padding: 1.5rem 3rem;
  font-size: 1.2rem;
}

.btn-primary {
  background: white;
  color: var(--primary-color);
}

.btn-primary:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 30px rgba(0, 0, 0, 0.3);
}

.btn-outline {
  background: transparent;
  color: white;
  border: 3px solid white;
}

.btn-outline:hover {
  background: white;
  color: var(--primary-color);
  transform: translateY(-3px);
}

.btn-outline-white {
  background: transparent;
  color: white;
  border: 3px solid white;
}

.btn-outline-white:hover {
  background: white;
  color: var(--primary-color);
  transform: translateY(-3px);
}

.btn-icon {
  font-size: 1.3rem;
}

.hero-stats {
  display: flex;
  justify-content: center;
  gap: 4rem;
  flex-wrap: wrap;
  animation: fadeInUp 0.8s ease 0.8s both;
}

.stat-item {
  text-align: center;
}

.stat-number {
  font-size: 2.5rem;
  font-weight: 800;
  margin-bottom: 0.5rem;
}

.stat-label {
  font-size: 1rem;
  opacity: 0.9;
}

/* Features Section */
.features {
  padding: 6rem 0;
  background: linear-gradient(180deg, #f5f7fa 0%, #ffffff 100%);
}

.section-header {
  text-align: center;
  margin-bottom: 4rem;
}

.section-badge {
  display: inline-block;
  background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  color: white;
  padding: 0.5rem 1.5rem;
  border-radius: 50px;
  font-weight: 600;
  font-size: 0.875rem;
  text-transform: uppercase;
  letter-spacing: 1px;
  margin-bottom: 1rem;
}

.section-title {
  font-size: 3rem;
  margin-bottom: 1rem;
  color: var(--text-color);
}

.section-subtitle {
  font-size: 1.2rem;
  color: var(--text-light);
  max-width: 600px;
  margin: 0 auto;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: 2rem;
}

.feature-card {
  background: white;
  padding: 2.5rem;
  border-radius: 20px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  border: 2px solid transparent;
  position: relative;
  overflow: hidden;
}

.feature-card::before {
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

.feature-card:hover::before {
  transform: scaleX(1);
}

.feature-card:hover {
  transform: translateY(-10px);
  box-shadow: 0 12px 40px rgba(102, 126, 234, 0.2);
  border-color: var(--primary-color);
}

.feature-icon-wrapper {
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.1), rgba(118, 75, 162, 0.1));
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 1.5rem;
  transition: all 0.3s ease;
}

.feature-card:hover .feature-icon-wrapper {
  background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  transform: scale(1.1) rotate(5deg);
}

.feature-icon {
  font-size: 2.5rem;
  transition: all 0.3s ease;
}

.feature-card:hover .feature-icon {
  transform: scale(1.2);
}

.feature-card h3 {
  font-size: 1.5rem;
  margin-bottom: 1rem;
  color: var(--text-color);
}

.feature-card p {
  color: var(--text-light);
  line-height: 1.8;
  margin-bottom: 1.5rem;
}

.feature-link {
  color: var(--primary-color);
  font-weight: 600;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.feature-link:hover {
  gap: 1rem;
}

/* How It Works Section */
.how-it-works {
  padding: 6rem 0;
  background: white;
}

.steps-grid {
  display: grid;
  grid-template-columns: 1fr auto 1fr auto 1fr;
  gap: 2rem;
  align-items: center;
  max-width: 1000px;
  margin: 0 auto;
}

.step-card {
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.05), rgba(118, 75, 162, 0.05));
  padding: 2.5rem;
  border-radius: 20px;
  text-align: center;
  position: relative;
  transition: all 0.3s ease;
  border: 2px solid transparent;
}

.step-card:hover {
  transform: translateY(-10px);
  border-color: var(--primary-color);
  box-shadow: 0 12px 40px rgba(102, 126, 234, 0.2);
}

.step-number {
  position: absolute;
  top: -20px;
  left: 50%;
  transform: translateX(-50%);
  width: 50px;
  height: 50px;
  background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  font-weight: 800;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.step-icon {
  font-size: 3rem;
  margin: 1.5rem 0 1rem;
}

.step-card h3 {
  font-size: 1.5rem;
  margin-bottom: 1rem;
  color: var(--text-color);
}

.step-card p {
  color: var(--text-light);
  line-height: 1.6;
}

.step-arrow {
  font-size: 2rem;
  color: var(--primary-color);
  font-weight: bold;
}

/* Testimonials Section */
.testimonials {
  padding: 6rem 0;
  background: linear-gradient(180deg, #f5f7fa 0%, #ffffff 100%);
}

.testimonials-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 2rem;
}

.testimonial-card {
  background: white;
  padding: 2.5rem;
  border-radius: 20px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  border: 2px solid transparent;
}

.testimonial-card:hover {
  transform: translateY(-10px);
  box-shadow: 0 12px 40px rgba(102, 126, 234, 0.2);
  border-color: var(--primary-color);
}

.testimonial-rating {
  font-size: 1.2rem;
  margin-bottom: 1rem;
}

.testimonial-text {
  font-size: 1.1rem;
  line-height: 1.8;
  color: var(--text-color);
  margin-bottom: 2rem;
  font-style: italic;
}

.testimonial-author {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.author-avatar {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  border: 3px solid var(--border-color);
}

.author-name {
  font-weight: 700;
  color: var(--text-color);
  margin-bottom: 0.25rem;
}

.author-title {
  color: var(--text-light);
  font-size: 0.875rem;
}

/* CTA Section */
.cta {
  padding: 6rem 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  position: relative;
  overflow: hidden;
}

.cta::before {
  content: '♟️';
  position: absolute;
  font-size: 30rem;
  opacity: 0.05;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

.cta-content {
  position: relative;
  z-index: 1;
  text-align: center;
  max-width: 800px;
  margin: 0 auto;
}

.cta-icon {
  font-size: 5rem;
  margin-bottom: 1rem;
  animation: bounce 2s infinite;
}

.cta-title {
  font-size: 3rem;
  margin-bottom: 1rem;
  color: white;
  text-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
}

.cta-subtitle {
  font-size: 1.3rem;
  opacity: 0.95;
  margin-bottom: 2.5rem;
  line-height: 1.8;
}

.cta-buttons {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
  margin-bottom: 2rem;
}

.cta-note {
  font-size: 0.875rem;
  opacity: 0.8;
}

/* Animations */
@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
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

@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-20px); }
}

/* Responsive */
@media (max-width: 1024px) {
  .hero-title {
    font-size: 3.5rem;
  }
  
  .features-grid {
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  }
  
  .steps-grid {
    grid-template-columns: 1fr;
  }
  
  .step-arrow {
    display: none;
  }
}

@media (max-width: 768px) {
  .hero-title {
    font-size: 2.5rem;
  }
  
  .hero-subtitle {
    font-size: 1.1rem;
  }
  
  .hero-buttons {
    flex-direction: column;
    align-items: center;
  }
  
  .btn-lg, .btn-xl {
    width: 100%;
    max-width: 300px;
  }
  
  .hero-stats {
    gap: 2rem;
  }
  
  .section-title {
    font-size: 2rem;
  }
  
  .features-grid {
    grid-template-columns: 1fr;
  }
  
  .testimonials-grid {
    grid-template-columns: 1fr;
  }
  
  .cta-title {
    font-size: 2rem;
  }
  
  .cta-buttons {
    flex-direction: column;
    align-items: center;
  }
}
</style>
