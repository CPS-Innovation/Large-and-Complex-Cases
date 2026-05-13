import { useRef, useEffect, useState, useCallback, useMemo } from "react";
import { Radios, Button, ErrorSummary } from "../../govuk";
import { useNavigate, useLocation, Link } from "react-router-dom";
import { disconnectNetAppFolder } from "../../../apis/gateway-api";
import styles from "./DisconnectSharedDriveConfirmationPage.module.scss";

type GeneralRadioValue = "yes" | "no" | "";

const DisconnectSharedDriveConfirmationPage = () => {
  const navigate = useNavigate();
  const {
    state,
  }: {
    state?: {
      caseId: number;
      urn: string;
      isRouteValid: boolean;
    };
  } = useLocation();

  const { caseId, urn, isRouteValid } = state || {};
  type ErrorText = {
    errorSummaryText: string;
    inputErrorText?: string;
  };
  type FormDataErrors = {
    disconnectSharedDriveRadio?: ErrorText;
  };
  const errorSummaryRef = useRef<HTMLInputElement>(null);

  const [disableButtons, setDisableButtons] = useState(false);

  useEffect(() => {
    if (!isRouteValid) {
      navigate(`/`);
    }
  }, []);

  const [formData, setFormData] = useState<{
    disconnectSharedDriveRadio?: GeneralRadioValue;
  }>({
    disconnectSharedDriveRadio: "",
  });

  const [formDataErrors, setFormDataErrors] = useState<FormDataErrors>({});

  const errorSummaryProperties = useCallback(
    (errorKey: keyof FormDataErrors) => {
      return {
        children: formDataErrors[errorKey]?.errorSummaryText,
        href: "#disconnect-shared-drive-radio-yes",
        "data-testid": "disconnect-shared-drive-radio-link",
      };
    },
    [formDataErrors],
  );

  const validateFormData = () => {
    const errors: FormDataErrors = {};
    const { disconnectSharedDriveRadio = "" } = formData;

    if (!disconnectSharedDriveRadio) {
      errors.disconnectSharedDriveRadio = {
        errorSummaryText:
          "Select whether you want to disconnect Shared Drive folder",
        inputErrorText:
          "Select whether you want to disconnect Shared Drive folder",
      };
    }

    const isValid = !Object.entries(errors).filter(([, value]) => value).length;

    setFormDataErrors(errors);
    return isValid;
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
      disconnectSharedDriveRadio: value as GeneralRadioValue,
    });
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!caseId || !urn) return;

    if (!validateFormData()) return;

    if (formData.disconnectSharedDriveRadio === "no") {
      navigate(`/case/${caseId}/case-management`);
      return;
    }
    setDisableButtons(true);
    const response = await disconnectNetAppFolder(caseId);
    setDisableButtons(false);
    if (!response.ok) {
      navigate(
        `/case/${caseId}/case-management/disconnect-shared-drive-failure`,
        { state: { caseId, urn, isRouteValid: true } },
      );
      return;
    }

    navigate(
      `/case/${caseId}/case-management/disconnect-shared-drive-success`,
      { state: { urn, isRouteValid: true } },
    );
  };

  return (
    <div className={styles.contentWrapper}>
      {!!errorList.length && (
        <div
          ref={errorSummaryRef}
          tabIndex={-1}
          className={styles.errorSummaryWrapper}
        >
          <ErrorSummary
            data-testid={"disconnect-shared-drive-error-summary"}
            errorList={errorList}
            titleChildren="There is a problem"
          />
        </div>
      )}
      <form onSubmit={handleSubmit}>
        <div className={styles.inputWrapper}>
          <Radios
            name="disconnectSharedDriveConfirmationRadio"
            fieldset={{
              legend: {
                children: <h1>Disconnect this Shared Drive folder?</h1>,
              },
            }}
            errorMessage={
              formDataErrors["disconnectSharedDriveRadio"]
                ? {
                    children:
                      formDataErrors["disconnectSharedDriveRadio"]
                        .inputErrorText,
                  }
                : undefined
            }
            items={[
              {
                id: "disconnect-shared-drive-radio-yes",
                children: "Yes, disconnect this folder",
                value: "yes",
                "data-testid": "disconnect-shared-drive-radio-yes",
              },
              {
                id: "disconnect-shared-drive-radio-no",
                children: "No, keep this folder connected",
                value: "no",
                "data-testid": "disconnect-shared-drive-radio-no",
              },
            ]}
            value={formData.disconnectSharedDriveRadio}
            onChange={(value) => {
              if (value) setFormValue(value);
            }}
          ></Radios>
        </div>
        <div className={styles.buttonWrapper}>
          <Button type="submit" disabled={disableButtons}>
            Continue
          </Button>
          {!disableButtons && (
            <Link to={`/case/${caseId}/case-management`}>cancel</Link>
          )}
        </div>
      </form>
    </div>
  );
};

export default DisconnectSharedDriveConfirmationPage;
