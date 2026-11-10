
import { useCallback, useEffect, useState } from 'react'
import {
    Plus,
    BookOpen,
    Users,
    Armchair,
    RefreshCw,
    X,
} from 'lucide-react'
import { api } from '../services/api'

function ManageClasses() {
    const [classes, setClasses] = useState([])
    const [loading, setLoading] = useState(true)
    const [submitting, setSubmitting] = useState(false)
    const [showForm, setShowForm] = useState(false)
    const [error, setError] = useState('')
    const [message, setMessage] = useState('')

    const [formData, setFormData] = useState({
        name: '',
        totalSeats: '',
    })

    const loadClasses = useCallback(async () => {
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
        const timeoutId = window.setTimeout(() => {
            loadClasses()
        }, 0)

        // Automatically refresh class data every 5 seconds.
        const intervalId = window.setInterval(() => {
            loadClasses()
        }, 5000)

        return () => {
            window.clearTimeout(timeoutId)
            window.clearInterval(intervalId)
        }
    }, [loadClasses])


    const handleChange = (event) => {
        const { name, value } = event.target

        setFormData((previous) => ({
            ...previous,
            [name]: value,
        }))
    }

    const handleSubmit = async (event) => {
        event.preventDefault()

        const name = formData.name.trim()
        const totalSeats = Number(formData.totalSeats)

        if (!name) {
            setError('Please enter a class name.')
            return
        }

        if (!Number.isInteger(totalSeats) || totalSeats <= 0) {
            setError('Total seats must be a positive whole number.')
            return
        }

        setSubmitting(true)
        setError('')
        setMessage('')

        try {
            await api.createClass({ name, totalSeats })

            setMessage(`"${name}" has been created successfully.`)
            setFormData({ name: '', totalSeats: '' })
            setShowForm(false)

            await loadClasses()
        } catch (err) {
            setError(err.message)
        } finally {
            setSubmitting(false)
        }
    }

    return (
        <section className="manage-classes-page">
            <div className="manage-heading">
                <div>
                    <span className="page-eyebrow">ADMINISTRATION</span>
                    <h1>Manage Classes</h1>
                    <p>Create and manage admission classes and seat capacity.</p>
                </div>

                <button
                    className="primary-button create-class-button"
                    onClick={() => {
                        setShowForm(true)
                        setError('')
                        setMessage('')
                    }}
                >
                    <Plus size={17} />
                    Create New Class
                </button>
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

            {showForm && (
                <div className="class-form-panel">
                    <div className="class-form-heading">
                        <div>
                            <h2>Create Admission Class</h2>
                            <p>Enter the class information and seat capacity.</p>
                        </div>

                        <button
                            className="close-form-button"
                            onClick={() => setShowForm(false)}
                            aria-label="Close form"
                        >
                            <X size={19} />
                        </button>
                    </div>

                    <form onSubmit={handleSubmit}>
                        <div className="form-fields">
                            <label>
                                Class Name
                                <input
                                    type="text"
                                    name="name"
                                    value={formData.name}
                                    onChange={handleChange}
                                    placeholder="e.g. Class 6"
                                    maxLength={100}
                                    required
                                />
                            </label>

                            <label>
                                Total Seats
                                <input
                                    type="number"
                                    name="totalSeats"
                                    value={formData.totalSeats}
                                    onChange={handleChange}
                                    placeholder="e.g. 120"
                                    min="1"
                                    step="1"
                                    required
                                />
                            </label>
                        </div>

                        <div className="form-actions">
                            <button
                                type="button"
                                className="secondary-button"
                                onClick={() => setShowForm(false)}
                            >
                                Cancel
                            </button>

                            <button
                                type="submit"
                                className="primary-button"
                                disabled={submitting}
                            >
                                <Plus size={16} />
                                {submitting ? 'Creating...' : 'Create Class'}
                            </button>
                        </div>
                    </form>
                </div>
            )}

            <div className="classes-section-heading">
                <div>
                    <h2>All Admission Classes</h2>
                    <p>{classes.length} classes registered</p>
                </div>

                <button
                    className="refresh-button"
                    onClick={loadClasses}
                    disabled={loading}
                >
                    <RefreshCw size={16} />
                    Refresh
                </button>
            </div>

            {loading ? (
                <div className="classes-loading">Loading classes...</div>
            ) : classes.length === 0 ? (
                <div className="applications-empty">
                    <div className="empty-icon">
                        <BookOpen size={28} />
                    </div>
                    <h3>No classes created yet</h3>
                    <p>Create your first admission class to get started.</p>

                    <button
                        className="primary-button"
                        onClick={() => setShowForm(true)}
                    >
                        <Plus size={16} />
                        Create First Class
                    </button>
                </div>
            ) : (
                <div className="admin-classes-list">
                    {classes.map((item) => (
                        <article className="admin-class-card" key={item.id}>
                            <div className="admin-class-icon">
                                <BookOpen size={22} />
                            </div>

                            <div className="admin-class-details">
                                <h3>{item.name}</h3>
                                <span>Admission Class</span>
                            </div>

                            <div className="admin-class-stat">
                                <Users size={16} />
                                <strong>{item.totalSeats}</strong>
                                <span>Total seats</span>
                            </div>

                            <div className="admin-class-stat">
                                <Armchair size={16} />
                                <strong>{item.availableSeats}</strong>
                                <span>Available</span>
                            </div>
                        </article>
                    ))}
                </div>
            )}
        </section>
    )
}

export default ManageClasses
