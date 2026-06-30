import type { Meta, StoryObj } from '@storybook/react'
import { ErrorMessage } from './ErrorMessage'
import { TopologyApiError } from '../../api/topology'

const meta = {
  title: 'Components/ErrorMessage',
  component: ErrorMessage,
  tags: ['autodocs'],
  decorators: [
    (Story) => (
      <div style={{ width: '600px', height: '300px' }}>
        <Story />
      </div>
    ),
  ],
} satisfies Meta<typeof ErrorMessage>

export default meta
type Story = StoryObj<typeof meta>

export const ApiError: Story = {
  args: {
    error: new TopologyApiError(404, 'Not Found'),
    onRetry: undefined,
  },
}

export const NetworkError: Story = {
  args: {
    error: new TypeError('Failed to fetch'),
    onRetry: undefined,
  },
}

export const WithRetry: Story = {
  args: {
    error: new TopologyApiError(500, 'Internal Server Error'),
    onRetry: () => alert('再試行しました'),
  },
}
