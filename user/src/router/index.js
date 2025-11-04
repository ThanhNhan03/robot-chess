import { createRouter, createWebHistory } from 'vue-router'
import Register from '@/components/auth/Register.vue'
import Login from '@/components/auth/Login.vue'
import Logout from '@/components/auth/Logout.vue'

// Lazy load components
const Home = () => import('@/views/Home.vue')
const PlayChess = () => import('@/views/PlayChess.vue')
const GameHistory = () => import('@/views/GameHistory.vue')
const Leaderboard = () => import('@/views/Leaderboard.vue')
const Profile = () => import('@/views/Profile.vue')
const Settings = () => import('@/views/Settings.vue')
const Tutorial = () => import('@/views/Tutorial.vue')
const Practice = () => import('@/views/Practice.vue')

const routes = [
  { path: '/', redirect: '/home' },
  { path: '/home', name: 'Home', component: Home, meta: { requiresAuth: false } },
  { path: '/register', name: 'Register', component: Register },
  { path: '/login', name: 'Login', component: Login },
  { path: '/logout', name: 'Logout', component: Logout },
  { path: '/play', name: 'PlayChess', component: PlayChess, meta: { requiresAuth: true } },
  { path: '/history', name: 'GameHistory', component: GameHistory, meta: { requiresAuth: true } },
  { path: '/leaderboard', name: 'Leaderboard', component: Leaderboard },
  { path: '/profile', name: 'Profile', component: Profile, meta: { requiresAuth: true } },
  { path: '/settings', name: 'Settings', component: Settings, meta: { requiresAuth: true } },
  { path: '/tutorial', name: 'Tutorial', component: Tutorial },
  { path: '/practice', name: 'Practice', component: Practice },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

//Navigation guard - TEMPORARILY DISABLED FOR UI TESTING
// router.beforeEach((to, from, next) => {
//   const token = localStorage.getItem('token')
//   if (to.meta.requiresAuth && !token) {
//     next('/login')
//   } else {
//     next()
//   }
// })

export default router