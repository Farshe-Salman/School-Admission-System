import { useState } from 'react'
import { BookOpen, Armchair, ArrowRight } from 'lucide-react'

function ClassCard({ classData, onHold, booking = false }) {
    const [holding, setHolding] = useState(false)
    const { id, name, totalSeats, heldSeats, confirmedSeats, availableSeats } = classData
    const occupiedSeats = heldSeats + confirmedSeats
    const occupiedPercentage = totalSeats > 0 ? Math.min(100, (occupiedSeats / totalSeats) * 100) : 0
    const hasSeats = availableSeats > 0

    const handleBookSeat = async () => {
        if (!hasSeats || holding || booking) return
        setHolding(true)
        try { await onHold(id) } finally { setHolding(false) }
    }

    return (
        <article className="class-card">
            <div className="class-card-top">
                <div className="class-card-icon"><BookOpen size={23} /></div>
                <span className={`availability-badge ${hasSeats ? 'available' : 'full'}`}><span className="availability-dot" />{hasSeats ? 'Seats Available' : 'Class Full'}</span>
            </div>
            <div className="class-card-content">
                <h3>{name}</h3>
                <p className="class-description">School admission · Academic Session 2026–27</p>
            </div>
            <div className="seat-summary">
                <div><span>Total Seats</span><strong>{totalSeats}</strong></div>
                <div><span>On Hold</span><strong>{heldSeats}</strong></div>
                <div><span>Confirmed</span><strong>{confirmedSeats}</strong></div>
            </div>
            <div className="seat-progress-heading"><span>Seat Occupancy</span><strong>{Math.round(occupiedPercentage)}%</strong></div>
            <div className="seat-progress" role="progressbar" aria-label={`${name} seat occupancy`} aria-valuenow={occupiedSeats} aria-valuemin={0} aria-valuemax={totalSeats}>
                <div className="seat-progress-fill" style={{ width: `${occupiedPercentage}%` }} />
            </div>
            <div className="class-card-footer">
                <div className="available-count"><Armchair size={19} /><div><strong>{availableSeats}</strong><span>Seats Remaining</span></div></div>
                <button className="hold-seat-button" onClick={handleBookSeat} disabled={!hasSeats || holding || booking} title={!hasSeats ? 'No seats are currently available' : 'Reserve this class'}>
                    {holding || booking ? 'Checking...' : !hasSeats ? 'Class Full' : <>Book Seat <ArrowRight size={16} /></>}
                </button>
            </div>
        </article>
    )
}

export default ClassCard
