import type { TopologyGraph } from '../types/topology'

export class TopologyApiError extends Error {
  readonly status: number
  readonly statusText: string

  constructor(status: number, statusText: string) {
    super(`API error: ${status} ${statusText}`)
    this.name = 'TopologyApiError'
    this.status = status
    this.statusText = statusText
  }
}

export async function fetchTopology(signal?: AbortSignal): Promise<TopologyGraph> {
  const response = await fetch('/api/topology', { signal })

  if (!response.ok) {
    throw new TopologyApiError(response.status, response.statusText)
  }

  return response.json() as Promise<TopologyGraph>
}
