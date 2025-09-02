import { Mock } from "vitest";
import { renderHook } from "@testing-library/react";
import { useUserGroupsFeatureFlag } from "./useUserGroupsFeatureFlag";
import * as configModule from "../../config";
import * as msalInstanceModule from "../../auth/msal/msalInstance";
vi.mock("../../config");
vi.mock("../../auth/msal/msalInstance", () => ({
  msalInstance: {
    getAllAccounts: vi.fn(),
  },
}));

const mockConfig = configModule as {
  FEATURE_FLAG_CASE_DETAILS: boolean;
  PRIVATE_BETA_FEATURE_USER_GROUP2: string;
};

describe("useUserGroupsFeatureFlag", () => {
  describe("caseDetails feature flag", () => {
    test("Should return caseDetails feature false, if FEATURE_FLAG_CASE_DETAILS is false", () => {
      (msalInstanceModule.msalInstance.getAllAccounts as Mock).mockReturnValue([
        {
          username: "test_username",
          name: "test_name",
          idTokenClaims: {
            groups: ["private_beta_feature_group2"],
          },
        },
      ]);
      mockConfig.FEATURE_FLAG_CASE_DETAILS = false;
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP2 =
        "private_beta_feature_group2";
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.caseDetails).toStrictEqual(false);
    });

    test("Should return caseDetails feature false, if FEATURE_FLAG_CASE_DETAILS is true but the user is not added to PRIVATE_BETA_FEATURE_USER_GROUP2", () => {
      (msalInstanceModule.msalInstance.getAllAccounts as Mock).mockReturnValue([
        {
          username: "test_username",
          name: "test_name",
          idTokenClaims: {
            groups: ["private_beta_feature_group1"],
          },
        },
      ]);
      mockConfig.FEATURE_FLAG_CASE_DETAILS = true;
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP2 =
        "private_beta_feature_group2";
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.caseDetails).toStrictEqual(false);
    });

    test("Should return caseDetails feature true, if FEATURE_FLAG_CASE_DETAILS is true but the user is added to PRIVATE_BETA_FEATURE_USER_GROUP2", () => {
      (msalInstanceModule.msalInstance.getAllAccounts as Mock).mockReturnValue([
        {
          username: "test_username",
          name: "test_name",
          idTokenClaims: {
            groups: ["private_beta_feature_group2"],
          },
        },
      ]);
      mockConfig.FEATURE_FLAG_CASE_DETAILS = true;
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP2 =
        "private_beta_feature_group2";
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.caseDetails).toStrictEqual(true);
    });
  });
});
