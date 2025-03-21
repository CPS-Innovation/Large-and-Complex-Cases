import { useRef, useState, useEffect } from "react";
import { useApi } from "../../common/hooks/useApi";
import {
  Button,
  Input,
  Select,
  ErrorSummary,
  Table,
  BackLink,
  Tag,
} from "../govuk";
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
        return (
          <>
            Search results for operation{" "}
            <b>{`"${searchParams["operation-name"]}"`}</b>
          </>
        );
      case "defendant name":
        return (
          <>
            Search results for defendant surname{" "}
            <b>{`"${searchParams["defendant-name"]}"`}</b>
          </>
        );
      default:
        return (
          <>
            Search results for URN <b>{`"${searchParams["urn"]}"`}</b>
          </>
        );
    }
  };

  const getAreaTextFromValue = (value: string | undefined) => {
    if (!value) return "";
    console.log("formattedAreaValues>>>", value);
    console.log("formattedAreaValues optiom>>>", formattedAreaValues.options);
    const area = formattedAreaValues.options.find(
      (area) => area.value === parseInt(value),
    );
    console.log("area>>", area?.children);
    return area?.children;
  };

  const getResultsCountText = () => {
    if (apiState.status !== "succeeded") return <> </>;
    const resultString =
      apiState.status === "succeeded" && apiState?.data?.length < 2
        ? "case"
        : "cases";

    if (apiState.data.length) {
      const resultHtml = (
        <>
          <b>{apiState?.data?.length}</b> {resultString}{" "}
        </>
      );
      switch (formData[SearchFormField.searchType]) {
        case "operation name":
          return (
            <>
              {resultHtml}
              found in <b>{getAreaTextFromValue(searchParams["area"])}</b>.
              Select a case to view more details.
            </>
          );
        case "defendant name":
          return (
            <>
              {resultHtml}
              found in <b>{getAreaTextFromValue(searchParams["area"])}</b>.
              Select a case to view more details.
            </>
          );
        default:
          return (
            <>
              {resultHtml}
              found. Select a case to view more details.
            </>
          );
      }
    }

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
            There are <b>no cases</b> matching the defendant surname in{" "}
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

  const getSearchTypeText = () => {
    if (formData[SearchFormField.searchType] === "defendant name")
      return "defendant surname";
    return formData[SearchFormField.searchType];
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
            children:
              data.egressStatus === "connected" ? (
                <Tag gdsTagColour="green" className={styles.statusTag}>
                  Connected
                </Tag>
              ) : (
                <Tag gdsTagColour="grey" className={styles.statusTag}>
                  Inactive
                </Tag>
              ),
          },
          {
            children:
              data.sharedDriveStatus === "connected" ? (
                <Tag gdsTagColour="green" className={styles.statusTag}>
                  Connected
                </Tag>
              ) : (
                <Tag gdsTagColour="grey" className={styles.statusTag}>
                  Inactive
                </Tag>
              ),
          },
          {
            children: data.registrationDate || "Unknown",
          },
          {
            children: (
              <Link to="/" className={styles.link}>
                View{" "}
              </Link>
            ),
          },
        ],
      };
    });
  };

  if (apiState.status === "loading") {
    return <div className="govuk-width-container"> Loading...</div>;
  }
  return (
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

      <div className={styles.results}>
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
              <span>You can:</span>
            </div>
            <ul className="govuk-list govuk-list--bullet">
              <li>check for spelling mistakes in the {getSearchTypeText()}.</li>
              <li>
                check the Case Management System to make sure the case exists
                and that you have access.
              </li>
              <li>contact the product team if you need further help.</li>
            </ul>
          </div>
        )}
      </div>
    </div>
  );
};

export default CaseSearchResultPage;
