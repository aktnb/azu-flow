import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import {
  ReactFlow,
  applyNodeChanges,
  Background,
  Controls,
  MiniMap,
  BackgroundVariant,
  useNodesInitialized,
  useReactFlow,
  type Edge,
  type NodeChange,
} from '@xyflow/react'
import { nodeTypes } from '../nodes'
import { graphToFlow, applyDagreLayout } from '../../utils/layout'
import type { TopologyFlowNode, TopologyGraph as TopologyGraphData } from '../../types/topology'
import './TopologyGraph.scss'

interface TopologyGraphProps {
  graph: TopologyGraphData
}

function miniMapNodeColor(node: { type?: string }): string {
  switch (node.type) {
    case 'Function': return '#F25022'
    case 'ServiceBusQueue': return '#0078D4'
    case 'ServiceBusTopic': return '#00B4D8'
    case 'ServiceBusSubscription': return '#48CAE4'
    default: return '#999'
  }
}

interface AutoLayoutProps {
  edges: Edge[]
  graphKey: string
  onLayout: (nodes: TopologyFlowNode[]) => void
}

// Runs inside ReactFlow context; re-applies dagre once actual node sizes are known
function AutoLayout({ edges, graphKey, onLayout }: AutoLayoutProps) {
  const initialized = useNodesInitialized()
  const { getNodes, fitView } = useReactFlow<TopologyFlowNode>()
  const layoutedFor = useRef<string | null>(null)

  useEffect(() => {
    if (!initialized || layoutedFor.current === graphKey) return
    layoutedFor.current = graphKey

    const measured = getNodes() as TopologyFlowNode[]
    const laid = applyDagreLayout(measured, edges)
    onLayout(laid)
    requestAnimationFrame(() => fitView({ padding: 0.1 }))
  }, [initialized, graphKey, edges, getNodes, fitView, onLayout])

  return null
}

export function TopologyGraph({ graph }: TopologyGraphProps) {
  const { nodes: initialNodes, edges } = useMemo(() => graphToFlow(graph), [graph])
  const [nodes, setNodes] = useState<TopologyFlowNode[]>(initialNodes)

  // Reset to un-laid-out nodes whenever the graph changes so AutoLayout re-runs
  useEffect(() => {
    setNodes(initialNodes)
  }, [initialNodes])

  // Required for ReactFlow to write back measured sizes into our node state
  const onNodesChange = useCallback((changes: NodeChange[]) => {
    setNodes(nds => applyNodeChanges(changes, nds) as TopologyFlowNode[])
  }, [])

  return (
    <div className="topology-graph">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        onNodesChange={onNodesChange}
        nodeTypes={nodeTypes}
        nodesDraggable={false}
        nodesConnectable={false}
        elementsSelectable={false}
        minZoom={0.1}
        maxZoom={2}
        attributionPosition="bottom-right"
      >
        <AutoLayout edges={edges} graphKey={graph.generatedAt} onLayout={setNodes} />
        <Background variant={BackgroundVariant.Dots} gap={20} size={1} />
        <Controls showInteractive={false} />
        <MiniMap nodeColor={miniMapNodeColor} />
      </ReactFlow>
      <div className="topology-graph__meta">
        最終更新: {new Date(graph.generatedAt).toLocaleString('ja-JP')}
      </div>
    </div>
  )
}
