<template>
  <div class="method-detail">
    <button @click="$router.back()" class="btn-back">← Back</button>

    <div v-if="method" class="detail-card">
      <h2>{{ method.name }}</h2>
      <div class="meta">
        <span class="badge">{{ method.returnType }}</span>
        <span class="badge outline">Lines {{ method.lineStart }}-{{ method.lineEnd }}</span>
      </div>

      <div class="section">
        <h3>Signature</h3>
        <pre class="code-block">{{ method.signature }}</pre>
      </div>

      <div class="section">
        <h3>Parameters</h3>
        <pre class="code-block">{{ method.parameters }}</pre>
      </div>

      <div class="section">
        <h3>Documentation</h3>
        <pre v-if="method.docComment" class="doc-block">{{ method.docComment }}</pre>
        <p v-else class="no-doc">No documentation found for this method.</p>
      </div>

      <div v-if="method.alerts?.length" class="section">
        <h3>Drift Alerts ({{ method.alerts.length }})</h3>
        <div v-for="alert in method.alerts" :key="alert.id" class="alert-item" :class="alert.severity.toLowerCase()">
          <span class="severity-badge">{{ alert.severity }}</span>
          {{ alert.message }}
        </div>
      </div>
    </div>

    <div v-else class="loading">Loading method details...</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useApi } from '../composables/useApi'

const props = defineProps({ id: [String, Number] })
const { get } = useApi()
const method = ref(null)

onMounted(async () => {
  try {
    const files = await get('/files')
    for (const file of files) {
      const methods = await get(`/files/${file.id}/methods`)
      const found = methods.find(m => m.id === Number(props.id))
      if (found) {
        method.value = found
        break
      }
    }
  } catch (e) {
    console.error(e)
  }
})
</script>

<style scoped>
.btn-back { padding: 8px 16px; background: transparent; color: #00d4aa; border: 1px solid #00d4aa; border-radius: 6px; cursor: pointer; margin-bottom: 20px; }
.detail-card { background: #1e1e2e; border-radius: 12px; padding: 24px; }
.meta { display: flex; gap: 12px; margin: 12px 0 24px; }
.badge { padding: 4px 12px; background: #00d4aa22; color: #00d4aa; border-radius: 6px; font-size: 13px; }
.badge.outline { background: transparent; border: 1px solid #333; color: #888; }
.section { margin-bottom: 24px; }
.section h3 { color: #888; font-size: 12px; text-transform: uppercase; margin-bottom: 8px; }
.code-block { background: #0d0d1a; padding: 16px; border-radius: 8px; overflow-x: auto; font-family: 'Cascadia Code', 'Fira Code', monospace; font-size: 13px; color: #e0e0e0; }
.doc-block { background: #1a2a1a; padding: 16px; border-radius: 8px; font-family: 'Cascadia Code', 'Fira Code', monospace; font-size: 13px; color: #8bc34a; white-space: pre-wrap; }
.no-doc { color: #666; font-style: italic; }
.alert-item { padding: 8px 12px; margin-bottom: 8px; border-radius: 6px; font-size: 13px; border-left: 3px solid; }
.alert-item.warning { background: #2a2a1e; border-color: #f39c12; }
.alert-item.critical { background: #2a1e1e; border-color: #e74c3c; }
.alert-item.info { background: #1e2a3e; border-color: #3498db; }
.severity-badge { font-weight: 600; margin-right: 8px; }
.loading { text-align: center; padding: 40px; color: #666; }
</style>
