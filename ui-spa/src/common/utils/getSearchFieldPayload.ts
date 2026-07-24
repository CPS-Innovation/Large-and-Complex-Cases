import {
  SearchFormField,
  type SearchFormData,
} from "../../common/hooks/useCaseSearchForm";

export const getSearchFieldPayload = (formData: SearchFormData) => {
  let payload = { ...formData };
  const resetOperationNameFields = {
    [SearchFormField.operationName]: "",
    [SearchFormField.operationArea]: "",
  };
  const resetDefendantNameFields = {
    [SearchFormField.defendantName]: "",
    [SearchFormField.defendantArea]: "",
  };

  if (formData[SearchFormField.searchType] === "urn") {
    payload = {
      ...payload,
      ...resetOperationNameFields,
      ...resetDefendantNameFields,
    };
  }
  if (formData[SearchFormField.searchType] === "operation name") {
    payload = {
      ...payload,
      ...resetDefendantNameFields,
      [SearchFormField.urn]: "",
    };
  }
  if (formData[SearchFormField.searchType] === "defendant name") {
    payload = {
      ...payload,
      ...resetOperationNameFields,
      [SearchFormField.urn]: "",
    };
  }
  return payload;
};
