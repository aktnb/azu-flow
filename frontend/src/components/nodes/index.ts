import { FunctionNode } from './FunctionNode/FunctionNode'
import { ServiceBusQueueNode } from './ServiceBusQueueNode/ServiceBusQueueNode'
import { ServiceBusTopicNode } from './ServiceBusTopicNode/ServiceBusTopicNode'
import { ServiceBusSubscriptionNode } from './ServiceBusSubscriptionNode/ServiceBusSubscriptionNode'

export const nodeTypes = {
  Function: FunctionNode,
  ServiceBusQueue: ServiceBusQueueNode,
  ServiceBusTopic: ServiceBusTopicNode,
  ServiceBusSubscription: ServiceBusSubscriptionNode,
} as const
