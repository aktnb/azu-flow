import type { Meta, StoryObj } from '@storybook/react'
import { FunctionNode } from './FunctionNode'

const meta = {
  title: 'Nodes/FunctionNode',
  component: FunctionNode,
  tags: ['autodocs'],
  parameters: { layout: 'centered' },
  args: {
    id: 'fn-1',
    type: 'Function' as const,
    selected: false,
    selectable: false,
    deletable: false,
    draggable: false,
    zIndex: 1,
    isConnectable: false,
    dragging: false,
    positionAbsoluteX: 0,
    positionAbsoluteY: 0,
    data: { name: 'order-processor-dev', resourceGroup: 'azutopo-test-rg' },
  },
} satisfies Meta<typeof FunctionNode>

export default meta
type Story = StoryObj<typeof meta>

export const Default: Story = {}

export const LongName: Story = {
  args: {
    data: {
      name: 'very-long-function-app-name-that-overflows',
      resourceGroup: 'very-long-resource-group-name-that-also-overflows',
    },
  },
}
