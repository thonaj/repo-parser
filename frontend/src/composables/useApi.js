const API_BASE = '/api'

export function useApi() {
  async function get(endpoint) {
    const response = await fetch(`${API_BASE}${endpoint}`)
    if (!response.ok) throw new Error(`API error: ${response.status}`)
    return response.json()
  }

  async function post(endpoint, body) {
    const response = await fetch(`${API_BASE}${endpoint}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    })
    if (!response.ok) throw new Error(`API error: ${response.status}`)
    const text = await response.text()
    return text ? JSON.parse(text) : null
  }

  return { get, post, API_BASE }
}
