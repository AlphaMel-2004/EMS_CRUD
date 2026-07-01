import { apiFetch } from './httpClient'

export function login(payload) {
  return apiFetch('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify(payload),
    headers: {},
  })
}

export function getCurrentUser(token) {
  return apiFetch('/api/auth/me', {}, token)
}
