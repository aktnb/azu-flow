import { Handle, Position, type NodeProps } from '@xyflow/react'
import type { FunctionFlowNode } from '../../../types/topology'
import styles from './FunctionNode.module.scss'

export function FunctionNode({ data }: NodeProps<FunctionFlowNode>) {
  return (
    <div className={styles.root}>
      <Handle type="target" position={Position.Left} />
      <div className={styles.header}>
        <img src="/10029-icon-service-Function-Apps.svg" alt="" className={styles.icon} />
        <span className={styles.name}>{data.name}</span>
      </div>
      <div className={styles.subtitle}>{data.resourceGroup} / Function App</div>
      <div className={styles.status}>
        <span className={styles.statusDot} />
        Enabled
      </div>
      <Handle type="source" position={Position.Right} />
    </div>
  )
}
