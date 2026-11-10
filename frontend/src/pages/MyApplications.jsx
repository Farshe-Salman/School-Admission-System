
import { useCallback, useEffect, useRef, useState } from 'react'
import {
    ClipboardList,
    Search,
    RefreshCw,
    GraduationCap,
    ArrowRight,
    ShieldCheck,
} from 'lucide-react'
import { api } from '../services/api'
import ApplicationCard from '../components/admission/ApplicationCard'

function MyApplications({
    studentId,
    onStudentIdChange,
    authenticated,
    onAuthenticationChange,
}) {
    const [entryId, setEntryId] = useState('')
    const [applications, setApplications] = useState([])
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState('')
    const [message, setMessage] = useState('')

    // Cache class details to avoid duplicate API requests.
    const classCache = useRef(new Map())

    const loadApplications = useCallback(async (id) => {
        if (!id?.trim()) return

        try {
            const data = await api.getMyHolds(id.trim())
            const holds = Array.isArray(data) ? data : []

            // Fetch details for each unique Class ID.
            const enrichedApplications = await Promise.all(
                holds.map(async (hold) => {
                    const classId = hold.classId

                    if (!classId) {
                        return {
                            ...hold,
                            className: 'Admission Class',
                        }
                    }

                    try {
                        let classDetails = classCache.current.get(classId)

                        if (!classDetails) {
                            const classRequest = api.getClass(classId)
                            classCache.current.set(classId, classRequest)

                            try {
                                classDetails = await classRequest
                                classCache.current.set(classId, classDetails)
                            } catch (err) {
                                classCache.current.delete(classId)
                                throw err
                            }
                        } else {
                            classDetails = await classDetails
                        }

                        return {
                            ...hold,
                            className: classDetails?.name || 'Admission Class',
                        }
                    } catch {
                        return {
                            ...hold,
                            className: 'Admission Class',
                        }
                    }
                })
            )

            setApplications(enrichedApplications)
            setError('')
        } catch (err) {
            setError(err.message)
        }
    }, [])

    // BRN login
    const handleSearch = async (event) => {
        event.preventDefault()

        const normalizedId = entryId.trim()

        if (!normalizedId) {
            setError('Please enter your Birth Registration Number.')
            return
        }

        setLoading(true)
        setError('')
        setMessage('')

        try {
            const data = await api.getMyHolds(normalizedId)

            onStudentIdChange(normalizedId)
            setApplications(Array.isArray(data) ? data : [])
            onAuthenticationChange(true)

            // Load class names after login.
            await loadApplications(normalizedId)
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    // Automatic application refresh.
    useEffect(() => {
        if (!authenticated || !studentId?.trim()) return

        const id = studentId.trim()

        const initialLoad = window.setTimeout(() => {
            loadApplications(id)
        }, 0)

        const intervalId = window.setInterval(() => {
            loadApplications(id)
        }, 5000)

        return () => {
            window.clearTimeout(initialLoad)
            window.clearInterval(intervalId)
        }
    }, [authenticated, studentId, loadApplications])

    // Confirm admission.
    const handleConfirm = async (holdId) => {
        setError('')
        setMessage('')

        try {
            await api.confirmHold(holdId, studentId)

            setMessage('Congratulations! Your admission has been confirmed.')

            await loadApplications(studentId)
        } catch (err) {
            setError(err.message)
            await loadApplications(studentId)
            throw err
        }
    }

    // Cancel reservation.
    const handleCancel = async (holdId) => {
        const confirmed = window.confirm(
            'Are you sure you want to cancel this reservation?'
        )

        if (!confirmed) return false

        setError('')
        setMessage('')

        try {
            await api.cancelHold(holdId, studentId)

            setMessage('Your reservation has been cancelled.')

            await loadApplications(studentId)
            return true
        } catch (err) {
            setError(err.message)
            await loadApplications(studentId)
            throw err
        }
    }

    // Change student.
    const handleLogout = () => {
        onAuthenticationChange(false)
        onStudentIdChange('')

        setEntryId('')
        setApplications([])
        setError('')
        setMessage('')

        classCache.current.clear()
    }

    // BRN entry screen.
    if (!authenticated) {
        return (
            <section className="student-login-page">
                <div className="student-login-card">
                    <div className="student-login-icon">
                        <GraduationCap size={34} />
                    </div>

                    <span className="page-eyebrow">STUDENT PORTAL</span>

                    <h1>Welcome Back!</h1>

                    <p>
                        Enter your Birth Registration Number to access your
                        admission applications.
                    </p>

                    <form onSubmit={handleSearch}>
                        <label htmlFor="application-student-id">
                            Birth Registration Number
                        </label>

                        <div className="login-input-wrapper">
                            <Search size={19} />

                            <input
                                id="application-student-id"
                                type="text"
                                value={entryId}
                                onChange={(event) => setEntryId(event.target.value)}
                                placeholder="Enter your registration number"
                                autoComplete="off"
                                required
                            />
                        </div>

                        {error && (
                            <div className="feedback-message error-message" role="alert">
                                {error}
                            </div>
                        )}

                        <button
                            type="submit"
                            className="student-login-button"
                            disabled={loading}
                        >
                            {loading
                                ? 'Checking...'
                                : 'Continue to My Applications'}

                            {!loading && <ArrowRight size={17} />}
                        </button>
                    </form>

                    <div className="login-security-note">
                        <ShieldCheck size={16} />
                        <span>
                            Your applications are retrieved using your
                            registration number.
                        </span>
                    </div>
                </div>
            </section>
        )
    }

    // Student application dashboard.
    return (
        <section className="applications-page">
            <div className="applications-heading">
                <div>
                    <span className="page-eyebrow">STUDENT PORTAL</span>
                    <h1>My Applications</h1>
                    <p>Track your reservations and manage your admission status.</p>
                </div>

                <div className="application-header-actions">
                    <button
                        className="refresh-button"
                        onClick={() => loadApplications(studentId)}
                        disabled={loading}
                    >
                        <RefreshCw size={16} />
                        Refresh
                    </button>

                    <button
                        className="secondary-button"
                        onClick={handleLogout}
                    >
                        Change Student
                    </button>
                </div>
            </div>

            <div className="student-welcome-panel">
                <div className="search-panel-icon">
                    <GraduationCap size={24} />
                </div>

                <div>
                    <h3>Student Account</h3>
                    <p>Birth Registration Number: {studentId}</p>
                </div>
            </div>

            {error && (
                <div className="feedback-message error-message" role="alert">
                    {error}
                </div>
            )}

            {message && (
                <div className="feedback-message success-message" role="status">
                    {message}
                </div>
            )}

            <div className="applications-list-heading">
                <div>
                    <h2>Your Applications</h2>
                    <p>
                        {applications.length} application
                        {applications.length !== 1 ? 's' : ''} found
                    </p>
                </div>
            </div>

            {applications.length === 0 ? (
                <div className="applications-empty">
                    <div className="empty-icon">
                        <ClipboardList size={28} />
                    </div>

                    <h3>No applications found</h3>

                    <p>
                        You have not made any admission reservations yet.
                        Visit Admissions to book a seat.
                    </p>
                </div>
            ) : (
                <div className="applications-list">
                    {applications.map((hold) => (
                        <ApplicationCard
                            key={hold.holdId}
                            hold={hold}
                            onConfirm={handleConfirm}
                            onCancel={handleCancel}
                            onRefresh={() => loadApplications(studentId)}
                        />
                    ))}
                </div>
            )}
        </section>
    )
}

export default MyApplications
