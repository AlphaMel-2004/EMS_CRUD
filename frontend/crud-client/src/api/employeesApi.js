import { apiFetch } from './httpClient'

export function getEmployees(query, token) {
  const params = new URLSearchParams()
  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      params.set(key, value)
    }
  })

  return apiFetch(`/api/employees?${params.toString()}`, {}, token)
}

export function getMyEmployee(token) {
  return apiFetch('/api/employees/me', {}, token)
}

export function createEmployee(payload, token) {
  return apiFetch('/api/employees', {
    method: 'POST',
    body: JSON.stringify(payload),
  }, token)
}

export function updateEmployee(id, payload, token) {
  return apiFetch(`/api/employees/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  }, token)
}

export function softDeleteEmployee(id, token) {
  return apiFetch(`/api/employees/${id}`, { method: 'DELETE' }, token)
}

export function updateEmployeeStatus(id, workStatus, token) {
  return apiFetch(`/api/employees/${id}/status`, {
    method: 'PATCH',
    body: JSON.stringify({ workStatus }),
  }, token)
}
