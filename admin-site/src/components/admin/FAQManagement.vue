<template>
  <div class="faq-management">
    <div class="panel-header">
      <h2 class="panel-title">
        <span>❓</span>
        FAQ Management
      </h2>
      <button class="btn-flat btn-primary" @click="showAddModal = true">
        <span>➕</span> Add New FAQ
      </button>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon">📚</div>
        <div class="stat-content">
          <div class="stat-value">24</div>
          <div class="stat-label">Total FAQs</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon">✅</div>
        <div class="stat-content">
          <div class="stat-value">18</div>
          <div class="stat-label">Published</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon">📝</div>
        <div class="stat-content">
          <div class="stat-value">6</div>
          <div class="stat-label">Draft</div>
        </div>
      </div>
      <div class="stat-card stat-primary">
        <div class="stat-icon">📂</div>
        <div class="stat-content">
          <div class="stat-value">4</div>
          <div class="stat-label">Categories</div>
        </div>
      </div>
    </div>

    <!-- Search and Filter -->
    <div class="panel-section">
      <div class="search-filter-bar">
        <div class="search-box">
          <input type="text" class="input-flat" placeholder="Search FAQs by question or answer..." />
        </div>
        <div class="filter-controls">
          <select class="select-flat">
            <option value="">All Categories</option>
            <option value="general">General</option>
            <option value="gameplay">Gameplay</option>
            <option value="technical">Technical</option>
            <option value="account">Account</option>
          </select>
          <select class="select-flat">
            <option value="">All Status</option>
            <option value="published">Published</option>
            <option value="draft">Draft</option>
          </select>
        </div>
      </div>
    </div>

    <!-- FAQ List -->
    <div class="panel-section">
      <h3 class="section-title">FAQ List</h3>
      <div class="faq-list">
        <!-- FAQ Item 1 -->
        <div class="faq-item">
          <div class="faq-header">
            <div class="faq-info">
              <h4 class="faq-question">How do I start playing chess with the robot?</h4>
              <div class="faq-meta">
                <span class="badge-flat badge-primary">GENERAL</span>
                <span class="badge-flat badge-success">PUBLISHED</span>
                <span class="faq-order">Display Order: 1</span>
              </div>
            </div>
            <div class="faq-actions">
              <button class="btn-flat btn-sm btn-primary">✏️ Edit</button>
              <button class="btn-flat btn-sm btn-danger">🗑️ Delete</button>
              <button class="btn-flat btn-sm">👁️ View</button>
            </div>
          </div>
          <div class="faq-answer">
            <strong>Answer:</strong> To start playing, first ensure the robot is connected and online. Then, navigate to the "New Game" section, select your preferred difficulty level, and click "Start Game". The robot will automatically set up the board and wait for your first move.
          </div>
          <div class="faq-footer">
            <span class="faq-date">Created: 2024-11-15 | Updated: 2024-11-20</span>
            <span class="faq-author">By: admin_001</span>
          </div>
        </div>

        <!-- FAQ Item 2 -->
        <div class="faq-item">
          <div class="faq-header">
            <div class="faq-info">
              <h4 class="faq-question">What should I do if the robot doesn't respond?</h4>
              <div class="faq-meta">
                <span class="badge-flat badge-warning">TECHNICAL</span>
                <span class="badge-flat badge-success">PUBLISHED</span>
                <span class="faq-order">Display Order: 2</span>
              </div>
            </div>
            <div class="faq-actions">
              <button class="btn-flat btn-sm btn-primary">✏️ Edit</button>
              <button class="btn-flat btn-sm btn-danger">🗑️ Delete</button>
              <button class="btn-flat btn-sm">👁️ View</button>
            </div>
          </div>
          <div class="faq-answer">
            <strong>Answer:</strong> If the robot is not responding: 1) Check the connection status in the dashboard, 2) Verify the robot's power supply, 3) Try reconnecting from the Robot Management panel, 4) If the issue persists, contact technical support with the robot ID and error message.
          </div>
          <div class="faq-footer">
            <span class="faq-date">Created: 2024-11-18 | Updated: 2024-11-25</span>
            <span class="faq-author">By: admin_001</span>
          </div>
        </div>

        <!-- FAQ Item 3 -->
        <div class="faq-item">
          <div class="faq-header">
            <div class="faq-info">
              <h4 class="faq-question">Can I play against different difficulty levels?</h4>
              <div class="faq-meta">
                <span class="badge-flat badge-info">GAMEPLAY</span>
                <span class="badge-flat badge-success">PUBLISHED</span>
                <span class="faq-order">Display Order: 3</span>
              </div>
            </div>
            <div class="faq-actions">
              <button class="btn-flat btn-sm btn-primary">✏️ Edit</button>
              <button class="btn-flat btn-sm btn-danger">🗑️ Delete</button>
              <button class="btn-flat btn-sm">👁️ View</button>
            </div>
          </div>
          <div class="faq-answer">
            <strong>Answer:</strong> Yes! Our system offers multiple AI difficulty levels: Beginner (ELO 800-1000), Intermediate (ELO 1200-1500), Advanced (ELO 1600-1900), and Master (ELO 2000+). You can select your preferred level when starting a new game.
          </div>
          <div class="faq-footer">
            <span class="faq-date">Created: 2024-11-10 | Updated: 2024-11-22</span>
            <span class="faq-author">By: admin_002</span>
          </div>
        </div>

        <!-- FAQ Item 4 - Draft -->
        <div class="faq-item faq-draft">
          <div class="faq-header">
            <div class="faq-info">
              <h4 class="faq-question">How do I reset my password?</h4>
              <div class="faq-meta">
                <span class="badge-flat badge-secondary">ACCOUNT</span>
                <span class="badge-flat badge-warning">DRAFT</span>
                <span class="faq-order">Display Order: 5</span>
              </div>
            </div>
            <div class="faq-actions">
              <button class="btn-flat btn-sm btn-primary">✏️ Edit</button>
              <button class="btn-flat btn-sm btn-danger">🗑️ Delete</button>
              <button class="btn-flat btn-sm btn-success">📤 Publish</button>
            </div>
          </div>
          <div class="faq-answer">
            <strong>Answer:</strong> Click on "Forgot Password" on the login page, enter your email address, and follow the instructions sent to your email to reset your password.
          </div>
          <div class="faq-footer">
            <span class="faq-date">Created: 2024-11-28 | Updated: 2024-11-28</span>
            <span class="faq-author">By: admin_001</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Add/Edit Modal (placeholder) -->
    <div v-if="showAddModal" class="modal-overlay" @click="showAddModal = false">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>Add New FAQ</h3>
          <button class="modal-close" @click="showAddModal = false">✕</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label>Question</label>
            <input type="text" class="input-flat" placeholder="Enter question..." />
          </div>
          <div class="form-group">
            <label>Answer</label>
            <textarea class="input-flat" rows="5" placeholder="Enter answer..."></textarea>
          </div>
          <div class="form-group">
            <label>Category</label>
            <select class="select-flat">
              <option value="general">General</option>
              <option value="gameplay">Gameplay</option>
              <option value="technical">Technical</option>
              <option value="account">Account</option>
            </select>
          </div>
          <div class="form-group">
            <label>Display Order</label>
            <input type="number" class="input-flat" value="1" min="0" />
          </div>
          <div class="form-group">
            <label>Published</label>
            <select class="select-flat">
              <option value="false">Draft (Not Published)</option>
              <option value="true">Published</option>
            </select>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn-flat btn-secondary" @click="showAddModal = false">Cancel</button>
          <button class="btn-flat btn-primary">Save FAQ</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const showAddModal = ref(false)
</script>

<style scoped>
@import '../../assets/styles/FAQManagement.css';
</style>
