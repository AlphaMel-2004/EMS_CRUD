export const statusOptions = ['Working', 'Absent', 'Leave']
export const attendanceStates = ['CheckedIn', 'CheckedOut']

export function validateLogin(values) {
  const errors = {}
  if (!values.username.trim()) errors.username = 'Username is required.'
  if (!values.password) errors.password = 'Password is required.'
  return errors
}

export function validateEmployee(values, existingEmployees = [], editingId = null) {
  const errors = {}
  const username = values.username.trim().toLowerCase()

  if (!values.fullName.trim()) errors.fullName = 'Full name is required.'
  if (!isValidEmail(values.email)) errors.email = 'Enter a valid email address.'
  if (!values.department.trim()) errors.department = 'Department is required.'
  if (!values.position.trim()) errors.position = 'Position is required.'

  if (!editingId) {
    if (!username) errors.username = 'Username is required.'
    if (!isStrongPassword(values.password)) {
      errors.password = 'Use at least 8 characters with uppercase, lowercase, number, and symbol.'
    }
  }

  const duplicateUsername = existingEmployees.some((employee) =>
    employee.id !== editingId && employee.username?.toLowerCase() === username)
  if (username && duplicateUsername) errors.username = 'This username already appears in the table.'

  return errors
}

export function validateStatus(status) {
  return statusOptions.includes(status) ? '' : 'Choose a valid status.'
}

export function canCheckIn(employee) {
  return employee && employee.attendanceState !== 'CheckedIn' && !employee.isDeleted
}

export function canCheckOut(employee) {
  return employee && employee.attendanceState !== 'CheckedOut' && !employee.isDeleted
}

function isValidEmail(value) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim())
}

function isStrongPassword(value) {
  return value.length >= 8 &&
    /[A-Z]/.test(value) &&
    /[a-z]/.test(value) &&
    /\d/.test(value) &&
    /[^A-Za-z0-9]/.test(value)
}
