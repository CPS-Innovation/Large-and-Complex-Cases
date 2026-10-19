import { useRef, useEffect, useState, useCallback, useMemo } from "react";
import { Radios, Button, ErrorSummary } from "../../../govuk";
import { useNavigate, useLocation } from "react-router";
import { useMutation } from "@tanstack/react-query";
import { ApiError } from "../../../../common/errors/ApiError";
import { useGetCaseDivisionsOrAreas } from "../../../../common/hooks/useGetCaseDivisionsOrAreas";
import { createEgressCase } from "../../../../apis/gateway-api";
import { Spinner } from "../../../common/Spinner";
import { PageContentWrapper } from "../../../govuk/PageContentWrapper";
import styles from "./index.module.scss";
import commonStyles from "../../../case-management/index.module.scss";

type ManageEgressRadioValue =
  | "create-new"
  | "connect-existing"
  | "skip-egress"
  | "";

const ManageEgressPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { caseId, templateId, description } = useMemo(() => {
    const params = new URLSearchParams(location.search);
    return {
      caseId: params.get("caseId") ?? "",
      templateId: params.get("templateId") ?? "",
      description: params.get("description") ?? "",
    };
  }, [location.search]);

  useEffect(() => {
    if (!caseId || !templateId || !description) {
      void navigate(`/`);
    }
  }, [caseId, templateId, description, navigate]);
  const createEgressCaseMutation = useMutation({
    mutationFn: createEgressCase,
    throwOnError: (error: ApiError) => {
      return error.code !== 403;
    },
  });
  const { isLoading: isDivisionsOrAreasLoading } = useGetCaseDivisionsOrAreas();
  type ErrorText = {
    errorSummaryText: string;
    inputErrorText?: string;
  };
  type FormDataErrors = {
    manageEgressRadio?: ErrorText;
  };
  const errorSummaryRef = useRef<HTMLInputElement>(null);

  const [disableButtons, setDisableButtons] = useState(false);

  const [formData, setFormData] = useState<{
    manageEgressRadio: ManageEgressRadioValue;
  }>({
    manageEgressRadio: "",
  });

  const [formDataErrors, setFormDataErrors] = useState<FormDataErrors>({});

  const errorSummaryProperties = useCallback(
    (errorKey: keyof FormDataErrors) => {
      return {
        children: formDataErrors[errorKey]?.errorSummaryText,
        href: "#manage-egress-radio-create-new",
        "data-testid": "manage-egress-radio-link",
      };
    },
    [formDataErrors],
  );

  const validateFormData = () => {
    const errors: FormDataErrors = {};
    const { manageEgressRadio = "" } = formData;

    if (!manageEgressRadio) {
      errors.manageEgressRadio = {
        errorSummaryText: "Select how you want to manage Egress",
        inputErrorText: "Select how you want to manage Egress",
      };
      setFormDataErrors(errors);
      return false;
    }
    return true;
  };

  const errorList = useMemo(() => {
    const validErrorKeys = Object.keys(formDataErrors).filter(
      (errorKey) => formDataErrors[errorKey as keyof FormDataErrors],
    );

    const errorSummary = validErrorKeys.map((errorKey, index) => ({
      reactListKey: `${index}`,
      ...errorSummaryProperties(errorKey as keyof FormDataErrors),
    }));

    return errorSummary;
  }, [formDataErrors, errorSummaryProperties]);

  useEffect(() => {
    if (errorList.length) errorSummaryRef.current?.focus();
  }, [errorList]);

  const setFormValue = (value: string) => {
    setFormData({
      ...formData,
      manageEgressRadio: value as ManageEgressRadioValue,
    });
  };

  const handleSubmit = async (event: React.SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!validateFormData()) return;

    setDisableButtons(true);
    if (formData.manageEgressRadio === "skip-egress") {
      void navigate(`/manage-set-up-shared-drive`);
      return;
    }
    console.log("hiiiiii000>>>", formData.manageEgressRadio);
    if (formData.manageEgressRadio === "connect-existing") {
      console.log("hiiiiii");
      void navigate(
        `/case/${caseId}/egress-connect?workspace-name=${description}`,
      );
      return;
    }

    await createEgressCaseMutation.mutateAsync({
      templateId: templateId,
      caseId: Number.parseInt(caseId),
      description,
    });

    void navigate("/egress-confirmation");

    setDisableButtons(false);
  };

  if (isDivisionsOrAreasLoading) {
    return (
      <PageContentWrapper>
        <div className={commonStyles.pageLoadingSpinnerWrapper}>
          <Spinner data-testid="page-loading-spinner" diameterPx={50} />
          <div aria-live="polite">Loading...</div>
        </div>
      </PageContentWrapper>
    );
  }

  return (
    <div className={styles.contentWrapper}>
      {!!errorList.length && (
        <div
          ref={errorSummaryRef}
          tabIndex={-1}
          className={styles.errorSummaryWrapper}
        >
          <ErrorSummary
            data-testid={"manage-egress-error-summary"}
            errorList={errorList}
            titleChildren="There is a problem"
          />
        </div>
      )}
      <form onSubmit={handleSubmit}>
        <div className={styles.inputWrapper}>
          <Radios
            name="manageEgressRadio"
            fieldset={{
              legend: {
                children: <h1>How do you want to manage Egress?</h1>,
              },
            }}
            errorMessage={
              formDataErrors["manageEgressRadio"]
                ? {
                    children:
                      formDataErrors["manageEgressRadio"].inputErrorText,
                  }
                : undefined
            }
            items={[
              {
                id: "manage-egress-radio-create-new",
                children: "Create a new case on Egress",
                value: "create-new",
                "data-testid": "manage-egress-radio-create-new",
              },
              {
                id: "manage-egress-radio-connect-existing",
                children: "Connect an existing Egress case",
                value: "connect-existing",
                "data-testid": "manage-egress-radio-connect-existing",
              },
              {
                id: "manage-egress-radio-skip",
                children: "Egress is not needed now",
                value: "skip",
                "data-testid": "manage-egress-radio-skip",
              },
            ]}
            value={formData.manageEgressRadio}
            onChange={(value) => {
              if (value) setFormValue(value);
            }}
          ></Radios>
        </div>
        <div className={styles.buttonWrapper}>
          <Button type="submit" disabled={disableButtons}>
            Continue
          </Button>
        </div>
      </form>
    </div>
  );
};

export default ManageEgressPage;
