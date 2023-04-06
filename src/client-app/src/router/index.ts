import { createRouter, createWebHistory, type RouteLocationNormalized } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import { useAuth } from '@/stores/auth'

const parseId = (route: RouteLocationNormalized) => {
  let parsed = parseInt(route.params.id.toString())
  if (isNaN(parsed)) parsed = 0

  return { ...route.params, id: parsed }
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/prijava',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
    },
    {
      path: '/odjava',
      name: 'logout',
      component: () => import('../views/LogoutView.vue'),
    },
    {
      path: '/korisnici',
      name: 'users',
      component: () => import('../views/UsersView.vue'),
    },
    {
      path: '/otprema',
      name: 'uploads',
      component: () => import('../views/UploadsView.vue'),
    }
    // {
    //   path: '/users/:id(\\d+)',
    //   name: 'users',
    //   component: () => import('../views/HomeView.vue'),
    //   props: parseId
    // }
  ]
})

const publicPaths = ['/prijava', '/odjava']
router.beforeEach(async (to) => {
  const auth = useAuth()
  const authRequired = !publicPaths.includes(to.path)

  // Must wait for auth to intialize before making a decision
  while (!auth.initialized) await new Promise((f) => setTimeout(f, 500))

  if (!auth.isAuthenticated && authRequired) return { name: 'login' }
})

export default router
