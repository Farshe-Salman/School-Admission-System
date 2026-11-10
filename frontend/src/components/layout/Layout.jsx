
import { useState } from 'react'
import Sidebar from './Sidebar'
import Header from './Header'

function Layout({ activePage, onNavigate, children }) {
    const [sidebarOpen, setSidebarOpen] = useState(false)

    const handleCloseSidebar = () => {
        setSidebarOpen(false)
    }

    return (
        <div className="app-shell">
            <Sidebar
                activePage={activePage}
                onNavigate={onNavigate}
                isOpen={sidebarOpen}
                onClose={handleCloseSidebar}
            />

            <main className="main-content">
                <Header
                    activePage={activePage}
                    onMenuClick={() => setSidebarOpen(true)}
                />

                <div className="page-content">
                    {children}
                </div>
            </main>
        </div>
    )
}

export default Layout
