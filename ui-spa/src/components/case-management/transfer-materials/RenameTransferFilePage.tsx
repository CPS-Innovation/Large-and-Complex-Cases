import { useState, useCallback } from "react";
import { BackLink, Input, Tag, Button, LinkButton } from "../../govuk";
import styles from "./RenameTransferFilePage.module.scss";

type RenameTransferFilePageProps = {
  backLinkUrl: string;
  fileName: string;
  relativeFilePath: string;
  handleCancel: () => void;
  handleContinue: (name: string) => void;
};

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
    if (characterCount > 260)
      return (
        <p>
          File path length:
          <Tag
            gdsTagColour="red"
            className={styles.statusTag}
            data-testid="character-tag"
          >
            {characterCount} characters
          </Tag>
        </p>
      );
    return (
      <p>
        File path length:
        <Tag
          gdsTagColour="green"
          className={styles.statusTag}
          data-testid="character-tag"
        >
          {characterCount} characters
        </Tag>
      </p>
    );
  }, [relativeFilePath, inputValue]);
  return (
    <div className="govuk-width-container">
      <BackLink to={backLinkUrl} replace state={{ isRouteValid: true }}>
        Back
      </BackLink>
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">Edit file name</h1>

        <Input
          value={inputValue}
          onChange={handleInputValueChange}
          className={styles.fileNameInput}
        />
        {getCharactersText()}
        <p>You must reduce this to 260 characters or fewer.</p>
        <div className={styles.btnWrapper}>
          <Button onClick={() => handleContinue(inputValue)}>Continue</Button>
          <LinkButton onClick={handleCancel}>Cancel</LinkButton>
        </div>
      </div>
    </div>
  );
};
