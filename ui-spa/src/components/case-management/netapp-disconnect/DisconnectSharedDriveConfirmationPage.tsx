import { useRef, useEffect, useState, useCallback, useMemo } from "react";
import { Radios, Button, ErrorSummary } from "../../govuk";
import { useNavigate, useLocation, Link } from "react-router-dom";
import { disconnectNetAppFolder } from "../../../apis/gateway-api";
import styles from "./DisconnectSharedDriveConfirmationPage.module.scss";

type GeneralRadioValue = "yes" | "no" | "";

const DisconnectSharedDriveConfirmationPage = () => {
  const navigate = useNavigate();
  const {
    state: { caseId, urn },
  }: {
    state: {
      caseId: number;
      urn: string;
    };
  } = useLocation();
  type ErrorText = {
    errorSummaryText: string;
    inputErrorText?: string;
  };
  type FormDataErrors = {
    disconnectSharedDriveRadio?: ErrorText;
  };
  const errorSummaryRef = useRef<HTMLInputElement>(null);

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
        href: "#disconnect-shared-drive-yes",
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

    if (!validateFormData()) return;

    const response = await disconnectNetAppFolder(caseId);
    if (!response.ok)
      navigate(
        `/case/${caseId}/case-management/disconnect-shared-drive-failure`,
        { state: { caseId, urn } },
      );

    navigate(
      `/case/${caseId}/case-management/disconnect-shared-drive-success`,
      { state: { urn } },
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
                id: "disconnect-shared-drive-yes",
                children: "Yes, cancel registration and delete the information",
                value: "yes",
                "data-testid": "disconnect-shared-drive-yes",
              },
              {
                id: "disconnect-shared-drive-no",
                children: "No, go back and continue registration",
                value: "no",
                "data-testid": "disconnect-shared-drive-no",
              },
            ]}
            value={formData.disconnectSharedDriveRadio}
            onChange={(value) => {
              if (value) setFormValue(value);
            }}
          ></Radios>
        </div>
        <div className={styles.buttonWrapper}>
          <Button type="submit" onClick={() => handleSubmit}>
            Continue
          </Button>
          <Link to={`/case/${caseId}/case-management`}>cancel</Link>
        </div>
      </form>
    </div>
  );
};

export default DisconnectSharedDriveConfirmationPage;
