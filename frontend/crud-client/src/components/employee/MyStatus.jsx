import { useEffect, useState } from 'react'
import Field from '../common/Field'
import LoadingButton from '../common/LoadingButton'
import SectionHeader from '../common/SectionHeader'
import { canCheckIn, canCheckOut, statusOptions, validateStatus } from '../../utils/validation'

export default function MyStatus({ employee, employeesState, attendanceState }) {
  const [status, setStatus] = useState(employee?.workStatus ?? 'Working')
  const [error, setError] = useState('')

  useEffect(() => {
    setStatus(employee?.workStatus ?? 'Working')
    setError('')
  }, [employee])

  async function submit(event) {
    event.preventDefault()
    const statusError = validateStatus(status)
    setError(statusError)
    if (statusError || !employee) return
    await employeesState.updateStatus(employee.id, status)
  }

  return (
    <section className="section-card">
      <SectionHeader title="My status" description="Update your availability and record attendance." />
      <form className="employee-actions" onSubmit={submit}>
        <Field label="Work status" error={error}>
          <select value={status} onChange={(event) => setStatus(event.target.value)}>
            {statusOptions.map((option) => <option key={option} value={option}>{option}</option>)}
          </select>
        </Field>
        <div className="button-row">
          <LoadingButton type="submit" loading={employeesState.action === `status-${employee?.id}`} loadingText="Saving...">
            Save status
          </LoadingButton>
          <LoadingButton
            className="secondary-button"
            loading={attendanceState.action === 'check-in'}
            loadingText="Checking in..."
            disabled={!canCheckIn(employee)}
            onClick={attendanceState.checkIn}
          >
            Check in
          </LoadingButton>
          <LoadingButton
            className="secondary-button"
            loading={attendanceState.action === 'check-out'}
            loadingText="Checking out..."
            disabled={!canCheckOut(employee)}
            onClick={attendanceState.checkOut}
          >
            Check out
          </LoadingButton>
        </div>
      </form>
    </section>
  )
}
