import type { Node } from '@xyflow/react'

// API レスポンス型

export type TopologyNodeType =
  | 'Function'
  | 'ServiceBusQueue'
  | 'ServiceBusTopic'
  | 'ServiceBusSubscription'

export interface TopologyNode {
  id: string
  type: TopologyNodeType
  name: string
  resourceGroup: string
}

export interface TopologyEdge {
  id: string
  sourceNodeId: string
  targetNodeId: string
}

export interface TopologyGraph {
  nodes: TopologyNode[]
  edges: TopologyEdge[]
  generatedAt: string
}

// ReactFlow 用ノードデータ型

export interface TopologyNodeData extends Record<string, unknown> {
  name: string
  resourceGroup: string
  namespace?: string
  topicName?: string
}

export type FunctionFlowNode = Node<TopologyNodeData, 'Function'>
export type ServiceBusQueueFlowNode = Node<TopologyNodeData, 'ServiceBusQueue'>
export type ServiceBusTopicFlowNode = Node<TopologyNodeData, 'ServiceBusTopic'>
export type ServiceBusSubscriptionFlowNode = Node<TopologyNodeData, 'ServiceBusSubscription'>

export type TopologyFlowNode =
  | FunctionFlowNode
  | ServiceBusQueueFlowNode
  | ServiceBusTopicFlowNode
  | ServiceBusSubscriptionFlowNode
