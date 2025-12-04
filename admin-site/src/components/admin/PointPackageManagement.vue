<template>
  <div class="point-package-management">
    <div class="panel-header">
      <h2 class="panel-title">
        <DollarSign :size="24" />
        Point Package Management
      </h2>
      <button class="btn-flat btn-primary" @click="showCreateDialog = true">
        <Plus :size="18" /> Create Package
      </button>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon"><Package :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ packages.length }}</div>
          <div class="stat-label">Total Packages</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon"><CheckCircle :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ activePackages }}</div>
          <div class="stat-label">Active Packages</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon"><XCircle :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ packages.length - activePackages }}</div>
          <div class="stat-label">Inactive Packages</div>
        </div>
      </div>
    </div>

    <!-- Packages List -->
    <div class="panel-section">
      <h3 class="section-title">Point Packages ({{ packages.length }})</h3>
      
      <!-- Loading State -->
      <div v-if="isLoading" class="loading-state">
        <div class="spinner-large"></div>
        <p>Loading packages...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="error-state">
        <AlertCircle :size="48" class="error-icon" />
        <p>{{ error }}</p>
        <button class="btn-flat btn-primary" @click="loadPackages">
          <RefreshCw :size="16" /> Retry
        </button>
      </div>

      <!-- Packages Grid -->
      <div v-else class="packages-grid">
        <div 
          v-for="pkg in packages" 
          :key="pkg.id"
          class="package-card"
          :class="{ 'package-inactive': !pkg.isActive }"
        >
          <div class="package-header">
            <h4>{{ pkg.name }}</h4>
            <span 
              class="badge-flat"
              :class="pkg.isActive ? 'badge-success' : 'badge-danger'"
            >
              {{ pkg.isActive ? 'Active' : 'Inactive' }}
            </span>
          </div>
          
          <div class="package-body">
            <div class="package-points">
              <Coins :size="24" />
              <span class="points-value">{{ pkg.points.toLocaleString() }}</span>
              <span class="points-label">points</span>
            </div>
            
            <div class="package-price">
              <span class="price-value">{{ formatCurrency(pkg.price) }}</span>
            </div>
            
            <p v-if="pkg.description" class="package-description">
              {{ pkg.description }}
            </p>
          </div>
          
          <div class="package-footer">
            <button 
              class="btn-flat btn-sm btn-primary" 
              @click="editPackage(pkg)"
            >
              <Edit2 :size="14" /> Edit
            </button>
            <button 
              class="btn-flat btn-sm btn-danger" 
              @click="deletePackageConfirm(pkg)"
            >
              <Trash2 :size="14" /> Delete
            </button>
          </div>
        </div>

        <!-- Empty State -->
        <div v-if="packages.length === 0" class="empty-state">
          <Package :size="64" class="empty-icon" />
          <p>No point packages found</p>
          <button class="btn-flat btn-primary" @click="showCreateDialog = true">
            <Plus :size="18" /> Create First Package
          </button>
        </div>
      </div>
    </div>

    <!-- Create/Edit Package Dialog -->
    <div v-if="showCreateDialog || showEditDialog" class="modal-overlay" @click.self="closeDialogs">
      <div class="modal-dialog">
        <div class="modal-header">
          <h3>{{ showEditDialog ? 'Edit Package' : 'Create New Package' }}</h3>
          <button class="btn-close" @click="closeDialogs">
            <X :size="20" />
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="showEditDialog ? updatePackageSubmit() : createPackageSubmit()">
            <div class="form-group">
              <label>Package Name *</label>
              <input v-model="packageForm.name" type="text" class="input-flat" required />
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Points *</label>
                <input v-model.number="packageForm.points" type="number" class="input-flat" required min="1" />
              </div>
              <div class="form-group">
                <label>Price (VND) *</label>
                <input v-model.number="packageForm.price" type="number" class="input-flat" required min="0" step="1000" />
              </div>
            </div>
            <div class="form-group">
              <label>Description</label>
              <textarea v-model="packageForm.description" class="input-flat" rows="3"></textarea>
            </div>
            <div class="form-group" v-if="showEditDialog">
              <label class="checkbox-label">
                <input v-model="packageForm.isActive" type="checkbox" />
                <span>Active (available for purchase)</span>
              </label>
            </div>
            <div class="modal-actions">
              <button type="button" class="btn-flat btn-secondary" @click="closeDialogs">Cancel</button>
              <button type="submit" class="btn-flat btn-primary" :disabled="isSaving">
                {{ isSaving ? 'Saving...' : (showEditDialog ? 'Update' : 'Create') }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { 
  DollarSign, Plus, Package, CheckCircle, XCircle, AlertCircle, 
  RefreshCw, Edit2, Trash2, X, Coins
} from 'lucide-vue-next'
import { 
  pointPackageService, 
  type PointPackage, 
  type CreatePointPackageRequest,
  type UpdatePointPackageRequest
} from '../../services/pointPackageService'

// State
const packages = ref<PointPackage[]>([])
const isLoading = ref(false)
const error = ref('')

// Dialogs
const showCreateDialog = ref(false)
const showEditDialog = ref(false)
const isSaving = ref(false)

// Package Form
const packageForm = ref({
  id: 0,
  name: '',
  points: 0,
  price: 0,
  description: '',
  isActive: true
})

// Computed
const activePackages = computed(() => 
  packages.value.filter(p => p.isActive).length
)

// Methods
const loadPackages = async () => {
  isLoading.value = true
  error.value = ''
  
  try {
    packages.value = await pointPackageService.getAllPackages()
  } catch (err: any) {
    error.value = err.message || 'Failed to load packages'
    console.error('Load packages error:', err)
  } finally {
    isLoading.value = false
  }
}

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount)
}

const editPackage = (pkg: PointPackage) => {
  packageForm.value = {
    id: pkg.id,
    name: pkg.name,
    points: pkg.points,
    price: pkg.price,
    description: pkg.description || '',
    isActive: pkg.isActive
  }
  showEditDialog.value = true
}

const deletePackageConfirm = async (pkg: PointPackage) => {
  if (!confirm(`Are you sure you want to delete "${pkg.name}"?`)) {
    return
  }
  
  try {
    await pointPackageService.deletePackage(pkg.id)
    await loadPackages()
    alert('Package deleted successfully')
  } catch (err: any) {
    alert(`Failed to delete package: ${err.message}`)
  }
}

const createPackageSubmit = async () => {
  isSaving.value = true
  
  try {
    const data: CreatePointPackageRequest = {
      name: packageForm.value.name,
      points: packageForm.value.points,
      price: packageForm.value.price,
      description: packageForm.value.description || undefined
    }
    
    await pointPackageService.createPackage(data)
    await loadPackages()
    closeDialogs()
    alert('Package created successfully')
  } catch (err: any) {
    alert(`Failed to create package: ${err.message}`)
  } finally {
    isSaving.value = false
  }
}

const updatePackageSubmit = async () => {
  isSaving.value = true
  
  try {
    const data: UpdatePointPackageRequest = {
      name: packageForm.value.name,
      points: packageForm.value.points,
      price: packageForm.value.price,
      description: packageForm.value.description || undefined,
      isActive: packageForm.value.isActive
    }
    
    await pointPackageService.updatePackage(packageForm.value.id, data)
    await loadPackages()
    closeDialogs()
    alert('Package updated successfully')
  } catch (err: any) {
    alert(`Failed to update package: ${err.message}`)
  } finally {
    isSaving.value = false
  }
}

const closeDialogs = () => {
  showCreateDialog.value = false
  showEditDialog.value = false
  packageForm.value = {
    id: 0,
    name: '',
    points: 0,
    price: 0,
    description: '',
    isActive: true
  }
}

// Lifecycle
onMounted(() => {
  loadPackages()
})
</script>

<style scoped>
@import '../../assets/styles/PointPackageManagement.css';
</style>
