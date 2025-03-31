import { FallbackProps } from "react-error-boundary";
import Layout from "./Layout";

export const ErrorBoundaryFallback = ({ error }: FallbackProps) => {
  return (
    <Layout>
      <div role="alert" className={`govuk-width-container`}>
        <h1>Something went wrong!</h1>
        <p>{error.message}</p>
      </div>
    </Layout>
  );
};
