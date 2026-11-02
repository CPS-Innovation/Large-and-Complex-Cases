import { useMemo, useEffect, useRef } from "react";

type ErrorText = {
  errorSummaryText: string;
  inputErrorText?: string;
};

type FormDataErrors = Record<string, ErrorText | undefined>;

type UseErrorSummaryProps<T extends FormDataErrors> = {
  formDataErrors: T;
  errorSummaryProperties: (errorKey: keyof T) => {
    children: string | undefined;
    href: string;
    "data-testid": string;
  };
};

export const useErrorSummary = <T extends FormDataErrors>({
  formDataErrors,
  errorSummaryProperties,
}: UseErrorSummaryProps<T>) => {
  const errorSummaryRef = useRef<HTMLInputElement>(null);

  const errorList = useMemo(() => {
    const validErrorKeys = Object.keys(formDataErrors).filter(
      (errorKey) => formDataErrors[errorKey as keyof T],
    );

    const errorSummary = validErrorKeys.map((errorKey, index) => ({
      reactListKey: `${index}`,
      ...errorSummaryProperties(errorKey as keyof T),
    }));

    return errorSummary;
  }, [formDataErrors, errorSummaryProperties]);

  useEffect(() => {
    if (errorList.length) errorSummaryRef.current?.focus();
  }, [errorList]);

  return { errorList, errorSummaryRef };
};
