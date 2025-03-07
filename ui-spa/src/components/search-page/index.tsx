import { useRef } from "react";
import { useQueryParamsState } from "../../common/hooks/useQueryParamsState";
import { useCaseSearchInputLogic } from "../../common/hooks/useCaseSearchInputLogic";
import { CaseSearchQueryParams } from "../../common/types/CaseSearchQueryParams";
import { H1, Input, Button, Label, LabelText, ErrorSummary } from "govuk-react";
import styles from "./index.module.scss";

const validationFailMessage = "Enter a URN in the right format";
const CaseSearchPage = () => {
  const {
    search: searchKeyFromSearchParams,
    setParams,
    search,
  } = useQueryParamsState<CaseSearchQueryParams>();
  const inputRef = useRef<HTMLInputElement>(null);

  const { handleChange, handleSubmit, isError, searchKey } =
    useCaseSearchInputLogic({ searchKeyFromSearchParams, setParams, search });

  const handleSearch = () => {
    handleSubmit();
  };
  const onHandleErrorClick = () => {
    console.log("hiiiii");
    inputRef.current?.focus();
  };
  return (
    <div className={`govuk-width-container ${styles.pageWrapper}`}>
      <div>
        <H1 className="govuk-heading-xl govuk-!-margin-bottom-4">
          Find a case
        </H1>
        {isError && (
          <ErrorSummary
            errors={[
              {
                targetName: "case-search-text-input",
                text: validationFailMessage,
              },
            ]}
            className={styles.errorSummary}
            onHandleErrorClick={onHandleErrorClick}
          />
        )}

        {
          <div className={styles.inputWrapper}>
            <Label>
              <LabelText>Search by Operation name</LabelText>
              <Input
                className="govuk-input--width-20"
                value={searchKey}
                ref={inputRef}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                  handleChange(e.target.value)
                }
              />
            </Label>
            <Button onClick={handleSearch}>Search</Button>
          </div>
        }
      </div>
    </div>
  );
};

export default CaseSearchPage;
