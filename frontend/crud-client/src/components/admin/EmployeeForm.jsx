import { useEffect, useState } from 'react'
import Field from '../common/Field'
import LoadingButton from '../common/LoadingButton'
import { validateEmployee } from '../../utils/validation'

const emptyForm = {
  fullName: '',
  email: '',
  department: '',
  position: '',
  username: '',
  password: '',
}

export default function EmployeeForm({ editingEmployee, employees, onCreate, onUpdate, action, onCancel }) {
  const [form, setForm] = useState(emptyForm)
  const [errors, setErrors] = useState({})

  useEffect(() => {
    if (editingEmployee) {
      setForm({
        fullName: editingEmployee.fullName,
        email: editingEmployee.email,
        department: editingEmployee.department,
        position: editingEmployee.position,
        username: editingEmployee.username,
        password: '',
      })
    } else {
      setForm(emptyForm)
    }
    setErrors({})
  }, [editingEmployee])

  async function submit(event) {
    event.preventDefault()
    const nextErrors = validateEmployee(form, employees, editingEmployee?.id)
    setErrors(nextErrors)
    if (Object.keys(nextErrors).length) return

    if (editingEmployee) {
      await onUpdate(editingEmployee.id, pickEditableFields(form))
    } else {
      await onCreate(form)
      setForm(emptyForm)
    }
  }

  const isSaving = action === 'create' || action === `update-${editingEmployee?.id}`

  return (
    <form className="employee-form" onSubmit={submit}>
      <Field label="Full name" error={errors.fullName}>
        <input value={form.fullName} onChange={(event) => setForm({ ...form, fullName: event.target.value })} />
      </Field>
      <Field label="Email" error={errors.email}>
        <input value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} />
      </Field>
      <Field label="Department" error={errors.department}>
        <input value={form.department} onChange={(event) => setForm({ ...form, department: event.target.value })} />
      </Field>
      <Field label="Position" error={errors.position}>
        <input value={form.position} onChange={(event) => setForm({ ...form, position: event.target.value })} />
      </Field>
      {!editingEmployee ? (
        <>
          <Field label="Username" error={errors.username}>
            <input value={form.username} onChange={(event) => setForm({ ...form, username: event.target.value })} />
          </Field>
          <Field label="Password" error={errors.password}>
            <input type="password" value={form.password} onChange={(event) => setForm({ ...form, password: event.target.value })} />
          </Field>
        </>
      ) : null}
      <div className="button-row">
        <LoadingButton type="submit" loading={isSaving} loadingText="Saving...">
          {editingEmployee ? 'Update employee' : 'Create employee'}
        </LoadingButton>
        {editingEmployee ? <button type="button" className="secondary-button" onClick={onCancel}>Cancel</button> : null}
      </div>
    </form>
  )
}

function pickEditableFields(form) {
  return {
    fullName: form.fullName,
    email: form.email,
    department: form.department,
    position: form.position,
  }
}
