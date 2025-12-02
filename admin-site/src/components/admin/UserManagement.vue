<template>
  <div class="user-management">
    <div class="panel-header">
      <h2 class="panel-title">
        <Users :size="24" />
        User Management
      </h2>
      <button class="btn-flat btn-primary" title="Add a new user to the system">
        <Plus :size="18" /> Add New User
      </button>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid">
      <div class="stat-card stat-success">
        <div class="stat-icon"><Users :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.totalUsers }}</div>
          <div class="stat-label">Total Users</div>
        </div>
      </div>
      <div class="stat-card stat-info">
        <div class="stat-icon"><UserCheck :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.activeUsers }}</div>
          <div class="stat-label">Active Users</div>
        </div>
      </div>
      <div class="stat-card stat-warning">
        <div class="stat-icon"><Shield :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.adminUsers }}</div>
          <div class="stat-label">Admins</div>
        </div>
      </div>
      <div class="stat-card stat-primary">
        <div class="stat-icon"><UserPlus :size="32" /></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.newUsersThisWeek }}</div>
          <div class="stat-label">New This Week</div>
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
            placeholder="Search users by name or email..." 
            v-model="searchQuery"
            @input="filterUsers"
          />
        </div>
        <div class="filter-controls">
          <select class="select-flat" v-model="filterRole" @change="filterUsers">
            <option value="">All Roles</option>
            <option value="admin">Admin</option>
            <option value="player">Player</option>
            <option value="viewer">Viewer</option>
          </select>
          <select class="select-flat" v-model="filterStatus" @change="filterUsers">
            <option value="">All Status</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Users List -->
    <div class="panel-section">
      <h3 class="section-title">Users List ({{ filteredUsers.length }})</h3>
      
      <!-- Loading State -->
      <div v-if="isLoading" class="loading-state">
        <div class="spinner-large"></div>
        <p>Loading users...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="error-state">
        <AlertCircle :size="48" class="error-icon" />
        <p>{{ error }}</p>
        <button class="btn-flat btn-primary" @click="loadUsers">
          <RefreshCw :size="16" /> Retry
        </button>
      </div>

      <!-- Users Table -->
      <div v-else class="users-table">
        <table class="table-flat">
          <thead>
            <tr>
              <th>Avatar</th>
              <th>Username</th>
              <th>Email</th>
              <th>Full Name</th>
              <th>Phone</th>
              <th>Role</th>
              <th>Status</th>
              <th>Last Login</th>
              <th>Joined</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="filteredUsers.length === 0">
              <td colspan="10" style="text-align: center; padding: 40px;">
                No users found
              </td>
            </tr>
            <tr 
              v-for="user in paginatedUsers" 
              :key="user.id"
              :class="{ 'user-inactive': !user.isActive }"
            >
              <td>
                <div class="user-avatar">
                  {{ user.avatarUrl || getUserInitial(user.username) }}
                </div>
              </td>
              <td><strong>{{ user.username }}</strong></td>
              <td>{{ user.email }}</td>
              <td>{{ user.fullName || '-' }}</td>
              <td>{{ user.phoneNumber || '-' }}</td>
              <td>
                <span 
                  class="badge-flat"
                  :class="{
                    'badge-danger': user.role === 'admin',
                    'badge-primary': user.role === 'player',
                    'badge-info': user.role === 'viewer'
                  }"
                >
                  {{ user.role.toUpperCase() }}
                </span>
              </td>
              <td>
                <span 
                  class="badge-flat"
                  :class="user.isActive ? 'badge-success' : 'badge-danger'"
                >
                  {{ user.isActive ? 'ACTIVE' : 'INACTIVE' }}
                </span>
              </td>
              <td>{{ formatDate(user.lastLoginAt) }}</td>
              <td>{{ formatDate(user.createdAt) }}</td>
              <td>
                <div class="action-buttons">
                  <button 
                    class="btn-flat btn-sm btn-primary" 
                    @click="editUser(user)"
                    title="Edit user details"
                  >
                    <Edit2 :size="14" /> Edit
                  </button>
                  <button 
                    v-if="user.isActive"
                    class="btn-flat btn-sm btn-warning" 
                    @click="toggleUserStatus(user)"
                    title="Suspend user account"
                  >
                    <UserX :size="14" /> Suspend
                  </button>
                  <button 
                    v-else
                    class="btn-flat btn-sm btn-success" 
                    @click="toggleUserStatus(user)"
                    title="Activate user account"
                  >
                    <UserCheck2 :size="14" /> Activate
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="pagination" v-if="totalPages > 1">
        <button 
          class="btn-flat btn-sm btn-secondary" 
          @click="currentPage--"
          :disabled="currentPage === 1"
          title="Go to previous page"
        >
          <ChevronLeft :size="16" /> Previous
        </button>
        <div class="page-numbers">
          <button 
            v-for="page in displayedPages"
            :key="page"
            class="btn-flat btn-sm"
            :class="{ 'btn-primary': page === currentPage }"
            @click="currentPage = page"
            :title="`Go to page ${page}`"
          >
            {{ page }}
          </button>
        </div>
        <button 
          class="btn-flat btn-sm btn-secondary" 
          @click="currentPage++"
          :disabled="currentPage === totalPages"
          title="Go to next page"
        >
          Next <ChevronRight :size="16" />
        </button>
      </div>
    </div>

    <!-- Create/Edit User Dialog -->
    <div v-if="showCreateDialog || showEditDialog" class="modal-overlay" @click.self="closeDialogs">
      <div class="modal-dialog">
        <div class="modal-header">
          <h3>{{ showEditDialog ? 'Edit User' : 'Create New User' }}</h3>
          <button class="btn-close" @click="closeDialogs">
            <X :size="20" />
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="showEditDialog ? updateUserSubmit() : createUserSubmit()">
            <div class="form-group">
              <label>Email *</label>
              <input v-model="userForm.email" type="email" class="input-flat" required />
            </div>
            <div class="form-group">
              <label>Username *</label>
              <input v-model="userForm.username" type="text" class="input-flat" required />
            </div>
            <div class="form-group" v-if="!showEditDialog">
              <label>Password *</label>
              <input v-model="userForm.password" type="password" class="input-flat" required minlength="6" />
            </div>
            <div class="form-group">
              <label>Full Name</label>
              <input v-model="userForm.fullName" type="text" class="input-flat" />
            </div>
            <div class="form-group">
              <label>Phone Number</label>
              <input v-model="userForm.phoneNumber" type="tel" class="input-flat" />
            </div>
            <div class="form-group">
              <label>Role *</label>
              <select v-model="userForm.role" class="select-flat" required>
                <option value="player">Player</option>
                <option value="admin">Admin</option>
                <option value="viewer">Viewer</option>
              </select>
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
  Users, Plus, UserCheck, Shield, UserPlus, Search,
  Edit2, UserX, UserCheck2, AlertCircle, RefreshCw,
  ChevronLeft, ChevronRight, X
} from 'lucide-vue-next'
import { userService, type User, type CreateUserRequest, type UpdateUserRequest, type UserStats } from '../../services/userService'

// State
const users = ref<User[]>([])
const filteredUsers = ref<User[]>([])
const stats = ref<UserStats>({
  totalUsers: 0,
  activeUsers: 0,
  adminUsers: 0,
  newUsersThisWeek: 0
})
const isLoading = ref(false)
const error = ref('')

// Filters
const searchQuery = ref('')
const filterRole = ref('')
const filterStatus = ref('')

// Pagination
const currentPage = ref(1)
const pageSize = 10

// Dialogs
const showCreateDialog = ref(false)
const showEditDialog = ref(false)
const isSaving = ref(false)

// User Form
const userForm = ref({
  id: '',
  email: '',
  username: '',
  password: '',
  fullName: '',
  phoneNumber: '',
  role: 'player'
})

// Computed
const totalPages = computed(() => Math.ceil(filteredUsers.value.length / pageSize))

const paginatedUsers = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  const end = start + pageSize
  return filteredUsers.value.slice(start, end)
})

const displayedPages = computed(() => {
  const pages = []
  const maxPages = 5
  let startPage = Math.max(1, currentPage.value - 2)
  let endPage = Math.min(totalPages.value, startPage + maxPages - 1)
  
  if (endPage - startPage < maxPages - 1) {
    startPage = Math.max(1, endPage - maxPages + 1)
  }
  
  for (let i = startPage; i <= endPage; i++) {
    pages.push(i)
  }
  
  return pages
})

// Methods
const loadUsers = async () => {
  isLoading.value = true
  error.value = ''
  
  try {
    const [usersData, statsData] = await Promise.all([
      userService.getAllUsers(true),
      userService.getUserStats()
    ])
    
    users.value = usersData
    stats.value = statsData
    filterUsers()
  } catch (err: any) {
    error.value = err.message || 'Failed to load users'
    console.error('Load users error:', err)
  } finally {
    isLoading.value = false
  }
}

const filterUsers = () => {
  let result = [...users.value]
  
  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(u => 
      u.username.toLowerCase().includes(query) ||
      u.email.toLowerCase().includes(query) ||
      (u.fullName && u.fullName.toLowerCase().includes(query))
    )
  }
  
  // Role filter
  if (filterRole.value) {
    result = result.filter(u => u.role === filterRole.value)
  }
  
  // Status filter
  if (filterStatus.value) {
    const isActive = filterStatus.value === 'active'
    result = result.filter(u => u.isActive === isActive)
  }
  
  filteredUsers.value = result
  currentPage.value = 1 // Reset to first page
}

const getUserInitial = (username: string) => {
  return username.charAt(0).toUpperCase()
}

const formatDate = (dateStr?: string) => {
  if (!dateStr) return '-'
  
  const date = new Date(dateStr)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  
  return `${year}-${month}-${day} ${hours}:${minutes}`
}

const editUser = (user: User) => {
  userForm.value = {
    id: user.id,
    email: user.email,
    username: user.username,
    password: '',
    fullName: user.fullName || '',
    phoneNumber: user.phoneNumber || '',
    role: user.role
  }
  showEditDialog.value = true
}

const toggleUserStatus = async (user: User) => {
  const action = user.isActive ? 'suspend' : 'activate'
  
  if (!confirm(`Are you sure you want to ${action} user ${user.username}?`)) {
    return
  }
  
  try {
    await userService.updateUserStatus(user.id, !user.isActive)
    await loadUsers()
    alert(`User ${action}d successfully`)
  } catch (err: any) {
    alert(`Failed to ${action} user: ${err.message}`)
  }
}

const createUserSubmit = async () => {
  isSaving.value = true
  
  try {
    const data: CreateUserRequest = {
      email: userForm.value.email,
      username: userForm.value.username,
      password: userForm.value.password,
      fullName: userForm.value.fullName || undefined,
      phoneNumber: userForm.value.phoneNumber || undefined,
      role: userForm.value.role
    }
    
    await userService.createUser(data)
    await loadUsers()
    closeDialogs()
    alert('User created successfully')
  } catch (err: any) {
    alert(`Failed to create user: ${err.message}`)
  } finally {
    isSaving.value = false
  }
}

const updateUserSubmit = async () => {
  isSaving.value = true
  
  try {
    const data: UpdateUserRequest = {
      email: userForm.value.email,
      username: userForm.value.username,
      fullName: userForm.value.fullName || undefined,
      phoneNumber: userForm.value.phoneNumber || undefined,
      role: userForm.value.role
    }
    
    await userService.updateUser(userForm.value.id, data)
    await loadUsers()
    closeDialogs()
    alert('User updated successfully')
  } catch (err: any) {
    alert(`Failed to update user: ${err.message}`)
  } finally {
    isSaving.value = false
  }
}

const closeDialogs = () => {
  showCreateDialog.value = false
  showEditDialog.value = false
  userForm.value = {
    id: '',
    email: '',
    username: '',
    password: '',
    fullName: '',
    phoneNumber: '',
    role: 'player'
  }
}

// Lifecycle
onMounted(() => {
  loadUsers()
})
</script>

<style scoped>
@import '../../assets/styles/UserManagement.css';
</style>
