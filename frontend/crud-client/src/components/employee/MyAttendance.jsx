import EmptyState from '../common/EmptyState'
import SectionHeader from '../common/SectionHeader'
import { formatDateTime } from '../../utils/formatters'

export default function MyAttendance({ attendance, isLoading, reload }) {
  return (
    <section className="section-card">
      <SectionHeader
        title="My attendance"
        description="Your recent check-in and check-out history."
        actions={<button type="button" className="secondary-button" onClick={reload}>Refresh</button>}
      />
      <div className="activity-list">
        {isLoading ? <p>Loading attendance...</p> : attendance.length ? attendance.map((entry) => (
          <article className="activity-item" key={entry.id}>
            <div>
              <strong>{entry.eventType}</strong>
              <p>{formatDateTime(entry.occurredAt)}</p>
            </div>
            <span>{entry.note ?? 'Logged by system'}</span>
          </article>
        )) : <EmptyState title="No attendance yet" description="Use Check in to start your attendance timeline." />}
      </div>
    </section>
  )
}
