import dagre from '@dagrejs/dagre'

function segmentAfter(id: string, key: string): string | undefined {
  const parts = id.split('/')
  const idx = parts.indexOf(key)
  return idx !== -1 ? parts[idx + 1] : undefined
}

function extractNamespace(id: string): string | undefined {
  return segmentAfter(id, 'namespaces')
}

function extractTopicName(id: string): string | undefined {
  return segmentAfter(id, 'topics')
}
import { MarkerType, type Edge } from '@xyflow/react'
import type { TopologyFlowNode, TopologyGraph, TopologyNodeData } from '../types/topology'

const NODE_WIDTH = 200
const NODE_HEIGHT = 80

export function applyDagreLayout(
  nodes: TopologyFlowNode[],
  edges: Edge[],
): TopologyFlowNode[] {
  if (nodes.length === 0) return nodes

  const g = new dagre.graphlib.Graph()
  g.setDefaultEdgeLabel(() => ({}))
  g.setGraph({
    rankdir: 'LR',
    nodesep: 60,
    ranksep: 120,
    marginx: 40,
    marginy: 40,
  })

  for (const node of nodes) {
    g.setNode(node.id, { width: NODE_WIDTH, height: NODE_HEIGHT })
  }

  for (const edge of edges) {
    if (g.hasNode(edge.source) && g.hasNode(edge.target)) {
      g.setEdge(edge.source, edge.target)
    }
  }

  dagre.layout(g)

  return nodes.map(node => {
    const positioned = g.node(node.id)
    if (!positioned) return node
    return {
      ...node,
      position: {
        x: positioned.x - NODE_WIDTH / 2,
        y: positioned.y - NODE_HEIGHT / 2,
      },
    }
  })
}

export function graphToFlow(graph: TopologyGraph): {
  nodes: TopologyFlowNode[]
  edges: Edge[]
} {
  const flowEdges: Edge[] = graph.edges.map(edge => ({
    id: edge.id,
    source: edge.sourceNodeId,
    target: edge.targetNodeId,
    type: 'smoothstep',
    markerEnd: { type: MarkerType.ArrowClosed },
  }))

  const rawNodes = graph.nodes.map(node => ({
    id: node.id,
    type: node.type,
    position: { x: 0, y: 0 },
    data: {
      name: node.name,
      resourceGroup: node.resourceGroup,
      namespace: extractNamespace(node.id),
      topicName: extractTopicName(node.id),
    } satisfies TopologyNodeData,
  })) as TopologyFlowNode[]

  const layoutNodes = applyDagreLayout(rawNodes, flowEdges)
  return { nodes: layoutNodes, edges: flowEdges }
}
