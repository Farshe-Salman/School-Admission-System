
import { useCallback, useEffect, useState } from 'react'
import {
    Users,
    Clock3,
    CheckCircle2,
    BookOpen,
    GraduationCap,
    ArrowUpRight,
    Plus,
    RefreshCw,
    Sparkles,
} from 'lucide-react'
import { api } from '../services/api'

function Dashboard({ onNavigate }) {
    const [classes, setClasses] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState('')

    const loadDashboard = useCallback(async () => {
        try {
            const data = await api.getClasses()
            setClasses(Array.isArray(data) ? data : [])
            setError('')
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }, [])

    useEffect(() => {
        const initialLoad = window.setTimeout(loadDashboard, 0)

        const intervalId = window.setInterval(loadDashboard, 5000)

        return () => {
            window.clearTimeout(initialLoad)
            window.clearInterval(intervalId)
        }
    }, [loadDashboard])

    const totals = classes.reduce(
        (result, item) => ({
            totalSeats: result.totalSeats + item.totalSeats,
            heldSeats: result.heldSeats + item.heldSeats,
            confirmedSeats: result.confirmedSeats + item.confirmedSeats,
            availableSeats: result.availableSeats + item.availableSeats,
        }),
        {
            totalSeats: 0,
            heldSeats: 0,
            confirmedSeats: 0,
            availableSeats: 0,
        }
    )

    const statistics = [
        {
            label: 'Total Classes',
            value: classes.length,
            caption: 'Registered admission classes',
            icon: BookOpen,
            color: 'purple',
        },
        {
            label: 'Seats on Hold',
            value: totals.heldSeats,
            caption: 'Temporary reservations',
            icon: Clock3,
            color: 'orange',
        },
        {
            label: 'Confirmed Admissions',
            value: totals.confirmedSeats,
            caption: 'Successfully confirmed',
            icon: CheckCircle2,
            color: 'green',
        },
        {
            label: 'Available Seats',
            value: totals.availableSeats,
            caption: 'Currently available seats',
            icon: Users,
            color: 'blue',
        },
    ]

    return (
        <section className="dashboard-page">
            <section className="welcome-section">
                <div>
                    <div className="eyebrow">
                        <Sparkles size={14} />
                        SCHOOL ADMISSION 2026–27
                    </div>

                    <h1>Welcome to EduFlow <span>👋</span></h1>

                    <p>
                        Manage admissions, monitor seat availability and track
                        application activities.
                    </p>
                </div>

                <button
                    className="date-button"
                    onClick={loadDashboard}
                    disabled={loading}
                >
                    <RefreshCw size={16} />
                    Refresh data
                </button>
            </section>

            <section className="hero-banner">
                <div className="hero-content">
                    <span className="hero-tag">ADMISSION SEASON IS LIVE</span>

                    <h2>
                        A brighter future
                        <br />
                        starts with education.
                    </h2>

                    <p>
                        Manage applications, reserve seats and make admission
                        simple for every family.
                    </p>

                    <button
                        className="hero-button"
                        onClick={() => onNavigate('Admissions')}
                    >
                        Explore Admissions <ArrowUpRight size={16} />
                    </button>
                </div>

                <div className="hero-decoration">
                    <div className="hero-circle circle-one" />
                    <div className="hero-circle circle-two" />
                    <GraduationCap size={115} strokeWidth={1.1} />

                    <div className="floating-card">
                        <CheckCircle2 size={19} />
                        <div>
                            <strong>Admission made easy</strong>
                            <span>Simple. Secure. Transparent.</span>
                        </div>
                    </div>
                </div>
            </section>

            {error && (
                <div className="feedback-message error-message" role="alert">
                    {error}
                    <button onClick={loadDashboard}>Try again</button>
                </div>
            )}

            <section className="stats-grid">
                {statistics.map(({ label, value, caption, icon: Icon, color }) => (
                    <div className="stat-card" key={label}>
                        <div className="stat-top">
                            <div className={`stat-icon ${color}`}>
                                <Icon size={20} />
                            </div>
                            <span className="stat-trend">Live data</span>
                        </div>

                        <p>{label}</p>
                        <h2>{loading ? '—' : value.toLocaleString()}</h2>
                        <span className="stat-caption">{caption}</span>
                    </div>
                ))}
            </section>

            <div className="dashboard-capacity">
                <strong>Total Seat Capacity</strong>
                <span>
                    {loading
                        ? 'Loading...'
                        : `${totals.totalSeats.toLocaleString()} seats across ${classes.length} classes`}
                </span>
            </div>

            <section className="content-grid">
                <div className="panel classes-panel">
                    <div className="panel-heading">
                        <div>
                            <h3>Admission Classes</h3>
                            <p>Current seat availability across classes.</p>
                        </div>

                        <button
                            className="text-button"
                            onClick={() => onNavigate('Admissions')}
                        >
                            View all <ArrowUpRight size={15} />
                        </button>
                    </div>

                    {loading ? (
                        <div className="classes-loading">Loading classes...</div>
                    ) : classes.length === 0 ? (
                        <div className="empty-state">
                            <div className="empty-icon">
                                <GraduationCap size={28} />
                            </div>
                            <h4>No classes created yet</h4>
                            <p>Create an admission class to get started.</p>

                            <button
                                className="primary-button"
                                onClick={() => onNavigate('Manage Classes')}
                            >
                                <Plus size={17} /> Create a class
                            </button>
                        </div>
                    ) : (
                        <div className="dashboard-class-list">
                            {classes.slice(0, 4).map((item) => (
                                <div className="dashboard-class-row" key={item.id}>
                                    <div className="admin-class-icon">
                                        <BookOpen size={20} />
                                    </div>

                                    <div className="dashboard-class-name">
                                        <strong>{item.name}</strong>
                                        <span>{item.totalSeats} total seats</span>
                                    </div>

                                    <div className="dashboard-class-availability">
                                        <strong>{item.availableSeats}</strong>
                                        <span>Available</span>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                <div className="panel activity-panel">
                    <div className="panel-heading">
                        <div>
                            <h3>Quick Actions</h3>
                            <p>Common admission activities.</p>
                        </div>
                    </div>

                    <button
                        className="action-card"
                        onClick={() => onNavigate('Admissions')}
                    >
                        <div className="action-icon purple">
                            <GraduationCap size={20} />
                        </div>
                        <div>
                            <strong>Browse Admissions</strong>
                            <span>Explore classes and reserve a seat</span>
                        </div>
                        <ArrowUpRight size={17} />
                    </button>

                    <button
                        className="action-card"
                        onClick={() => onNavigate('My Applications')}
                    >
                        <div className="action-icon orange">
                            <Clock3 size={20} />
                        </div>
                        <div>
                            <strong>My Applications</strong>
                            <span>Track your admission status</span>
                        </div>
                        <ArrowUpRight size={17} />
                    </button>

                    <button
                        className="action-card"
                        onClick={() => onNavigate('Manage Classes')}
                    >
                        <div className="action-icon green">
                            <Plus size={20} />
                        </div>
                        <div>
                            <strong>Create New Class</strong>
                            <span>Set up a new admission class</span>
                        </div>
                        <ArrowUpRight size={17} />
                    </button>
                </div>
            </section>
        </section>
    )
}

export default Dashboard
