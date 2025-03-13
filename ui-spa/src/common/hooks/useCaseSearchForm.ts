import { useState, useCallback, useMemo } from "react";
import { SearchParamsType } from "../../common/hooks/useSearchNavigation";
import { validateUrn } from "../utils/validateUrn";

export enum SearchFormField {
  searchType = "searchType",
  operationName = "operationName",
  operationArea = "operationArea",
  defendantName = "defendantName",
  defendantArea = "defendantArea",
  urn = "urn",
}

export type SearchFromData = {
  [SearchFormField.searchType]: "operation name" | "defendant name" | "urn";
  [SearchFormField.operationName]: string;
  [SearchFormField.operationArea]: string;
  [SearchFormField.defendantName]: string;
  [SearchFormField.defendantArea]: string;
  [SearchFormField.urn]: string;
};
type ErrorText = {
  errorSummaryText: string;
  inputErrorText?: string;
};
type SearchFormDataErrors = {
  [SearchFormField.operationName]?: ErrorText;
  [SearchFormField.operationArea]?: ErrorText;
  [SearchFormField.defendantName]?: ErrorText;
  [SearchFormField.defendantArea]?: ErrorText;
  [SearchFormField.urn]?: ErrorText;
};

export const useCaseSearchForm = (initialData: SearchFromData) => {
  const initialErrorState: SearchFormDataErrors = {};

  const [formData, setFormData] = useState<SearchFromData>(initialData);

  const [formDataErrors, setFormDataErrors] =
    useState<SearchFormDataErrors>(initialErrorState);

  const errorSummaryProperties = useCallback(
    (inputName: SearchFormField) => {
      switch (inputName) {
        case SearchFormField.defendantName:
          return {
            children: formDataErrors[inputName]?.errorSummaryText,
            href: "#search-defendant-name",
            "data-testid": "search-defendant-name-link",
          };
        case SearchFormField.defendantArea:
          return {
            children: formDataErrors[inputName]?.errorSummaryText,
            href: "#search-defendant-area",
            "data-testid": "search-operation-area-link",
          };
        case SearchFormField.operationName:
          return {
            children: formDataErrors[inputName]?.errorSummaryText,
            href: "#search-operation-name",
            "data-testid": "search-operation-name-link",
          };
        case SearchFormField.operationArea:
          return {
            children: formDataErrors[inputName]?.errorSummaryText,
            href: "#search-operation-area",
            "data-testid": "search-operation-area-link",
          };
        case SearchFormField.urn:
          return {
            children: formDataErrors[inputName]?.errorSummaryText,
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

  const validateFormData = () => {
    const errorTexts: SearchFormDataErrors = {};
    switch (formData[SearchFormField.searchType]) {
      case "operation name": {
        if (!formData[SearchFormField.operationName]) {
          errorTexts[SearchFormField.operationName] = {
            errorSummaryText: "Operation name should not be empty",
          };
        }
        if (!formData[SearchFormField.operationArea]) {
          errorTexts[SearchFormField.operationArea] = {
            errorSummaryText: "Operation area should not be empty",
          };
        }
        break;
      }
      case "defendant name": {
        if (!formData[SearchFormField.defendantName]) {
          errorTexts[SearchFormField.defendantName] = {
            errorSummaryText: "Defendant surname should not be empty",
          };
        }
        if (!formData[SearchFormField.defendantArea]) {
          errorTexts[SearchFormField.defendantArea] = {
            errorSummaryText: "Defendant area should not be empty",
          };
        }
        break;
      }
      case "urn": {
        const { isValid } = validateUrn(formData[SearchFormField.urn]);
        if (!formData[SearchFormField.urn]) {
          errorTexts[SearchFormField.urn] = {
            errorSummaryText: "URN should not be empty",
          };
          break;
        }
        if (!isValid) {
          errorTexts[SearchFormField.urn] = {
            errorSummaryText: "Enter a valid unique reference number",
            inputErrorText:
              "The unique reference number must be 11 characters long and include only letters and numbers",
          };
          break;
        }
      }
    }

    const isValid = !Object.entries(errorTexts).filter(([_, value]) => value)
      .length;

    setFormDataErrors(errorTexts);
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
        const { isValid, rootUrn } = validateUrn(formData[SearchFormField.urn]);
        searchParams = { urn: isValid ? rootUrn : "" };
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

  return {
    formData,
    formDataErrors,
    errorList,
    validateFormData,
    handleFormChange,
    getSearchParams,
  };
};
