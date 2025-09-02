import { msalInstance } from "../../auth/msal/msalInstance";
import {
  FEATURE_FLAG_CASE_DETAILS,
  PRIVATE_BETA_FEATURE_USER_GROUP2,
} from "../../config";

import { FeatureFlagData } from "../types/FeatureFlagData";
import { useCallback, useMemo } from "react";

const shouldShowFeature = (
  isFeatureFlagOnInApp: boolean,
  groupClaims?: { groupKey: string; groups: string[] },
) => {
  if (!isFeatureFlagOnInApp) {
    return false;
  }
  const shouldConsiderGroupClaims = !!groupClaims;

  return shouldConsiderGroupClaims
    ? groupClaims.groups.includes(groupClaims.groupKey)
    : true;
};

export const useUserGroupsFeatureFlag = (): FeatureFlagData => {
  const [account] = msalInstance.getAllAccounts();

  const groups = useMemo(
    () => (account?.idTokenClaims?.groups as string[]) ?? [],
    [account?.idTokenClaims?.groups],
  );

  const getFeatureFlags = useCallback(
    () => ({
      caseDetails: shouldShowFeature(FEATURE_FLAG_CASE_DETAILS, {
        groups: groups,
        groupKey: PRIVATE_BETA_FEATURE_USER_GROUP2,
      }),
    }),
    [groups],
  );
  return useMemo(() => getFeatureFlags(), [getFeatureFlags]);
};
