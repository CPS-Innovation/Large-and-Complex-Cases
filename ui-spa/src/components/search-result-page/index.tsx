import { useRef } from "react";
import { useApi } from "../../common/hooks/useApi";
import { useQueryParamsState } from "../../common/hooks/useQueryParamsState";
import { useCaseSearchInputLogic } from "../../common/hooks/useCaseSearchInputLogic";
import { CaseSearchQueryParams } from "../../common/types/CaseSearchQueryParams";
import { getCaseSearchResults } from "../../apis/gateway-api";
import { Input, Button, Label, LabelText, ErrorSummary } from "govuk-react";
import styles from "./index.module.scss";

const validationFailMessage = "Operation name should not be empty";
const CaseSearchResultPage = () => {
  const inputRef = useRef<HTMLInputElement>(null);

  const {
    search: searchKeyFromSearchParams,
    setParams,
    search,
  } = useQueryParamsState<CaseSearchQueryParams>();
  const apiState = useApi(getCaseSearchResults, [search], !!search);
  const { handleChange, handleSubmit, isError, searchKey } =
    useCaseSearchInputLogic({ searchKeyFromSearchParams, setParams, search });

  const handleSearch = () => {
    handleSubmit();
  };

  const onHandleErrorClick = () => {
    inputRef.current?.focus();
  };

  if (apiState.status === "loading") {
    return <div> Loading...</div>;
  }

  console.log("apiState>>", apiState);
  return (
    <div>
      <div className={styles.fullWidthContainer}>
        <div className="govuk-width-container">
          <div className={styles.contentTop}>
            {isError && (
              <ErrorSummary
                errors={[
                  {
                    targetName: "case-search-text-input",
                    text: validationFailMessage,
                  },
                ]}
                className={styles.errorSummary}
                onHandleErrorClick={onHandleErrorClick}
              />
            )}

            <div className={styles.inputWrapper}>
              <Label>
                <LabelText>
                  <b>Search by Operation name</b>
                </LabelText>
                <Input
                  id="case-search-text-input"
                  ref={inputRef}
                  className="govuk-input--width-20"
                  value={searchKey}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    handleChange(e.target.value)
                  }
                />
              </Label>
              <Button onClick={handleSearch}>Search</Button>
            </div>
            {apiState.status === "succeeded" && (
              <span className={styles.searchResultsCount}>
                We've found <b>{apiState.data.length}</b> case that matches{" "}
                <b>{search}</b>.
              </span>
            )}
          </div>
        </div>
      </div>
      <div className="govuk-width-container">
        <ul className={styles.searchResults}>
          {apiState.status === "succeeded" &&
            apiState.data.map((result: any) => {
              return (
                <li>
                  <div className={styles.resultHeading}>
                    <h2
                      className={`govuk-heading-l ${styles.resultHeadingHeader}`}
                    >
                      {result.operationName}
                    </h2>
                    <span>{result.urn}</span>
                  </div>

                  <dl className={styles.resultContent}>
                    <dt className={styles.contentKey}>Defendants:</dt>
                    <dd>{result.leadDefendantName}</dd>

                    <dt className={styles.contentKey}>Date Created:</dt>
                    <dd>{result.dateCreated}</dd>
                  </dl>
                </li>
              );
            })}
        </ul>
      </div>
    </div>
  );
};

export default CaseSearchResultPage;
