<template>
  <div class="faq-management">
    <div class="panel-header">
      <h2 class="panel-title">
        <HelpCircle :size="24" />
        FAQ Management
      </h2>
      <button class="btn-flat btn-primary" @click="showAddModal = true">
        <Plus :size="18" /> Add New FAQ
      </button>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon"><BookOpen :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.totalFaqs }}</div>
          <div class="stat-label">Total FAQs</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon"><CheckCircle2 :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.publishedFaqs }}</div>
          <div class="stat-label">Published</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon"><FileEdit :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.draftFaqs }}</div>
          <div class="stat-label">Draft</div>
        </div>
      </div>
      <div class="stat-card stat-primary">
        <div class="stat-icon"><FolderOpen :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.categoriesCount }}</div>
          <div class="stat-label">Categories</div>
        </div>
      </div>
    </div>

    <!-- Search and Filter -->
    <div class="panel-section">
      <div class="search-filter-bar">
        <div class="search-box">
          <Search :size="20" class="search-icon" />
          <input 
            type="text" 
            class="input-flat" 
            placeholder="Search FAQs by question or answer..." 
            v-model="searchQuery"
            @input="filterFaqs"
          />
        </div>
        <div class="filter-controls">
          <select class="select-flat" v-model="filterCategory" @change="filterFaqs">
            <option value="">All Categories</option>
            <option v-for="cat in categories" :key="cat" :value="cat">
              {{ cat }}
            </option>
          </select>
          <select class="select-flat" v-model="filterStatus" @change="filterFaqs">
            <option value="">All Status</option>
            <option value="published">Published</option>
            <option value="draft">Draft</option>
          </select>
        </div>
      </div>
    </div>

    <!-- FAQ List -->
    <div class="panel-section">
      <h3 class="section-title">FAQ List ({{ filteredFaqs.length }})</h3>
      
      <!-- Loading State -->
      <div v-if="isLoading" class="loading-state">
        <div class="spinner-large"></div>
        <p>Loading FAQs...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="error-state">
        <AlertCircle :size="48" class="error-icon" />
        <p>{{ error }}</p>
        <button class="btn-flat btn-primary" @click="loadFaqs">
          <RefreshCw :size="16" /> Retry
        </button>
      </div>

      <!-- FAQ List -->
      <div v-else class="faq-list">
        <div v-if="filteredFaqs.length === 0" class="empty-state">
          <p>No FAQs found</p>
        </div>

        <div 
          v-for="faq in filteredFaqs" 
          :key="faq.id"
          class="faq-item"
          :class="{ 'faq-draft': !faq.isPublished }"
        >
          <div class="faq-header">
            <div class="faq-info">
              <h4 class="faq-question">{{ faq.question }}</h4>
              <div class="faq-meta">
                <span 
                  class="badge-flat"
                  :class="getCategoryBadgeClass(faq.category)"
                >
                  {{ (faq.category || 'UNCATEGORIZED').toUpperCase() }}
                </span>
                <span 
                  class="badge-flat"
                  :class="faq.isPublished ? 'badge-success' : 'badge-warning'"
                >
                  {{ faq.isPublished ? 'PUBLISHED' : 'DRAFT' }}
                </span>
                <span class="faq-order">Display Order: {{ faq.displayOrder || 0 }}</span>
              </div>
            </div>
            <div class="faq-actions">
              <button 
                class="btn-flat btn-sm btn-primary" 
                @click="editFaq(faq)"
                title="Edit FAQ"
              >
                <Edit2 :size="14" /> Edit
              </button>
              <button 
                class="btn-flat btn-sm btn-danger" 
                @click="deleteFaqConfirm(faq)"
                title="Delete FAQ"
              >
                <Trash2 :size="14" /> Delete
              </button>
              <button 
                v-if="!faq.isPublished"
                class="btn-flat btn-sm btn-success" 
                @click="togglePublishStatus(faq)"
                title="Publish FAQ"
              >
                <Upload :size="14" /> Publish
              </button>
              <button 
                v-else
                class="btn-flat btn-sm btn-warning" 
                @click="togglePublishStatus(faq)"
                title="Unpublish FAQ"
              >
                <Download :size="14" /> Unpublish
              </button>
            </div>
          </div>
          <div class="faq-answer">
            <strong>Answer:</strong> {{ faq.answer }}
          </div>
          <div class="faq-footer">
            <span class="faq-date">
              Created: {{ formatDate(faq.createdAt) }} | 
              Updated: {{ formatDate(faq.updatedAt) }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Add/Edit Modal -->
    <div v-if="showAddModal || showEditModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>{{ showEditModal ? 'Edit FAQ' : 'Add New FAQ' }}</h3>
          <button class="modal-close" @click="closeModal">
            <X :size="20" />
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="showEditModal ? updateFaqSubmit() : createFaqSubmit()">
            <div class="form-group">
              <label>Question *</label>
              <input 
                type="text" 
                class="input-flat" 
                placeholder="Enter question..." 
                v-model="faqForm.question"
                required
                maxlength="500"
              />
            </div>
            <div class="form-group">
              <label>Answer *</label>
              <textarea 
                class="input-flat" 
                rows="5" 
                placeholder="Enter answer..."
                v-model="faqForm.answer"
                required
              ></textarea>
            </div>
            <div class="form-group">
              <label>Category</label>
              <select class="select-flat" v-model="faqForm.category">
                <option value="">Select category</option>
                <option v-for="cat in categories" :key="cat" :value="cat">
                  {{ cat }}
                </option>
              </select>
            </div>
            <div class="form-group">
              <label>Display Order</label>
              <input 
                type="number" 
                class="input-flat" 
                v-model.number="faqForm.displayOrder" 
                min="0" 
              />
            </div>
            <div class="form-group">
              <label>Status</label>
              <select class="select-flat" v-model="faqForm.isPublished">
                <option :value="false">Draft (Not Published)</option>
                <option :value="true">Published</option>
              </select>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn-flat btn-secondary" @click="closeModal">
                Cancel
              </button>
              <button type="submit" class="btn-flat btn-primary" :disabled="isSaving">
                {{ isSaving ? 'Saving...' : (showEditModal ? 'Update FAQ' : 'Save FAQ') }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { 
  HelpCircle, Plus, BookOpen, CheckCircle2, FileEdit, FolderOpen,
  AlertCircle, RefreshCw, Edit2, Trash2, Upload, Download, X, Search
} from 'lucide-vue-next'
import { faqService, type Faq, type CreateFaqRequest, type UpdateFaqRequest, type FaqStats } from '../../services/faqService'

// State
const faqs = ref<Faq[]>([])
const filteredFaqs = ref<Faq[]>([])
const categories = ref<string[]>([])
const stats = ref<FaqStats>({
  totalFaqs: 0,
  publishedFaqs: 0,
  draftFaqs: 0,
  categoriesCount: 0
})
const isLoading = ref(false)
const error = ref('')

// Filters
const searchQuery = ref('')
const filterCategory = ref('')
const filterStatus = ref('')

// Modals
const showAddModal = ref(false)
const showEditModal = ref(false)
const isSaving = ref(false)

// FAQ Form
const faqForm = ref({
  id: '',
  question: '',
  answer: '',
  category: '',
  displayOrder: 0,
  isPublished: false
})

// Methods
const loadFaqs = async () => {
  isLoading.value = true
  error.value = ''
  
  try {
    const [faqsData, categoriesData, statsData] = await Promise.all([
      faqService.getAllFaqs(true),
      faqService.getCategories(),
      faqService.getFaqStats()
    ])
    
    faqs.value = faqsData.sort((a, b) => (a.displayOrder || 0) - (b.displayOrder || 0))
    categories.value = categoriesData
    stats.value = statsData
    filterFaqs()
  } catch (err: any) {
    error.value = err.message || 'Failed to load FAQs'
    console.error('Load FAQs error:', err)
  } finally {
    isLoading.value = false
  }
}

const filterFaqs = () => {
  let result = [...faqs.value]
  
  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(f => 
      f.question.toLowerCase().includes(query) ||
      f.answer.toLowerCase().includes(query)
    )
  }
  
  // Category filter
  if (filterCategory.value) {
    result = result.filter(f => f.category === filterCategory.value)
  }
  
  // Status filter
  if (filterStatus.value) {
    const isPublished = filterStatus.value === 'published'
    result = result.filter(f => f.isPublished === isPublished)
  }
  
  filteredFaqs.value = result
}

const formatDate = (dateStr?: string) => {
  if (!dateStr) return 'N/A'
  
  const date = new Date(dateStr)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  
  return `${year}-${month}-${day}`
}

const getCategoryBadgeClass = (category?: string) => {
  if (!category) return 'badge-secondary'
  
  const lowerCategory = category.toLowerCase()
  if (lowerCategory.includes('general')) return 'badge-primary'
  if (lowerCategory.includes('gameplay')) return 'badge-info'
  if (lowerCategory.includes('technical')) return 'badge-warning'
  if (lowerCategory.includes('account')) return 'badge-secondary'
  return 'badge-primary'
}

const editFaq = (faq: Faq) => {
  faqForm.value = {
    id: faq.id,
    question: faq.question,
    answer: faq.answer,
    category: faq.category || '',
    displayOrder: faq.displayOrder || 0,
    isPublished: faq.isPublished
  }
  showEditModal.value = true
}

const deleteFaqConfirm = async (faq: Faq) => {
  if (!confirm(`Are you sure you want to delete this FAQ?\n\n"${faq.question}"`)) {
    return
  }
  
  try {
    await faqService.deleteFaq(faq.id)
    await loadFaqs()
    alert('FAQ deleted successfully')
  } catch (err: any) {
    alert(`Failed to delete FAQ: ${err.message}`)
  }
}

const togglePublishStatus = async (faq: Faq) => {
  const action = faq.isPublished ? 'unpublish' : 'publish'
  
  if (!confirm(`Are you sure you want to ${action} this FAQ?`)) {
    return
  }
  
  try {
    await faqService.togglePublishStatus(faq.id, !faq.isPublished)
    await loadFaqs()
    alert(`FAQ ${action}ed successfully`)
  } catch (err: any) {
    alert(`Failed to ${action} FAQ: ${err.message}`)
  }
}

const createFaqSubmit = async () => {
  isSaving.value = true
  
  try {
    const data: CreateFaqRequest = {
      question: faqForm.value.question,
      answer: faqForm.value.answer,
      category: faqForm.value.category || undefined,
      displayOrder: faqForm.value.displayOrder || undefined,
      isPublished: faqForm.value.isPublished
    }
    
    await faqService.createFaq(data)
    await loadFaqs()
    closeModal()
    alert('FAQ created successfully')
  } catch (err: any) {
    alert(`Failed to create FAQ: ${err.message}`)
  } finally {
    isSaving.value = false
  }
}

const updateFaqSubmit = async () => {
  isSaving.value = true
  
  try {
    const data: UpdateFaqRequest = {
      question: faqForm.value.question,
      answer: faqForm.value.answer,
      category: faqForm.value.category || undefined,
      displayOrder: faqForm.value.displayOrder || undefined,
      isPublished: faqForm.value.isPublished
    }
    
    await faqService.updateFaq(faqForm.value.id, data)
    await loadFaqs()
    closeModal()
    alert('FAQ updated successfully')
  } catch (err: any) {
    alert(`Failed to update FAQ: ${err.message}`)
  } finally {
    isSaving.value = false
  }
}

const closeModal = () => {
  showAddModal.value = false
  showEditModal.value = false
  faqForm.value = {
    id: '',
    question: '',
    answer: '',
    category: '',
    displayOrder: 0,
    isPublished: false
  }
}

// Lifecycle
onMounted(() => {
  loadFaqs()
})
</script>

<style scoped>
@import '../../assets/styles/FAQManagement.css';
</style>
