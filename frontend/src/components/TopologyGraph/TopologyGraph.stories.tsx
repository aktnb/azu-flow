import type { Meta, StoryObj } from '@storybook/react'
import { TopologyGraph } from './TopologyGraph'
import type { TopologyGraph as TopologyGraphData } from '../../types/topology'

const mockGraph: TopologyGraphData = {
  nodes: [
    {
      id: '/subscriptions/eb13c19d/resourceGroups/rg/providers/Microsoft.ServiceBus/namespaces/ns/queues/orders',
      type: 'ServiceBusQueue',
      name: 'orders',
      resourceGroup: 'azutopo-test-rg',
    },
    {
      id: '/subscriptions/eb13c19d/resourceGroups/rg/providers/Microsoft.ServiceBus/namespaces/ns/queues/notifications',
      type: 'ServiceBusQueue',
      name: 'notifications',
      resourceGroup: 'azutopo-test-rg',
    },
    {
      id: '/subscriptions/eb13c19d/resourceGroups/rg/providers/Microsoft.Web/sites/order-processor-dev',
      type: 'Function',
      name: 'order-processor-dev',
      resourceGroup: 'azutopo-test-rg',
    },
    {
      id: '/subscriptions/eb13c19d/resourceGroups/rg/providers/Microsoft.Web/sites/notification-sender',
      type: 'Function',
      name: 'notification-sender',
      resourceGroup: 'azutopo-test-rg',
    },
    {
      id: '/subscriptions/eb13c19d/resourceGroups/rg/providers/Microsoft.ServiceBus/namespaces/ns/topics/events',
      type: 'ServiceBusTopic',
      name: 'events',
      resourceGroup: 'azutopo-test-rg',
    },
  ],
  edges: [
    {
      id: 'edge-1',
      sourceNodeId: '/subscriptions/eb13c19d/resourceGroups/rg/providers/Microsoft.ServiceBus/namespaces/ns/queues/orders',
      targetNodeId: '/subscriptions/eb13c19d/resourceGroups/rg/providers/Microsoft.Web/sites/order-processor-dev',
    },
    {
      id: 'edge-2',
      sourceNodeId: '/subscriptions/eb13c19d/resourceGroups/rg/providers/Microsoft.ServiceBus/namespaces/ns/queues/notifications',
      targetNodeId: '/subscriptions/eb13c19d/resourceGroups/rg/providers/Microsoft.Web/sites/notification-sender',
    },
  ],
  generatedAt: '2026-06-30T15:36:47.248307+00:00',
}

const meta = {
  title: 'Components/TopologyGraph',
  component: TopologyGraph,
  tags: ['autodocs'],
  decorators: [
    (Story) => (
      <div style={{ width: '800px', height: '500px' }}>
        <Story />
      </div>
    ),
  ],
} satisfies Meta<typeof TopologyGraph>

export default meta
type Story = StoryObj<typeof meta>

export const WithMockData: Story = {
  args: { graph: mockGraph },
}

export const EmptyGraph: Story = {
  args: {
    graph: {
      nodes: [],
      edges: [],
      generatedAt: '2026-06-30T15:36:47.248307+00:00',
    },
  },
}
