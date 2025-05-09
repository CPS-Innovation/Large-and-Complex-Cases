import { FallbackProps } from "react-error-boundary";
import Layout from "./Layout";
import styles from "./ErrorBoundaryFallback.module.scss";

export const ErrorBoundaryFallback = ({ error }: FallbackProps) => {
  return (
    <Layout>
      <div role="alert" className={`govuk-width-container ${styles.content}`}>
        <h1 className="govuk-heading-l" data-testid="txt-error-page-heading">
          Sorry, there is a problem with the service
        </h1>

        <p className="govuk-body-l">
          Please try this case again later. If the problem continues, contact
          the product team.
        </p>

        <p className="govuk-inset-text">{error?.toString()}</p>
      </div>
    </Layout>
  );
};
