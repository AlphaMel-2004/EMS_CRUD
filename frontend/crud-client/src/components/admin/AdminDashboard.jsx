import { useMemo, useState } from 'react'
import AttendanceOverview from './AttendanceOverview'
import EmployeeForm from './EmployeeForm'
import EmployeeTable from './EmployeeTable'
import ReportsPanel from './ReportsPanel'
import ErrorMessage from '../common/ErrorMessage'
import SectionHeader from '../common/SectionHeader'
import StatCard from '../common/StatCard'

export default function AdminDashboard({ token, employeesState, attendanceState, activeView }) {
  const [editingEmployee, setEditingEmployee] = useState(null)
  const { employees, pagination, query, setQuery, isLoading, action, error } = employeesState
  const metrics = useMemo(() => {
    const active = employees.filter((employee) => !employee.isDeleted)
    return {
      total: pagination.totalItems,
      working: active.filter((employee) => employee.workStatus === 'Working').length,
      absent: active.filter((employee) => employee.workStatus === 'Absent').length,
      leave: active.filter((employee) => employee.workStatus === 'Leave').length,
    }
  }, [employees, pagination.totalItems])

  return (
    <div className="dashboard-grid">
      <section className="kpi-grid">
        <StatCard label="Employees" value={metrics.total} />
        <StatCard label="Working" value={metrics.working} tone="good" />
        <StatCard label="Absent" value={metrics.absent} tone="warn" />
        <StatCard label="Leave" value={metrics.leave} />
      </section>
      <ErrorMessage message={error} />

      {activeView === 'overview' ? (
        <section className="section-card">
          <SectionHeader title="Workspace summary" description="A quick view of current employee status." />
          <EmployeeTable
            employees={employees}
            query={query}
            pagination={pagination}
            isLoading={isLoading}
            action={action}
            onQueryChange={setQuery}
            onEdit={setEditingEmployee}
            onDelete={employeesState.deleteEmployee}
            onStatusChange={employeesState.updateStatus}
          />
        </section>
      ) : null}

      {activeView === 'employees' ? (
        <section className="section-card">
          <SectionHeader title={editingEmployee ? 'Edit employee' : 'Create employee'} description="Admins can manage employee records and login accounts." />
          <EmployeeForm
            editingEmployee={editingEmployee}
            employees={employees}
            onCreate={employeesState.createEmployee}
            onUpdate={employeesState.updateEmployee}
            action={action}
            onCancel={() => setEditingEmployee(null)}
          />
          <EmployeeTable
            employees={employees}
            query={query}
            pagination={pagination}
            isLoading={isLoading}
            action={action}
            onQueryChange={setQuery}
            onEdit={setEditingEmployee}
            onDelete={employeesState.deleteEmployee}
            onStatusChange={employeesState.updateStatus}
          />
        </section>
      ) : null}

      {activeView === 'attendance' ? <AttendanceOverview {...attendanceState} /> : null}
      {activeView === 'reports' ? <ReportsPanel token={token} /> : null}
    </div>
  )
}
