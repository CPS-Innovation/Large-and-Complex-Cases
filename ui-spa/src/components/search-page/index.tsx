import { useRef, useState, useCallback, useMemo, useEffect } from "react";
// import { useQueryParamsState } from "../../common/hooks/useQueryParamsState";
// import { useCaseSearchInputLogic } from "../../common/hooks/useCaseSearchInputLogic";
// import { CaseSearchQueryParams } from "../../common/types/CaseSearchQueryParams";
import { Button, Radios, Input, Select, ErrorSummary } from "../govuk";
import useSearchNavigation, {
  SearchParamsType,
} from "../../common/hooks/useSearchNavigation";
import styles from "./index.module.scss";

const CaseSearchPage = () => {
  const errorSummaryRef = useRef<HTMLInputElement>(null);

  const { navigateWithParams } = useSearchNavigation();

  enum SearchFormField {
    searchType = "searchType",
    operationName = "operationName",
    operationArea = "operationArea",
    defendantName = "defendantName",
    defendantArea = "defendantArea",
    urn = "urn",
  }

  type SearchFromData = {
    [SearchFormField.searchType]:
      | "operation name"
      | "defendant name"
      | "urn"
      | undefined;
    [SearchFormField.operationName]: string;
    [SearchFormField.operationArea]: string;
    [SearchFormField.defendantName]: string;
    [SearchFormField.defendantArea]: string;
    [SearchFormField.urn]: string;
  };

  type SearchFormDataErrors = {
    [SearchFormField.operationName]: string;
    [SearchFormField.operationArea]: string;
    [SearchFormField.defendantName]: string;
    [SearchFormField.defendantArea]: string;
    [SearchFormField.urn]: string;
  };

  const initialData: SearchFromData = {
    searchType: "urn",
    operationName: "",
    operationArea: "",
    defendantName: "",
    defendantArea: "",
    urn: "",
  };

  const initialErrorState: SearchFormDataErrors = {
    operationName: "",
    operationArea: "",
    defendantName: "",
    defendantArea: "",
    urn: "",
  };

  const [formData, setFormData] = useState<SearchFromData>(initialData);

  const [formDataErrors, setFormDataErrors] =
    useState<SearchFormDataErrors>(initialErrorState);

  const errorSummaryProperties = useCallback(
    (inputName: SearchFormField) => {
      switch (inputName) {
        case SearchFormField.defendantName:
          return {
            children: formDataErrors[inputName],
            href: "#search-defendant-name",
            "data-testid": "search-defendant-name-link",
          };
        case SearchFormField.defendantArea:
          return {
            children: formDataErrors[inputName],
            href: "#search-defendant-area",
            "data-testid": "search-operation-area-link",
          };
        case SearchFormField.operationName:
          return {
            children: formDataErrors[inputName],
            href: "#search-operation-name",
            "data-testid": "search-operation-name-link",
          };
        case SearchFormField.operationArea:
          return {
            children: formDataErrors[inputName],
            href: "#search-operation-area",
            "data-testid": "search-operation-area-link",
          };
        case SearchFormField.urn:
          return {
            children: formDataErrors[inputName],
            href: "#search-urn",
            "data-testid": "search-urn-link",
          };
      }
    },
    [formDataErrors],
  );

  const errorList = useMemo(() => {
    const validErrorKeys = Object.keys(formDataErrors).filter(
      (errorKey) => formDataErrors[errorKey as keyof SearchFormDataErrors],
    );

    console.log("validErrorKeys>>", validErrorKeys);

    const errorSummary = validErrorKeys.map((errorKey, index) => ({
      reactListKey: `${index}`,
      ...errorSummaryProperties(errorKey as SearchFormField)!,
    }));

    console.log("errorSummary>>", errorSummary);
    return errorSummary;
  }, [formDataErrors]);

  useEffect(() => {
    if (errorList.length) errorSummaryRef.current?.focus();
  }, [errorList]);

  const validateFormData = () => {
    const errorTexts: SearchFormDataErrors = {
      operationName: "",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "",
    };
    switch (formData[SearchFormField.searchType]) {
      case "operation name": {
        if (!formData[SearchFormField.operationName]) {
          errorTexts[SearchFormField.operationName] =
            "Operation name should not be empty";
        }
        if (!formData[SearchFormField.operationArea]) {
          errorTexts[SearchFormField.operationArea] =
            "Operation area should not be empty";
        }
        break;
      }
      case "defendant name": {
        if (!formData[SearchFormField.defendantName]) {
          errorTexts[SearchFormField.defendantName] =
            "Defendant name should not be empty";
        }
        if (!formData[SearchFormField.defendantArea]) {
          errorTexts[SearchFormField.defendantArea] =
            "Defendant area should not be empty";
        }
        break;
      }
      case "urn": {
        if (!formData[SearchFormField.urn]) {
          errorTexts[SearchFormField.urn] = "urn should not be empty";
        }
        break;
      }
    }

    const isValid = !Object.entries(errorTexts).filter(([_, value]) => value)
      .length;

    if (!isValid) setFormDataErrors(errorTexts);
    return isValid;
  };

  const handleFormChange = (
    field: SearchFormField,
    value: string | number | boolean,
  ) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const getSearchParams = () => {
    let searchParams: SearchParamsType = {};

    switch (formData[SearchFormField.searchType]) {
      case "urn":
        searchParams = { urn: formData[SearchFormField.urn] };
        break;
      case "defendant name":
        searchParams = {
          "defendant-name": formData[SearchFormField.defendantName],
          area: formData[SearchFormField.defendantArea],
        };
        break;
      case "operation name":
        searchParams = {
          "operation-name": formData[SearchFormField.operationName],
          area: formData[SearchFormField.operationArea],
        };
        break;
    }

    return searchParams;
  };

  const handleSearch = () => {
    const isFromValid = validateFormData();
    if (isFromValid) {
      const searchParams = getSearchParams();
      navigateWithParams(searchParams);
    }
  };

  return (
    <div className={`govuk-width-container ${styles.pageWrapper}`}>
      <div>
        <h1 className="govuk-heading-xl govuk-!-margin-bottom-4">
          Find a case
        </h1>
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
        <div>
          <div className={styles.inputWrapper}>
            <Radios
              fieldset={{
                legend: {
                  children: <b>Search Large and Complex Cases</b>,
                },
              }}
              hint={{
                children: "Select one option",
              }}
              items={[
                {
                  children: "Operation name",
                  conditional: {
                    children: [
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
                                  formDataErrors[SearchFormField.operationName],
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
                      />,

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
                          { children: "--Select Area--", value: 0 },
                          { children: "option 1", value: 1 },
                          { children: "option 2", value: 2 },
                        ]}
                        formGroup={{
                          className: styles.select,
                        }}
                        onChange={(ev) =>
                          handleFormChange(
                            SearchFormField.operationArea,
                            ev.target.value,
                          )
                        }
                        errorMessage={
                          formDataErrors[SearchFormField.operationArea]
                            ? {
                                children:
                                  formDataErrors[SearchFormField.operationArea],
                              }
                            : undefined
                        }
                      />,
                    ],
                  },
                  value: "operation name",
                },
                {
                  children: "Defendant name",
                  conditional: {
                    children: [
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
                                children:
                                  formDataErrors[SearchFormField.defendantName],
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
                      />,

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
                          { children: "--Select Area--", value: 0 },
                          { children: "option 1", value: 1 },
                          { children: "option 2", value: 2 },
                        ]}
                        formGroup={{
                          className: styles.select,
                        }}
                        onChange={(ev) =>
                          handleFormChange(
                            SearchFormField.defendantArea,
                            ev.target.value,
                          )
                        }
                        value={formData[SearchFormField.defendantArea]}
                        errorMessage={
                          formDataErrors[SearchFormField.defendantArea]
                            ? {
                                children:
                                  formDataErrors[SearchFormField.defendantArea],
                              }
                            : undefined
                        }
                      />,
                    ],
                  },
                  value: "defendant name",
                },
                {
                  children: "URN",
                  conditional: {
                    children: [
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
                      />,
                    ],
                  },
                  value: "urn",
                },
              ]}
              name="case-search-types"
              value={formData.searchType}
              onChange={(value) => {
                if (value) handleFormChange(SearchFormField.searchType, value);
              }}
            />
          </div>
          <Button onClick={handleSearch}>Search</Button>
        </div>
      </div>
    </div>
  );
};

export default CaseSearchPage;
