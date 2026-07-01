import { useCallback, useEffect, useState } from 'react'
import { checkIn, checkOut, getAttendance } from '../api/attendanceApi'

export function useAttendance(token, employeeId) {
  const [attendance, setAttendance] = useState([])
  const [isLoading, setIsLoading] = useState(false)
  const [action, setAction] = useState('')
  const [error, setError] = useState('')

  const loadAttendance = useCallback(async () => {
    if (!token || !employeeId) return

    setIsLoading(true)
    setError('')
    try {
      setAttendance(await getAttendance(employeeId, token))
    } catch (loadError) {
      setError(loadError.message)
    } finally {
      setIsLoading(false)
    }
  }, [token, employeeId])

  useEffect(() => {
    void loadAttendance()
  }, [loadAttendance])

  async function runAction(name, callback) {
    setAction(name)
    setError('')
    try {
      const result = await callback()
      await loadAttendance()
      return result
    } catch (actionError) {
      setError(actionError.message)
      return null
    } finally {
      setAction('')
    }
  }

  return {
    attendance,
    isLoading,
    action,
    error,
    reload: loadAttendance,
    checkIn: () => runAction('check-in', () => checkIn(employeeId, token)),
    checkOut: () => runAction('check-out', () => checkOut(employeeId, token)),
  }
}
