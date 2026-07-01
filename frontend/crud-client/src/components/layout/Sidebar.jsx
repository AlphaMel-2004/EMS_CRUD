const menus = {
  Admin: [
    { id: 'overview', label: 'Overview' },
    { id: 'employees', label: 'Employees' },
    { id: 'attendance', label: 'Attendance' },
    { id: 'reports', label: 'Reports' },
  ],
  Employee: [
    { id: 'overview', label: 'Overview' },
    { id: 'status', label: 'My Status' },
    { id: 'attendance', label: 'Attendance' },
  ],
}

export default function Sidebar({ role, user, activeView, onNavigate, onLogout }) {
  return (
    <aside className="sidebar">
      <div className="brand-block">
        <div className="brand-mark">EMS</div>
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
        {(menus[role] ?? []).map((item) => (
          <button
            key={item.id}
            type="button"
            className={activeView === item.id ? 'sidebar-button active' : 'sidebar-button'}
            onClick={() => onNavigate(item.id)}
          >
            {item.label}
          </button>
        ))}
      </nav>

      <button className="secondary-button" type="button" onClick={onLogout}>Sign out</button>
    </aside>
  )
}
