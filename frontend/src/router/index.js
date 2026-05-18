import { createRouter, createWebHistory } from 'vue-router'
import FileExplorer from '../components/FileExplorer.vue'
import DependencyGraph from '../components/DependencyGraph.vue'
import DriftAlerts from '../components/DriftAlerts.vue'
import MethodDetail from '../components/MethodDetail.vue'

const routes = [
  { path: '/', redirect: '/files' },
  { path: '/files', name: 'Files', component: FileExplorer },
  { path: '/graph', name: 'Graph', component: DependencyGraph },
  { path: '/alerts', name: 'Alerts', component: DriftAlerts },
  { path: '/methods/:id', name: 'MethodDetail', component: MethodDetail, props: true }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
