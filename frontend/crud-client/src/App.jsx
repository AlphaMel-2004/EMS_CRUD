import { useEffect, useMemo, useState } from 'react'
import './ui.css'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5299'

const roleMenus = {
  Admin: [
    { id: 'overview', label: 'Overview', icon: 'dashboard' },
    { id: 'employees', label: 'Employees', icon: 'users' },
    { id: 'attendance', label: 'Attendance', icon: 'clock' },
  ],
  Employee: [
    { id: 'overview', label: 'Overview', icon: 'dashboard' },
    { id: 'my-status', label: 'My Status', icon: 'badge' },
    { id: 'attendance', label: 'Attendance', icon: 'clock' },
  ],
}

const statusOptions = ['Working', 'Absent', 'Leave']

const emptyEmployeeForm = {
  fullName: '',
  email: '',
  department: '',
  position: '',
  username: '',
  password: '',
}

function App() {
  const [session, setSession] = useState(null)
  const [activeView, setActiveView] = useState('overview')
  const [employees, setEmployees] = useState([])
  const [attendance, setAttendance] = useState([])
  const [isLoading, setIsLoading] = useState(false)
  const [notice, setNotice] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    const rawSession = sessionStorage.getItem('crud-learning-session')
    if (rawSession) {
      setSession(JSON.parse(rawSession))
    }
  }, [])

  useEffect(() => {
    if (!session?.token) return

    sessionStorage.setItem('crud-learning-session', JSON.stringify(session))
  }, [session])

  useEffect(() => {
    if (!session?.token) return

    void loadData()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [session, activeView])

  const activeRole = session?.role ?? null
  const menu = activeRole ? roleMenus[activeRole] : []
  const employeeRecord = activeRole === 'Employee'
    ? employees.find((employee) => employee.id === session?.employeeId)
    : null

  const metrics = useMemo(() => {
    const liveEmployees = employees.filter((employee) => !employee.isDeleted)
    return {
      total: liveEmployees.length,
      working: liveEmployees.filter((employee) => employee.workStatus === 'Working').length,
      absent: liveEmployees.filter((employee) => employee.workStatus === 'Absent').length,
      leave: liveEmployees.filter((employee) => employee.workStatus === 'Leave').length,
    }
  }, [employees])

  async function apiFetch(path, options = {}) {
    const response = await fetch(`${API_BASE_URL}${path}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...(options.headers ?? {}),
        ...(session?.token ? { Authorization: `Bearer ${session.token}` } : {}),
      },
    })

    if (!response.ok) {
      const message = await response.text()
      throw new Error(message || 'Request failed')
    }

    if (response.status === 204) {
      return null
    }

    return response.json()
  }

  async function loadData() {
    setError('')
    setNotice('')

    try {
      if (activeRole === 'Admin') {
        const [employeeList, attendanceList] = await Promise.all([
          apiFetch('/api/employees'),
          activeView === 'attendance' ? apiFetch(`/api/employees/${session.employeeId ?? employeeRecord?.id ?? 0}/attendance`).catch(() => []) : Promise.resolve([]),
        ])

        setEmployees(employeeList)
        setAttendance(attendanceList)
      } else if (activeRole === 'Employee') {
        const [profile, attendanceList] = await Promise.all([
          apiFetch('/api/employees/me'),
          apiFetch(`/api/employees/${session.employeeId}/attendance`),
        ])

        setEmployees(profile ? [profile] : [])
        setAttendance(attendanceList)
      }
    } catch (fetchError) {
      setError(fetchError.message)
    }
  }

  async function handleLogin(loginPayload) {
    setIsLoading(true)
    setError('')
    try {
      const data = await apiFetch('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(loginPayload),
        headers: {},
      })

      setSession(data)
      setActiveView('overview')
      setNotice(`Welcome back, ${data.fullName ?? data.username}`)
    } catch (loginError) {
      setError('Invalid credentials. Check the username, password, and selected role.')
    } finally {
      setIsLoading(false)
    }
  }

  function handleLogout() {
    sessionStorage.removeItem('crud-learning-session')
    setSession(null)
    setEmployees([])
    setAttendance([])
    setNotice('')
    setError('')
  }

  async function refreshEmployees() {
    const data = await apiFetch('/api/employees')
    setEmployees(data)
  }

  async function handleCreateEmployee(formValue) {
    await apiFetch('/api/employees', {
      method: 'POST',
      body: JSON.stringify(formValue),
    })

    setNotice('Employee created successfully.')
    await refreshEmployees()
  }

  async function handleUpdateEmployee(id, formValue) {
    await apiFetch(`/api/employees/${id}`, {
      method: 'PUT',
      body: JSON.stringify(formValue),
    })

    setNotice('Employee updated successfully.')
    await refreshEmployees()
  }

  async function handleSoftDeleteEmployee(id) {
    await apiFetch(`/api/employees/${id}`, {
      method: 'DELETE',
    })

    setNotice('Employee soft deleted.')
    await refreshEmployees()
  }

  async function handleStatusChange(id, workStatus) {
    await apiFetch(`/api/employees/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ workStatus }),
    })

    setNotice('Status updated.')
    await refreshEmployees()
  }

  async function handleCheckIn(id) {
    await apiFetch(`/api/employees/${id}/check-in`, { method: 'POST' })
    setNotice('Check-in saved.')
    await loadData()
  }

  async function handleCheckOut(id) {
    await apiFetch(`/api/employees/${id}/check-out`, { method: 'POST' })
    setNotice('Check-out saved.')
    await loadData()
  }

  if (!session) {
    return <LoginPage onLogin={handleLogin} loading={isLoading} error={error} notice={notice} />
  }

  return (
    <DashboardLayout
      role={activeRole}
      user={session}
      activeView={activeView}
      onNavigate={setActiveView}
      onLogout={handleLogout}
      menu={menu}
      error={error}
      notice={notice}
    >
      {activeRole === 'Admin' ? (
        <AdminDashboard
          employees={employees}
          attendance={attendance}
          metrics={metrics}
          onCreate={handleCreateEmployee}
          onUpdate={handleUpdateEmployee}
          onDelete={handleSoftDeleteEmployee}
          onStatusChange={handleStatusChange}
          activeView={activeView}
          onRefresh={loadData}
        />
      ) : (
        <EmployeeDashboard
          employee={employeeRecord ?? employees[0]}
          attendance={attendance}
          onStatusChange={handleStatusChange}
          onCheckIn={handleCheckIn}
          onCheckOut={handleCheckOut}
          onRefresh={loadData}
        />
      )}
    </DashboardLayout>
  )
}

function LoginPage({ onLogin, loading, error, notice }) {
  const [role, setRole] = useState('Admin')
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  function submit(event) {
    event.preventDefault()
    void onLogin({ username, password, role })
  }

  return (
    <main className="auth-shell">
      <section className="auth-copy panel">
        <span className="eyebrow">Employee management system</span>
        <h1>Sign in to the dashboard built for operations, attendance, and access control.</h1>
        <p>
          Use the standard login below. Admin and Employee accounts land in different views with
          role-based permissions enforced by the API.
        </p>
        <div className="quick-cards">
          <StatCard label="Admin access" value="Create, edit, soft delete" />
          <StatCard label="Employee access" value="Status, check-in, check-out" />
        </div>
      </section>

      <section className="panel auth-form-panel">
        <LoginForm
          role={role}
          setRole={setRole}
          username={username}
          setUsername={setUsername}
          password={password}
          setPassword={setPassword}
          onSubmit={submit}
          loading={loading}
        />
        {error ? <p className="alert error">{error}</p> : null}
        {notice ? <p className="alert success">{notice}</p> : null}
      </section>
    </main>
  )
}

function DashboardLayout({ role, user, activeView, onNavigate, onLogout, menu, error, notice, children }) {
  return (
    <main className="app-shell">
      <Sidebar role={role} user={user} activeView={activeView} onNavigate={onNavigate} onLogout={onLogout} menu={menu} />
      <section className="workspace">
        <header className="topbar panel">
          <div>
            <span className="eyebrow">{role} workspace</span>
            <h1>{role === 'Admin' ? 'Employee command center' : 'My work dashboard'}</h1>
          </div>
          <div className="topbar-actions">
            <button className="ghost-button" type="button" onClick={onLogout}>
              Sign out
            </button>
          </div>
        </header>

        {error ? <p className="alert error">{error}</p> : null}
        {notice ? <p className="alert success">{notice}</p> : null}

        {children}
      </section>
    </main>
  )
}

function Sidebar({ role, user, activeView, onNavigate, onLogout, menu }) {
  return (
    <aside className="sidebar panel">
      <div className="brand-block">
        <div className="brand-mark">CL</div>
        <div>
          <strong>Crud Learning</strong>
          <span>{role} portal</span>
        </div>
      </div>

      <div className="profile-card">
        <p>{user.fullName ?? user.username}</p>
        <span>{user.role}</span>
      </div>

      <nav className="sidebar-nav">
        {menu.map((item) => (
          <SidebarButton
            key={item.id}
            active={activeView === item.id}
            label={item.label}
            icon={item.icon}
            onClick={() => onNavigate(item.id)}
          />
        ))}
      </nav>

      <button className="logout-button" type="button" onClick={onLogout}>
        Sign out
      </button>
    </aside>
  )
}

function AdminDashboard({ employees, attendance, metrics, onCreate, onUpdate, onDelete, onStatusChange, activeView, onRefresh }) {
  const [editingEmployee, setEditingEmployee] = useState(null)
  const [employeeForm, setEmployeeForm] = useState(emptyEmployeeForm)

  useEffect(() => {
    if (editingEmployee) {
      setEmployeeForm({
        fullName: editingEmployee.fullName,
        email: editingEmployee.email,
        department: editingEmployee.department,
        position: editingEmployee.position,
        username: editingEmployee.username,
        password: '',
      })
    }
  }, [editingEmployee])

  async function submitEmployee(event) {
    event.preventDefault()

    if (editingEmployee) {
      await onUpdate(editingEmployee.id, employeeForm)
    } else {
      await onCreate(employeeForm)
      setEmployeeForm(emptyEmployeeForm)
    }

    setEditingEmployee(null)
  }

  return (
    <div className="dashboard-grid">
      <section className="kpi-grid">
        <StatCard label="Employees" value={metrics.total} />
        <StatCard label="Working" value={metrics.working} tone="good" />
        <StatCard label="Absent" value={metrics.absent} tone="warn" />
        <StatCard label="Leave" value={metrics.leave} tone="muted" />
      </section>

      {activeView === 'overview' ? <OverviewPanel employees={employees} /> : null}

      {activeView === 'employees' ? (
        <section className="panel section-card">
          <SectionHeader
            title={editingEmployee ? 'Edit employee' : 'Create employee'}
            description="Admins can create, update, and soft delete employee records."
          />
          <EmployeeForm
            value={employeeForm}
            onChange={setEmployeeForm}
            onSubmit={submitEmployee}
            submitLabel={editingEmployee ? 'Update employee' : 'Create employee'}
            onCancel={editingEmployee ? () => setEditingEmployee(null) : null}
          />

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
                {employees.length ? (
                  employees.map((employee) => (
                    <tr key={employee.id} className={employee.isDeleted ? 'is-deleted' : ''}>
                      <td>
                        <strong>{employee.fullName}</strong>
                        <span>{employee.email}</span>
                      </td>
                      <td>{employee.department}</td>
                      <td>{employee.position}</td>
                      <td>
                        <select value={employee.workStatus} onChange={(event) => onStatusChange(employee.id, event.target.value)}>
                          {statusOptions.map((option) => (
                            <option key={option} value={option}>{option}</option>
                          ))}
                        </select>
                      </td>
                      <td>{employee.attendanceState}</td>
                      <td className="row-actions">
                        <button type="button" onClick={() => setEditingEmployee(employee)}>Edit</button>
                        <button type="button" className="danger" onClick={() => onDelete(employee.id)}>Soft delete</button>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan="6">
                      <EmptyState
                        title="No employees yet"
                        description="Create the first employee record to unlock the admin workflow."
                      />
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </section>
      ) : null}

      {activeView === 'attendance' ? (
        <section className="panel section-card">
          <SectionHeader title="Recent attendance" description="Latest check-in and check-out activity." />
          <div className="activity-list">
            {attendance.length ? (
              attendance.map((entry) => (
                <article className="activity-item" key={entry.id}>
                  <div>
                    <strong>{entry.employeeName}</strong>
                    <p>{entry.note ?? 'Recorded in the system'}</p>
                  </div>
                  <span>{entry.eventType}</span>
                </article>
              ))
            ) : (
              <EmptyState
                title="No attendance yet"
                description="Attendance logs will appear here after the first check-in or check-out."
              />
            )}
          </div>
          <button className="ghost-button" type="button" onClick={onRefresh}>Refresh</button>
        </section>
      ) : null}
    </div>
  )
}

function EmployeeDashboard({ employee, attendance, onStatusChange, onCheckIn, onCheckOut, onRefresh }) {
  const [status, setStatus] = useState(employee?.workStatus ?? 'Working')

  useEffect(() => {
    setStatus(employee?.workStatus ?? 'Working')
  }, [employee])

  async function handleStatusSubmit(event) {
    event.preventDefault()
    await onStatusChange(employee.id, status)
  }

  return (
    <div className="dashboard-grid">
      <section className="kpi-grid">
        <StatCard label="Current status" value={employee?.workStatus ?? 'Working'} tone="good" />
        <StatCard label="Attendance" value={employee?.attendanceState ?? 'CheckedOut'} />
        <StatCard label="Last check-in" value={formatDate(employee?.lastCheckInAt)} />
        <StatCard label="Last check-out" value={formatDate(employee?.lastCheckOutAt)} />
      </section>

      <section className="panel section-card">
        <SectionHeader title="My status" description="Update your availability and mark your attendance." />

        {!employee ? (
          <EmptyState
            title="No employee profile found"
            description="Your account does not have an employee record yet. Ask an admin to assign one."
          />
        ) : (
          <form className="employee-actions" onSubmit={handleStatusSubmit}>
            <label>
              Work status
              <select value={status} onChange={(event) => setStatus(event.target.value)}>
                {statusOptions.map((option) => (
                  <option key={option} value={option}>{option}</option>
                ))}
              </select>
            </label>

            <div className="button-row">
              <button type="submit">Save status</button>
              <button type="button" className="ghost-button" onClick={() => onCheckIn(employee.id)}>Check in</button>
              <button type="button" className="ghost-button" onClick={() => onCheckOut(employee.id)}>Check out</button>
            </div>
          </form>
        )}
      </section>

      <section className="panel section-card">
        <SectionHeader title="Recent activity" description="Your latest attendance events." />
        <div className="activity-list">
          {attendance.length ? (
            attendance.map((entry) => (
              <article className="activity-item" key={entry.id}>
                <div>
                  <strong>{entry.eventType}</strong>
                  <p>{formatDate(entry.occurredAt)}</p>
                </div>
                <span>{entry.note ?? 'Logged by the system'}</span>
              </article>
            ))
          ) : (
            <EmptyState
              title="No activity yet"
              description="Your attendance timeline will appear here after your first check-in or status update."
            />
          )}
        </div>
        <button className="ghost-button" type="button" onClick={onRefresh}>Refresh</button>
      </section>
    </div>
  )
}

function OverviewPanel({ employees }) {
  const latest = employees.slice(0, 3)

  return (
    <section className="panel section-card">
      <SectionHeader title="Workspace summary" description="A quick glance at employee activity and status." />
      <div className="summary-grid">
        {latest.length ? (
          latest.map((employee) => (
            <article className="summary-card" key={employee.id}>
              <strong>{employee.fullName}</strong>
              <p>{employee.department}</p>
              <span>{employee.workStatus} · {employee.attendanceState}</span>
            </article>
          ))
        ) : (
          <EmptyState
            title="No employee overview yet"
            description="This section fills once the first employee profile is created."
          />
        )}
      </div>
    </section>
  )
}

function EmployeeForm({ value, onChange, onSubmit, submitLabel, onCancel }) {
  return (
    <form className="employee-form" onSubmit={onSubmit}>
      <Field label="Full name">
        <input value={value.fullName} onChange={(event) => onChange({ ...value, fullName: event.target.value })} />
      </Field>
      <Field label="Email">
        <input value={value.email} onChange={(event) => onChange({ ...value, email: event.target.value })} />
      </Field>
      <Field label="Department">
        <input value={value.department} onChange={(event) => onChange({ ...value, department: event.target.value })} />
      </Field>
      <Field label="Position">
        <input value={value.position} onChange={(event) => onChange({ ...value, position: event.target.value })} />
      </Field>
      <Field label="Username">
        <input value={value.username} onChange={(event) => onChange({ ...value, username: event.target.value })} />
      </Field>
      <Field label="Password">
        <input type="password" value={value.password} onChange={(event) => onChange({ ...value, password: event.target.value })} />
      </Field>

      <div className="button-row">
        <button type="submit">{submitLabel}</button>
        {onCancel ? <button type="button" className="ghost-button" onClick={onCancel}>Cancel</button> : null}
      </div>
    </form>
  )
}

function LoginForm({ role, setRole, username, setUsername, password, setPassword, onSubmit, loading }) {
  return (
    <form className="login-form" onSubmit={onSubmit}>
      <div className="role-switcher">
        <button type="button" className={role === 'Admin' ? 'pill active' : 'pill'} onClick={() => setRole('Admin')}>Admin</button>
        <button type="button" className={role === 'Employee' ? 'pill active' : 'pill'} onClick={() => setRole('Employee')}>Employee</button>
      </div>

      <Field label="Username">
        <input value={username} onChange={(event) => setUsername(event.target.value)} placeholder="Your username" autoComplete="username" />
      </Field>

      <Field label="Password">
        <input type="password" value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Your password" autoComplete="current-password" />
      </Field>

      <button type="submit" disabled={loading}>
        {loading ? 'Signing in...' : 'Sign in'}
      </button>

      <div className="hint-box secure-note">
        <strong>Secure sign-in</strong>
        <p>Credentials are not shown in the UI and session data expires when the browser session ends.</p>
      </div>
    </form>
  )
}

function Field({ label, children }) {
  return (
    <label className="field">
      <span>{label}</span>
      {children}
    </label>
  )
}

function StatCard({ label, value, tone = 'default' }) {
  return (
    <article className={`stat-card ${tone}`}>
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  )
}

function SectionHeader({ title, description }) {
  return (
    <div className="section-header">
      <div>
        <h2>{title}</h2>
        <p>{description}</p>
      </div>
    </div>
  )
}

function EmptyState({ title, description }) {
  return (
    <div className="empty-state">
      <strong>{title}</strong>
      <p>{description}</p>
    </div>
  )
}

function SidebarButton({ active, label, icon, onClick }) {
  return (
    <button type="button" className={active ? 'sidebar-button active' : 'sidebar-button'} onClick={onClick}>
      <span className="sidebar-icon" aria-hidden="true">{iconGlyph(icon)}</span>
      {label}
    </button>
  )
}

function iconGlyph(name) {
  const glyphs = {
    dashboard: '▣',
    users: '◫',
    clock: '◷',
    badge: '◆',
  }

  return glyphs[name] ?? '•'
}

function formatDate(value) {
  if (!value) return 'Not yet recorded'
  return new Date(value).toLocaleString()
}

export default App