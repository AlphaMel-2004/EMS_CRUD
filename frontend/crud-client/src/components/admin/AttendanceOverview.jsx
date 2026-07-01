import EmptyState from '../common/EmptyState'
import SectionHeader from '../common/SectionHeader'
import { formatDateTime } from '../../utils/formatters'

export default function AttendanceOverview({ attendance, isLoading, onRefresh }) {
  return (
    <section className="section-card">
      <SectionHeader
        title="Recent attendance"
        description="Latest check-in and check-out activity for the selected employee context."
        actions={<button type="button" className="secondary-button" onClick={onRefresh}>Refresh</button>}
      />
      <div className="activity-list">
        {isLoading ? <p>Loading attendance...</p> : attendance.length ? attendance.map((entry) => (
          <article className="activity-item" key={entry.id}>
            <div>
              <strong>{entry.employeeName}</strong>
              <p>{formatDateTime(entry.occurredAt)}</p>
            </div>
            <span className="badge">{entry.eventType}</span>
          </article>
        )) : <EmptyState title="No attendance yet" description="Attendance logs appear after check-in or check-out." />}
      </div>
    </section>
  )
}
