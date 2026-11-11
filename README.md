# School Admission Management System

A Dockerized school admission application built with ASP.NET Core, React, and PostgreSQL.

The system supports admission class management, temporary seat reservations, admission confirmation, cancellation, automatic hold expiration, and student-specific application history.

## Technology Stack

| Layer               | Technology              |
| ------------------- | ----------------------- |
| Backend             | C#, ASP.NET Core        |
| ORM                 | Entity Framework Core   |
| Database            | PostgreSQL              |
| Frontend            | React, JavaScript, Vite |
| Web server          | Nginx                   |
| Containerization    | Docker, Docker Compose  |
| Backend tests       | xUnit                   |
| Concurrency testing | k6                      |

## Main Features

- Create, list, and retrieve admission classes.
- View total, held, confirmed, and available seats.
- Create temporary seat holds for students.
- Confirm or cancel a student's own hold.
- Automatically expire holds when their time limit is reached.
- Prevent a student from maintaining multiple active applications.
- Protect seat allocation during concurrent requests.
- Preserve admission data in PostgreSQL across container restarts.
- Identify students through the `X-Student-Id` request header.

## Project Structure

```text
SchoolAdmission/
├── backend/
│   ├── SchoolAdmission.API/
│   ├── SchoolAdmission.Business/
│   ├── SchoolAdmission.Data/
│   ├── SchoolAdmission.Domain/
│   └── SchoolAdmission.Tests/
├── frontend/
├── test-results/
├── docker-compose.yml
├── .env.example
├── README.md
├── ASSUMPTIONS.md
└── AI_USAGE.md
```

The backend follows a layered structure:

- **API:** HTTP endpoints, request handling, and response generation.
- **Business:** Application services, business rules, and repository contracts.
- **Data:** Entity Framework Core, PostgreSQL access, and repository implementations.
- **Domain:** Core entities, validation, and domain state transitions.
- **Tests:** Backend automated tests.

The frontend is served through Nginx. Nginx forwards `/api` requests to the backend service inside the Docker Compose network.

## Run the Application with Docker

### Prerequisites

Install and start **Docker Desktop** with Docker Compose support. You do not need to install PostgreSQL, the .NET SDK, Node.js, npm, or Visual Studio separately to run the application through Docker.

Git is needed for the clone instructions below. Alternatively, download the repository as a ZIP file and extract it.

### 1. Clone the repository

```bash
git clone https://github.com/Farshe-Salman/School-Admission-System.git
cd School-Admission-System
```

### 2. Create the environment file

On Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

On Linux/macOS:

```bash
cp .env.example .env
```

The example file contains development database settings. You can edit `.env` and choose your own database password before starting the application. The same values are used by Docker Compose for PostgreSQL and the backend connection string.

### 3. Start the complete application

```bash
docker compose up --build -d
```

On the first run, Docker needs an internet connection to download the required base images and build dependencies.

### 4. Open the application

Visit:

**http://localhost:8080**

### 5. Check the containers

```bash
docker compose ps
```

To view logs:

```bash
docker compose logs -f
```

To stop the application:

```bash
docker compose down
```

The PostgreSQL data is stored in a named Docker volume. Running `docker compose down` does not remove that volume, so data is retained when the containers are started again.

> Do not use `docker compose down -v` if you want to preserve the database volume.

## First-Time Setup and Manual Testing

A new installation creates a **new, empty database**. Therefore, the first time the application is opened, there may be no admission classes or applications to display. This is expected; it does not mean the application has failed.

### Step 1: Create the first admission class

1. Open `http://localhost:8080`.
2. Go to **Manage Class** (or the class-management section in the interface).
3. Choose the option to create/add a class.
4. Enter the required class details, including its name/shift and seat capacity.
5. Save the class.
6. Return to the class list or dashboard and confirm that the new class and its seat count are displayed.

Create a class with a small capacity (for example, **2 seats**) if you want to test the capacity limit easily.

### Step 2: Test a student seat hold

1. Open the student/application area of the frontend.
2. Enter a student BRN/ID when prompted.
3. Select the class you created.
4. Create a seat hold.
5. Confirm that the application appears with `HELD` status and that the available seat count decreases.

The frontend sends the student's identifier through the `X-Student-Id` request header.

### Step 3: Test the one-active-application rule

1. Keep the first student's hold active.
2. Try to create another application using the same student ID, either for the same class or another class.
3. The system should reject the second active application.
4. Use a different student ID to create another hold if you want to test another seat.

This implementation allows only one active application per student across the system.

### Step 4: Test confirmation

1. Open the student's application/hold.
2. Use the confirmation action for the active hold.
3. Confirm that its status changes to `CONFIRMED`.
4. Check that the confirmed seat continues to count as occupied.
5. If you repeat the confirmation request, it should not create a duplicate confirmation.

### Step 5: Test cancellation

1. Create a new hold using a different student ID.
2. Cancel that student's held application.
3. Confirm that its status changes to `CANCELLED`.
4. Check that the released seat becomes available again.

A student should only be able to cancel their own application.

### Step 6: Test automatic expiration

The default hold duration is 600 seconds (10 minutes).

1. Create a hold using a student ID that has no other active application.
2. Do not confirm or cancel it.
3. Wait for the configured hold duration to pass.
4. Check the application and class seat count again.
5. The hold should become `EXPIRED`, and its seat should become available.

Expiration is handled by the backend background cleanup service; it does not require the student to manually call an API to release the seat. The frontend should reflect the updated status and seat availability.

### Step 7: Test the capacity limit

1. Create a class with a capacity of 2 seats.
2. Create holds for two different student IDs.
3. Try to create a third hold using a third student ID.
4. The third request should be rejected because the class is full (HTTP `409 Conflict`).
5. Confirm that the active held and confirmed seats do not exceed 2.

### Step 8: Test data persistence

1. Create a class and at least one application.
2. Stop the application containers:

   ```bash
   docker compose down
   ```

3. Start the application again:

   ```bash
   docker compose up -d
   ```

4. Open `http://localhost:8080`.
5. Check that the previously created class and application data are still present.

Do not run `docker compose down -v` for this test because `-v` removes the named database volume.

## Configuration

The root `.env.example` file contains:

```env
POSTGRES_DB=SchoolAdmissionDb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_postgres_password_here
```

The actual `.env` file is intended for local configuration and is excluded from Git.

The backend database connection is supplied through Docker environment variables. The frontend uses `/api` as its API base path, and Nginx forwards those requests to the backend container.

The hold duration can be configured through:

```env
HOLD_DURATION_SECONDS=600
```

The default duration is 600 seconds (10 minutes) when no override is supplied.

## API Overview

The main API endpoints are:

| Method   | Endpoint                   | Purpose                                |
| -------- | -------------------------- | -------------------------------------- |
| `POST`   | `/classes`                 | Create an admission class              |
| `GET`    | `/classes`                 | List classes and seat counts           |
| `GET`    | `/classes/{classId}`       | Get a specific class                   |
| `POST`   | `/classes/{classId}/holds` | Create a seat hold                     |
| `GET`    | `/holds/{holdId}`          | Get hold details                       |
| `POST`   | `/holds/{holdId}/confirm`  | Confirm a hold                         |
| `DELETE` | `/holds/{holdId}`          | Cancel a hold                          |
| `GET`    | `/me/holds`                | Get the current student's applications |

Student-specific endpoints use the `X-Student-Id` HTTP header.

A hold can have one of these statuses:

- `HELD`
- `CONFIRMED`
- `EXPIRED`
- `CANCELLED`

A request to create a hold when no seats are available returns HTTP `409 Conflict`.

## Concurrency and Seat Allocation

Seat allocation uses PostgreSQL transaction-level advisory locks and row-level locks. A database-level unique partial index provides an additional safeguard against multiple active applications for the same student.

The intent is to keep active held and confirmed seats within the configured class capacity, including when requests arrive concurrently.

## Testing

### Backend automated tests

Backend tests use xUnit. The recorded test run contains:

| Result      | Count |
| ----------- | ----: |
| Total tests |    15 |
| Passed      |    15 |
| Failed      |     0 |

The test result file is:

`test-results/backend-tests.trx`

### Concurrent seat-hold stress test

A k6 stress test was run against a class with a capacity of 120 seats.

| Metric                            | Recorded result |
| --------------------------------- | --------------: |
| Configured virtual users          |             500 |
| Completed iterations              |             500 |
| Successful holds                  |             120 |
| Capacity rejections               |             380 |
| Unexpected responses              |               0 |
| Active seats verified in database |             120 |
| Class capacity                    |             120 |

The database verification showed that active seats did not exceed the configured capacity in this test.

The k6 HTTP failure percentage includes expected `409 Conflict` responses for requests rejected because the class was full. These are expected business outcomes, not unexpected application errors.

### Test evidence

Additional evidence is stored in `test-results/`, including:

- Backend automated test results.
- k6 concurrency test output.
- Database seat-count verification screenshot.
- Backend concurrency summary.
- Manual frontend testing report.

## Notes

- This project is a take-home assignment implementation, not a production payment system.
- Payment gateway integration is not included; confirmation represents successful payment for assignment purposes.
- Student identification uses `X-Student-Id` as specified in the assignment. It is not a complete authentication system.
- Do not commit or publicly share your local `.env` file.
