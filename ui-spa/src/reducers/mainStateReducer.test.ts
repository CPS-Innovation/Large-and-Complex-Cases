import { mainStateReducer } from "./mainStateReducer";
import * as areaLookups from "./utils/mapAreaLookups";

describe("mainStateReducer", () => {
  const initialState = {
    apiData: {
      caseDivisionsOrAreas: null,
    },
  };
  it("Should return the state, if there are no matching action types found", () => {
    const newState = mainStateReducer(initialState, {} as any);
    expect(newState).toStrictEqual(initialState);
  });
  describe("SET_CASE_DIVISIONS_OR_AREAS action", () => {
    it("SET_CASE_DIVISIONS_OR_AREAS should update the state correctly", () => {
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
        type: "SET_CASE_DIVISIONS_OR_AREAS",
        payload: { caseDivisionsOrAreas: areaLookupData },
      });
      expect(mapAreaLookupsSpy).toHaveBeenCalledTimes(1);
      expect(mapAreaLookupsSpy).toHaveBeenCalledWith(areaLookupData);
      expect(newState).toStrictEqual({
        ...initialState,
        apiData: {
          caseDivisionsOrAreas: { ...expectedData },
        },
      });
    });
  });
});
