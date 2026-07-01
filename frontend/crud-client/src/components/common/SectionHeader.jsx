export default function SectionHeader({ title, description, actions }) {
  return (
    <div className="section-header">
      <div>
        <h2>{title}</h2>
        {description ? <p>{description}</p> : null}
      </div>
      {actions}
    </div>
  )
}
