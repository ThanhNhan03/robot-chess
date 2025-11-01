import { createRouter, createWebHistory } from 'vue-router'
import ChessRobotScreen from '../screen/chess-robot/ChessRobotScreen.vue'
import AdminScreen from '../screen/admin/AdminScreen.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'chess-robot',
      component: ChessRobotScreen
    },
    {
      path: '/admin',
      name: 'admin',
      component: AdminScreen
    }
  ]
})

export default router
