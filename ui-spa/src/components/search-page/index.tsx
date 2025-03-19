import { useRef, useEffect } from "react";
import { Button, Radios, Input, Select, ErrorSummary } from "../govuk";
import useSearchNavigation from "../../common/hooks/useSearchNavigation";
import {
  useCaseSearchForm,
  SearchFormField,
  SearchFromData,
} from "../../common/hooks/useCaseSearchForm";
import { useFormattedAreaValues } from "../../common/hooks/useFormattedAreaValues";

import styles from "./index.module.scss";

const CaseSearchPage = () => {
  const errorSummaryRef = useRef<HTMLInputElement>(null);
  const { navigateWithParams } = useSearchNavigation();
  const formattedAreaValues = useFormattedAreaValues();
  const initialData: SearchFromData = {
    searchType: "urn",
    operationName: "",
    operationArea: "",
    defendantName: "",
    defendantArea: "",
    urn: "",
  };

  const {
    formData,
    formDataErrors,
    errorList,
    validateFormData,
    handleFormChange,
    getSearchParams,
  } = useCaseSearchForm(initialData);

  useEffect(() => {
    if (
      !formData[SearchFormField.defendantArea] &&
      formattedAreaValues.defaultValue
    ) {
      handleFormChange(
        SearchFormField.defendantArea,
        String(formattedAreaValues.defaultValue),
      );
    }
    if (
      !formData[SearchFormField.operationArea] &&
      formattedAreaValues.defaultValue
    ) {
      handleFormChange(
        SearchFormField.operationArea,
        String(formattedAreaValues.defaultValue),
      );
    }
  }, [formattedAreaValues]);

  useEffect(() => {
    if (errorList.length) errorSummaryRef.current?.focus();
  }, [errorList]);

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
                        defaultValue={formattedAreaValues.defaultValue}
                        items={formattedAreaValues.options}
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
                                  formDataErrors[SearchFormField.operationArea]
                                    .errorSummaryText,
                              }
                            : undefined
                        }
                      />,
                    ],
                  },
                  value: "operation name",
                },
                {
                  children: "Defendant surname",
                  conditional: {
                    children: [
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
                        defaultValue={formattedAreaValues.defaultValue}
                        items={formattedAreaValues.options}
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
                                  formDataErrors[SearchFormField.defendantArea]
                                    .errorSummaryText,
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
                                children:
                                  formDataErrors[SearchFormField.urn]
                                    .inputErrorText ??
                                  formDataErrors[SearchFormField.urn]
                                    .errorSummaryText,
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
