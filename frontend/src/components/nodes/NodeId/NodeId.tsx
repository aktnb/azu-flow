import { useState } from 'react'
import type { MouseEvent, PointerEvent, TouchEvent } from 'react'
import styles from './NodeId.module.scss'

interface NodeIdProps {
  id: string
}

export function NodeId({ id }: NodeIdProps) {
  const [copyState, setCopyState] = useState<'idle' | 'copied' | 'failed'>('idle')

  const stopReactFlowInteraction = (
    event: MouseEvent<HTMLButtonElement> | PointerEvent<HTMLButtonElement> | TouchEvent<HTMLButtonElement>,
  ) => {
    event.stopPropagation()
  }

  const handleCopy = async (event: MouseEvent<HTMLButtonElement>) => {
    event.stopPropagation()

    try {
      await navigator.clipboard.writeText(id)
      setCopyState('copied')
    } catch {
      setCopyState('failed')
    }
  }

  const ariaLabel = copyState === 'copied'
    ? 'Node ID copied'
    : copyState === 'failed'
      ? 'Failed to copy node ID'
      : 'Copy node ID'

  return (
    <button
      type="button"
      className={`${styles.root} nodrag nopan`}
      aria-label={ariaLabel}
      data-nodrag
      data-nopan
      onPointerDownCapture={stopReactFlowInteraction}
      onPointerDown={stopReactFlowInteraction}
      onMouseDownCapture={stopReactFlowInteraction}
      onMouseDown={stopReactFlowInteraction}
      onTouchStartCapture={stopReactFlowInteraction}
      onTouchStart={stopReactFlowInteraction}
      onClick={handleCopy}
    >
      Id
      <span className={styles.tooltip} role="tooltip" aria-hidden="true">
        {id}
      </span>
    </button>
  )
}
