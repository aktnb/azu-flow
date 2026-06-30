import { useTopology } from './hooks/useTopology'
import { TopologyGraph } from './components/TopologyGraph/TopologyGraph'
import { LoadingSpinner } from './components/LoadingSpinner/LoadingSpinner'
import { ErrorMessage } from './components/ErrorMessage/ErrorMessage'
import './App.scss'

export function App() {
  const { state, retry } = useTopology()

  return (
    <div className="app">
      <header className="app__header">
        <h1 className="app__title">Azure Topology Visualizer</h1>
      </header>
      <main className="app__main">
        {state.status === 'loading' && <LoadingSpinner />}
        {state.status === 'error' && (
          <ErrorMessage error={state.error} onRetry={retry} />
        )}
        {state.status === 'success' && (
          <TopologyGraph graph={state.data} />
        )}
      </main>
    </div>
  )
}
