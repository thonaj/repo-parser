<template>
  <div class="alerts-view">
    <div class="header">
      <h2>Documentation Drift Alerts</h2>
      <label class="toggle">
        <input type="checkbox" v-model="unresolvedOnly" @change="loadAlerts" />
        Unresolved only
      </label>
    </div>

    <div v-if="allAlerts.length > 0" class="metrics-bar">
      <div class="metric">
        <span class="metric-value">{{ activeCount }}</span>
        <span class="metric-label">Active</span>
      </div>

      <div class="metric critical">
        <span class="metric-value">{{ countBySeverity('critical') }}</span>
        <span class="metric-label">Critical</span>
      </div>
      <div class="metric warning">
        <span class="metric-value">{{ countBySeverity('warning') }}</span>
        <span class="metric-label">Warning</span>
      </div>
      <div class="metric info">
        <span class="metric-value">{{ countBySeverity('info') }}</span>
        <span class="metric-label">Info</span>
      </div>
      <div class="metric resolved">
        <span class="metric-value">{{ resolvedCount }}</span>
        <span class="metric-label">Resolved</span>
      </div>
    </div>

    <div v-if="allAlerts.length === 0" class="empty">
      No drift alerts found. Your documentation is up to date!
    </div>


    <div v-for="alert in visibleAlerts" :key="alert.id" class="alert-card" :class="severityClass(alert.severity)">


      <div class="alert-header">
        <span class="severity-badge" :class="severityClass(alert.severity)">
          {{ alert.severity }}
        </span>
        <span class="alert-type">{{ alert.alertType }}</span>
        <span class="alert-date">{{ formatDate(alert.createdAt) }}</span>
      </div>
      <div class="alert-message">{{ alert.message }}</div>
      <div class="alert-meta">
        <span class="file-path">{{ alert.methodDefinition?.sourceFile?.filePath }}</span>
        <span class="method-name">{{ alert.methodDefinition?.name }}</span>
      </div>
      <div v-if="alert.oldSignature || alert.newSignature" class="signature-diff">
        <div v-if="alert.oldSignature" class="old-sig">
          <strong>Old:</strong> {{ alert.oldSignature }}
        </div>
        <div v-if="alert.newSignature" class="new-sig">
          <strong>New:</strong> {{ alert.newSignature }}
        </div>
      </div>
      <div class="alert-actions">
        <span class="similarity">Similarity: {{ (alert.similarityScore * 100).toFixed(1) }}%</span>
        <button v-if="!alert.isResolved" @click="resolveAlert(alert.id)" class="btn-resolve">
          Resolve
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

import { useApi } from '../composables/useApi'
import { useSignalR } from '../composables/useSignalR'

const { get, post } = useApi()
const { on } = useSignalR('/hubs/alerts')
const allAlerts = ref([])
const unresolvedOnly = ref(true)

const visibleAlerts = computed(() => {
  if (unresolvedOnly.value) {
    return allAlerts.value.filter(a => !a.isResolved)
  }
  return allAlerts.value
})

const activeCount = computed(() => allAlerts.value.filter(a => !a.isResolved).length)
const resolvedCount = computed(() => allAlerts.value.filter(a => a.isResolved).length)


onMounted(() => {
  const savedPath = localStorage.getItem('lastScanPath')
  if (savedPath) {
    loadAlerts()
  }
  on('NewAlert', (alert) => {
    allAlerts.value.unshift(alert)
  })
  on('AlertResolved', (id) => {
    const alert = allAlerts.value.find(a => a.id === id)
    if (alert) alert.isResolved = true
  })
})


async function loadAlerts() {
  try {
    const savedPath = localStorage.getItem('lastScanPath')
    let url = `/alerts?unresolvedOnly=false`
    if (savedPath) {
      url += `&rootPath=${encodeURIComponent(savedPath)}`
    }
    allAlerts.value = await get(url)
  } catch (e) {
    console.error(e)
  }
}


async function resolveAlert(id) {
  try {
    await post(`/alerts/${id}/resolve`)
    const alert = allAlerts.value.find(a => a.id === id)
    if (alert) alert.isResolved = true
  } catch (e) {
    console.error(e)
  }
}

function countBySeverity(severity) {
  return allAlerts.value.filter(a => {
    if (a.isResolved) return false
    const s = typeof a.severity === 'number'
      ? ({ 0: 'info', 1: 'warning', 2: 'critical' })[a.severity] || 'info'
      : (a.severity || '').toLowerCase()
    return s === severity
  }).length
}





function severityClass(severity) {
  if (typeof severity === 'number') {
    const map = { 0: 'info', 1: 'warning', 2: 'critical' }
    return map[severity] || 'info'
  }
  return severity?.toLowerCase() || 'info'
}


function formatDate(date) {
  return new Date(date).toLocaleString()
}
</script>

<style scoped>
.header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.toggle { display: flex; align-items: center; gap: 8px; color: #888; font-size: 14px; cursor: pointer; }
.alert-card { padding: 16px; margin-bottom: 12px; border-radius: 8px; border-left: 4px solid; }
.alert-card.info { background: #1e2a3e; border-color: #3498db; }
.alert-card.warning { background: #2a2a1e; border-color: #f39c12; }
.alert-card.critical { background: #2a1e1e; border-color: #e74c3c; }
.alert-header { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; }
.severity-badge { padding: 2px 10px; border-radius: 4px; font-size: 11px; font-weight: 600; text-transform: uppercase; }
.severity-badge.info { background: #3498db22; color: #3498db; }
.severity-badge.warning { background: #f39c1222; color: #f39c12; }
.severity-badge.critical { background: #e74c3c22; color: #e74c3c; }
.alert-type { font-size: 12px; color: #888; text-transform: uppercase; }
.alert-date { margin-left: auto; font-size: 12px; color: #666; }
.alert-message { font-size: 14px; color: #e0e0e0; margin-bottom: 8px; }
.alert-meta { display: flex; gap: 12px; font-size: 12px; color: #888; margin-bottom: 8px; }
.signature-diff { font-size: 12px; padding: 8px; background: rgba(0,0,0,0.2); border-radius: 4px; margin-bottom: 8px; }
.old-sig { color: #e74c3c; margin-bottom: 4px; }
.new-sig { color: #2ecc71; }
.alert-actions { display: flex; justify-content: space-between; align-items: center; }
.similarity { font-size: 12px; color: #888; }
.btn-resolve { padding: 6px 14px; background: #00d4aa; color: #1a1a2e; border: none; border-radius: 6px; cursor: pointer; font-size: 12px; font-weight: 600; }
.empty { text-align: center; padding: 40px; color: #666; }
.metrics-bar { display: flex; gap: 16px; margin-bottom: 24px; flex-wrap: wrap; }
.metric { display: flex; flex-direction: column; align-items: center; padding: 16px 24px; background: #1e1e2e; border-radius: 10px; min-width: 100px; border-top: 3px solid #555; }
.metric.critical { border-top-color: #e74c3c; }
.metric.warning { border-top-color: #f39c12; }
.metric.info { border-top-color: #3498db; }
.metric.resolved { border-top-color: #2ecc71; }
.metric-value { font-size: 28px; font-weight: 700; color: #e0e0e0; }
.metric-label { font-size: 11px; color: #888; text-transform: uppercase; margin-top: 4px; letter-spacing: 1px; }
</style>


