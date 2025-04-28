import { useMemo } from "react";
import { LinkButton } from "../govuk";
import styles from "./FolderPath.module.scss";

type FolderPathProps = {
  path: string;
  disabled: boolean;
  folderClickHandler: (folderPath: string) => void;
};

type Folder = {
  folderName: string;
  folderPath: string;
};

const FolderPath: React.FC<FolderPathProps> = ({
  path,
  disabled,
  folderClickHandler,
}) => {
  const folders: Folder[] = useMemo(() => {
    const parts = path.split("/").filter(Boolean);

    const result = parts.map((folderName, index) => ({
      folderName,
      folderPath: `${parts.slice(0, index + 1).join("/")}/`,
    }));
    const withHome = [{ folderName: "Home", folderPath: "" }, ...result];
    return withHome;
  }, [path]);

  return (
    <div>
      <ol className={styles.orderedList}>
        {folders.map((folder, index) => {
          return (
            <li className={styles.listItem}>
              {index !== folders.length - 1 ? (
                <LinkButton
                  onClick={() => folderClickHandler(folder.folderPath)}
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
