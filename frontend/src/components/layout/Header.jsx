
import {
    Bell,
    Menu,
    Search,
} from 'lucide-react'

function Header({ activePage, onMenuClick }) {
    return (
        <header className="topbar">
            <button
                className="mobile-menu"
                onClick={onMenuClick}
                aria-label="Open navigation menu"
            >
                <Menu size={22} />
            </button>

            <div className="breadcrumb">
                <span>Pages</span>
                <span>/</span>
                <strong>{activePage}</strong>
            </div>

            <div className="topbar-actions">
                <div className="search-box">
                    <Search size={17} />
                    <input
                        type="search"
                        placeholder="Search anything..."
                        aria-label="Search"
                    />
                    <kbd>⌘ K</kbd>
                </div>

                <button
                    className="icon-button"
                    aria-label="Notifications"
                    type="button"
                >
                    <Bell size={19} />
                    <span className="notification-dot" />
                </button>

                <div className="top-avatar" aria-label="Administrator profile">
                    SA
                </div>
            </div>
        </header>
    )
}

export default Header
