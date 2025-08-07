import { useRef, useEffect, useMemo } from "react";
import { Button, Radios, Input, Select, ErrorSummary } from "../govuk";
import useSearchNavigation from "../../common/hooks/useSearchNavigation";
import {
  useCaseSearchForm,
  SearchFormField,
  SearchFromData,
} from "../../common/hooks/useCaseSearchForm";
import { useLocation } from "react-router";
import { useFormattedAreaValues } from "../../common/hooks/useFormattedAreaValues";

import styles from "./index.module.scss";

const CaseSearchPage = () => {
  const errorSummaryRef = useRef<HTMLInputElement>(null);
  const { navigateWithParams } = useSearchNavigation();
  const formattedAreaValues = useFormattedAreaValues();
  const location = useLocation();

  const initialData: SearchFromData = useMemo(
    () => ({
      searchType: location.state?.searchType ?? "urn",
      operationName: location.state?.operationName ?? "",
      operationArea: location.state?.operationArea ?? "",
      defendantName: location.state?.defendantName ?? "",
      defendantArea: location.state?.defendantArea ?? "",
      urn: location.state?.urn ?? "",
    }),
    [location],
  );

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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formattedAreaValues]);

  useEffect(() => {
    if (errorList.length) errorSummaryRef.current?.focus();
  }, [errorList]);

  const handleSearch = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
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
              errorList={errorList}
              titleChildren="There is a problem"
            />
          </div>
        )}
        <form onSubmit={handleSearch}>
          <div className={styles.inputWrapper}>
            <Radios
              aria-label="choose search type"
              hint={{
                children: <>Select how you want to find a case.<br/>For operation name and defendant name you will also need to select an area.</>,
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
                  "data-testid": "radio-search-operation-name",
                  disabled: !formattedAreaValues.options.length,
                },

                {
                  children: "Defendant last name",
                  conditional: {
                    children: [
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
                  "data-testid": "radio-search-defendant-name",
                  disabled: !formattedAreaValues.options.length,
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
                          handleFormChange(
                            SearchFormField.urn,
                            value.toUpperCase(),
                          )
                        }
                        disabled={false}
                      />,
                    ],
                  },
                  value: "urn",
                  "data-testid": "radio-search-urn",
                },
              ]}
              name="case-search-types"
              value={formData.searchType}
              onChange={(value) => {
                if (value) handleFormChange(SearchFormField.searchType, value);
              }}
            />
          </div>
          <Button type={"submit"}>Search</Button>
        </form>
      </div>
    </div>
  );
};

export default CaseSearchPage;
