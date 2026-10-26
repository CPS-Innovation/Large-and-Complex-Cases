import {
  useRef,
  useEffect,
  useState,
  useCallback,
  useMemo,
  useContext,
} from "react";
import { Radios, Button, ErrorSummary } from "../../govuk";
import { useNavigate, useLocation } from "react-router";
import { useMutation, useQuery } from "@tanstack/react-query";
import { ApiError } from "../../../common/errors/ApiError";
import { useGetCaseDivisionsOrAreas } from "../../../common/hooks/useGetCaseDivisionsOrAreas";
import {
  createEgressCase,
  getCaseSearchResults,
} from "../../../apis/gateway-api";
import { Spinner } from "../../common/Spinner";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import { MainStateContext } from "../../../providers/MainStateProvider";
import styles from "./index.module.scss";
import commonStyles from "../../case-management/index.module.scss";

type ManageEgressRadioValue =
  | "create-new"
  | "connect-existing"
  | "skip-egress"
  | "";

const ManageEgressPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { urn } = useMemo(() => {
    const params = new URLSearchParams(location.search);
    return {
      urn: params.get("urn") ?? "",
    };
  }, [location.search]);
  const { state } = useContext(MainStateContext);

  useEffect(() => {
    if (!urn) {
      void navigate(`/`);
    }
  }, [urn, navigate]);
  const createEgressCaseMutation = useMutation({
    mutationFn: createEgressCase,
    throwOnError: (error: ApiError) => {
      return error.code !== 403;
    },
  });

  const { isLoading: isDivisionsOrAreasLoading } = useGetCaseDivisionsOrAreas();
  const { data: searchResults, isLoading: isSearchResultsLoading } = useQuery({
    queryKey: [urn],
    queryFn: () => getCaseSearchResults({ urn: urn }),
    retry: false,
    enabled: !!state.apiData?.caseDivisionsOrAreas,
    throwOnError: true,
    staleTime: 0,
    gcTime: 0,
  });
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

    const description =
      searchResults?.[0].operationName || searchResults?.[0].leadDefendantName;
    const caseId = searchResults?.[0].caseId;

    setDisableButtons(true);
    if (formData.manageEgressRadio === "skip-egress") {
      void navigate(`/manage-set-up-shared-drive`);
      return;
    }
    console.log("hiiiiii000>>>", formData.manageEgressRadio);
    if (formData.manageEgressRadio === "connect-existing") {
      console.log("hiiiiii");
      void navigate(
        `/case-registration/${caseId}/egress-connect-search-results?workspace-name=${description}`,
      );
      return;
    }

    if (caseId && description) {
      await createEgressCaseMutation.mutateAsync({
        templateId: "1223",
        caseId,
        description,
      });
    }

    void navigate(`/case-registration/${caseId}/egress-connect-confirmation`);

    setDisableButtons(false);
  };

  if (isDivisionsOrAreasLoading || isSearchResultsLoading) {
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
                disabled: disableButtons,
              },
              {
                id: "manage-egress-radio-connect-existing",
                children: "Connect an existing Egress case",
                value: "connect-existing",
                "data-testid": "manage-egress-radio-connect-existing",
                disabled: disableButtons,
              },
              {
                id: "manage-egress-radio-skip",
                children: "Egress is not needed now",
                value: "skip",
                "data-testid": "manage-egress-radio-skip",
                disabled: disableButtons,
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
