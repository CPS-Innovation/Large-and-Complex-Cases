import { useRef, useState, useEffect, useContext } from "react";
import { Button, Input, Select, ErrorSummary, BackLink } from "../govuk";
import { MainStateContext } from "../../providers/MainStateProvider";
import { getCaseSearchResults } from "../../apis/gateway-api";
import useSearchNavigation from "../../common/hooks/useSearchNavigation";
import {
  useCaseSearchForm,
  SearchFormField,
} from "../../common/hooks/useCaseSearchForm";
import SearchResults from "./SearchResults";
import { useFormattedAreaValues } from "../../common/hooks/useFormattedAreaValues";
import { useGetCaseDivisionsOrAreas } from "../../common/hooks/useGetCaseDivisionsOrAreas";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import { useQuery } from "@tanstack/react-query";
import styles from "./index.module.scss";

const CaseSearchResultPage = () => {
  const [triggerSearchApi, setTriggerSearchApi] = useState(false);
  const [validatedAreaValues, setValidatedAreaValues] = useState(false);
  const errorSummaryRef = useRef<HTMLInputElement>(null);
  const { state, dispatch } = useContext(MainStateContext);
  const { updateSearchParams, searchParams, queryString } =
    useSearchNavigation();
  const { formData } = state;
  useEffect(() => {
    const searchParamKeys = Object.keys(searchParams);
    if (
      searchParamKeys.includes("defendant-name") &&
      searchParamKeys.includes("area") &&
      !formData[SearchFormField.defendantName]
    ) {
      dispatch({
        type: "SET_FORM_DATA_FIELD",
        payload: {
          searchType: "defendant name",
          defendantName: searchParams["defendant-name"] ?? "",
          defendantArea: searchParams["area"] ?? "",
        },
      });
    }
    if (
      searchParamKeys.includes("operation-name") &&
      searchParamKeys.includes("area") &&
      !formData[SearchFormField.operationName]
    ) {
      dispatch({
        type: "SET_FORM_DATA_FIELD",
        payload: {
          searchType: "operation name",
          operationName: searchParams["operation-name"] ?? "",
          operationArea: searchParams["area"] ?? "",
        },
      });
    }
    if (searchParamKeys.includes("urn") && !formData[SearchFormField.urn]) {
      dispatch({
        type: "SET_FORM_DATA_FIELD",
        payload: {
          searchType: "urn",
          urn: searchParams["urn"] ?? "",
        },
      });
    }
  }, [dispatch, formData, searchParams]);

  const {
    formDataErrors,
    errorList,
    validateFormData,
    handleFormChange,
    getSearchParams,
  } = useCaseSearchForm();

  const formattedAreaValues = useFormattedAreaValues();

  const { data: searchResults, isLoading: isSearchResultsLoading } = useQuery({
    queryKey: [searchParams],
    queryFn: () => getCaseSearchResults(searchParams),
    retry: false,
    enabled: triggerSearchApi,
    throwOnError: true,
    staleTime: 0,
    gcTime: 0,
  });
  const { isLoading: isDivisionsOrAreasLoading } = useGetCaseDivisionsOrAreas();

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
  }, [formattedAreaValues, formData]);

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
                          .inputErrorText ??
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
                          .inputErrorText ??
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
                          .inputErrorText ??
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
                          .inputErrorText ??
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
    const instructionText = (
      <>
        Select <b>View</b> to transfer files or folders or <b>Connect</b> to
        setup storage locations.
      </>
    );

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
            found in <b>{getAreaTextFromValue(searchParams["area"])}</b>.{" "}
            {instructionText}
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
  if ((isSearchResultsLoading || isDivisionsOrAreasLoading) && !searchResults) {
    return <div>Loading...</div>;
  }
  return (
    <div>
      <BackLink to="/" state={{ ...formData }}>
        Back
      </BackLink>
      <PageContentWrapper>
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
          {!!searchResults?.length && (
            <div className={styles.searchResultsCount}>
              {getResultsCountText(searchResults.length)}
            </div>
          )}
          {!searchResults?.length && <div>{getNoResultsText()}</div>}
        </div>

        {
          <SearchResults
            searchQueryString={queryString}
            searchApiResults={searchResults ?? []}
            searchType={formData[SearchFormField.searchType]}
          />
        }
      </PageContentWrapper>
    </div>
  );
};

export default CaseSearchResultPage;
