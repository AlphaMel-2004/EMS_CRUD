import { useState } from 'react'
import ErrorMessage from '../common/ErrorMessage'
import Field from '../common/Field'
import LoadingButton from '../common/LoadingButton'
import StatCard from '../common/StatCard'
import { validateLogin } from '../../utils/validation'

export default function LoginPage({ onLogin, loading, error }) {
  const [form, setForm] = useState({ username: '', password: '' })
  const [errors, setErrors] = useState({})

  async function submit(event) {
    event.preventDefault()
    const nextErrors = validateLogin(form)
    setErrors(nextErrors)
    if (Object.keys(nextErrors).length) return
    await onLogin(form)
  }

  return (
    <main className="auth-shell">
      <section className="auth-copy">
        <span className="eyebrow">Employee management system</span>
        <h1>Clean operations, attendance, and employee records.</h1>
        <p>Sign in with the seeded admin or employee account to manage daily EMS workflows.</p>
        <div className="quick-cards">
          <StatCard label="Admin" value="Records and reports" />
          <StatCard label="Employee" value="Status and attendance" />
        </div>
      </section>

      <section className="auth-form-panel">
        <form className="login-form" onSubmit={submit}>
          <Field label="Username" error={errors.username}>
            <input
              value={form.username}
              onChange={(event) => setForm({ ...form, username: event.target.value })}
              autoComplete="username"
            />
          </Field>
          <Field label="Password" error={errors.password}>
            <input
              type="password"
              value={form.password}
              onChange={(event) => setForm({ ...form, password: event.target.value })}
              autoComplete="current-password"
            />
          </Field>
          <LoadingButton type="submit" loading={loading} loadingText="Logging in...">
            Sign in
          </LoadingButton>
          <ErrorMessage message={error} />
        </form>
      </section>
    </main>
  )
}
