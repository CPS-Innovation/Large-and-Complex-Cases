import { renderHook, waitFor, act } from "@testing-library/react";
import { vi } from "vitest";
import {
  useCaseSearchForm,
  SearchFromData,
  SearchFormField,
} from "./useCaseSearchForm";

describe("useCaseSearchForm", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });
  it("Should initialize the formdata correctly", async () => {
    const initialFormData: SearchFromData = {
      searchType: "urn",
      operationName: "",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "",
    };
    const { result } = renderHook(() => useCaseSearchForm(initialFormData));
    expect(result.current.formData).toEqual(initialFormData);
  });

  it("Should initialize the formdata correctly", async () => {
    const initialFormData: SearchFromData = {
      searchType: "urn",
      operationName: "",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "",
    };
    const { result } = renderHook(() => useCaseSearchForm(initialFormData));
    expect(result.current.formData).toEqual(initialFormData);
  });

  it("Should be able to validate the formdata for urn type search, when urn is empty", async () => {
    const initialFormData: SearchFromData = {
      searchType: "urn",
      operationName: "",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "",
    };
    const { result } = renderHook(() => useCaseSearchForm(initialFormData));

    let isValid = true;
    act(() => {
      isValid = result.current.validateFormData();
    });

    expect(isValid).toEqual(false);
    const expectedFormDataErrors = {
      urn: {
        errorSummaryText: "URN should not be empty",
      },
    };

    const expectedErrorList = [
      {
        children: "URN should not be empty",
        "data-testid": "search-urn-link",
        href: "#search-urn",
        reactListKey: "0",
      },
    ];
    await waitFor(() => {
      expect(result.current.formDataErrors).toEqual(expectedFormDataErrors);
      expect(result.current.errorList).toEqual(expectedErrorList);
    });
  });

  it("Should be able to validate the formdata for urn type search, when urn is invalid", async () => {
    const initialFormData: SearchFromData = {
      searchType: "urn",
      operationName: "",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "www",
    };
    const { result } = renderHook(() => useCaseSearchForm(initialFormData));

    let isValid = true;
    act(() => {
      isValid = result.current.validateFormData();
    });

    expect(isValid).toEqual(false);
    const expectedFormDataErrors = {
      urn: {
        errorSummaryText: "Enter a valid unique reference number",
        inputErrorText:
          "The unique reference number must be 11 characters long and include only letters and numbers",
      },
    };

    const expectedErrorList = [
      {
        children: "Enter a valid unique reference number",
        "data-testid": "search-urn-link",
        href: "#search-urn",
        reactListKey: "0",
      },
    ];
    await waitFor(() => {
      expect(result.current.formDataErrors).toEqual(expectedFormDataErrors);
      expect(result.current.errorList).toEqual(expectedErrorList);
    });
  });

  it("Should be able to validate the formdata for operation name type search, when fields are empty", async () => {
    const initialFormData: SearchFromData = {
      searchType: "operation name",
      operationName: "",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "",
    };
    const { result } = renderHook(() => useCaseSearchForm(initialFormData));

    let isValid = true;
    act(() => {
      isValid = result.current.validateFormData();
    });

    expect(isValid).toEqual(false);
    const expectedFormDataErrors = {
      operationArea: {
        errorSummaryText: "Operation area should not be empty",
      },
      operationName: {
        errorSummaryText: "Operation name should not be empty",
      },
    };
    const expectedErrorList = [
      {
        children: "Operation area should not be empty",
        "data-testid": "search-operation-area-link",
        href: "#search-operation-area",
        reactListKey: "0",
      },
      {
        children: "Operation name should not be empty",
        "data-testid": "search-operation-name-link",
        href: "#search-operation-name",
        reactListKey: "1",
      },
    ];
    await waitFor(() => {
      expect(result.current.formDataErrors).toEqual(expectedFormDataErrors);
      expect(result.current.errorList).toEqual(expectedErrorList);
    });
  });
  it("Should be able to validate the formdata for operation name type search, when operation name crosses max character limit", async () => {
    const initialFormData: SearchFromData = {
      searchType: "operation name",
      operationName: "operation1operation2operation3operation4operation5_",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "",
    };
    const { result } = renderHook(() => useCaseSearchForm(initialFormData));

    let isValid = true;
    act(() => {
      isValid = result.current.validateFormData();
    });

    expect(isValid).toEqual(false);
    const expectedFormDataErrors = {
      operationArea: {
        errorSummaryText: "Operation area should not be empty",
      },
      operationName: {
        errorSummaryText: "Operation name should be less than 50 characters",
      },
    };
    const expectedErrorList = [
      {
        children: "Operation area should not be empty",
        "data-testid": "search-operation-area-link",
        href: "#search-operation-area",
        reactListKey: "0",
      },
      {
        children: "Operation name should be less than 50 characters",
        "data-testid": "search-operation-name-link",
        href: "#search-operation-name",
        reactListKey: "1",
      },
    ];
    await waitFor(() => {
      expect(result.current.formDataErrors).toEqual(expectedFormDataErrors);
      expect(result.current.errorList).toEqual(expectedErrorList);
    });
  });
  it("Should be able to validate the formdata for defendant name type search, when fields are empty", async () => {
    const initialFormData: SearchFromData = {
      searchType: "defendant name",
      operationName: "",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "",
    };
    const { result } = renderHook(() => useCaseSearchForm(initialFormData));

    let isValid = true;
    act(() => {
      isValid = result.current.validateFormData();
    });

    expect(isValid).toEqual(false);
    const expectedFormDataErrors = {
      defendantArea: {
        errorSummaryText: "Defendant area should not be empty",
      },
      defendantName: {
        errorSummaryText: "Defendant last name should not be empty",
      },
    };

    const expectedErrorList = [
      {
        children: "Defendant area should not be empty",
        "data-testid": "search-defendant-area-link",
        href: "#search-defendant-area",
        reactListKey: "0",
      },
      {
        children: "Defendant last name should not be empty",
        "data-testid": "search-defendant-name-link",
        href: "#search-defendant-name",
        reactListKey: "1",
      },
    ];
    await waitFor(() => {
      expect(result.current.formDataErrors).toEqual(expectedFormDataErrors);
      expect(result.current.errorList).toEqual(expectedErrorList);
    });
  });
  it("Should be able to validate the formdata for defendant name type search, when defendant  surname crosses max character limit", async () => {
    const initialFormData: SearchFromData = {
      searchType: "defendant name",
      operationName: "",
      operationArea: "",
      defendantName: "defendant1defendant2defendant3defendant4defendant5_",
      defendantArea: "",
      urn: "",
    };
    const { result } = renderHook(() => useCaseSearchForm(initialFormData));

    let isValid = true;
    act(() => {
      isValid = result.current.validateFormData();
    });

    expect(isValid).toEqual(false);
    const expectedFormDataErrors = {
      defendantArea: {
        errorSummaryText: "Defendant area should not be empty",
      },
      defendantName: {
        errorSummaryText: "Defendant last name should be less than 50 characters",
      },
    };

    const expectedErrorList = [
      {
        children: "Defendant area should not be empty",
        "data-testid": "search-defendant-area-link",
        href: "#search-defendant-area",
        reactListKey: "0",
      },
      {
        children: "Defendant last name should be less than 50 characters",
        "data-testid": "search-defendant-name-link",
        href: "#search-defendant-name",
        reactListKey: "1",
      },
    ];
    await waitFor(() => {
      expect(result.current.formDataErrors).toEqual(expectedFormDataErrors);
      expect(result.current.errorList).toEqual(expectedErrorList);
    });
  });

  it("Should be able to update the formdata", async () => {
    const initialFormData: SearchFromData = {
      searchType: "urn",
      operationName: "",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "",
    };
    const { result } = renderHook(() => useCaseSearchForm(initialFormData));
    expect(result.current.formData).toEqual(initialFormData);

    act(() => {
      result.current.handleFormChange(
        SearchFormField.searchType,
        "operation name",
      );
      result.current.handleFormChange(SearchFormField.operationName, "op1");
      result.current.handleFormChange(
        SearchFormField.operationArea,
        "op area1",
      );
    });

    await waitFor(() => {
      expect(result.current.formData).toEqual({
        searchType: "operation name",
        operationName: "op1",
        operationArea: "op area1",
        defendantName: "",
        defendantArea: "",
        urn: "",
      });
    });
    act(() => {
      result.current.handleFormChange(
        SearchFormField.searchType,
        "defendant name",
      );
      result.current.handleFormChange(
        SearchFormField.defendantName,
        "def name1",
      );
      result.current.handleFormChange(
        SearchFormField.defendantArea,
        "def area1",
      );
    });
    await waitFor(() => {
      expect(result.current.formData).toEqual({
        searchType: "defendant name",
        operationName: "op1",
        operationArea: "op area1",
        defendantName: "def name1",
        defendantArea: "def area1",
        urn: "",
      });
    });
    act(() => {
      result.current.handleFormChange(SearchFormField.searchType, "urn");
      result.current.handleFormChange(SearchFormField.urn, "abc");
    });
    await waitFor(() => {
      expect(result.current.formData).toEqual({
        searchType: "urn",
        operationName: "op1",
        operationArea: "op area1",
        defendantName: "def name1",
        defendantArea: "def area1",
        urn: "abc",
      });
    });
  });

  it("Should be able to get the search params correctly based on the formData", async () => {
    const initialFormData: SearchFromData = {
      searchType: "urn",
      operationName: "",
      operationArea: "",
      defendantName: "",
      defendantArea: "",
      urn: "11AA2222233",
    };
    const { result, rerender } = renderHook(() =>
      useCaseSearchForm(initialFormData),
    );
    const searchParams = result.current.getSearchParams();
    await waitFor(() => {
      expect(searchParams).toEqual({ urn: "11AA2222233" });
    });

    act(() => {
      result.current.handleFormChange(
        SearchFormField.searchType,
        "defendant name",
      );
      result.current.handleFormChange(
        SearchFormField.defendantName,
        "def name1",
      );
      result.current.handleFormChange(
        SearchFormField.defendantArea,
        "def area1",
      );
    });
    rerender();
    await waitFor(() => {
      expect(result.current.getSearchParams()).toEqual({
        area: "def area1",
        "defendant-name": "def name1",
      });
    });
    act(() => {
      result.current.handleFormChange(
        SearchFormField.searchType,
        "operation name",
      );
      result.current.handleFormChange(SearchFormField.operationName, "op1");
      result.current.handleFormChange(
        SearchFormField.operationArea,
        "op area1",
      );
    });
    rerender();
    await waitFor(() => {
      expect(result.current.getSearchParams()).toEqual({
        area: "op area1",
        "operation-name": "op1",
      });
    });
  });
});
