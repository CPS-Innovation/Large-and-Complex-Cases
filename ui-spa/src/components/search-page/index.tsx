import { useRef, useEffect, useContext } from "react";
import { MainStateContext } from "../../providers/MainStateProvider";
import { Button, Radios, Input, Select, ErrorSummary } from "../govuk";
import useSearchNavigation from "../../common/hooks/useSearchNavigation";
import {
  useCaseSearchForm,
  SearchFormField,
  SearchFormData,
} from "../../common/hooks/useCaseSearchForm";
import { useFormattedAreaValues } from "../../common/hooks/useFormattedAreaValues";
import { useGetCaseDivisionsOrAreas } from "../../common/hooks/useGetCaseDivisionsOrAreas";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import { getSearchFieldPayload } from "../../common/utils/getSearchFieldPayload";

import styles from "./index.module.scss";

const CaseSearchPage = () => {
  const errorSummaryRef = useRef<HTMLInputElement>(null);
  const { state, dispatch } = useContext(MainStateContext);
  const { navigateWithParams } = useSearchNavigation();
  useGetCaseDivisionsOrAreas();
  const formattedAreaValues = useFormattedAreaValues();
  const getInitialState = () => {
    const initialData: SearchFormData = {
      searchType: state.formData.searchType,
      operationName: state.formData.operationName,
      operationArea: state.formData.operationArea,
      defendantName: state.formData.defendantName,
      defendantArea: state.formData.defendantArea,
      urn: state.formData.urn,
    };
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
  }, [formattedAreaValues, formData]);

  useEffect(() => {
    if (errorList.length) errorSummaryRef.current?.focus();
  }, [errorList]);

  const handleSearch = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const isFromValid = validateFormData();
    if (isFromValid) {
      const payload = getSearchFieldPayload(formData);
      dispatch({
        type: "SET_FORM_DATA_FIELD",
        payload: payload,
      });
      const searchParams = getSearchParams();
      navigateWithParams(searchParams);
    }
  };

  return (
    <div className={styles.pageWrapper}>
      <PageContentWrapper>
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
                  children: (
                    <>
                      Select how you want to find a case.
                      <br />
                      For operation name and defendant name you will also need
                      to select an area.
                    </>
                  ),
                }}
                items={[
                  {
                    children: "Operation name",
                    conditional: {
                      children: [
                        <Input
                          key="search-operation-name"
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
                                    formDataErrors[
                                      SearchFormField.operationName
                                    ].inputErrorText ??
                                    formDataErrors[
                                      SearchFormField.operationName
                                    ].errorSummaryText,
                                }
                              : undefined
                          }
                          name="operation-name"
                          type="text"
                          value={formData[SearchFormField.operationName]}
                          onChange={(value: string) =>
                            handleFormChange(
                              SearchFormField.operationName,
                              value,
                            )
                          }
                          disabled={false}
                        />,

                        <Select
                          key="search-operation-area"
                          label={{
                            htmlFor: "search-operation-area",
                            children: "Select Area",
                            className: styles.areaSelectLabel,
                          }}
                          id="search-operation-area"
                          data-testid="search-operation-area"
                          value={formData[SearchFormField.operationArea]}
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
                                    formDataErrors[
                                      SearchFormField.operationArea
                                    ].inputErrorText ??
                                    formDataErrors[
                                      SearchFormField.operationArea
                                    ].errorSummaryText,
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
                          key="1"
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
                                    formDataErrors[
                                      SearchFormField.defendantName
                                    ].inputErrorText ??
                                    formDataErrors[
                                      SearchFormField.defendantName
                                    ].errorSummaryText,
                                }
                              : undefined
                          }
                          name="defendant-name"
                          type="text"
                          value={formData[SearchFormField.defendantName]}
                          onChange={(value: string) =>
                            handleFormChange(
                              SearchFormField.defendantName,
                              value,
                            )
                          }
                          disabled={false}
                        />,

                        <Select
                          key="search-defendant-area"
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
                                    formDataErrors[
                                      SearchFormField.defendantArea
                                    ].inputErrorText ??
                                    formDataErrors[
                                      SearchFormField.defendantArea
                                    ].errorSummaryText,
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
                    children: "URN (Unique Reference Number)",
                    conditional: {
                      children: [
                        <Input
                          key="search-urn"
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
                  if (value)
                    handleFormChange(SearchFormField.searchType, value);
                }}
              />
            </div>
            <Button type={"submit"}>Search</Button>
          </form>
        </div>
      </PageContentWrapper>
    </div>
  );
};

export default CaseSearchPage;
