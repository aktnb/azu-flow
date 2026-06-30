import { Handle, Position, type NodeProps } from '@xyflow/react'
import type { ServiceBusSubscriptionFlowNode } from '../../../types/topology'
import { NodeId } from '../NodeId/NodeId'
import styles from './ServiceBusSubscriptionNode.module.scss'

export function ServiceBusSubscriptionNode({ id, data }: NodeProps<ServiceBusSubscriptionFlowNode>) {
  const subtitle = data.namespace && data.topicName
    ? `${data.namespace} / ${data.topicName} / Subscription`
    : data.namespace
      ? `${data.namespace} / Subscription`
      : 'Subscription'

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
        <NodeId id={id} />
      </div>
      <Handle type="source" position={Position.Right} />
    </div>
  )
}
