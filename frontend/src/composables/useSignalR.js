import { ref, onMounted, onUnmounted } from 'vue'
import * as signalR from '@microsoft/signalr'

// Singleton connection management to avoid duplicate connections
let sharedConnection = null
let sharedIsConnected = ref(false)
let connectionPromise = null
let handlerQueue = [] // Queue handlers that need to be registered after connection starts

function getOrCreateConnection(hubUrl) {
  if (!sharedConnection) {
    sharedConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build()

    sharedConnection.onreconnecting(() => {
      sharedIsConnected.value = false
    })

    sharedConnection.onreconnected(() => {
      sharedIsConnected.value = true
    })

    sharedConnection.onclose(() => {
      sharedIsConnected.value = false
    })

    connectionPromise = (async () => {
      try {
        await sharedConnection.start()
        sharedIsConnected.value = true
        console.log('SignalR connected')
        // Register any queued handlers
        for (const { eventName, callback } of handlerQueue) {
          sharedConnection.on(eventName, callback)
        }
        handlerQueue = []
      } catch (err) {
        console.error('SignalR connection error:', err)
      }
    })()
  }
  return { connection: sharedConnection, isConnected: sharedIsConnected, promise: connectionPromise }
}

export function useSignalR(hubUrl) {
  const { connection, isConnected, promise } = getOrCreateConnection(hubUrl)

  function on(eventName, callback) {
    if (connection.state === signalR.HubConnectionState.Connected) {
      connection.on(eventName, callback)
    } else {
      // Queue the handler to be registered once connected
      handlerQueue.push({ eventName, callback })
    }
  }

  return { isConnected, on }
}


