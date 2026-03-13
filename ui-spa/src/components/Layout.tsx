import { useEffect, useRef } from "react";
import Footer from "./Footer";
import Header from "./Header";
import { useUserGroupsFeatureFlag } from "../common/hooks/useUserGroupsFeatureFlag";
import { useLocation } from "react-router-dom";
import { SkipLink } from "../components/govuk";
import styles from "./Layout.module.scss";

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const location = useLocation();
  const skipLinkSiblingRef = useRef(null);
  const featureFlags = useUserGroupsFeatureFlag();

  useEffect(() => {
    /*This is a way to bring the focus back to the top of the page
    whenever location change. We bring the focus to the dummy element just above the skip link and the call the blur(for the screen reader not to say it loud),
    so that the next tabbable element will be skip link when user start tabbing on the page
    */

    if (skipLinkSiblingRef.current) {
      (skipLinkSiblingRef.current as HTMLButtonElement).focus();
      (skipLinkSiblingRef.current as HTMLButtonElement).blur();
    }
  }, [location.pathname]);
  return (
    <div ref={skipLinkSiblingRef} tabIndex={-1} className={styles.rootLayout}>
      <SkipLink href="#main-content">Skip to main content</SkipLink>
      {featureFlags.globalNav && (
        <div className="govuk-width-container">
          <cps-global-header></cps-global-header>
        </div>
      )}
      {!featureFlags.globalNav && <Header />}
      <div className={styles.mainContent}>{children}</div>
      <Footer />
    </div>
  );
}
