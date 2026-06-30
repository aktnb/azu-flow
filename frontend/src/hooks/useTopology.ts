import { useCallback, useEffect, useReducer } from 'react'
import { fetchTopology } from '../api/topology'
import type { TopologyGraph } from '../types/topology'

type State =
  | { status: 'loading' }
  | { status: 'success'; data: TopologyGraph }
  | { status: 'error'; error: Error }

type Action =
  | { type: 'FETCH_START' }
  | { type: 'FETCH_SUCCESS'; payload: TopologyGraph }
  | { type: 'FETCH_ERROR'; payload: Error }

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'FETCH_START':
      return { status: 'loading' }
    case 'FETCH_SUCCESS':
      return { status: 'success', data: action.payload }
    case 'FETCH_ERROR':
      return { status: 'error', error: action.payload }
    default:
      return state
  }
}

export function useTopology() {
  const [state, dispatch] = useReducer(reducer, { status: 'loading' })

  const load = useCallback(() => {
    const controller = new AbortController()
    dispatch({ type: 'FETCH_START' })

    fetchTopology(controller.signal)
      .then(data => dispatch({ type: 'FETCH_SUCCESS', payload: data }))
      .catch(error => {
        if (error instanceof Error && error.name === 'AbortError') return
        dispatch({
          type: 'FETCH_ERROR',
          payload: error instanceof Error ? error : new Error(String(error)),
        })
      })

    return controller
  }, [])

  useEffect(() => {
    const controller = load()
    return () => controller.abort()
  }, [load])

  return { state, retry: load }
}
