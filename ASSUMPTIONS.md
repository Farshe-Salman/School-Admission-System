# Assumptions

While developing this School Admission System, I had to make a few decisions because some parts of the assignment were not explained in detail. I followed the requirements and chose the approaches that I thought would be practical for this project.

## 1. Student Identification

I used the `X-Student-Id` request header to identify a student.

The assignment mentioned using this header, so I followed that approach instead of implementing a complete login and registration system.

The frontend saves the student's ID in browser storage and sends it with student-related API requests.

For this assignment, I treated the student ID as an identifier, not as a secure authentication mechanism.

## 2. One Active Application per Student

I decided that one student can have only one active application across the entire system.

This means a student cannot hold seats in multiple classes at the same time.

I chose this rule to avoid one student reserving multiple seats unnecessarily. A student can apply again after the previous application expires or is cancelled.

A confirmed application remains active.

## 3. Hold Duration

I used 600 seconds (10 minutes) as the default hold duration.

The duration can be changed using the `HOLD_DURATION_SECONDS` environment variable.

I used this approach because the assignment requires the hold duration to be configurable.

All expiration timestamps are stored in UTC.

## 4. Automatic Hold Expiration

I implemented a background service that checks for expired holds periodically.

I did not want the system to depend on a student making another API request to release an expired seat.

When a hold expires, its status becomes `EXPIRED`, and the seat becomes available again.

I also check expiration during relevant hold operations to avoid treating an already expired hold as active.

## 5. Seat Availability

I considered only unexpired held applications and confirmed applications as occupied seats.

The calculation is:

```text
Available Seats = Total Seats - Held Seats - Confirmed Seats
```

Expired and cancelled applications do not consume seats.

## 6. Handling Concurrent Requests

The assignment describes a situation where many guardians may apply at the same time. Because of this, I considered concurrency an important part of the implementation.

I used PostgreSQL transaction-level advisory locks for student-level operations and row-level locks when allocating seats for a class.

I also added a database-level unique partial index to prevent a student from having multiple active applications.

I chose database-level locking instead of relying on application-level in-memory locks because multiple backend instances may access the same database.

## 7. Admission Fee and Confirmation

I assumed that a successful confirmation request means the admission fee has been successfully paid.

I did not integrate any real payment gateway because the assignment says that payment gateway integration is not required.

I also made the confirmation operation idempotent, so repeating a confirmation request does not create a duplicate confirmation.

## 8. Application Cancellation

A student can cancel their own active hold.

Before performing the operation, the system checks whether the hold belongs to the student identified by the `X-Student-Id` header.

I did not allow a student to cancel another student's application.

## 9. Class and Shift

I treated each class and shift combination as a separate admission class.

For example:

- Class 6 - Morning Shift
- Class 6 - Day Shift

Each one has its own seat capacity.

## 10. Database Persistence

I used a named Docker volume for PostgreSQL data.

This means stopping or recreating the application containers does not normally delete the admission records.

## 11. Configuration

I used environment variables for database configuration and hold duration.

The actual `.env` file is kept locally and excluded from Git. I included `.env.example` so that someone cloning the repository can create their own environment file.

The frontend uses `/api` as its API base path, and Nginx forwards those requests to the backend service.

## 12. Features Not Included

I kept the implementation focused on the assignment requirements.

I did not implement the following features because they were not required:

- Real payment gateway integration.
- Full authentication and registration.
- Admin role and permission management.
- Email or SMS notifications.
- Production cloud deployment.

These features can be considered for future improvements.
