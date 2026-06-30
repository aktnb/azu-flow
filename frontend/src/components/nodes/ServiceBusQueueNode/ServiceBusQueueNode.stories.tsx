import type { Meta, StoryObj } from '@storybook/react'
import { ServiceBusQueueNode } from './ServiceBusQueueNode'

const meta = {
  title: 'Nodes/ServiceBusQueueNode',
  component: ServiceBusQueueNode,
  tags: ['autodocs'],
  parameters: { layout: 'centered' },
  args: {
    id: 'queue-1',
    type: 'ServiceBusQueue' as const,
    selected: false,
    selectable: false,
    deletable: false,
    draggable: false,
    zIndex: 1,
    isConnectable: false,
    dragging: false,
    positionAbsoluteX: 0,
    positionAbsoluteY: 0,
    data: { name: 'orders', resourceGroup: 'azutopo-test-rg' },
  },
} satisfies Meta<typeof ServiceBusQueueNode>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {}

export const LongName: Story = {
  args: {
    data: {
      name: 'very-long-queue-name-that-overflows-the-container',
      resourceGroup: 'very-long-resource-group-name-overflows',
    },
  },
}
