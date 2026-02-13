import { msalInstance } from "../../auth/msal/msalInstance";
import {
  FEATURE_FLAG_CASE_DETAILS,
  FEATURE_FLAG_TRANSFER_MOVE,
  PRIVATE_BETA_FEATURE_USER_GROUP2,
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

export const useUserGroupsFeatureFlag = (): FeatureFlagData => {
  const [account] = msalInstance.getAllAccounts();
  const userDetails = useUserDetails();
  const [searchParams] = useSearchParams();

  const groups = useMemo(
    () => (account?.idTokenClaims?.groups as string[]) ?? [],
    [account?.idTokenClaims?.groups],
  );

  const getFeatureFlags = useCallback(
    () => ({
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
    }),
    [groups, searchParams, userDetails.username],
  );
  return useMemo(() => getFeatureFlags(), [getFeatureFlags]);
};
