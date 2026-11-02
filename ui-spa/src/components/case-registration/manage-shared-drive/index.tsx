import { useRef, useEffect, useState, useCallback, useMemo } from "react";
import { Radios, Button, ErrorSummary } from "../../govuk";
import { useNavigate } from "react-router";
import { getUrlSearchParam } from "../../../common/utils/getUrlSearchParam";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import styles from "./index.module.scss";

type ManageSharedDriveRadioValue =
  | "create-new"
  | "connect-existing"
  | "skip-shared-drive"
  | "";

const ManageSharedDrivePage = () => {
  const navigate = useNavigate();

  type ErrorText = {
    errorSummaryText: string;
    inputErrorText?: string;
  };
  type FormDataErrors = {
    manageSharedDriveRadio?: ErrorText;
  };
  const errorSummaryRef = useRef<HTMLInputElement>(null);

  const [disableButtons, setDisableButtons] = useState(false);

  const [formData, setFormData] = useState<{
    manageSharedDriveRadio: ManageSharedDriveRadioValue;
  }>({
    manageSharedDriveRadio: "",
  });

  const [formDataErrors, setFormDataErrors] = useState<FormDataErrors>({});

  const errorSummaryProperties = useCallback(
    (errorKey: keyof FormDataErrors) => {
      return {
        children: formDataErrors[errorKey]?.errorSummaryText,
        href: "#manage-shared-drive-radio-create-new",
        "data-testid": "manage-shared-drive-radio-link",
      };
    },
    [formDataErrors],
  );

  const validateFormData = () => {
    const errors: FormDataErrors = {};
    const { manageSharedDriveRadio = "" } = formData;

    if (!manageSharedDriveRadio) {
      errors.manageSharedDriveRadio = {
        errorSummaryText: "Select how you want to manage the Shared Drive",
        inputErrorText: "Select how you want to manage the Shared Drive",
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
      manageSharedDriveRadio: value as ManageSharedDriveRadioValue,
    });
  };

  const handleSubmit = async (event: React.SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!validateFormData()) return;

    setDisableButtons(true);
    if (formData.manageSharedDriveRadio === "skip-shared-drive") {
      // Navigate the user to the case registration confirmation page
      return;
    }

    if (formData.manageSharedDriveRadio === "connect-existing") {
      void navigate(
        `/case-registration/egress-connect-search-results?${getUrlSearchParam("workspace-name", "description")}`,
      );
    }
  };

  return (
    <PageContentWrapper>
      <div className={styles.contentWrapper}>
        {!!errorList.length && (
          <div
            ref={errorSummaryRef}
            tabIndex={-1}
            className={styles.errorSummaryWrapper}
          >
            <ErrorSummary
              data-testid={"manage-shared-drive-error-summary"}
              errorList={errorList}
              titleChildren="There is a problem"
            />
          </div>
        )}
        <form onSubmit={handleSubmit}>
          <div className={styles.inputWrapper}>
            <Radios
              name="manageSharedDriveRadio"
              fieldset={{
                legend: {
                  children: (
                    <h1>How do you want to manage the Shared Drive?</h1>
                  ),
                },
              }}
              errorMessage={
                formDataErrors["manageSharedDriveRadio"]
                  ? {
                      children:
                        formDataErrors["manageSharedDriveRadio"].inputErrorText,
                    }
                  : undefined
              }
              items={[
                {
                  id: "manage-shared-drive-radio-create-new",
                  children: "Create new Shared Drive folders",
                  value: "create-new",
                  "data-testid": "manage-shared-drive-radio-create-new",
                  disabled: disableButtons,
                },
                {
                  id: "manage-shared-drive-radio-connect-existing",
                  children:
                    "Connect existing Shared Drive folders to this case",
                  value: "connect-existing",
                  "data-testid": "manage-shared-drive-radio-connect-existing",
                  disabled: disableButtons,
                },
                {
                  id: "manage-shared-drive-radio-skip",
                  children: "Shared Drive folders not needed",
                  value: "skip-shared-drive",
                  "data-testid": "manage-shared-drive-radio-skip",
                  disabled: disableButtons,
                },
              ]}
              value={formData.manageSharedDriveRadio}
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
    </PageContentWrapper>
  );
};

export default ManageSharedDrivePage;
