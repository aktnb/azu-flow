import { Handle, Position, type NodeProps } from '@xyflow/react'
import type { ServiceBusQueueFlowNode } from '../../../types/topology'
import styles from './ServiceBusQueueNode.module.scss'

export function ServiceBusQueueNode({ data }: NodeProps<ServiceBusQueueFlowNode>) {
  const subtitle = data.namespace
    ? `${data.namespace} / Service Bus Queue`
    : 'Service Bus Queue'

  return (
    <div className={styles.root}>
      <Handle type="target" position={Position.Left} />
      <div className={styles.header}>
        <img src="/10836-icon-service-Azure-Service-Bus.svg" alt="" className={styles.icon} />
        <span className={styles.name}>{data.name}</span>
      </div>
      <div className={styles.subtitle}>{subtitle}</div>
      <div className={styles.status}>
        <span className={styles.statusDot} />
        Active
      </div>
      <Handle type="source" position={Position.Right} />
    </div>
  )
}
