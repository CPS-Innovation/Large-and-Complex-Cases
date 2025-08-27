import styles from "./AccessErrorPage.module.scss";

const AccessErrorPage: React.FC = () => {
  return (
    <div className="govuk-width-container">
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">Access Error</h1>
        <p>You cannot access this page. You are not a member of this group.</p>
        <p>
          If you think this is a mistake, contact{" "}
          <a href="mailto:meadhbh.major@cps.gov.uk">meadhbh.major@cps.gov.uk</a>{" "}
          for assistance.
        </p>
      </div>
    </div>
  );
};

export default AccessErrorPage;
