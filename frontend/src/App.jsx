
import { useState } from 'react'
import Layout from './components/layout/Layout'
import Dashboard from './pages/Dashboard'
import Admissions from './pages/Admissions'
import MyApplications from './pages/MyApplications'
import ManageClasses from './pages/ManageClasses'
import './App.css'

function App() {
    // Restore the last visited page after browser refresh.
    const [activePage, setActivePage] = useState(
        () => localStorage.getItem('activePage') || 'Overview'
    )

    // Restore the student's Birth Registration Number.
    const [studentId, setStudentId] = useState(
        () => localStorage.getItem('studentId') || ''
    )

    // Restore student portal session.
    const [studentAuthenticated, setStudentAuthenticated] = useState(
        () => localStorage.getItem('studentPortalAuthenticated') === 'true'
    )

    // Centralized navigation.
    const handleNavigate = (page) => {
        setActivePage(page)
        localStorage.setItem('activePage', page)
    }

    // Update and persist student identity.
    const handleStudentIdChange = (value) => {
        setStudentId(value)

        if (value.trim()) {
            localStorage.setItem('studentId', value.trim())
        } else {
            localStorage.removeItem('studentId')
        }
    }

    // Update student portal authentication state.
    const handleStudentAuthentication = (authenticated) => {
        setStudentAuthenticated(authenticated)

        localStorage.setItem(
            'studentPortalAuthenticated',
            String(authenticated)
        )
    }

    const renderPage = () => {
        switch (activePage) {
            case 'Admissions':
                return (
                    <Admissions
                        studentId={studentId}
                        onStudentIdChange={handleStudentIdChange}
                        onNavigate={handleNavigate}
                        onBookingSuccess={() => {
                            handleStudentAuthentication(true)
                            handleNavigate('My Applications')
                        }}
                    />
                )

            case 'My Applications':
                return (
                    <MyApplications
                        studentId={studentId}
                        onStudentIdChange={handleStudentIdChange}
                        authenticated={studentAuthenticated}
                        onAuthenticationChange={handleStudentAuthentication}
                    />
                )

            case 'Manage Classes':
                return <ManageClasses />

            case 'Overview':
            default:
                return <Dashboard onNavigate={handleNavigate} />
        }
    }

    return (
        <Layout
            activePage={activePage}
            onNavigate={handleNavigate}
        >
            {renderPage()}
        </Layout>
    )
}

export default App
