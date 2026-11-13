import {
  useState,
  useCallback,
  useMemo,
  useContext,
  useEffect,
  useRef,
} from "react";
import { Radios, Button, ErrorSummary, BackLink } from "../../govuk";
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
import { getUrlSearchParam } from "../../../common/utils/getUrlSearchParam";
import styles from "./index.module.scss";
import commonStyles from "../../case-management/index.module.scss";

const SharedDriveTemplatePage = () => {
  const navigate = useNavigate();
  const hasAutoSubmittedRef = useRef(false);
  const hasNavigatedToErrorRef = useRef(false);

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
    onSuccess: () => {
      void navigate("/case-registration/case-registration-final-confirmation");
      setDisableButtons(false);
    },
  });

  const selectedFolderPath = useMemo(() => {
    return state.appData.selectedCreateSharedDriveFolderPath || "";
  }, [state.appData.selectedCreateSharedDriveFolderPath]);

  const {
    data: templatesData,
    isLoading: isTemplatesLoading,
    isError: isTemplatesError,
  } = useQuery({
    queryKey: ["get-templates", selectedFolderPath],
    queryFn: () => getSharedDriveTemplates(selectedFolderPath),
    retry: false,
    enabled: !!selectedFolderPath,
    throwOnError: false,
    staleTime: 0,
    gcTime: 0,
  });

  useEffect(() => {
    if (hasNavigatedToErrorRef.current || isTemplatesLoading) return;

    if (isTemplatesError || !templatesData?.templates.length) {
      hasNavigatedToErrorRef.current = true;
      dispatch({
        type: "SET_SHARED_DRIVE_CREATE_CONNECT_FAILURE_PAGE",
        payload: {
          backLinkUrl: `/case-registration/manage-set-up-shared-drive`,
        },
      });
      void navigate(`/case-registration/create-shared-drive-folder-error`);
    }
  }, [
    isTemplatesError,
    templatesData?.templates.length,
    isTemplatesLoading,
    dispatch,
    navigate,
  ]);

  type ErrorText = {
    errorSummaryText: string;
    inputErrorText?: string;
  };
  type FormDataErrors = {
    sharedDriveTemplatesRadio?: ErrorText;
  };

  const [disableButtons, setDisableButtons] = useState(false);

  const handleCreateSharedDriveFolder = useCallback<
    (
      selectedTemplate:
        | {
            name: string;
            path: string;
          }
        | undefined,
      caseId: number | undefined,
      templateParentFolderPath: string | undefined,
    ) => Promise<void>
  >(
    async (selectedTemplate, caseId, templateParentFolderPath) => {
      if (!selectedTemplate || !caseId || !templateParentFolderPath) {
        setDisableButtons(false);
        dispatch({
          type: "SET_SHARED_DRIVE_CREATE_CONNECT_FAILURE_PAGE",
          payload: {
            backLinkUrl: `/case-registration/manage-set-up-shared-drive`,
          },
        });
        void navigate(`/case-registration/create-shared-drive-folder-error`);
        return;
      }

      setDisableButtons(true);
      createSharedDriveFolderMutation.mutate({
        caseId,
        payload: {
          templateFolderPath: selectedTemplate.path,
          parentFolderPath: templateParentFolderPath,
        },
      });
    },
    [createSharedDriveFolderMutation, dispatch, navigate],
  );
  useEffect(() => {
    if (hasAutoSubmittedRef.current) return;
    if (
      !templatesData?.templates.length ||
      templatesData.templates.length !== 1
    )
      return;

    const selectedTemplate = templatesData.templates[0];
    const caseId = state.appData.caseRegistrationCaseDetails?.caseId;
    const templateParentFolderPath = templatesData.parentFolderPath;
    hasAutoSubmittedRef.current = true;

    void handleCreateSharedDriveFolder(
      selectedTemplate,
      caseId,
      templateParentFolderPath,
    );
  }, [
    handleCreateSharedDriveFolder,
    templatesData,
    createSharedDriveFolderMutation,
    disableButtons,
    state.appData.caseRegistrationCaseDetails?.caseId,
  ]);

  const [formData, setFormData] = useState<{
    sharedDriveTemplatesRadio: string;
  }>({
    sharedDriveTemplatesRadio: "",
  });

  const [formDataErrors, setFormDataErrors] = useState<FormDataErrors>({});

  const errorSummaryProperties = useCallback(
    (errorKey: keyof FormDataErrors) => {
      return {
        children: formDataErrors[errorKey]?.errorSummaryText,
        href: `#shared-drive-template-radio-template-0`,
        "data-testid": "shared-drive-template-radio-link",
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
    const { sharedDriveTemplatesRadio = "" } = formData;

    if (!sharedDriveTemplatesRadio) {
      errors.sharedDriveTemplatesRadio = {
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
      sharedDriveTemplatesRadio: value,
    });
  };

  const handleSubmit = async (event: React.SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!validateFormData()) return;

    const selectedTemplate = templatesData?.templates.find(
      (template) => template.path === formData.sharedDriveTemplatesRadio,
    );

    const caseId = state.appData.caseRegistrationCaseDetails?.caseId;
    const templateParentFolderPath = templatesData?.parentFolderPath;

    await handleCreateSharedDriveFolder(
      selectedTemplate,
      caseId,
      templateParentFolderPath,
    );
  };

  if (isTemplatesLoading || templatesData?.templates.length === 1) {
    return (
      <PageContentWrapper>
        <div className={commonStyles.pageLoadingSpinnerWrapper}>
          <Spinner data-testid="page-loading-spinner" diameterPx={50} />
          <div aria-live="polite">Loading...</div>
        </div>
      </PageContentWrapper>
    );
  }

  const operationName =
    state.appData.caseRegistrationCaseDetails?.operationName ||
    state.appData.caseRegistrationCaseDetails?.leadDefendantName;
  const backLinkUrl = `/case-registration/shared-drive-create?${getUrlSearchParam("operation-name", operationName)}`;

  return (
    <PageContentWrapper>
      <div className={styles.contentWrapper}>
        <BackLink to={backLinkUrl}>Back</BackLink>
        {!!errorList.length && (
          <div
            ref={errorSummaryRef}
            tabIndex={-1}
            className={styles.errorSummaryWrapper}
          >
            <ErrorSummary
              data-testid={"shared-drive-templates-error-summary"}
              errorList={errorList}
              titleChildren="There is a problem"
            />
          </div>
        )}
        <form onSubmit={handleSubmit}>
          <div className={styles.inputWrapper}>
            <Radios
              name="sharedDriveTemplatesRadio"
              fieldset={{
                legend: {
                  children: <h1>Select a Shared Drive folder template</h1>,
                },
              }}
              errorMessage={
                formDataErrors["sharedDriveTemplatesRadio"]
                  ? {
                      children:
                        formDataErrors["sharedDriveTemplatesRadio"]
                          .inputErrorText,
                    }
                  : undefined
              }
              items={
                templatesData?.templates.map((template, index) => ({
                  id: `shared-drive-template-radio-template-${index}`,
                  children: template.name,
                  value: template.path,
                  "data-testid": `shared-drive-template-radio-template-${index}`,
                  disabled: disableButtons,
                })) ?? []
              }
              value={formData.sharedDriveTemplatesRadio}
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
