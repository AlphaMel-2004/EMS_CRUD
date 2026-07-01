import { apiFetch } from './httpClient'

export function getDailyReport(date, token) {
  return apiFetch(`/api/reports/daily?date=${date}`, {}, token)
}

export function getMonthlyReport(year, month, token) {
  return apiFetch(`/api/reports/monthly?year=${year}&month=${month}`, {}, token)
}
