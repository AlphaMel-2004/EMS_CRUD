import { useMemo, useState } from 'react'
import ConfirmDialog from '../common/ConfirmDialog'
import EmptyState from '../common/EmptyState'
import LoadingButton from '../common/LoadingButton'
import { attendanceStates, statusOptions } from '../../utils/validation'

export default function EmployeeTable({
  employees,
  query,
  pagination,
  isLoading,
  action,
  onQueryChange,
  onEdit,
  onDelete,
  onStatusChange,
}) {
  const [pendingDelete, setPendingDelete] = useState(null)
  const departments = useMemo(() => [...new Set(employees.map((item) => item.department).filter(Boolean))], [employees])

  function updateQuery(patch) {
    onQueryChange({ ...query, page: 1, ...patch })
  }

  return (
    <>
      <div className="filters">
        <input placeholder="Search name or email" value={query.search} onChange={(event) => updateQuery({ search: event.target.value })} />
        <select value={query.department} onChange={(event) => updateQuery({ department: event.target.value })}>
          <option value="">All departments</option>
          {departments.map((department) => <option key={department} value={department}>{department}</option>)}
        </select>
        <select value={query.workStatus} onChange={(event) => updateQuery({ workStatus: event.target.value })}>
          <option value="">All statuses</option>
          {statusOptions.map((status) => <option key={status} value={status}>{status}</option>)}
        </select>
        <select value={query.attendanceState} onChange={(event) => updateQuery({ attendanceState: event.target.value })}>
          <option value="">All attendance</option>
          {attendanceStates.map((state) => <option key={state} value={state}>{state}</option>)}
        </select>
        <label className="check-field">
          <input type="checkbox" checked={query.includeDeleted} onChange={(event) => updateQuery({ includeDeleted: event.target.checked })} />
          Show deleted
        </label>
      </div>

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Department</th>
              <th>Position</th>
              <th>Status</th>
              <th>Attendance</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr><td colSpan="6">Loading employees...</td></tr>
            ) : employees.length ? employees.map((employee) => (
              <tr key={employee.id} className={employee.isDeleted ? 'is-deleted' : ''}>
                <td><strong>{employee.fullName}</strong><span>{employee.email}</span></td>
                <td>{employee.department}</td>
                <td>{employee.position}</td>
                <td>
                  <select
                    value={employee.workStatus}
                    disabled={employee.isDeleted || action === `status-${employee.id}`}
                    onChange={(event) => onStatusChange(employee.id, event.target.value)}
                  >
                    {statusOptions.map((status) => <option key={status} value={status}>{status}</option>)}
                  </select>
                </td>
                <td><span className="badge">{employee.attendanceState}</span></td>
                <td className="row-actions">
                  <button type="button" onClick={() => onEdit(employee)} disabled={employee.isDeleted}>Edit</button>
                  <LoadingButton
                    className="danger-button"
                    loading={action === `delete-${employee.id}`}
                    loadingText="Deleting..."
                    onClick={() => setPendingDelete(employee)}
                    disabled={employee.isDeleted}
                  >
                    Soft delete
                  </LoadingButton>
                </td>
              </tr>
            )) : (
              <tr>
                <td colSpan="6">
                  <EmptyState title="No employees found" description="Adjust filters or create a new employee record." />
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <div className="pagination">
        <button type="button" disabled={pagination.page <= 1} onClick={() => onQueryChange({ ...query, page: pagination.page - 1 })}>Previous</button>
        <span>Page {pagination.page} of {pagination.totalPages || 1}</span>
        <button type="button" disabled={pagination.page >= pagination.totalPages} onClick={() => onQueryChange({ ...query, page: pagination.page + 1 })}>Next</button>
      </div>

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        title="Soft delete employee"
        description={`Disable ${pendingDelete?.fullName ?? 'this employee'} and prevent future login?`}
        confirmLabel="Soft delete"
        onCancel={() => setPendingDelete(null)}
        onConfirm={() => {
          onDelete(pendingDelete.id)
          setPendingDelete(null)
        }}
      />
    </>
  )
}
