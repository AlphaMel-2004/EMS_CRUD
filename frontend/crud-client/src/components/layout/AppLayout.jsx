import Header from './Header'
import Sidebar from './Sidebar'
import ErrorMessage from '../common/ErrorMessage'

export default function AppLayout({ role, user, activeView, onNavigate, onLogout, error, notice, children }) {
  return (
    <main className="app-shell">
      <Sidebar role={role} user={user} activeView={activeView} onNavigate={onNavigate} onLogout={onLogout} />
      <section className="workspace">
        <Header role={role} onLogout={onLogout} />
        <ErrorMessage message={error} />
        <ErrorMessage message={notice} tone="success" />
        {children}
      </section>
    </main>
  )
}
