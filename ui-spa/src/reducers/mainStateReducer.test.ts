import { mainStateReducer } from "./mainStateReducer";
import * as areaLookups from "./utils/mapAreaLookups";

describe("mainStateReducer", () => {
  const initialState = {
    appData: { featureFlags: null },
    apiData: {
      caseDivisionsOrAreas: null,
    },
  } as any;
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

  it("merges partial form data (SET_FORM_DATA_FIELD)", () => {
    const action = {
      type: "SET_FORM_DATA_FIELD" as const,
      payload: { operationName: "Op A", urn: "URN-123" },
    } as any;

    const next = mainStateReducer(initialState, action);

    expect(next.formData.operationName).toBe("Op A");
    expect(next.formData.urn).toBe("URN-123");
  });

  it("sets case meta data (SET_CASE_META_DATA)", () => {
    const caseMeta = {
      caseId: 1,
      egressWorkspaceId: "ew1",
      netappFolderPath: "path",
      operationName: "Op",
      leadDefendantName: "John",
      activeTransferId: null,
      urn: "URN-1",
    } as any;

    const next = mainStateReducer(initialState, {
      type: "SET_CASE_META_DATA",
      payload: { caseMetaData: caseMeta },
    } as any);

    expect(next.apiData.caseMetaData).toEqual(caseMeta);
  });

  it("sets feature flags (SET_FEATURE_FLAGS)", () => {
    const flags = {
      caseDetails: true,
      transferMove: false,
      globalNav: true,
      disconnectSharedDrive: false,
      transferMaterialsV1: true,
    } as any;

    const next = mainStateReducer(initialState, {
      type: "SET_FEATURE_FLAGS",
      payload: { featureFlags: flags },
    } as any);

    expect(next.appData.featureFlags).toEqual(flags);
  });

  it("replaces egress connect page (SET_EGRESS_CONNECT_PAGE)", () => {
    const payload = { searchQueryString: "q", isNetAppConnected: true };
    const next = mainStateReducer(initialState, {
      type: "SET_EGRESS_CONNECT_PAGE",
      payload,
    } as any);

    expect(next.appData.connectEgressPage).toEqual(payload);
  });

  it("merges shared drive connect page partial (SET_SHARED_DRIVE_CONNECT_PAGE)", () => {
    const action = {
      type: "SET_SHARED_DRIVE_CONNECT_PAGE",
      payload: { searchQueryString: "abc" },
    } as any;
    const next = mainStateReducer(initialState, action);

    expect(next.appData.connectSharedDrivePage.searchQueryString).toBe("abc");
  });

  it("sets egress connect confirmation page (SET_EGRESS_CONNECT_CONFIRMATION_PAGE)", () => {
    const payload = {
      backLinkUrl: "/back",
      searchQueryString: "s",
      isNetAppConnected: false,
      selectedWorkspace: { id: "1", name: "n" },
    };
    const next = mainStateReducer(initialState, {
      type: "SET_EGRESS_CONNECT_CONFIRMATION_PAGE",
      payload,
    } as any);

    expect(next.appData.egressConnectConfirmationPage).toEqual(payload);
  });

  it("sets shared drive connect confirmation page (SET_SHARED_DRIVE_CONNECT_CONFIRMATION_PAGE)", () => {
    const payload = {
      operationName: "op",
      backLinkUrl: "/b",
      selectedWorkspace: { folderPath: "fp" },
    };
    const next = mainStateReducer(initialState, {
      type: "SET_SHARED_DRIVE_CONNECT_CONFIRMATION_PAGE",
      payload,
    } as any);

    expect(next.appData.connectSharedDriveConfirmationPage).toEqual(payload);
  });

  it("sets egress/shared drive failure pages", () => {
    const state = mainStateReducer(initialState, {
      type: "SET_EGRESS_CONNECT_FAILURE_PAGE",
      payload: { backLinkUrl: "/x" },
    } as any);
    expect(state.appData.connectEgressFailurePage.backLinkUrl).toBe("/x");

    const newState = mainStateReducer(initialState, {
      type: "SET_SHARED_DRIVE_CONNECT_FAILURE_PAGE",
      payload: { backLinkUrl: "/y" },
    } as any);
    expect(newState.appData.connectSharedDriveFailurePage.backLinkUrl).toBe(
      "/y",
    );
  });

  it("sets transfer destination page (SET_TRANSFER_DESTINATION_PAGE)", () => {
    const payload = {
      transferSource: null,
      selectedTransferAction: null,
      sourcePaths: [],
      egressWorkspaceId: "e",
      netAppPath: "n",
      operationName: "op",
    } as any;
    const state = mainStateReducer(initialState, {
      type: "SET_TRANSFER_DESTINATION_PAGE",
      payload: payload,
    } as any);
    expect(state.appData.transferDestinationPage.egressWorkspaceId).toBe("e");
  });

  it("sets transfer resolve file path page (SET_TRANSFER_RESOLVE_FILE_PATH_PAGE)", () => {
    const state = mainStateReducer(initialState, {
      type: "SET_TRANSFER_RESOLVE_FILE_PATH_PAGE",
      payload: {
        validationErrors: [],
        destinationPath: "d",
        initiateTransferPayload: null,
        baseFolderName: "b",
      },
    } as any);
    expect(state.appData.transferResolveFilePathPage.destinationPath).toBe("d");
  });

  it("sets transfer error page (SET_TRANSFER_ERROR_PAGE)", () => {
    const state = mainStateReducer(initialState, {
      type: "SET_TRANSFER_ERROR_PAGE",
      payload: { transferId: "t1", failedItems: [] },
    } as any);
    expect(state.appData.transferErrorsPage?.transferId).toBe("t1");
  });

  it("sets transfer page (SET_TRANSFER_PAGE)", () => {
    const state = mainStateReducer(initialState, {
      type: "SET_TRANSFER_PAGE",
      payload: {
        transferSource: "egress",
        transferSourceEgressFolderPath: "p1",
        transferSourceNetAppFolderPath: "p2",
      },
    } as any);
    expect(state.appData.transferPage?.transferSource).toBe("egress");
  });
});
