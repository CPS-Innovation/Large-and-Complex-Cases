import { useRef, useState, useEffect } from "react";
import { useApi } from "../../common/hooks/useApi";
import { Button, Input, Select, ErrorSummary, BackLink } from "../govuk";
import { getCaseSearchResults } from "../../apis/gateway-api";
import useSearchNavigation from "../../common/hooks/useSearchNavigation";
import {
  useCaseSearchForm,
  SearchFormField,
  SearchFromData,
} from "../../common/hooks/useCaseSearchForm";
import SearchResults from "./SearchResults";
import { useFormattedAreaValues } from "../../common/hooks/useFormattedAreaValues";
import styles from "./index.module.scss";

const CaseSearchResultPage = () => {
  const [triggerSearchApi, setTriggerSearchApi] = useState(false);
  const [validatedAreaValues, setValidatedAreaValues] = useState(false);
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

    if (
      searchParamKeys.includes("defendant-name") &&
      searchParamKeys.includes("area")
    ) {
      initialData.searchType = "defendant name";
      initialData.defendantName = searchParams["defendant-name"] ?? "";
      return initialData;
    }
    if (
      searchParamKeys.includes("operation-name") &&
      searchParamKeys.includes("area")
    ) {
      initialData.searchType = "operation name";
      initialData.operationName = searchParams["operation-name"] ?? "";
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

  const formattedAreaValues = useFormattedAreaValues(
    formData[SearchFormField.searchType] === "urn",
  );

  const caseSearchApi = useApi(
    getCaseSearchResults,
    [queryString],
    triggerSearchApi,
  );

  useEffect(() => {
    if (caseSearchApi.status === "failed")
      throw new Error(`${caseSearchApi.error}`);
  }, [caseSearchApi]);

  useEffect(() => {
    if (formData[SearchFormField.searchType] === "urn" || validatedAreaValues) {
      const isValid = validateFormData();
      if (!triggerSearchApi && isValid) setTriggerSearchApi(true);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [queryString, validatedAreaValues]);

  useEffect(() => {
    if (errorList.length) errorSummaryRef.current?.focus();
  }, [errorList]);

  useEffect(() => {
    const isAreaValid = formattedAreaValues.options.find(
      (option) => `${option.value}` === searchParams["area"],
    );

    if (!formData[SearchFormField.defendantArea] && isAreaValid) {
      handleFormChange(
        SearchFormField.defendantArea,
        String(searchParams["area"]),
      );
    }
    if (!formData[SearchFormField.operationArea] && isAreaValid) {
      handleFormChange(
        SearchFormField.operationArea,
        String(searchParams["area"]),
      );
    }
    if (formattedAreaValues.options.length) setValidatedAreaValues(true);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formattedAreaValues]);

  const handleSearch = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
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
                      children:
                        formDataErrors[SearchFormField.operationName]
                          .errorSummaryText,
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
                children: "Select area",
                className: styles.areaSelectLabel,
              }}
              id="search-operation-area"
              data-testid="search-operation-area"
              value={formData.operationArea}
              items={formattedAreaValues.options}
              formGroup={{
                className: styles.select,
              }}
              onChange={(ev) =>
                handleFormChange(SearchFormField.operationArea, ev.target.value)
              }
              errorMessage={
                formDataErrors[SearchFormField.operationArea]
                  ? {
                      children:
                        formDataErrors[SearchFormField.operationArea]
                          .errorSummaryText,
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
                children: "Defendant last name",
              }}
              errorMessage={
                formDataErrors[SearchFormField.defendantName]
                  ? {
                      children:
                        formDataErrors[SearchFormField.defendantName]
                          .errorSummaryText,
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
              items={formattedAreaValues.options}
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
                      children:
                        formDataErrors[SearchFormField.defendantArea]
                          .errorSummaryText,
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
                      children:
                        formDataErrors[SearchFormField.urn].inputErrorText ??
                        formDataErrors[SearchFormField.urn].errorSummaryText,
                    }
                  : undefined
              }
              name="urn"
              type="text"
              value={formData[SearchFormField.urn]}
              onChange={(value: string) =>
                handleFormChange(SearchFormField.urn, value.toUpperCase())
              }
              disabled={false}
            />
          </>
        );
      }
    }
  };

  const getTitleText = () => {
    return "Search results";
  };

  const getAreaTextFromValue = (value: string | undefined) => {
    if (!value) return "";
    const area = formattedAreaValues.options.find(
      (area) => area.value === parseInt(value),
    );
    return area?.children;
  };

  const getResultsCountText = (resultsCount: number) => {
    const resultString = resultsCount < 2 ? "case" : "cases";
    const instructionText = <>Select <b>view</b> to transfer files or folders      or <b>connect</b> to setup storage locations.</>

    const resultHtml = (
      <>
        <b>{resultsCount}</b> {resultString}{" "}
      </>
    );
    switch (formData[SearchFormField.searchType]) {
      case "operation name":
      case "defendant name":
        return (
          <>
            {resultHtml}
            found in <b>{getAreaTextFromValue(searchParams["area"])}</b>. {instructionText}
          </>
        );
      default:
        return (
          <>
            {resultHtml}
            found. {instructionText}
          </>
        );
    }
  };

  const getNoResultsText = () => {
    switch (formData[SearchFormField.searchType]) {
      case "operation name":
        return (
          <>
            There are <b>no cases</b> matching the operation name in{" "}
            <b>{getAreaTextFromValue(searchParams["area"])}</b>.
          </>
        );
      case "defendant name":
        return (
          <>
            There are <b>no cases</b> matching the defendant last name in{" "}
            <b>{getAreaTextFromValue(searchParams["area"])}</b>.
          </>
        );
      case "urn":
        return (
          <>
            There are <b>no cases</b> matching the urn.
          </>
        );
    }
  };

  if (
    ((caseSearchApi.status === "loading" ||
      caseSearchApi.status === "initial") &&
      !errorList.length) ||
    (formData[SearchFormField.searchType] !== "urn" &&
      !formattedAreaValues.options.length)
  ) {
    return <div className="govuk-width-container">Loading...</div>;
  }
  return (
    <div className="govuk-width-container">
      <BackLink to="/" state={{ ...formData }}>
        Back
      </BackLink>
      <div className={styles.contentTop}>
        {!!errorList.length && (
          <div
            ref={errorSummaryRef}
            tabIndex={-1}
            className={styles.errorSummaryWrapper}
          >
            <ErrorSummary
              data-testid={"search-error-summary"}
              errorList={errorList}
              titleChildren="There is a problem"
            />
          </div>
        )}
        <h1>{getTitleText()}</h1>
        <form onSubmit={handleSearch}>
          <div className={styles.inputWrapper}>
            {renderSearchForm()}
            <div className={styles.btnWrapper}>
              <Button type="submit">Search</Button>
            </div>
          </div>
        </form>
        {!!caseSearchApi.data?.length && (
          <div className={styles.searchResultsCount}>
            {getResultsCountText(caseSearchApi.data.length)}
          </div>
        )}
        {!caseSearchApi.data?.length && <div>{getNoResultsText()}</div>}
      </div>

      <SearchResults
        searchQueryString={queryString}
        searchApiResults={caseSearchApi}
        searchType={formData[SearchFormField.searchType]}
      />
    </div>
  );
};

export default CaseSearchResultPage;
