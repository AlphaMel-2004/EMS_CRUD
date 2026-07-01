export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5299'

export class ApiError extends Error {
  constructor(message, details, status) {
    super(message)
    this.name = 'ApiError'
    this.details = details
    this.status = status
  }
}

export async function apiFetch(path, options = {}, token) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers ?? {}),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
  })

  if (!response.ok) {
    const error = await readError(response)
    throw new ApiError(error.message, error.details, response.status)
  }

  if (response.status === 204) return null
  return response.json()
}

async function readError(response) {
  const fallback = { message: 'Request failed. Please try again.' }

  try {
    const contentType = response.headers.get('content-type') ?? ''
    if (contentType.includes('application/json')) {
      return { ...fallback, ...(await response.json()) }
    }

    const text = await response.text()
    return { message: text || fallback.message }
  } catch {
    return fallback
  }
}
