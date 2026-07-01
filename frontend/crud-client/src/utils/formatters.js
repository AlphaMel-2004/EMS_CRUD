export function formatDateTime(value) {
  if (!value) return 'Not yet recorded'
  return new Date(value).toLocaleString()
}

export function formatNumber(value) {
  return new Intl.NumberFormat().format(value ?? 0)
}
