import { asyncActionHandlers } from "./asyncActionHandlers";
import { vi, Mock } from "vitest";
import { getCaseDivisionsOrAreas } from "../apis/gateway-api";

vi.mock("../apis/gateway-api", () => {
  return { getCaseDivisionsOrAreas: vi.fn() };
});

describe("asyncActionHandlers", () => {
  describe("GET_CASE_DIVISIONS_OR_AREAS action", () => {
    it("Should correctly dispatch actions", async () => {
      const dispatchMock = vi.fn();
      const areaDivisions = { id: "1" };
      (getCaseDivisionsOrAreas as Mock).mockReturnValue(areaDivisions);
      const handler = asyncActionHandlers.GET_CASE_DIVISIONS_OR_AREAS({
        dispatch: dispatchMock,
        getState: vi.fn(),
        signal: new AbortController().signal,
      });

      await handler({
        type: "GET_CASE_DIVISIONS_OR_AREAS",
      });

      expect(getCaseDivisionsOrAreas).toHaveBeenCalledTimes(1);
      expect(dispatchMock).toHaveBeenCalledTimes(2);
      expect(dispatchMock).toHaveBeenNthCalledWith(1, {
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "loading" },
      });
      expect(dispatchMock).toHaveBeenNthCalledWith(2, {
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "succeeded", data: areaDivisions },
      });
    });
  });
});
