import { LinkButton } from "../govuk";
import styles from "./FolderPath.module.scss";

type FolderPathProps = {
  disabled: boolean;
  folders: Folder[];
  handleFolderPathClick: (folderPath: string) => void;
};

export type Folder = {
  folderName: string;
  folderPath: string;
  folderId?: string;
};

const FolderPath: React.FC<FolderPathProps> = ({
  folders,
  disabled,
  handleFolderPathClick,
}) => {
  return (
    <div>
      <ol className={styles.orderedList}>
        {folders.map((folder, index) => {
          return (
            <li
              key={`${folder.folderName}-${index}`}
              className={styles.listItem}
            >
              {index !== folders.length - 1 ? (
                <LinkButton
                  onClick={() => handleFolderPathClick(folder.folderPath)}
                  disabled={disabled}
                >
                  {folder.folderName}
                </LinkButton>
              ) : (
                <span className={styles.currentFolderName}>
                  {folder.folderName}
                </span>
              )}
            </li>
          );
        })}
      </ol>
    </div>
  );
};

export default FolderPath;
