import { createRouter, createWebHistory } from 'vue-router'
import AdminScreen from '../view/admin/AdminScreen.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'Admin',
      component: AdminScreen
    },
    {
      path: '/admin',
      name: 'AdminDashboard',
      component: AdminScreen
    },
    // Redirect any unknown routes to admin
    {
      path: '/:pathMatch(.*)*',
      redirect: '/'
    }
  ]
})

export default router
