# Testing Evidence — School Admission System

This directory contains the testing results and supporting evidence for the School Admission System.

It includes concurrency testing, backend automated tests, and manual frontend testing.

## 1. Concurrency Testing

**Tool:** Grafana k6  
**Test Script:** `../concurrency-test.js`

The concurrency test evaluates whether the admission system can handle simultaneous seat reservation requests without exceeding the available seat capacity.

### Test Configuration

| Metric                   |       Value |
| ------------------------ | ----------: |
| Virtual users configured |         500 |
| Total requests           |         500 |
| Test duration            | 4.9 seconds |
| Maximum observed VUs     |         489 |

### Test Results

| Result                       | Count |
| ---------------------------- | ----: |
| Successful holds (HTTP 201)  |   120 |
| Rejected requests (HTTP 409) |   380 |
| Unexpected responses         |     0 |
| Total requests               |   500 |

### Database Verification

After the concurrency test, PostgreSQL was queried to verify the final seat allocation.

| Database Metric      | Result |
| -------------------- | -----: |
| Held seats           |    120 |
| Confirmed seats      |      0 |
| Total active seats   |    120 |
| Class capacity       |    120 |
| Overbooking detected |     No |

**Result: PASSED**

The system accepted exactly 120 reservations for a class with 120 available seats. The remaining 380 requests were correctly rejected with HTTP 409 (Conflict).

No unexpected responses were recorded.

### Concurrency Test Evidence

- `backend-concurrency-summary.json` — Structured summary of the test results and database verification.
- `../concurrency-test.js` — Reproducible k6 test script.

The terminal output and PostgreSQL query results are available in `backend-concurrency-screensort/`.

## 2. Backend Automated Tests

**Framework:** xUnit  
**Project:** `backend/SchoolAdmission.Tests`

### Test Results

| Test Category                   | Test Cases | Result                  |
| ------------------------------- | ---------: | ----------------------- |
| AdmissionClass Unit Tests       |          3 | Passed                  |
| AdmissionHold Unit Tests        |          5 | Passed                  |
| AdmissionHoldService Unit Tests |          7 | Passed                  |
| **Total**                       |     **15** | **15 Passed, 0 Failed** |

### Test Coverage

- Class creation and input validation.
- Hold creation and initial status.
- Hold confirmation before expiration.
- Hold expiration.
- Hold cancellation.
- Idempotent confirmation.
- Successful hold creation through the service.
- Prevention of multiple active applications by the same student.
- Class capacity validation.
- Invalid class and student ID validation.
- Handling of nonexistent classes.

### Execute Backend Tests

Run from the repository root:

```powershell
dotnet test backend/SchoolAdmission.Tests/SchoolAdmission.Tests.csproj
```

### Generate the TRX Test Report

```powershell
dotnet test backend/SchoolAdmission.Tests/SchoolAdmission.Tests.csproj --logger "trx;LogFileName=backend-tests.trx" --results-directory ./test-results
```

**Execution Report:** `backend-tests.trx`

The TRX file contains the actual automated test execution results.

**Note:** Domain and service unit tests use fake dependencies. They verify business logic but do not replace database integration tests.

## 3. Manual Frontend Testing

**Evidence:** `Manual_Frontend_Testing_Report_with_Frontend-triggered_concurrent_hold_stress_test.pdf`

The report contains screenshots and results from manually performed frontend test scenarios.

### Tested Scenarios

- Birth Registration Number validation.
- Seat booking and hold creation.
- Hold countdown and automatic expiration.
- Confirming admission.
- Cancelling a reservation.
- Preventing multiple active applications.
- Displaying updated seat counts.
- Full-capacity handling.
- Frontend-triggered concurrent hold stress testing.

The PDF provides visual evidence of the tested frontend workflows.

## 4. Evidence Directory

| Evidence                           | File                                                                                     |
| ---------------------------------- | ---------------------------------------------------------------------------------------- |
| k6 concurrency test summary        | `backend-concurrency-summary.json`                                                       |
| Backend automated test results     | `backend-tests.trx`                                                                      |
| Manual frontend testing report     | `Manual_Frontend_Testing_Report_with_Frontend-triggered_concurrent_hold_stress_test.pdf` |
| k6 concurrency terminal screenshot | `backend-concurrency-screensort/concurrency-k6-results.png`                              |
| PostgreSQL verification screenshot | `backend-concurrency-screensort/database-seat-verification.png`                          |
| k6 concurrency test script         | `../concurrency-test.js`                                                                 |
