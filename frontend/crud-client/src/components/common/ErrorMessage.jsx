export default function ErrorMessage({ message, tone = 'error' }) {
  if (!message) return null
  return <p className={`alert ${tone}`}>{message}</p>
}
