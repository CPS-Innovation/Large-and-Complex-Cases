import { useRef, useEffect, useState, useCallback, useMemo } from "react";
import { Radios, Button, ErrorSummary } from "../../govuk";
import { useNavigate, useParams, useLocation } from "react-router";
import {
  disconnectNetAppFolder,
  disconnectEgressFolder,
} from "../../../apis/gateway-api";
import styles from "./DisconnectConfirmationPage.module.scss";

type GeneralRadioValue = "yes" | "no" | "";

const DisconnectConfirmationPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { queryType } = useMemo(() => {
    const params = new URLSearchParams(location.search);
    return {
      queryType: params.get("type") ?? "",
    };
  }, [location.search]);

  const { caseId } = useParams() as { caseId: string };

  type ErrorText = {
    errorSummaryText: string;
    inputErrorText?: string;
  };
  type FormDataErrors = {
    disconnectRadio?: ErrorText;
  };
  const errorSummaryRef = useRef<HTMLInputElement>(null);

  const [disableButtons, setDisableButtons] = useState(false);

  const [formData, setFormData] = useState<{
    disconnectRadio?: GeneralRadioValue;
  }>({
    disconnectRadio: "",
  });

  const [formDataErrors, setFormDataErrors] = useState<FormDataErrors>({});

  const errorSummaryProperties = useCallback(
    (errorKey: keyof FormDataErrors) => {
      return {
        children: formDataErrors[errorKey]?.errorSummaryText,
        href: "#disconnect-radio-yes",
        "data-testid": "disconnect-radio-link",
      };
    },
    [formDataErrors],
  );

  const validateFormData = () => {
    const errors: FormDataErrors = {};
    const { disconnectRadio = "" } = formData;

    if (!disconnectRadio) {
      errors.disconnectRadio = {
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
      disconnectRadio: value as GeneralRadioValue,
    });
  };

  const handleSubmit = async (event: React.SubmitEvent) => {
    event.preventDefault();

    if (!caseId) return;

    if (!validateFormData()) return;

    if (formData.disconnectRadio === "no") {
      void navigate(`/case/${caseId}/case-management`);
      return;
    }
    setDisableButtons(true);
    try {
      const response =
        queryType === "shared-drive"
          ? await disconnectNetAppFolder(Number.parseInt(caseId))
          : await disconnectEgressFolder(Number.parseInt(caseId));
      if (!response.ok) {
        void navigate(
          `/case/${caseId}/case-management/disconnect-failure?type=${queryType}`,
        );
        return;
      }
    } catch (e) {
      console.error(e);
      void navigate(
        `/case/${caseId}/case-management/disconnect-failure?type=${queryType}`,
      );
      return;
    } finally {
      setDisableButtons(false);
    }

    void navigate(
      `/case/${caseId}/case-management/disconnect-success?type=${queryType}`,
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
            data-testid={"disconnect-error-summary"}
            errorList={errorList}
            titleChildren="There is a problem"
          />
        </div>
      )}
      <form onSubmit={handleSubmit}>
        <div className={styles.inputWrapper}>
          <Radios
            name="disconnectConfirmationRadio"
            fieldset={{
              legend: {
                children: (
                  <h1>
                    {queryType === "shared-drive"
                      ? "Disconnect this Shared Drive folder?"
                      : "Disconnect Egress folder?"}
                  </h1>
                ),
              },
            }}
            errorMessage={
              formDataErrors["disconnectRadio"]
                ? {
                    children: formDataErrors["disconnectRadio"].inputErrorText,
                  }
                : undefined
            }
            items={[
              {
                id: "disconnect-radio-yes",
                children:
                  queryType === "shared-drive"
                    ? "Yes, disconnect this folder"
                    : "Yes, disconnect an Egress case",
                value: "yes",
                "data-testid": "disconnect-radio-yes",
              },
              {
                id: "disconnect-radio-no",
                children:
                  queryType === "shared-drive"
                    ? "No, keep this folder connected"
                    : "No, keep Egress connected",
                value: "no",
                "data-testid": "disconnect-radio-no",
              },
            ]}
            value={formData.disconnectRadio}
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

export default DisconnectConfirmationPage;
