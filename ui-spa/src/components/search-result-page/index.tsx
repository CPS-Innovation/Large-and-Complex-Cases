import { useRef, useState, useEffect } from "react";
import { useApi } from "../../common/hooks/useApi";
import { Button, Input, Select, ErrorSummary } from "../govuk";
import { getCaseSearchResults } from "../../apis/gateway-api";
import useSearchNavigation from "../../common/hooks/useSearchNavigation";
import {
  useCaseSearchForm,
  SearchFormField,
  SearchFromData,
} from "../../common/hooks/useCaseSearchForm";
import styles from "./index.module.scss";

const CaseSearchResultPage = () => {
  const [triggerSearchApi, setTriggerSearchApi] = useState(false);
  const errorSummaryRef = useRef<HTMLInputElement>(null);
  const { updateSearchParams, searchParams, queryString } =
    useSearchNavigation();
  const getInitialState = () => {
    const searchParamKeys = Object.keys(searchParams);
    const initialData: SearchFromData = {
      searchType: "urn",
      operationName: "",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "",
    };

    console.log("searchParamKeys>>", searchParams);
    if (
      searchParamKeys.includes("defendant-name") &&
      searchParamKeys.includes("area")
    ) {
      initialData.searchType = "defendant name";
      initialData.defendantName = searchParams["defendant-name"] ?? "";
      initialData.defendantArea = searchParams["area"] ?? "";
      return initialData;
    }
    if (
      searchParamKeys.includes("operation-name") &&
      searchParamKeys.includes("area")
    ) {
      initialData.searchType = "operation name";
      initialData.operationName = searchParams["operation-name"] ?? "";
      initialData.operationArea = searchParams["area"] ?? "";
      return initialData;
    }

    initialData.urn = searchParams["urn"] ?? "";
    return initialData;
  };
  const {
    formData,
    formDataErrors,
    errorList,
    validateFormData,
    handleFormChange,
    getSearchParams,
  } = useCaseSearchForm(getInitialState());

  const apiState = useApi(
    getCaseSearchResults,
    [queryString],
    triggerSearchApi,
  );
  useEffect(() => {
    const isValid = validateFormData();
    setTriggerSearchApi(isValid);
  }, [queryString]);
  const handleSearch = () => {
    const isFromValid = validateFormData();
    if (isFromValid) {
      const searchParams = getSearchParams();
      updateSearchParams(searchParams);
    }
  };
  const renderSearchForm = () => {
    switch (formData[SearchFormField.searchType]) {
      case "operation name": {
        return (
          <>
            <Input
              id="search-operation-name"
              data-testid="search-operation-name"
              className="govuk-input--width-20"
              label={{
                children: "Operation name",
              }}
              errorMessage={
                formDataErrors[SearchFormField.operationName]
                  ? {
                      children: formDataErrors[SearchFormField.operationName],
                    }
                  : undefined
              }
              name="operation-name"
              type="text"
              value={formData[SearchFormField.operationName]}
              onChange={(value: string) =>
                handleFormChange(SearchFormField.operationName, value)
              }
              disabled={false}
            />

            <Select
              label={{
                htmlFor: "search-operation-area",
                children: "Select Area",
                className: styles.areaSelectLabel,
              }}
              id="search-operation-area"
              data-testid="search-operation-area"
              value={formData.operationArea}
              items={[
                { children: "--Select Area--", value: "" },
                { children: "option 1", value: 1 },
                { children: "option 2", value: 2 },
              ]}
              formGroup={{
                className: styles.select,
              }}
              onChange={(ev) =>
                handleFormChange(SearchFormField.operationArea, ev.target.value)
              }
              errorMessage={
                formDataErrors[SearchFormField.operationArea]
                  ? {
                      children: formDataErrors[SearchFormField.operationArea],
                    }
                  : undefined
              }
            />
          </>
        );
      }
      case "defendant name": {
        return (
          <>
            <Input
              id="search-defendant-name"
              data-testid="search-defendant-name"
              className="govuk-input--width-20"
              label={{
                children: "Defendant name",
              }}
              errorMessage={
                formDataErrors[SearchFormField.defendantName]
                  ? {
                      children: formDataErrors[SearchFormField.defendantName],
                    }
                  : undefined
              }
              name="defendant-name"
              type="text"
              value={formData[SearchFormField.defendantName]}
              onChange={(value: string) =>
                handleFormChange(SearchFormField.defendantName, value)
              }
              disabled={false}
            />

            <Select
              key="2"
              label={{
                htmlFor: "search-defendant-area",
                children: "Select Area",
                className: styles.areaSelectLabel,
              }}
              id="search-defendant-area"
              data-testid="search-defendant-area"
              items={[
                { children: "--Select Area--", value: "" },
                { children: "option 1", value: 1 },
                { children: "option 2", value: 2 },
              ]}
              formGroup={{
                className: styles.select,
              }}
              onChange={(ev) =>
                handleFormChange(SearchFormField.defendantArea, ev.target.value)
              }
              value={formData[SearchFormField.defendantArea]}
              errorMessage={
                formDataErrors[SearchFormField.defendantArea]
                  ? {
                      children: formDataErrors[SearchFormField.defendantArea],
                    }
                  : undefined
              }
            />
          </>
        );
      }
      case "urn": {
        return (
          <>
            <Input
              id="search-urn"
              data-testid="search-urn"
              className="govuk-input--width-20"
              label={{
                children: "URN",
              }}
              errorMessage={
                formDataErrors[SearchFormField.urn]
                  ? {
                      children: formDataErrors[SearchFormField.urn],
                    }
                  : undefined
              }
              name="urn"
              type="text"
              value={formData[SearchFormField.urn]}
              onChange={(value: string) =>
                handleFormChange(SearchFormField.urn, value)
              }
              disabled={false}
            />
          </>
        );
      }
    }
  };

  const getTitleText = () => {
    switch (formData[SearchFormField.searchType]) {
      case "operation name":
        return "Search for Operation name search";
      case "defendant name":
        return "Search for defendant surname search";
      default:
        return "Search for urn search";
    }
  };

  const getResultsCountText = () => {
    if (apiState.status !== "succeeded") return <> </>;
    const resultString =
      apiState.status === "succeeded" && apiState?.data?.length < 2
        ? "result"
        : "results";

    const resultHtml = (
      <>
        We've found {apiState?.data?.length} {resultString} for{" "}
      </>
    );
    switch (formData[SearchFormField.searchType]) {
      case "operation name":
        return (
          <>
            {resultHtml}
            <b>{searchParams["operation-name"]}</b> in {searchParams["area"]}.
          </>
        );
      case "defendant name":
        return (
          <>
            {resultHtml}
            <b>{searchParams["defendant-name"]}</b> in {searchParams["area"]}.
          </>
        );
      default:
        return (
          <>
            {resultHtml}
            <b>{searchParams["urn"]}</b>.
          </>
        );
    }
  };
  if (apiState.status === "loading") {
    return <div> Loading...</div>;
  }
  return (
    <div>
      <div className={styles.fullWidthContainer}>
        <div className="govuk-width-container">
          <div className={styles.contentTop}>
            {!!errorList.length && (
              <div
                ref={errorSummaryRef}
                tabIndex={-1}
                className={styles.errorSummaryWrapper}
              >
                <ErrorSummary
                  data-testid={"search-error-summary"}
                  className={styles.errorSummary}
                  errorList={errorList}
                  titleChildren="There is a problem"
                />
              </div>
            )}
            <h2>{getTitleText()}</h2>
            <div className={styles.inputWrapper}>
              {renderSearchForm()}
              <Button onClick={handleSearch}>Search</Button>
            </div>
            {apiState.status === "succeeded" && (
              <span className={styles.searchResultsCount}>
                {getResultsCountText()}
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
