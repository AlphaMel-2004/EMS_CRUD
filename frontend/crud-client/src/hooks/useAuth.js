import { useEffect, useMemo, useState } from 'react'
import { getCurrentUser, login } from '../api/authApi'

const storageKey = 'crud-learning-session'

export function useAuth() {
  const [session, setSession] = useState(null)
  const [error, setError] = useState('')
  const [isLoggingIn, setIsLoggingIn] = useState(false)

  useEffect(() => {
    const rawSession = sessionStorage.getItem(storageKey)
    if (rawSession) {
      setSession(JSON.parse(rawSession))
    }
  }, [])

  useEffect(() => {
    if (!session) return
    // sessionStorage is acceptable for this learning app. Production apps should
    // prefer short-lived access tokens and secure refresh-token handling.
    sessionStorage.setItem(storageKey, JSON.stringify(session))
  }, [session])

  const token = session?.token
  const role = session?.role

  async function signIn(values) {
    setIsLoggingIn(true)
    setError('')
    try {
      const data = await login(values)
      setSession(data)
      return data
    } catch (loginError) {
      setError(loginError.message || 'Invalid username or password.')
      return null
    } finally {
      setIsLoggingIn(false)
    }
  }

  async function refreshCurrentUser() {
    if (!token) return null
    const data = await getCurrentUser(token)
    setSession((current) => ({ ...current, ...data, token }))
    return data
  }

  function signOut() {
    sessionStorage.removeItem(storageKey)
    setSession(null)
    setError('')
  }

  return useMemo(() => ({
    session,
    token,
    role,
    error,
    isLoggingIn,
    signIn,
    signOut,
    refreshCurrentUser,
  }), [session, token, role, error, isLoggingIn])
}
