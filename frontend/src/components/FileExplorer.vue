<template>
  <div class="file-explorer">
    <div class="header">
      <h2>Repository Files</h2>
      <div class="controls">
        <input v-model="scanPath" placeholder="Enter directory path to scan..." class="path-input" />
        <button @click="scanAll" :disabled="scanning" class="btn-primary">
          {{ scanning ? 'Scanning...' : 'Scan Directory' }}
        </button>
      </div>
    </div>

    <div v-if="scanResult" class="scan-result">{{ scanResult }}</div>
    <div v-if="error" class="error">{{ error }}</div>

    <div class="file-list">
      <div v-for="file in files" :key="file.id" class="file-card" :class="{ expanded: expandedFileId === file.id }" @click="toggleFile(file)">
        <div class="file-card-header">
          <div class="file-icon">{{ getIcon(file.language) }}</div>
          <div class="file-info">
            <div class="file-name">{{ getFileName(file.filePath) }}</div>
            <div class="file-path">{{ file.filePath }}</div>
            <div class="file-meta">
              <span class="badge">{{ file.language }}</span>
              <span>{{ file.methods?.length || 0 }} methods</span>
            </div>
          </div>
          <span class="expand-icon">{{ expandedFileId === file.id ? '▼' : '▶' }}</span>
        </div>
        <div v-if="expandedFileId === file.id" class="methods-accordion">
          <div v-if="!file.methods || file.methods.length === 0" class="no-methods">No methods found in this file.</div>
          <div v-for="method in file.methods" :key="method.id" class="method-row" @click.stop="viewMethod(method.id)">
            <div class="method-name">
              <span class="method-icon">λ</span>
              {{ method.name }}
            </div>
            <div class="method-details">
              <span class="method-return">{{ method.returnType }}</span>
              <span class="method-params">({{ method.parameters }})</span>
              <span class="method-line">Line {{ method.lineStart }}</span>
              <span class="method-doc" :class="{ has: method.docComment }">{{ method.docComment ? '✓ Doc' : '✗ No Doc' }}</span>
            </div>
          </div>
        </div>
      </div>
      <div v-if="files.length === 0 && !scanning" class="empty">
        No files scanned yet. Enter a directory path and click "Scan Directory".
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '../composables/useApi'

const { get, post } = useApi()
const router = useRouter()
const files = ref([])
const expandedFileId = ref(null)
const scanPath = ref('')
const scanning = ref(false)
const error = ref('')
const scanResult = ref('')


onMounted(async () => {
  const savedPath = localStorage.getItem('lastScanPath')
  if (savedPath) {
    scanPath.value = savedPath
    await loadFiles()
  }
})



async function loadFiles() {
  try {
    const query = scanPath.value ? `?rootPath=${encodeURIComponent(scanPath.value)}` : ''
    files.value = await get(`/files${query}`)
  } catch (e) {
    console.log('No files yet')
  }
}

async function scanAll() {
  if (!scanPath.value) return
  scanning.value = true
  error.value = ''
  try {
    const result = await post('/analysis/scan-all', { rootPath: scanPath.value })
    localStorage.setItem('lastScanPath', scanPath.value)
    await loadFiles()
    scanResult.value = `✓ Scanned ${result.filesScanned} files, found ${result.alertsGenerated} drift alerts`
    setTimeout(() => { scanResult.value = '' }, 5000)
  } catch (e) {
    error.value = `Scan failed: ${e.message}`
  } finally {
    scanning.value = false
  }
}


function toggleFile(file) {
  expandedFileId.value = expandedFileId.value === file.id ? null : file.id
}

function viewMethod(id) {

  router.push(`/methods/${id}`)
}

function getFileName(path) {
  return path?.split('\\').pop()?.split('/').pop() || path
}

function getIcon(lang) {
  const icons = { 'C#': '🔷', JavaScript: '🟨', TypeScript: '🔵', Python: '🐍', Go: '🔷', Java: '☕', Rust: '🦀' }
  return icons[lang] || '📄'
}
</script>

<style scoped>
.header { margin-bottom: 20px; }
.controls { display: flex; gap: 12px; margin-top: 12px; }
.path-input { flex: 1; padding: 10px 14px; border: 1px solid #333; border-radius: 8px; background: #1e1e2e; color: #e0e0e0; font-size: 14px; }
.btn-primary { padding: 10px 20px; background: #00d4aa; color: #1a1a2e; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; }
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
.error { color: #ff6b6b; padding: 12px; background: rgba(255,107,107,0.1); border-radius: 8px; margin-bottom: 16px; }
.scan-result { color: #00d4aa; padding: 12px; background: rgba(0,212,170,0.1); border-radius: 8px; margin-bottom: 16px; font-weight: 500; }
.file-list { display: grid; gap: 8px; }
.file-card { background: #1e1e2e; border-radius: 8px; cursor: pointer; transition: background 0.2s; overflow: hidden; }
.file-card:hover { background: #2a2a3e; }
.file-card.expanded { background: #252540; }
.file-card-header { display: flex; align-items: center; gap: 12px; padding: 12px 16px; }
.file-icon { font-size: 24px; flex-shrink: 0; }
.file-info { flex: 1; min-width: 0; }
.file-name { font-weight: 600; color: #e0e0e0; }
.file-path { font-size: 12px; color: #666; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.file-meta { display: flex; gap: 12px; margin-top: 4px; font-size: 12px; color: #888; }
.badge { background: #00d4aa22; color: #00d4aa; padding: 2px 8px; border-radius: 4px; }
.expand-icon { font-size: 12px; color: #666; flex-shrink: 0; transition: transform 0.2s; }
.empty { text-align: center; padding: 40px; color: #666; }
.methods-accordion { border-top: 1px solid #333; padding: 8px 0; }
.no-methods { padding: 16px; color: #666; text-align: center; font-size: 13px; }
.method-row { display: flex; flex-direction: column; gap: 4px; padding: 10px 16px; transition: background 0.15s; }
.method-row:hover { background: #2a2a4e; }
.method-name { font-weight: 600; color: #e0e0e0; font-size: 14px; display: flex; align-items: center; gap: 8px; }
.method-icon { color: #00d4aa; font-size: 16px; font-weight: 700; }
.method-details { display: flex; gap: 12px; font-size: 12px; color: #888; flex-wrap: wrap; }
.method-return { color: #569cd6; }
.method-params { color: #888; }
.method-line { color: #666; }
.method-doc { padding: 1px 6px; border-radius: 3px; background: #333; }
.method-doc.has { color: #2ecc71; background: #2ecc7122; }

</style>
