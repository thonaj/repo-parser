<template>
  <div class="graph-view">
    <div class="header">
      <h2>Dependency Graph</h2>
      <div class="controls">
        <input v-model="scanPath" placeholder="Enter directory path..." class="path-input" />
        <button @click="buildGraph" :disabled="building" class="btn-primary">
          {{ building ? 'Building...' : 'Build Graph' }}
        </button>
      </div>
    </div>
    <div ref="graphContainer" class="graph-container"></div>
    <div v-if="!graphData" class="empty">
      Build a dependency graph to visualize file relationships.
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue'
import { useApi } from '../composables/useApi'

const { get, post } = useApi()
const graphContainer = ref(null)
const graphData = ref(null)
const scanPath = ref('')
const building = ref(false)
let network = null

onMounted(async () => {
  try {
    graphData.value = await get('/graph')
    await nextTick()
    renderGraph()
  } catch (e) {
    console.log('No graph data yet')
  }
})

watch(graphData, async () => {
  await nextTick()
  renderGraph()
})

async function buildGraph() {
  if (!scanPath.value) return
  building.value = true
  try {
    await post('/graph/build', { rootPath: scanPath.value })
    graphData.value = await get('/graph')
  } catch (e) {
    console.error('Build failed:', e)
  } finally {
    building.value = false
  }
}

async function renderGraph() {
  if (!graphContainer.value || !graphData.value) return

  const { Network } = await import('vis-network')
  const { DataSet } = await import('vis-data')

  const nodes = new DataSet(graphData.value.nodes.map(n => ({
    id: n.id,
    label: n.label,
    title: `${n.filePath}\nLanguage: ${n.language}\nMethods: ${n.methodCount}`,
    group: n.language,
    value: n.methodCount
  })))

  const edges = new DataSet(graphData.value.edges.map(e => ({
    from: e.from,
    to: e.to,
    label: e.type,
    arrows: 'to'
  })))

  const options = {
    physics: { stabilization: { iterations: 100 } },
    groups: {
      'C#': { color: { background: '#178600', border: '#0d5c00' } },
      JavaScript: { color: { background: '#f0db4f', border: '#b8a42e' } },
      TypeScript: { color: { background: '#3178c6', border: '#235a97' } },
      Python: { color: { background: '#3572A5', border: '#255682' } }
    },
    edges: { smooth: true },
    interaction: { hover: true, tooltipDelay: 200 }
  }

  network = new Network(graphContainer.value, { nodes, edges }, options)
}
</script>

<style scoped>
.header { margin-bottom: 20px; }
.controls { display: flex; gap: 12px; margin-top: 12px; }
.path-input { flex: 1; padding: 10px 14px; border: 1px solid #333; border-radius: 8px; background: #1e1e2e; color: #e0e0e0; font-size: 14px; }
.btn-primary { padding: 10px 20px; background: #00d4aa; color: #1a1a2e; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; }
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
.graph-container { width: 100%; height: 600px; border: 1px solid #333; border-radius: 8px; background: #1e1e2e; }
.empty { text-align: center; padding: 40px; color: #666; }
</style>
