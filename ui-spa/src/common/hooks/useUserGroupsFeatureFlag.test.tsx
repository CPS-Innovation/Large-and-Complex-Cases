import { Mock } from "vitest";
import { renderHook } from "@testing-library/react";
import { useUserGroupsFeatureFlag } from "./useUserGroupsFeatureFlag";
import * as configModule from "../../config";
import * as msalInstanceModule from "../../auth/msal/msalInstance";
import * as auth from "../../auth";
import * as router from "react-router-dom";

vi.mock("../../config");
vi.mock("../../auth/msal/msalInstance", () => ({
  msalInstance: {
    getAllAccounts: vi.fn(),
  },
}));
vi.mock("../../auth", () => {
  return {
    useUserDetails: vi.fn(() => ({
      username: "test_username",
    })),
  };
});
vi.mock("react-router-dom", () => {
  return {
    useSearchParams: vi.fn(() => [new URLSearchParams("foo=bar"), vi.fn()]),
  };
});

const mockConfig = configModule as {
  FEATURE_FLAG_CASE_DETAILS: boolean;
  PRIVATE_BETA_FEATURE_USER_GROUP2: string;
};

describe("useUserGroupsFeatureFlag", () => {
  afterEach(() => {
    vi.resetAllMocks();
  });
  describe("caseDetails feature flag", () => {
    test("Should return caseDetails feature false, if FEATURE_FLAG_CASE_DETAILS is false for normal user", () => {
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
    test("Should return caseDetails feature false, if FEATURE_FLAG_CASE_DETAILS is false for automation test user ", () => {
      (auth.useUserDetails as Mock).mockReturnValue({
        username: "dev_user@example.org",
      });
      (msalInstanceModule.msalInstance.getAllAccounts as Mock).mockReturnValue([
        {
          username: "dev_user@example.org",
          name: "dev_user",
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
    test("Should return caseDetails feature false, if FEATURE_FLAG_CASE_DETAILS is true and the user is not added to PRIVATE_BETA_FEATURE_USER_GROUP2 for normal user", () => {
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
    test("Should return caseDetails feature true, if FEATURE_FLAG_CASE_DETAILS is true and the user is added to PRIVATE_BETA_FEATURE_USER_GROUP2 for normal user", () => {
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
    test("Should return caseDetails feature true, if FEATURE_FLAG_CASE_DETAILS is true evne if the user is not added to PRIVATE_BETA_FEATURE_USER_GROUP2 for automation user", () => {
      (auth.useUserDetails as Mock).mockReturnValue({
        username: "dev_user@example.org",
      });
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
      expect(result?.current?.caseDetails).toStrictEqual(true);
    });
    test("Should return caseDetails feature true, if it is an automation user with search param case-details=true, serach param take priority for automation user", () => {
      (auth.useUserDetails as Mock).mockReturnValue({
        username: "dev_user@example.org",
      });
      (router.useSearchParams as Mock).mockReturnValue([
        new URLSearchParams("case-details=true"),
        vi.fn(),
      ]);

      (msalInstanceModule.msalInstance.getAllAccounts as Mock).mockReturnValue([
        {
          username: "test_username",
          name: "test_name",
          idTokenClaims: {
            groups: ["private_beta_feature_group1"],
          },
        },
      ]);
      mockConfig.FEATURE_FLAG_CASE_DETAILS = false;
      mockConfig.PRIVATE_BETA_FEATURE_USER_GROUP2 =
        "private_beta_feature_group2";
      const { result } = renderHook(() => useUserGroupsFeatureFlag());
      expect(result?.current?.caseDetails).toStrictEqual(true);
    });
    test("Should return caseDetails feature false, if it is an automation user with search param case-details=false,serach param take priority for automation user", () => {
      (auth.useUserDetails as Mock).mockReturnValue({
        username: "dev_user@example.org",
      });
      (router.useSearchParams as Mock).mockReturnValue([
        new URLSearchParams("case-details=false"),
        vi.fn(),
      ]);

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
  });
});
