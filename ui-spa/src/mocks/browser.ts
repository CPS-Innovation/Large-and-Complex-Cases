import { setupWorker } from "msw/browser";
import { setupHandlers } from "./handlers";
import { GATEWAY_BASE_URL } from "../config";

export const worker = setupWorker(...setupHandlers(GATEWAY_BASE_URL, "dev"));
