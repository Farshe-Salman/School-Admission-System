
import { useEffect, useState } from 'react'
import { Clock3 } from 'lucide-react'

function getRemainingSeconds(expiresAt) {
    const expiryTime = new Date(expiresAt).getTime()

    if (!Number.isFinite(expiryTime)) {
        return 0
    }

    return Math.max(0, Math.ceil((expiryTime - Date.now()) / 1000))
}

function formatTime(seconds) {
    const minutes = Math.floor(seconds / 60)
    const remainingSeconds = seconds % 60

    return `${String(minutes).padStart(2, '0')}:${String(remainingSeconds).padStart(2, '0')}`
}

function HoldCountdown({ expiresAt, onExpire }) {
    const [remaining, setRemaining] = useState(
        () => getRemainingSeconds(expiresAt)
    )

    useEffect(() => {
        let hasExpired = false

        const updateCountdown = () => {
            const seconds = getRemainingSeconds(expiresAt)

            setRemaining(seconds)

            if (seconds === 0 && !hasExpired) {
                hasExpired = true
                onExpire?.()
            }
        }

        updateCountdown()

        const intervalId = window.setInterval(updateCountdown, 1000)

        return () => window.clearInterval(intervalId)
    }, [expiresAt, onExpire])

    const isExpired = remaining === 0

    return (
        <div className={`hold-countdown ${isExpired ? 'expired' : ''}`}>
            <Clock3 size={17} />

            <div className="countdown-info">
                <span>{isExpired ? 'Reservation expired' : 'Time remaining'}</span>
                <strong>{formatTime(remaining)}</strong>
            </div>
        </div>
    )
}

export default HoldCountdown
