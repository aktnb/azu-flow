import './ErrorMessage.scss'

interface ErrorMessageProps {
  error: Error
  onRetry?: () => void
}

export function ErrorMessage({ error, onRetry }: ErrorMessageProps) {
  return (
    <div className="error-message">
      <div className="error-message__box">
        <div className="error-message__title">
          <span aria-hidden="true">⚠️</span>
          データの取得に失敗しました
        </div>
        <div className="error-message__detail">{error.message}</div>
        {onRetry && (
          <button className="error-message__retry" type="button" onClick={onRetry}>
            再試行
          </button>
        )}
      </div>
    </div>
  )
}
