import type { Meta, StoryObj } from '@storybook/react'
import { ServiceBusSubscriptionNode } from './ServiceBusSubscriptionNode'

const meta = {
  title: 'Nodes/ServiceBusSubscriptionNode',
  component: ServiceBusSubscriptionNode,
  tags: ['autodocs'],
  parameters: { layout: 'centered' },
  args: {
    id: 'sub-1',
    type: 'ServiceBusSubscription' as const,
    selected: false,
    selectable: false,
    deletable: false,
    draggable: false,
    zIndex: 1,
    isConnectable: false,
    dragging: false,
    positionAbsoluteX: 0,
    positionAbsoluteY: 0,
    data: { name: 'order-events-sub', resourceGroup: 'azutopo-test-rg' },
  },
} satisfies Meta<typeof ServiceBusSubscriptionNode>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {}

export const LongName: Story = {
  args: {
    data: {
      name: 'very-long-subscription-name-that-overflows',
      resourceGroup: 'very-long-resource-group-name-overflows',
    },
  },
}
