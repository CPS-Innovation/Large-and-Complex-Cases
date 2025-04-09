import { useRef, useEffect } from "react";
import { Button, Input, InsetText, ErrorSummary, BackLink } from "../govuk";
import EgressSearchResults from "./EgressSearchResults";
import { UseApiResult } from "../../common/hooks/useApiNew";
import { EgressSearchResultData } from "../../common/types/EgressSearchResponse";
import styles from "./EgressSearchPage.module.scss";

type EgressSearchPageProps = {
  backLinkUrl: string;
  searchValue: string;
  formDataErrorText: string;
  workspaceName: string;
  egressSearchApi: UseApiResult<EgressSearchResultData>;
  handleFormChange: (value: string) => void;
  handleSearch: () => void;
  handleConnectFolder: (id: string) => void;
};
const EgressSearchPage: React.FC<EgressSearchPageProps> = ({
  backLinkUrl,
  searchValue,
  formDataErrorText,
  egressSearchApi,
  workspaceName,
  handleFormChange,
  handleSearch,
  handleConnectFolder,
}) => {
  const errorSummaryRef = useRef<HTMLInputElement>(null);

  const handleFormSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    handleSearch();
  };

  useEffect(() => {
    if (formDataErrorText) errorSummaryRef.current?.focus();
  }, [formDataErrorText]);

  if (egressSearchApi.status === "loading")
    return <div className="govuk-width-container">Loading...</div>;

  return (
    <div className="govuk-width-container">
      <BackLink to={backLinkUrl}>Back</BackLink>

      <div>
        {!!formDataErrorText && (
          <div
            ref={errorSummaryRef}
            tabIndex={-1}
            className={styles.errorSummaryWrapper}
          >
            <ErrorSummary
              data-testid={"search-error-summary"}
              // className={styles.errorSummary}
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

        <h1>Select an Egress folder to link to the case</h1>
        <InsetText>
          <p>Select a folder from the list to link it to this case.</p>
          <p>
            If the folder you need is not listed, check that you have the
            correct permissions in Egress or contact the product team for
            support.
          </p>
        </InsetText>
      </div>
      <form onSubmit={handleFormSubmit}>
        <div className={styles.inputWrapper}>
          <Input
            id="search-folder-name"
            data-testid="search-folder-name"
            className="govuk-input--width-20"
            label={{
              children: "Egress folder name",
            }}
            errorMessage={
              formDataErrorText
                ? {
                    children: formDataErrorText,
                  }
                : undefined
            }
            name="Egress folder name"
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

      <EgressSearchResults
        workspaceName={workspaceName}
        egressSearchApi={egressSearchApi}
        handleConnectFolder={handleConnectFolder}
      />
    </div>
  );
};

export default EgressSearchPage;
