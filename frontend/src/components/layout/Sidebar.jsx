
import {
    LayoutDashboard,
    GraduationCap,
    ClipboardList,
    Users,
    Settings,
    Sparkles,
} from 'lucide-react'

const navigation = [
    { label: 'Overview', icon: LayoutDashboard },
    { label: 'Admissions', icon: GraduationCap },
    { label: 'My Applications', icon: ClipboardList },
    { label: 'Manage Classes', icon: Users },
]

function Sidebar({ activePage, onNavigate, isOpen, onClose }) {
    return (
        <>
            {isOpen && (
                <button
                    className="sidebar-overlay"
                    onClick={onClose}
                    aria-label="Close navigation"
                />
            )}

            <aside className={`sidebar ${isOpen ? 'sidebar-open' : ''}`}>
                <div className="brand">
                    <div className="brand-icon">
                        <GraduationCap size={25} />
                    </div>
                    <div>
                        <h2>EduFlow</h2>
                        <span>Admission Portal</span>
                    </div>
                </div>

                <div className="sidebar-label">MENU</div>

                <nav className="navigation">
                    {navigation.map(({ label, icon: Icon }) => (
                        <button
                            key={label}
                            className={`nav-item ${activePage === label ? 'active' : ''}`}
                            onClick={() => {
                                onNavigate(label)
                                onClose()
                            }}
                        >
                            <Icon size={19} />
                            <span>{label}</span>
                        </button>
                    ))}
                </nav>

                <div className="sidebar-label tools-label">PREFERENCES</div>

                <button
                    className={`nav-item ${activePage === 'Settings' ? 'active' : ''}`}
                    onClick={() => onNavigate('Settings')}
                >
                    <Settings size={19} />
                    <span>Settings</span>
                </button>

                <div className="sidebar-bottom">
                    <div className="help-card">
                        <div className="help-icon">
                            <Sparkles size={19} />
                        </div>
                        <h4>Need assistance?</h4>
                        <p>We're here to help you with the admission process.</p>
                        <button type="button">
                            Contact Support
                        </button>
                    </div>

                    <div className="profile">
                        <div className="avatar">SA</div>
                        <div className="profile-info">
                            <strong>School Administrator</strong>
                            <span>Admission Portal</span>
                        </div>
                        <Settings size={17} />
                    </div>
                </div>
            </aside>
        </>
    )
}

export default Sidebar
