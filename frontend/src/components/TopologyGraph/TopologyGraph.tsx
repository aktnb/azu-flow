import { useMemo } from 'react'
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  BackgroundVariant,
} from '@xyflow/react'
import { nodeTypes } from '../nodes'
import { graphToFlow } from '../../utils/layout'
import type { TopologyGraph as TopologyGraphData } from '../../types/topology'
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

export function TopologyGraph({ graph }: TopologyGraphProps) {
  const { nodes, edges } = useMemo(() => graphToFlow(graph), [graph])

  return (
    <div className="topology-graph">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={nodeTypes}
        nodesDraggable={false}
        nodesConnectable={false}
        elementsSelectable={false}
        fitView
        fitViewOptions={{ padding: 0.1 }}
        minZoom={0.1}
        maxZoom={2}
        attributionPosition="bottom-right"
      >
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
