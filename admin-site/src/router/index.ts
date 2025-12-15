import { createRouter, createWebHistory } from 'vue-router'
import AdminScreen from '../view/admin/AdminScreen.vue'
import Login from '../view/admin/Login.vue'
import PaymentManagement from '../view/admin/PaymentManagement.vue'
import PointManagement from '../view/admin/PointManagement.vue'
import NotificationManagement from '../view/admin/NotificationManagement.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: Login,
      meta: { requiresGuest: true }
    },
    {
      path: '/',
      redirect: '/admin'
    },
    {
      path: '/admin',
      name: 'AdminDashboard',
      component: AdminScreen,
      meta: { requiresAuth: true }
    },
    {
      path: '/admin/payments',
      name: 'PaymentManagement',
      component: PaymentManagement,
      meta: { requiresAuth: true }
    },
    {
      path: '/admin/points',
      name: 'PointManagement',
      component: PointManagement,
      meta: { requiresAuth: true }
    },
    {
      path: '/admin/notifications',
      name: 'NotificationManagement',
      component: NotificationManagement,
      meta: { requiresAuth: true }
    },
    // Redirect any unknown routes to admin
    {
      path: '/:pathMatch(.*)*',
      redirect: '/admin'
    }
  ]
})

// Navigation guard
router.beforeEach((to, _from, next) => {
  const token = localStorage.getItem('authToken')
  const userStr = localStorage.getItem('adminUser')
  const user = userStr ? JSON.parse(userStr) : null
  const isAuthenticated = !!token && user?.role === 'admin'

  if (to.meta.requiresAuth && !isAuthenticated) {
    // Redirect to login if trying to access protected route
    next('/login')
  } else if (to.meta.requiresGuest && isAuthenticated) {
    // Redirect to admin if already logged in and trying to access login page
    next('/admin')
  } else {
    next()
  }
})

export default router
