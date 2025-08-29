import { IPublicClientApplication } from "@azure/msal-browser";
import { FC } from "react";
import AccessErrorPage from "./AccessErrorPage";
import { PRIVATE_BETA_USER_GROUP } from "../../config";
type Props = {
  msalInstance: IPublicClientApplication;
  children: React.ReactNode;
};

const PrivateBetaAuthorisation: FC<Props> = ({ msalInstance, children }) => {
  const [account] = msalInstance.getAllAccounts();
  const groupClaims = account?.idTokenClaims?.groups as string[];

  const canProceedOnNoGroupInConfig = !PRIVATE_BETA_USER_GROUP;
  const canProceedOnGroupMembership = !!groupClaims?.includes(
    PRIVATE_BETA_USER_GROUP,
  );

  return canProceedOnNoGroupInConfig || canProceedOnGroupMembership ? (
    <>{children}</>
  ) : (
    <AccessErrorPage />
  );
};

export default PrivateBetaAuthorisation;
