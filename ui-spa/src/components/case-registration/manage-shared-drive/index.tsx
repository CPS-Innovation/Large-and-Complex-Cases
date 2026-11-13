import { useState, useCallback, useContext } from "react";
import { Radios, Button, ErrorSummary } from "../../govuk";
import { useNavigate } from "react-router";
import { useErrorSummary } from "../../../common/hooks/useErrorSummary";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import { MainStateContext } from "../../../providers/MainStateProvider";
import { getUrlSearchParam } from "../../../common/utils/getUrlSearchParam";
import styles from "./index.module.scss";

type ManageSharedDriveRadioValue =
  | "create-new"
  | "connect-existing"
  | "skip-shared-drive"
  | "";

const ManageSharedDrivePage = () => {
  type ErrorText = {
    errorSummaryText: string;
    inputErrorText?: string;
  };
  type FormDataErrors = {
    manageSharedDriveRadio?: ErrorText;
  };
  const { state } = useContext(MainStateContext);
  const [disableButtons] = useState(false);
  const navigate = useNavigate();

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

  const { errorList, errorSummaryRef } = useErrorSummary({
    formDataErrors,
    errorSummaryProperties,
  });

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

  const setFormValue = (value: string) => {
    setFormData({
      ...formData,
      manageSharedDriveRadio: value as ManageSharedDriveRadioValue,
    });
  };

  const handleSubmit = async (event: React.SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!validateFormData()) return;
    const operationName =
      state.appData.caseRegistrationCaseDetails?.operationName ||
      state.appData.caseRegistrationCaseDetails?.leadDefendantName;

    if (formData.manageSharedDriveRadio === "skip-shared-drive") {
      void navigate("/case-registration/case-registration-final-confirmation");
      return;
    }

    if (formData.manageSharedDriveRadio === "connect-existing") {
      void navigate(
        `/case-registration/shared-drive-connect?${getUrlSearchParam("operation-name", operationName)}`,
      );
      return;
    }
    if (formData.manageSharedDriveRadio === "create-new") {
      void navigate(
        `/case-registration/shared-drive-create?${getUrlSearchParam("operation-name", operationName)}`,
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
