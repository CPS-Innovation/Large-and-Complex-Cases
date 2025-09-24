import { useState, useCallback } from "react";
import { BackLink, Input, Tag, Button, LinkButton } from "../../govuk";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import styles from "./RenameTransferFilePage.module.scss";

type RenameTransferFilePageProps = {
  backLinkUrl: string;
  fileName: string;
  relativeFilePath: string;
  handleCancel: () => void;
  handleContinue: (name: string) => void;
};
const MAX_FILE_PATH_CHARACTERS = 260;

export const RenameTransferFilePage: React.FC<RenameTransferFilePageProps> = ({
  backLinkUrl,
  fileName,
  relativeFilePath,
  handleCancel,
  handleContinue,
}) => {
  const [inputValue, setInputValue] = useState(fileName);

  const handleInputValueChange = (val: string) => {
    setInputValue(val);
  };

  const getCharactersText = useCallback(() => {
    const characterCount = `${relativeFilePath}/${inputValue}`.length;
    const tagColor =
      characterCount > MAX_FILE_PATH_CHARACTERS ? "red" : "green";

    return (
      <>
        <output className="govuk-visually-hidden" aria-live="polite">
          File path length: {characterCount} characters
        </output>
        <p>
          File path length:
          <Tag
            gdsTagColour={tagColor}
            className={styles.statusTag}
            data-testid="character-tag"
          >
            {characterCount} characters
          </Tag>
        </p>
      </>
    );
  }, [relativeFilePath, inputValue]);
  return (
    <div>
      <BackLink to={backLinkUrl} replace state={{ isRouteValid: true }}>
        Back
      </BackLink>
      <PageContentWrapper>
        <div className={styles.contentWrapper}>
          <h1 className="govuk-heading-xl">Edit file name</h1>

          <Input
            value={inputValue}
            onChange={handleInputValueChange}
            className={styles.fileNameInput}
          />
          {getCharactersText()}
          <p>
            You must reduce this to {MAX_FILE_PATH_CHARACTERS} characters or
            fewer.
          </p>
          <div className={styles.btnWrapper}>
            <Button onClick={() => handleContinue(inputValue)}>Continue</Button>
            <LinkButton onClick={handleCancel}>Cancel</LinkButton>
          </div>
        </div>
      </PageContentWrapper>
    </div>
  );
};
