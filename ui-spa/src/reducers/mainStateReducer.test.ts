import { mainStateReducer } from "./mainStateReducer";
import * as areaLookups from "./utils/mapAreaLookups";

describe("mainStateReducer", () => {
  const initialState = {
    caseDivisionsOrAreas: { status: "loading" as const },
  };
  it("Should return the state, if there are no matching action types found", () => {
    const newState = mainStateReducer(initialState, {} as any);
    expect(newState).toStrictEqual(initialState);
  });
  describe("UPDATE_CASE_DIVISIONS_OR_AREAS action", () => {
    it("Should update the state correctly when the payload status is failed", () => {
      const newState = mainStateReducer(initialState, {
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "failed", error: {} },
      });
      expect(newState).toStrictEqual(initialState);
    });
    it("Should update the state correctly when the payload status is loading", () => {
      const newState = mainStateReducer(initialState, {
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "loading" },
      });
      expect(newState).toStrictEqual(initialState);
    });
    it("Should update the state correctly when the payload status is succeeded", () => {
      const areaLookupData = {
        allAreas: [
          {
            id: 5,
            description: "Mdbc",
          },
          {
            id: 2,
            description: "babc",
          },
        ],
        userAreas: [
          {
            id: 1057708,
            description: "habc",
          },
          {
            id: 1057709,
            description: "abc",
          },
        ],
        homeArea: {
          id: 1057709,
          description: "abc",
        },
      };
      const expectedData = {
        allAreas: [
          {
            id: 2,
            description: "babc",
          },
          {
            id: 5,
            description: "Mdbc",
          },
        ],
        userAreas: [
          {
            id: 1057709,
            description: "abc",
          },
          {
            id: 1057708,
            description: "habc",
          },
        ],
        homeArea: {
          id: 1057709,
          description: "abc",
        },
      };
      const mapAreaLookupsSpy = vi.spyOn(areaLookups, "mapAreaLookups");
      const newState = mainStateReducer(initialState, {
        type: "UPDATE_CASE_DIVISIONS_OR_AREAS",
        payload: { status: "succeeded", data: areaLookupData },
      });
      expect(mapAreaLookupsSpy).toHaveBeenCalledTimes(1);
      expect(mapAreaLookupsSpy).toHaveBeenCalledWith(areaLookupData);
      expect(newState).toStrictEqual({
        ...initialState,
        caseDivisionsOrAreas: { status: "succeeded", data: expectedData },
      });
    });
  });
});
