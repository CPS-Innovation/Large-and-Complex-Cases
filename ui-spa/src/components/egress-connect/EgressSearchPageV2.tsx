import { useRef, useEffect } from "react";
import { Button, Input, ErrorSummary, BackLink } from "../govuk";
import EgressSearchResultsV2 from "./EgressSearchResultsV2";
import { type EgressSearchResultData } from "../../schemas";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import styles from "./EgressSearchPage.module.scss";

type EgressSearchPageV2Props = {
  backLinkUrl: string;
  searchValue: string;
  formDataErrorText: string;
  workspaceName: string;
  egressSearchResults: EgressSearchResultData | undefined;
  isEgressSearchResultLoading: boolean;
  handleFormChange: (value: string) => void;
  handleSearch: () => void;
  handleConnectFolder: (id: string) => void;
};
const EgressSearchPageV2: React.FC<EgressSearchPageV2Props> = ({
  backLinkUrl,
  searchValue,
  formDataErrorText,
  egressSearchResults,
  isEgressSearchResultLoading,
  workspaceName,
  handleFormChange,
  handleSearch,
  handleConnectFolder,
}) => {
  const errorSummaryRef = useRef<HTMLInputElement>(null);

  const handleFormSubmit = (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();

    handleSearch();
  };

  useEffect(() => {
    if (formDataErrorText) errorSummaryRef.current?.focus();
  }, [formDataErrorText]);

  return (
    <div>
      <BackLink to={backLinkUrl}>Back</BackLink>
      <PageContentWrapper>
        <div>
          {!!formDataErrorText && (
            <div
              ref={errorSummaryRef}
              tabIndex={-1}
              className={styles.errorSummaryWrapper}
            >
              <ErrorSummary
                data-testid={"search-error-summary"}
                errorList={[
                  {
                    reactListKey: "1",
                    children: formDataErrorText,
                    href: "#search-folder-name",
                    "data-testid": "search-folder-name-link",
                  },
                ]}
                titleChildren="There is a problem"
              />
            </div>
          )}

          <h1 className="govuk-heading-xl">Connect Egress to this case</h1>
        </div>
        <form onSubmit={handleFormSubmit}>
          <div className={styles.inputWrapper}>
            <Input
              id="search-folder-name"
              data-testid="search-folder-name"
              className="govuk-input--width-20"
              label={{
                children: <b>Search</b>,
              }}
              errorMessage={
                formDataErrorText
                  ? {
                      children: formDataErrorText,
                    }
                  : undefined
              }
              name="Search for a case on Egress"
              type="text"
              value={searchValue}
              onChange={(value: string) => {
                handleFormChange(value);
              }}
              disabled={false}
            />
            <div className={styles.btnWrapper}>
              <Button type="submit">Search</Button>
            </div>
          </div>
        </form>

        {!isEgressSearchResultLoading && egressSearchResults && (
          <EgressSearchResultsV2
            workspaceName={workspaceName}
            egressSearchResults={egressSearchResults}
            handleConnectFolder={handleConnectFolder}
          />
        )}
      </PageContentWrapper>
    </div>
  );
};

export default EgressSearchPageV2;
