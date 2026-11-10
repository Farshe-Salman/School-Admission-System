import { useCallback, useEffect, useRef, useState } from 'react'
import { AlertCircle, CheckCircle2, GraduationCap, RefreshCw, X } from 'lucide-react'
import { api } from '../services/api'
import ClassCard from '../components/admission/ClassCard'

function Admissions({ studentId, onStudentIdChange, onBookingSuccess }) {
    const [classes, setClasses] = useState([])
    const [loading, setLoading] = useState(true)
    const [brnError, setBrnError] = useState('')
    const [requestError, setRequestError] = useState('')
    const [dialog, setDialog] = useState(null)
    const [booking, setBooking] = useState(false)
    const studentInputRef = useRef(null)

    const loadClasses = useCallback(async () => {
        try {
            const data = await api.getClasses()
            setClasses(Array.isArray(data) ? data : [])
            setRequestError('')
        } catch (err) {
            setRequestError(err.message || 'Unable to load classes. Please try again.')
        } finally {
            setLoading(false)
        }
    }, [])

    useEffect(() => {
        const initialLoad = window.setTimeout(loadClasses, 0)
        const intervalId = window.setInterval(loadClasses, 5000)
        return () => {
            window.clearTimeout(initialLoad)
            window.clearInterval(intervalId)
        }
    }, [loadClasses])

    const handleStudentIdChange = (event) => {
        onStudentIdChange(event.target.value)
        if (event.target.value.trim()) setBrnError('')
        setDialog(null)
    }

    const showExistingApplication = (holds) => {
        const active = holds.find((hold) => {
            const status = String(hold.status || '').toUpperCase()
            return status === 'HELD' || status === 'CONFIRMED'
        })
        if (!active) return false

        const confirmed = String(active.status).toUpperCase() === 'CONFIRMED'
        setDialog(confirmed ? {
            kind: 'admitted',
            title: 'Already admitted',
            message: 'You already have a confirmed admission. You cannot apply for another class.',
            action: 'View My Applications',
        } : {
            kind: 'reserved',
            title: 'You already have a seat reserved',
            message: 'You currently have an active seat reservation. Please complete your admission or cancel this reservation before applying for another class.',
            action: 'View My Applications',
        })
        return true
    }

    const handleBookSeat = async (classId) => {
        const normalizedId = studentId.trim()
        if (!normalizedId) {
            setBrnError('Please enter your Birth Registration Number before booking a seat.')
            studentInputRef.current?.focus()
            studentInputRef.current?.scrollIntoView({ behavior: 'smooth', block: 'center' })
            return
        }

        if (booking) return
        setBooking(true)
        setBrnError('')
        setRequestError('')

        try {
            // Friendly pre-check; the backend remains the authoritative validator.
            const existingHolds = await api.getMyHolds(normalizedId)
            if (showExistingApplication(Array.isArray(existingHolds) ? existingHolds : [])) return

            await api.createHold(classId, normalizedId)
            await loadClasses()
            onBookingSuccess?.()
        } catch (err) {
            const message = err.message || 'We could not reserve this seat. Please try again.'
            if (/active application|already have|already admitted|already reserved/i.test(message)) {
                setDialog({
                    kind: 'reserved',
                    title: 'You already have an active application',
                    message: 'Our records show that you already have an active admission application. You cannot reserve another class.',
                    action: 'View My Applications',
                })
            } else {
                setDialog({
                    kind: 'error',
                    title: 'Unable to book this seat',
                    message,
                    action: 'Close',
                })
            }
            await loadClasses()
        } finally {
            setBooking(false)
        }
    }

    const totalSeats = classes.reduce((total, item) => total + item.totalSeats, 0)
    const availableSeats = classes.reduce((total, item) => total + item.availableSeats, 0)

    return (
        <section className="admissions-page">
            <div className="admissions-heading">
                <div>
                    <span className="page-eyebrow">SCHOOL ADMISSION 2026–27</span>
                    <h1>Explore Admissions</h1>
                    <p>Find your preferred class and reserve your admission seat.</p>
                </div>
                <button className="refresh-button" onClick={loadClasses} disabled={loading}>
                    <RefreshCw size={16} /> Refresh
                </button>
            </div>

            <div className="admission-overview">
                <div className="overview-icon"><GraduationCap size={26} /></div>
                <div>
                    <strong>{classes.length} Classes Available</strong>
                    <span>{availableSeats} of {totalSeats} total seats currently available</span>
                </div>
            </div>

            <div className={`student-id-panel ${brnError ? 'has-error' : ''}`}>
                <div className="student-id-copy">
                    <h3>Student Information</h3>
                    <p>Enter your Birth Registration Number to continue.</p>
                    <span className="privacy-hint">Your number identifies your admission record and existing reservations.</span>
                </div>
                <div className="student-id-field-wrap">
                    <label htmlFor="birth-registration-number">Birth Registration Number <span>Required</span></label>
                    <input
                        ref={studentInputRef}
                        id="birth-registration-number"
                        className="student-id-input"
                        type="text"
                        value={studentId}
                        onChange={handleStudentIdChange}
                        onKeyDown={(event) => { if (event.key === 'Enter') studentInputRef.current?.blur() }}
                        placeholder="Enter your Birth Registration Number"
                        aria-invalid={Boolean(brnError)}
                        aria-describedby={brnError ? 'brn-error' : 'brn-hint'}
                        autoComplete="off"
                    />
                    {brnError ? (
                        <p className="brn-validation" id="brn-error" role="alert"><AlertCircle size={15} />{brnError}</p>
                    ) : (
                        <p className="brn-hint" id="brn-hint"><CheckCircle2 size={14} />Use the same number whenever you check your application.</p>
                    )}
                </div>
            </div>

            {requestError && <div className="feedback-message error-message" role="alert">{requestError}</div>}

            <div className="classes-section-heading">
                <div><h2>Available Classes</h2><p>Choose a class to book your admission seat.</p></div>
                <span>Live availability · Updates every 5 seconds</span>
            </div>

            {loading ? (
                <div className="classes-loading">Loading admission classes...</div>
            ) : classes.length === 0 ? (
                <div className="applications-empty">
                    <div className="empty-icon"><GraduationCap size={28} /></div>
                    <h3>No classes available yet</h3>
                    <p>Please check again later or contact the school administrator.</p>
                </div>
            ) : (
                <div className="classes-grid">
                    {classes.map((classData) => (
                        <ClassCard key={classData.id} classData={classData} onHold={handleBookSeat} booking={booking} />
                    ))}
                </div>
            )}

            {dialog && (
                <div className="admission-feedback-overlay" role="presentation" onMouseDown={(event) => { if (event.target === event.currentTarget) setDialog(null) }}>
                    <section className={`admission-feedback-dialog ${dialog.kind}`} role="alertdialog" aria-modal="true" aria-labelledby="admission-feedback-title">
                        <button className="admission-feedback-close" onClick={() => setDialog(null)} aria-label="Close dialog"><X size={19} /></button>
                        <div className="admission-feedback-icon">{dialog.kind === 'admitted' ? <GraduationCap size={27} /> : dialog.kind === 'error' ? <AlertCircle size={26} /> : <CheckCircle2 size={26} />}</div>
                        <h2 id="admission-feedback-title">{dialog.title}</h2>
                        <p>{dialog.message}</p>
                        <button className="admission-feedback-action" onClick={() => {
                            const shouldNavigate = dialog.action === 'View My Applications'
                            setDialog(null)
                            if (shouldNavigate) onBookingSuccess?.()
                        }}>{dialog.action}</button>
                    </section>
                </div>
            )}
        </section>
    )
}

export default Admissions
