import { msalInstance } from "../../auth/msal/msalInstance";
import {
  FEATURE_FLAG_CASE_DETAILS,
  FEATURE_FLAG_TRANSFER_MOVE,
  FEATURE_FLAG_GLOBAL_NAV,
  GLOBAL_NAV_SCRIPT_URL,
  FEATURE_FLAG_DISCONNECT_SHARED_DRIVE,
  PRIVATE_BETA_FEATURE_USER_GROUP2,
  FEATURE_FLAG_TRANSFER_MATERIALS_V1,
} from "../../config";
import { useUserDetails } from "../../auth";
import { FeatureFlagData } from "../types/FeatureFlagData";
import { useCallback, useMemo } from "react";
import { useSearchParams } from "react-router-dom";

const automationTestUsers = ["dev_user@example.org"];

const shouldShowFeature = (
  username: string,
  isUIFeatureFlagOn: boolean,
  searchParam: string | null,
  groupClaims?: { groupKey: string; groups: string[] },
) => {
  const isAutomationTestUser = automationTestUsers.includes(username);

  if (isAutomationTestUser) {
    return searchParam ? searchParam === "true" : isUIFeatureFlagOn;
  }

  return groupClaims
    ? groupClaims.groups.includes(groupClaims.groupKey) && isUIFeatureFlagOn
    : isUIFeatureFlagOn;
};

export const useUserGroupsFeatureFlag = (): FeatureFlagData | null => {
  const [account] = msalInstance.getAllAccounts();
  const userDetails = useUserDetails();
  const [searchParams] = useSearchParams();

  const groups = useMemo(
    () => (account?.idTokenClaims?.groups as string[]) ?? [],
    [account?.idTokenClaims?.groups],
  );

  const getFeatureFlags = useCallback(() => {
    //skip the account check for automation user, ideally if the account is not available normal user shouldn't be here
    if (
      (!account && userDetails.username !== "dev_user@example.org") ||
      !userDetails.username
    )
      return null;

    return {
      caseDetails: shouldShowFeature(
        userDetails.username,
        FEATURE_FLAG_CASE_DETAILS,
        searchParams?.get("case-details"),
        {
          groups: groups,
          groupKey: PRIVATE_BETA_FEATURE_USER_GROUP2,
        },
      ),
      transferMove: shouldShowFeature(
        userDetails.username,
        FEATURE_FLAG_TRANSFER_MOVE,
        searchParams?.get("transfer-move"),
        {
          groups: groups,
          groupKey: PRIVATE_BETA_FEATURE_USER_GROUP2,
        },
      ),
      globalNav: shouldShowFeature(
        userDetails.username,
        FEATURE_FLAG_GLOBAL_NAV && !!GLOBAL_NAV_SCRIPT_URL,
        searchParams?.get("global-nav"),
        {
          groups: groups,
          groupKey: PRIVATE_BETA_FEATURE_USER_GROUP2,
        },
      ),
      disconnectSharedDrive: shouldShowFeature(
        userDetails.username,
        FEATURE_FLAG_DISCONNECT_SHARED_DRIVE,
        searchParams?.get("disconnect-shared-drive"),
        {
          groups: groups,
          groupKey: PRIVATE_BETA_FEATURE_USER_GROUP2,
        },
      ),
      transferMaterialsV1: shouldShowFeature(
        userDetails.username,
        FEATURE_FLAG_TRANSFER_MATERIALS_V1,
        searchParams?.get("transfer-materials-v1"),
        {
          groups: groups,
          groupKey: PRIVATE_BETA_FEATURE_USER_GROUP2,
        },
      ),
    };
  }, [groups, searchParams, userDetails.username]);

  return useMemo(() => getFeatureFlags(), [getFeatureFlags]);
};
