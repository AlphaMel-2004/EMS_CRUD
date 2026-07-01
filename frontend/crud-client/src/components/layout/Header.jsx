export default function Header({ role, onLogout }) {
  return (
    <header className="topbar">
      <div>
        <span className="eyebrow">{role} workspace</span>
        <h1>{role === 'Admin' ? 'Employee management' : 'My work dashboard'}</h1>
      </div>
      <button className="secondary-button" type="button" onClick={onLogout}>Sign out</button>
    </header>
  )
}
