
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'

async function request(endpoint, options = {}) {
    const {
        studentId,
        headers = {},
        ...fetchOptions
    } = options

    const requestHeaders = {
        ...headers,
    }

    if (fetchOptions.body) {
        requestHeaders['Content-Type'] = 'application/json'
    }

    if (studentId) {
        requestHeaders['X-Student-Id'] = studentId
    }

    let response

    try {
        response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...fetchOptions,
            headers: requestHeaders,
        })
    } catch {
        throw new Error(
            'Unable to connect to the server. Please check your connection and try again.'
        )
    }

    if (response.status === 204) {
        return null
    }

    const data = await response.json().catch(() => null)

    if (!response.ok) {
        const message =
            data?.detail ||
            data?.message ||
            data?.title ||
            'Something went wrong. Please try again.'

        throw new Error(message)
    }

    return data
}

export const api = {
    getClasses: async () => {
        const data = await request('/classes')

        if (Array.isArray(data)) {
            return data
        }

        if (Array.isArray(data?.value)) {
            return data.value
        }

        return []
    },

    getClass: (classId) =>
        request(`/classes/${classId}`),

    createClass: (classData) =>
        request('/classes', {
            method: 'POST',
            body: JSON.stringify(classData),
        }),

    createHold: (classId, studentId) =>
        request(`/classes/${classId}/holds`, {
            method: 'POST',
            studentId,
        }),

    getHold: (holdId) =>
        request(`/holds/${holdId}`),

    confirmHold: (holdId, studentId) =>
        request(`/holds/${holdId}/confirm`, {
            method: 'POST',
            studentId,
        }),

    cancelHold: (holdId, studentId) =>
        request(`/holds/${holdId}`, {
            method: 'DELETE',
            studentId,
        }),

    getMyHolds: (studentId) =>
        request('/me/holds', {
            studentId,
        }),
}
