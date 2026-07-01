import SectionHeader from '../common/SectionHeader'
import StatCard from '../common/StatCard'
import { formatDateTime } from '../../utils/formatters'

export default function MyProfile({ employee }) {
  if (!employee) return null

  return (
    <section className="section-card">
      <SectionHeader title="My profile" description="Your current employee record." />
      <div className="profile-grid">
        <StatCard label="Name" value={employee.fullName} />
        <StatCard label="Department" value={employee.department} />
        <StatCard label="Position" value={employee.position} />
        <StatCard label="Last updated" value={formatDateTime(employee.updatedAt)} />
      </div>
    </section>
  )
}
