import ErrorMessage from '../common/ErrorMessage'
import EmptyState from '../common/EmptyState'
import StatCard from '../common/StatCard'
import MyAttendance from './MyAttendance'
import MyProfile from './MyProfile'
import MyStatus from './MyStatus'
import { formatDateTime } from '../../utils/formatters'

export default function EmployeeDashboard({ employee, employeesState, attendanceState, activeView }) {
  if (!employee && !employeesState.isLoading) {
    return (
      <EmptyState
        title="No employee profile found"
        description="Ask an administrator to connect your user account to an employee record."
      />
    )
  }

  return (
    <div className="dashboard-grid">
      <section className="kpi-grid">
        <StatCard label="Current status" value={employee?.workStatus ?? 'Loading'} tone="good" />
        <StatCard label="Attendance" value={employee?.attendanceState ?? 'Loading'} />
        <StatCard label="Last check-in" value={formatDateTime(employee?.lastCheckInAt)} />
        <StatCard label="Last check-out" value={formatDateTime(employee?.lastCheckOutAt)} />
      </section>
      <ErrorMessage message={employeesState.error || attendanceState.error} />

      {activeView === 'overview' ? <MyProfile employee={employee} /> : null}
      {activeView === 'status' ? (
        <MyStatus employee={employee} employeesState={employeesState} attendanceState={attendanceState} />
      ) : null}
      {activeView === 'attendance' ? <MyAttendance {...attendanceState} /> : null}
    </div>
  )
}
