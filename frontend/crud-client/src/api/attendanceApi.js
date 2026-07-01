import { apiFetch } from './httpClient'

export function getAttendance(employeeId, token) {
  return apiFetch(`/api/employees/${employeeId}/attendance`, {}, token)
}

export function checkIn(employeeId, token) {
  return apiFetch(`/api/employees/${employeeId}/check-in`, { method: 'POST' }, token)
}

export function checkOut(employeeId, token) {
  return apiFetch(`/api/employees/${employeeId}/check-out`, { method: 'POST' }, token)
}
