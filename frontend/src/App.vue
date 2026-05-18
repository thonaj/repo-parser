<template>
  <div id="app">
    <nav class="navbar">
      <div class="nav-brand">Repo Parser</div>
      <div class="nav-links">
        <router-link to="/files">Files</router-link>
        <router-link to="/graph">Dependency Graph</router-link>
        <router-link to="/alerts">Drift Alerts</router-link>
      </div>
      <div class="nav-status">
        <span class="status-dot" :class="{ connected: signalConnected }"></span>
        {{ signalConnected ? 'Connected' : 'Disconnected' }}
      </div>
    </nav>
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useSignalR } from './composables/useSignalR'

const signalConnected = ref(false)
let checkInterval = null

onMounted(() => {
  const { isConnected } = useSignalR('/hubs/alerts')
  // Poll for connection state changes
  checkInterval = setInterval(() => {
    signalConnected.value = isConnected.value
  }, 1000)
})

onUnmounted(() => {
  if (checkInterval) clearInterval(checkInterval)
})
</script>

<style scoped>
.navbar {
  display: flex;
  align-items: center;
  padding: 0 24px;
  height: 56px;
  background: #1a1a2e;
  color: #e0e0e0;
  gap: 32px;
}
.nav-brand {
  font-size: 18px;
  font-weight: 700;
  color: #00d4aa;
}
.nav-links {
  display: flex;
  gap: 16px;
}
.nav-links a {
  color: #a0a0b0;
  text-decoration: none;
  padding: 8px 12px;
  border-radius: 6px;
  transition: all 0.2s;
}
.nav-links a:hover,
.nav-links a.router-link-active {
  color: #fff;
  background: rgba(255,255,255,0.1);
}
.nav-status {
  margin-left: auto;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: #888;
}
.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #ff4444;
}
.status-dot.connected {
  background: #00d4aa;
}
.main-content {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
}
</style>
