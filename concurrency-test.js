import http from "k6/http";
import { Counter } from "k6/metrics";

export const options = {
  scenarios: {
    seat_race: {
      executor: "per-vu-iterations",
      vus: 500,
      iterations: 1,
      maxDuration: "2m",
      gracefulStop: "0s",
    },
  },
  insecureSkipTLSVerify: true,
};

const successfulHolds = new Counter("successful_holds");
const rejectedHolds = new Counter("rejected_holds");
const unexpectedResponses = new Counter("unexpected_responses");

const BASE_URL = __ENV.BASE_URL || "https://host.docker.internal:7073";
const CLASS_ID = __ENV.CLASS_ID;

export default function () {
  const studentId = `CAPACITY-TESTING-${__VU}`;

  const response = http.post(`${BASE_URL}/classes/${CLASS_ID}/holds`, null, {
    headers: {
      "X-Student-Id": studentId,
    },
  });

  if (response.status === 201) {
    successfulHolds.add(1);
  } else if (response.status === 409) {
    rejectedHolds.add(1);
  } else {
    unexpectedResponses.add(1);
    console.error(
      `Student ${studentId}: HTTP ${response.status} - ${response.body}`,
    );
  }
}
