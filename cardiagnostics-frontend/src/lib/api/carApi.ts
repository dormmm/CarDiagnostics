import { apiFetch } from "./http";

export interface SubmitProblemByPlateRequest {
  username: string;
  email: string;
  licensePlate: string;
  problemDescription: string;
}

export async function submitProblemByPlate(req: SubmitProblemByPlateRequest) {
  return apiFetch<any>("/api/Car/submitProblemByPlate", {
    method: "POST",
    body: JSON.stringify(req),
  });
}
