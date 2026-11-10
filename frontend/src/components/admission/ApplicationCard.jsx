
import { useCallback, useState } from 'react'
import {
    BookOpen,
    CheckCircle2,
    Clock3,
    XCircle,
    ShieldCheck,
    AlertCircle,
    Loader2,
    X,
    PartyPopper,
} from 'lucide-react'
import HoldCountdown from './HoldCountdown'

function ApplicationCard({ hold, onConfirm, onCancel, onRefresh }) {
    const [locallyExpired, setLocallyExpired] = useState(false)
    const [processing, setProcessing] = useState(false)
    const [modalOpen, setModalOpen] = useState(false)

    const status = hold.status?.toUpperCase()

    const handleExpire = useCallback(() => {
        setLocallyExpired(true)
        onRefresh?.(hold.holdId)
    }, [hold.holdId, onRefresh])

    const isHeld = status === 'HELD' && !locallyExpired

    const handleConfirm = async () => {
        if (processing || !isHeld) return

        setProcessing(true)

        try {
            const result = await onConfirm?.(hold.holdId)

            if (result !== false) {
                setModalOpen(true)
            }
        } finally {
            setProcessing(false)
        }
    }

    const handleCancel = async () => {
        if (processing || !isHeld) return

        setProcessing(true)

        try {
            await onCancel?.(hold.holdId)
        } finally {
            setProcessing(false)
        }
    }

    const statusConfig = {
        HELD: {
            label: 'Seat Reserved',
            className: 'status-held',
            icon: Clock3,
        },
        CONFIRMED: {
            label: 'Admitted',
            className: 'status-confirmed',
            icon: CheckCircle2,
        },
        CANCELLED: {
            label: 'Cancelled',
            className: 'status-cancelled',
            icon: XCircle,
        },
        EXPIRED: {
            label: 'Expired',
            className: 'status-expired',
            icon: Clock3,
        },
    }

    const currentStatus = locallyExpired && status === 'HELD'
        ? statusConfig.EXPIRED
        : statusConfig[status] || statusConfig.HELD

    const StatusIcon = currentStatus.icon

    return (
        <>
            <article className="application-card">
                <div className="application-card-header">
                    <div className="application-class-icon">
                        <BookOpen size={22} />
                    </div>

                    <div className="application-class-info">
                        <h3>{hold.className || 'Admission Class'}</h3>
                        <span>Application ID: {hold.holdId}</span>
                    </div>

                    <span className={`status-badge ${currentStatus.className}`}>
                        <StatusIcon size={14} />
                        {currentStatus.label}
                    </span>
                </div>

                {isHeld && hold.expiresAt && (
                    <HoldCountdown
                        expiresAt={hold.expiresAt}
                        onExpire={handleExpire}
                    />
                )}

                {isHeld && (
                    <div className="application-actions">
                        <button
                            className="confirm-button"
                            onClick={handleConfirm}
                            disabled={processing}
                        >
                            {processing ? (
                                <Loader2 size={16} className="spinning-icon" />
                            ) : (
                                <ShieldCheck size={16} />
                            )}
                            {processing ? 'Processing...' : 'Pay Admission Fee'}
                        </button>

                        <button
                            className="cancel-button"
                            onClick={handleCancel}
                            disabled={processing}
                        >
                            <XCircle size={16} />
                            Cancel Reservation
                        </button>
                    </div>
                )}

                {locallyExpired && status === 'HELD' && (
                    <p className="application-message expired-message">
                        <AlertCircle size={16} />
                        Your reservation time has ended. This seat is now available to other students.
                    </p>
                )}

                {status === 'CONFIRMED' && (
                    <p className="application-message confirmed-message">
                        <CheckCircle2 size={16} />
                        Congratulations! Your admission has been successfully confirmed.
                    </p>
                )}

                {status === 'CANCELLED' && (
                    <p className="application-message">
                        <XCircle size={16} />
                        This reservation was cancelled.
                    </p>
                )}

                {status === 'EXPIRED' && (
                    <p className="application-message expired-message">
                        <Clock3 size={16} />
                        This reservation expired before confirmation.
                    </p>
                )}
            </article>

            {modalOpen && (
                <div
                    className="admission-modal-overlay"
                    role="presentation"
                    onMouseDown={(event) => {
                        if (event.target === event.currentTarget) {
                            setModalOpen(false)
                        }
                    }}
                >
                    <div
                        className="admission-success-modal"
                        role="dialog"
                        aria-modal="true"
                        aria-labelledby="admission-success-title"
                    >
                        <button
                            className="admission-modal-close"
                            onClick={() => setModalOpen(false)}
                            aria-label="Close confirmation"
                        >
                            <X size={20} />
                        </button>

                        <div className="admission-success-icon">
                            <PartyPopper size={34} />
                        </div>

                        <span className="page-eyebrow">ADMISSION CONFIRMED</span>

                        <h2 id="admission-success-title">
                            Congratulations!
                        </h2>

                        <p>
                            Your admission has been successfully confirmed.
                            We are excited to welcome you!
                        </p>

                        <div className="admission-confirmation-details">
                            <span>Class</span>
                            <strong>{hold.className || 'Admission Class'}</strong>

                            <span>Application ID</span>
                            <strong>{hold.holdId}</strong>
                        </div>

                        <button
                            className="student-login-button"
                            onClick={() => setModalOpen(false)}
                        >
                            Continue
                            <CheckCircle2 size={17} />
                        </button>
                    </div>
                </div>
            )}
        </>
    )
}

export default ApplicationCard
