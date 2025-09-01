import { MsalProvider } from "@azure/msal-react";
import React, { FC, useEffect, useState } from "react";
import { msalInstance } from "./msalInstance";
import PrivateBetaAuthorisation from "./PrivateBetaAuthorisation";

export const Auth: FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>();

  useEffect(() => {
    (async () => {
      await msalInstance.initialize();
      // required so that when we are coming back from a redirect, that process is complete
      //  before we do any more auth interactions (otherwise an error is thrown)
      await msalInstance.handleRedirectPromise();

      const [account] = msalInstance.getAllAccounts();

      if (!account) {
        try {
          await msalInstance.loginRedirect({
            scopes: ["User.Read"],
          });
          return;
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
        } catch (err: any) {
          if (err?.errorCode !== "interaction_in_progress") {
            // When we redirect an "interaction_in_progress" error is thrown.
            //  Let's suppress this error, but not any other types
            throw err;
          }
        }
      }

      setIsLoggedIn(true);
    })();
  }, []);

  return isLoggedIn ? (
    <MsalProvider instance={msalInstance}>
      <PrivateBetaAuthorisation msalInstance={msalInstance}>
        {children}
      </PrivateBetaAuthorisation>
    </MsalProvider>
  ) : (
    <></>
  );
};
