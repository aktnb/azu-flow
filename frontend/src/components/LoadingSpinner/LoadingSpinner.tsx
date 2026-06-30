import './LoadingSpinner.scss'

export function LoadingSpinner() {
  return (
    <div className="loading-spinner" role="status" aria-label="読み込み中">
      <div className="loading-spinner__circle" />
      <span className="loading-spinner__text">読み込み中...</span>
    </div>
  )
}
