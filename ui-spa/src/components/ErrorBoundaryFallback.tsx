import { useEffect } from "react";
import { ApiError } from "../common/errors/ApiError";
import Layout from "./Layout";
import { PageContentWrapper } from "./govuk/PageContentWrapper";
import { telemetryService } from "../TelemetryLogger";
import styles from "./ErrorBoundaryFallback.module.scss";

export const ErrorBoundaryFallback = ({
  error,
}: {
  error: Error | ApiError;
}) => {
  useEffect(() => {
    if (error instanceof ApiError) {
      telemetryService.trackException(error, [
        {
          correlationId: error.correlationId,
        },
      ]);
      return;
    }
    telemetryService.trackException(error);
  }, [error]);

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
          {error instanceof ApiError ? (
            <p
              className="govuk-inset-text"
              data-testid="txt-error-correlation-id"
            >
              Error code: {error?.correlationId}
            </p>
          ) : (
            <p className="govuk-inset-text" data-testid="txt-error-message">
              Error code: {error?.message}
            </p>
          )}
        </div>
      </PageContentWrapper>
    </Layout>
  );
};
