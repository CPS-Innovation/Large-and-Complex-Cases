import { useEffect, useMemo } from "react";
import { ApiError } from "../common/errors/ApiError";
import Layout from "./Layout";
import { PageContentWrapper } from "./govuk/PageContentWrapper";
import { telemetryService } from "../TelemetryLogger";
import { v4 as uuidv4 } from "uuid";
import styles from "./ErrorBoundaryFallback.module.scss";

export const ErrorBoundaryFallback = ({
  error,
}: {
  error: Error | ApiError;
}) => {
  const referenceId = useMemo(() => {
    if (error instanceof ApiError && error.correlationId) {
      return error.correlationId;
    }
    return uuidv4();
  }, [error]);

  useEffect(() => {
    void telemetryService.trackException(error, [
      {
        referenceId: referenceId,
      },
      {
        errorSource:
          error instanceof ApiError ? "API_ERROR" : "UI_UNHANDLED_EXCEPTION",
      },
      { route: window.location.pathname },
    ]);
  }, [error, referenceId]);

  return (
    <Layout>
      <PageContentWrapper>
        <div role="alert" className={`${styles.content}`}>
          <h1 className="govuk-heading-l" data-testid="txt-error-page-heading">
            Sorry, there is a problem with the service
          </h1>

          <p className="govuk-body-l">
            Contact the product team and give them the error code.
          </p>
          <p className="govuk-inset-text" data-testid="error-reference">
            Error code: {referenceId}
          </p>
        </div>
      </PageContentWrapper>
    </Layout>
  );
};
