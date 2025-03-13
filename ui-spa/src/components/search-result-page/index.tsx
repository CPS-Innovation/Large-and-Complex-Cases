import { useRef, useState, useEffect } from "react";
import { useApi } from "../../common/hooks/useApi";
import { Button, Input, Select, ErrorSummary, Table, BackLink } from "../govuk";
import { getCaseSearchResults } from "../../apis/gateway-api";
import useSearchNavigation from "../../common/hooks/useSearchNavigation";
import {
  useCaseSearchForm,
  SearchFormField,
  SearchFromData,
} from "../../common/hooks/useCaseSearchForm";
import { Link } from "react-router";
import { useFormattedAreaValues } from "../../common/hooks/useFormattedAreaValues";
import styles from "./index.module.scss";

const CaseSearchResultPage = () => {
  const [triggerSearchApi, setTriggerSearchApi] = useState(false);
  const errorSummaryRef = useRef<HTMLInputElement>(null);
  const { updateSearchParams, searchParams, queryString } =
    useSearchNavigation();
  const formattedAreaValues = useFormattedAreaValues();
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
                children: "Select Area",
                className: styles.areaSelectLabel,
              }}
              id="search-operation-area"
              data-testid="search-operation-area"
              value={formData.operationArea}
              items={formattedAreaValues}
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
                children: "Defendant surname",
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
              items={formattedAreaValues}
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
        return "Search for Defendant surname search";
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

    const resultHtml = apiState.data.length ? (
      <>
        We've found {apiState?.data?.length} {resultString} for{" "}
      </>
    ) : (
      <>There are no matching results for </>
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

  const getTableRowData = () => {
    if (apiState.status !== "succeeded") return [];
    return apiState.data.map((data: any) => {
      return {
        cells: [
          {
            children: (
              <Link to="/" className={styles.link}>
                {data.operationName
                  ? data.operationName
                  : data.leadDefendantName}
              </Link>
            ),
          },
          {
            children: data.urn,
          },
          {
            children: data.leadDefendantName,
          },
          {
            children: data.egressStatus,
          },
          {
            children: data.sharedDrive,
          },
          {
            children: data.dateCreated,
          },
          {
            children: (
              <Link to="/" className={styles.link}>
                view{" "}
              </Link>
            ),
          },
        ],
      };
    });
  };

  if (apiState.status === "loading") {
    return <div> Loading...</div>;
  }
  return (
    <div>
      <div className={styles.fullWidthContainer}>
        <div className="govuk-width-container">
          <BackLink href="/">Back</BackLink>
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
            <h1>{getTitleText()}</h1>
            <div className={styles.inputWrapper}>
              {renderSearchForm()}
              <div className={styles.btnWrapper}>
                <Button onClick={handleSearch}>Search</Button>
              </div>
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
        {apiState.status === "succeeded" && !!apiState.data.length && (
          <Table
            head={[
              {
                children: "Defendant or Operation name",
              },
              {
                children: "URN",
              },
              {
                children: "Lead defendant",
              },
              {
                children: "Egress",
              },
              {
                children: "Shared Drive",
              },
              {
                children: "Case created",
              },
              {
                children: "",
              },
            ]}
            rows={getTableRowData()}
          ></Table>
        )}
        {apiState.status === "succeeded" && !apiState.data.length && (
          <div className={styles.noResultsContent}>
            <div>
              <b>You can:</b>
            </div>
            <ul className="govuk-list govuk-list--bullet">
              <li>check CMS to see if the case exists</li>
              <li>check the spelling of your search</li>
              <li>
                check with your Unit Manager to see if the case is restricted
              </li>
              <li>contact the Service Desk</li>
            </ul>
          </div>
        )}
      </div>
    </div>
  );
};

export default CaseSearchResultPage;
