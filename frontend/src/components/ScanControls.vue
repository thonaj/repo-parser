<template>
  <div class="scan-controls">
    <div class="control-group">
      <h3>File Watcher</h3>
      <div class="row">
        <input v-model="watchPath" placeholder="Directory to watch..." class="input" />
        <button @click="toggleWatcher" class="btn" :class="{ active: isWatching }">
          {{ isWatching ? 'Stop Watching' : 'Start Watching' }}
        </button>
      </div>
      <div class="status">
        Status: <span :class="isWatching ? 'active' : 'inactive'">
          {{ isWatching ? 'Watching for changes...' : 'Not watching' }}
        </span>
      </div>
    </div>

    <div class="control-group">
      <h3>Quick Actions</h3>
      <div class="row">
        <button @click="scanSingleFile" class="btn">Scan Single File</button>
        <button @click="refreshAlerts" class="btn">Refresh Alerts</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useApi } from '../composables/useApi'

const { post } = useApi()
const watchPath = ref('')
const isWatching = ref(false)

function toggleWatcher() {
  isWatching.value = !isWatching.value
}

async function scanSingleFile() {
  const path = prompt('Enter file path:')
  if (path) {
    try {
      await post('/files/scan', { filePath: path })
      alert('File scanned successfully')
    } catch (e) {
      alert(`Scan failed: ${e.message}`)
    }
  }
}

async function refreshAlerts() {
  // This will be handled by the Alerts component
  window.location.reload()
}
</script>

<style scoped>
.scan-controls { display: grid; gap: 20px; }
.control-group { background: #1e1e2e; padding: 20px; border-radius: 12px; }
.control-group h3 { color: #888; font-size: 12px; text-transform: uppercase; margin-bottom: 12px; }
.row { display: flex; gap: 12px; }
.input { flex: 1; padding: 10px 14px; border: 1px solid #333; border-radius: 8px; background: #0d0d1a; color: #e0e0e0; }
.btn { padding: 10px 20px; background: #2a2a3e; color: #e0e0e0; border: 1px solid #333; border-radius: 8px; cursor: pointer; }
.btn.active { background: #00d4aa22; border-color: #00d4aa; color: #00d4aa; }
.status { margin-top: 8px; font-size: 13px; color: #666; }
.status .active { color: #00d4aa; }
.status .inactive { color: #888; }
</style>
