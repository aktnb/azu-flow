import type { Meta, StoryObj } from '@storybook/react'
import { ServiceBusTopicNode } from './ServiceBusTopicNode'

const meta = {
  title: 'Nodes/ServiceBusTopicNode',
  component: ServiceBusTopicNode,
  tags: ['autodocs'],
  parameters: { layout: 'centered' },
  args: {
    id: 'topic-1',
    type: 'ServiceBusTopic' as const,
    selected: false,
    selectable: false,
    deletable: false,
    draggable: false,
    zIndex: 1,
    isConnectable: false,
    dragging: false,
    positionAbsoluteX: 0,
    positionAbsoluteY: 0,
    data: { name: 'notifications', resourceGroup: 'azutopo-test-rg' },
  },
} satisfies Meta<typeof ServiceBusTopicNode>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {}

export const LongName: Story = {
  args: {
    data: {
      name: 'very-long-topic-name-that-overflows-the-container',
      resourceGroup: 'very-long-resource-group-name-overflows',
    },
  },
}
