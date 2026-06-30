import type { Preview } from '@storybook/react'
import { ReactFlowProvider } from '@xyflow/react'
import '../src/styles/index.scss'

const preview: Preview = {
  decorators: [
    (Story) => (
      <ReactFlowProvider>
        <Story />
      </ReactFlowProvider>
    ),
  ],
  parameters: {
    layout: 'centered',
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },
    backgrounds: {
      default: 'app',
      values: [
        { name: 'app', value: '#F0F2F5' },
        { name: 'white', value: '#FFFFFF' },
      ],
    },
  },
}

export default preview
