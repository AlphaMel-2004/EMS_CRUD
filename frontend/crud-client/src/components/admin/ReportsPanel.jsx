import { useEffect, useState } from 'react'
import { API_BASE_URL } from '../../api/httpClient'
import { getDailyReport, getMonthlyReport } from '../../api/reportsApi'
import ErrorMessage from '../common/ErrorMessage'
import SectionHeader from '../common/SectionHeader'
import StatCard from '../common/StatCard'

export default function ReportsPanel({ token }) {
  const today = new Date().toISOString().slice(0, 10)
  const [daily, setDaily] = useState(null)
  const [monthly, setMonthly] = useState(null)
  const [error, setError] = useState('')
  const [isDownloading, setIsDownloading] = useState(false)

  useEffect(() => {
    async function loadReports() {
      try {
        const now = new Date()
        const [dailyResult, monthlyResult] = await Promise.all([
          getDailyReport(today, token),
          getMonthlyReport(now.getFullYear(), now.getMonth() + 1, token),
        ])
        setDaily(dailyResult)
        setMonthly(monthlyResult)
      } catch (reportError) {
        setError(reportError.message)
      }
    }

    void loadReports()
  }, [today, token])

  async function downloadCsv() {
    setIsDownloading(true)
    setError('')
    try {
      const response = await fetch(`${API_BASE_URL}/api/reports/attendance.csv?date=${today}`, {
        headers: { Authorization: `Bearer ${token}` },
      })
      if (!response.ok) throw new Error('CSV export failed.')
      const blob = await response.blob()
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `attendance-${today}.csv`
      link.click()
      URL.revokeObjectURL(url)
    } catch (downloadError) {
      setError(downloadError.message)
    } finally {
      setIsDownloading(false)
    }
  }

  return (
    <section className="section-card">
      <SectionHeader title="Attendance reports" description="Daily and monthly attendance summaries." />
      <ErrorMessage message={error} />
      <div className="kpi-grid">
        <StatCard label="Daily check-ins" value={daily?.checkedIn ?? 0} />
        <StatCard label="Daily absent" value={daily?.absent ?? 0} tone="warn" />
        <StatCard label="Monthly check-ins" value={monthly?.totalCheckIns ?? 0} />
        <StatCard label="Work days" value={monthly?.totalWorkDays ?? 0} />
      </div>
      <button className="download-link" type="button" onClick={downloadCsv} disabled={isDownloading}>
        {isDownloading ? 'Downloading...' : 'Download CSV'}
      </button>
    </section>
  )
}
