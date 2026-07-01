import { useCallback, useEffect, useState } from 'react'
import {
  createEmployee,
  getEmployees,
  getMyEmployee,
  softDeleteEmployee,
  updateEmployee,
  updateEmployeeStatus,
} from '../api/employeesApi'

const defaultQuery = {
  search: '',
  department: '',
  workStatus: '',
  attendanceState: '',
  includeDeleted: false,
  page: 1,
  pageSize: 10,
}

export function useEmployees(token, role, employeeId) {
  const [employees, setEmployees] = useState([])
  const [pagination, setPagination] = useState({ page: 1, pageSize: 10, totalItems: 0, totalPages: 0 })
  const [query, setQuery] = useState(defaultQuery)
  const [isLoading, setIsLoading] = useState(false)
  const [action, setAction] = useState('')
  const [error, setError] = useState('')

  const loadEmployees = useCallback(async () => {
    if (!token || !role) return

    setIsLoading(true)
    setError('')
    try {
      if (role === 'Admin') {
        const result = await getEmployees(query, token)
        setEmployees(result.items ?? [])
        setPagination({
          page: result.page,
          pageSize: result.pageSize,
          totalItems: result.totalItems,
          totalPages: result.totalPages,
        })
      } else if (employeeId) {
        const profile = await getMyEmployee(token)
        setEmployees(profile ? [profile] : [])
      }
    } catch (loadError) {
      setError(loadError.message)
    } finally {
      setIsLoading(false)
    }
  }, [token, role, employeeId, query])

  useEffect(() => {
    void loadEmployees()
  }, [loadEmployees])

  async function runAction(name, callback) {
    setAction(name)
    setError('')
    try {
      const result = await callback()
      await loadEmployees()
      return result
    } catch (actionError) {
      setError(actionError.message)
      return null
    } finally {
      setAction('')
    }
  }

  return {
    employees,
    employee: employees.find((item) => item.id === employeeId) ?? employees[0],
    pagination,
    query,
    setQuery,
    isLoading,
    action,
    error,
    reload: loadEmployees,
    createEmployee: (payload) => runAction('create', () => createEmployee(payload, token)),
    updateEmployee: (id, payload) => runAction(`update-${id}`, () => updateEmployee(id, payload, token)),
    deleteEmployee: (id) => runAction(`delete-${id}`, () => softDeleteEmployee(id, token)),
    updateStatus: (id, status) => runAction(`status-${id}`, () => updateEmployeeStatus(id, status, token)),
  }
}
