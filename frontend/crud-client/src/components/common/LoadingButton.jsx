export default function LoadingButton({ loading, loadingText, children, disabled, ...props }) {
  return (
    <button type="button" disabled={disabled || loading} {...props}>
      {loading ? loadingText : children}
    </button>
  )
}
