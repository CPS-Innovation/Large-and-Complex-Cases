import { useState, useCallback, useMemo, useContext } from "react";
import { Radios, Button, ErrorSummary } from "../../govuk";
import { useNavigate } from "react-router";
import { useMutation, useQuery } from "@tanstack/react-query";
import {
  createSharedDriveFolder,
  getSharedDriveTemplates,
} from "../../../apis/gateway-api";
import { Spinner } from "../../common/Spinner";
import { useErrorSummary } from "../../../common/hooks/useErrorSummary";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import { MainStateContext } from "../../../providers/MainStateProvider";
import styles from "./index.module.scss";
import commonStyles from "../../case-management/index.module.scss";

const SharedDriveTemplatePage = () => {
  const navigate = useNavigate();

  const { state, dispatch } = useContext(MainStateContext);

  const createSharedDriveFolderMutation = useMutation({
    mutationFn: (params: {
      caseId: number;
      payload: { templateFolderPath: string; parentFolderPath: string };
    }) => createSharedDriveFolder(params.caseId, params.payload),
    onError: () => {
      setDisableButtons(false);
      dispatch({
        type: "SET_SHARED_DRIVE_CREATE_CONNECT_FAILURE_PAGE",
        payload: {
          backLinkUrl: `/case-registration/manage-set-up-shared-drive`,
        },
      });
      void navigate(`/case-registration/create-shared-drive-folder-error`);
    },
  });

  const selectedFolderPath = useMemo(() => {
    return state.appData.selectedCreateSharedDriveFolderPath || "";
  }, [state.appData.selectedCreateSharedDriveFolderPath]);

  const { data: templatesData, isLoading: isTemplatesLoading } = useQuery({
    queryKey: ["get-templates", selectedFolderPath],
    queryFn: () => getSharedDriveTemplates(selectedFolderPath),
    retry: false,
    enabled: true,
    throwOnError: true,
    staleTime: 0,
    gcTime: 0,
    select: (data) => {
      if (!data || data.templates.length === 0) {
        //handle this nicely or throw Error
        throw new Error("No templates found for");
      }

      return data;
    },
  });
  type ErrorText = {
    errorSummaryText: string;
    inputErrorText?: string;
  };
  type FormDataErrors = {
    manageTemplatesRadio?: ErrorText;
  };

  const [disableButtons, setDisableButtons] = useState(false);

  const [formData, setFormData] = useState<{
    manageTemplatesRadio: string;
  }>({
    manageTemplatesRadio: "",
  });

  const [formDataErrors, setFormDataErrors] = useState<FormDataErrors>({});

  const errorSummaryProperties = useCallback(
    (errorKey: keyof FormDataErrors) => {
      return {
        children: formDataErrors[errorKey]?.errorSummaryText,
        href: `#manage-templates-radio-template-0`,
        "data-testid": "manage-templates-radio-link",
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
    const { manageTemplatesRadio = "" } = formData;

    if (!manageTemplatesRadio) {
      errors.manageTemplatesRadio = {
        errorSummaryText: "Select which template you want to use",
        inputErrorText: "Select which template you want to use",
      };
      setFormDataErrors(errors);
      return false;
    }
    return true;
  };

  const setFormValue = (value: string) => {
    setFormData({
      ...formData,
      manageTemplatesRadio: value,
    });
  };

  const handleSubmit = async (event: React.SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!validateFormData()) return;

    const caseId = state.appData.caseRegistrationCaseDetails?.caseId;

    const selectedTemplate = templatesData?.templates.find(
      (template) => template.path === formData.manageTemplatesRadio,
    );
    const templateParentFolderPath = templatesData?.parentFolderPath;
    if (selectedTemplate && caseId && templateParentFolderPath) {
      setDisableButtons(true);
      await createSharedDriveFolderMutation.mutateAsync({
        caseId,
        payload: {
          templateFolderPath: selectedTemplate.path,
          parentFolderPath: templateParentFolderPath,
        },
      });
    }
    void navigate("/case-registration/case-registration-final-confirmation");
    setDisableButtons(false);
  };

  if (isTemplatesLoading) {
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
    <PageContentWrapper>
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
              name="manageTemplatesRadio"
              fieldset={{
                legend: {
                  children: <h1>How do you want to manage SharedDrive?</h1>,
                },
              }}
              errorMessage={
                formDataErrors["manageTemplatesRadio"]
                  ? {
                      children:
                        formDataErrors["manageTemplatesRadio"].inputErrorText,
                    }
                  : undefined
              }
              items={
                templatesData?.templates.map((template, index) => ({
                  id: `manage-templates-radio-template-${index}`,
                  children: template.name,
                  value: template.path,
                  "data-testid": `manage-templates-radio-template-${index}`,
                })) ?? []
              }
              value={formData.manageTemplatesRadio}
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

export default SharedDriveTemplatePage;
