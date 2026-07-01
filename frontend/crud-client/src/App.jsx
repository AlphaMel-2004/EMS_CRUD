import { useEffect, useState } from 'react'
import './styles/app.css'
import AdminDashboard from './components/admin/AdminDashboard'
import LoginPage from './components/auth/LoginPage'
import EmployeeDashboard from './components/employee/EmployeeDashboard'
import AppLayout from './components/layout/AppLayout'
import { useAttendance } from './hooks/useAttendance'
import { useAuth } from './hooks/useAuth'
import { useEmployees } from './hooks/useEmployees'

function App() {
  const auth = useAuth()
  const [activeView, setActiveView] = useState('overview')
  const employeesState = useEmployees(auth.token, auth.role, auth.session?.employeeId)
  const employee = employeesState.employee
  const attendanceEmployeeId = auth.role === 'Admin'
    ? employeesState.employees[0]?.id
    : auth.session?.employeeId
  const attendanceState = useAttendance(auth.token, attendanceEmployeeId)

  useEffect(() => {
    setActiveView('overview')
  }, [auth.role])

  async function handleLogin(values) {
    const session = await auth.signIn(values)
    if (session) setActiveView('overview')
  }

  if (!auth.session) {
    return <LoginPage onLogin={handleLogin} loading={auth.isLoggingIn} error={auth.error} />
  }

  return (
    <AppLayout
      role={auth.role}
      user={auth.session}
      activeView={activeView}
      onNavigate={setActiveView}
      onLogout={auth.signOut}
      error={employeesState.error || attendanceState.error}
    >
      {auth.role === 'Admin' ? (
        <AdminDashboard
          token={auth.token}
          employeesState={employeesState}
          attendanceState={attendanceState}
          activeView={activeView}
        />
      ) : (
        <EmployeeDashboard
          employee={employee}
          employeesState={employeesState}
          attendanceState={attendanceState}
          activeView={activeView}
        />
      )}
    </AppLayout>
  )
}

export default App
