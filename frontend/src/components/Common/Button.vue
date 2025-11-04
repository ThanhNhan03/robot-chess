//registration form
<template>
  <div class="auth-container">
    <h2>Register</h2>
    <form @submit.prevent="onRegister">
      <input v-model="form.username" placeholder="Username" required />
      <input v-model="form.email" placeholder="Email" type="email" required />
      <input v-model="form.password" placeholder="Password" type="password" required />
      <Button type="submit">Register</Button>
    </form>
    <p v-if="error" class="error">{{ error }}</p>
  </div>
</template>


//login form 
<script setup>
import { ref } from 'vue'
import Button from '@/components/Common/Button.vue'

const form = ref({ username: '', email: '', password: '' })
const error = ref(null)

async function onRegister() {
  try {
    // Logic đăng ký
    alert('Registration successful!')
  } catch (e) {
    error.value = e.message || 'Registration failed'
  }
}
</script>
<template>
  <div class="auth-container">
    <h2>Login</h2>
    <form @submit.prevent="onLogin">
      <input v-model="form.email" placeholder="Email" type="email" required />
      <input v-model="form.password" placeholder="Password" type="password" required />
      <Button type="submit">Login</Button>
    </form>
    <p v-if="error" class="error">{{ error }}</p>

    <!-- Modal hiển thị khi đăng nhập thất bại -->
    <Modal v-if="showModal" @close="showModal = false">
      <template #header>
        <h3>Login Failed</h3>
      </template>
      <template #body>
        <p>{{ error }}</p>
      </template>
      <template #footer>
        <Button @click="showModal = false">Close</Button>
      </template>
    </Modal>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import Button from '@/components/Common/Button.vue'
import Modal from '@/components/Common/Modal.vue'

const form = ref({ email: '', password: '' })
const error = ref(null)
const showModal = ref(false)

async function onLogin() {
  try {
    // Logic đăng nhập
    alert('Login successful!')
  } catch (e) {
    error.value = e.message || 'Login failed'
    showModal.value = true
  }
}
</script>