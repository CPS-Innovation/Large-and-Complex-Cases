import { mainStateReducer } from "./mainStateReducer";
import * as areaLookups from "./utils/mapAreaLookups";

describe("mainStateReducer", () => {
  const initialState = {
    appData: { featureFlags: null },
    apiData: {
      caseDivisionsOrAreas: null,
    },
  };
  it("Should return the state, if there are no matching action types found", () => {
    const newState = mainStateReducer(initialState, {} as any);
    expect(newState).toStrictEqual(initialState);
  });

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

  it("SET_FEATURE_FLAGS should update the state correctly", () => {
    const featureFlagData = {
      caseDetails: false,
      transferMove: true,
      globalNav: false,
      disconnectSharedDrive: true,
      transferMaterialsV1: true,
    };

    const newState = mainStateReducer(initialState, {
      type: "SET_FEATURE_FLAGS",
      payload: { featureFlags: featureFlagData },
    });

    expect(newState).toStrictEqual({
      ...initialState,
      appData: {
        featureFlags: { ...featureFlagData },
      },
    });
  });
});
