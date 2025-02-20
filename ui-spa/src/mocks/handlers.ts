import { http, HttpResponse } from "msw";
import { GATEWAY_BASE_URL } from "../config";

export const handlers = [
  http.get(`${GATEWAY_BASE_URL}/api/initialmessage`, () => {
    return HttpResponse.json({
      message: "Hello World!",
    });
  }),
];
