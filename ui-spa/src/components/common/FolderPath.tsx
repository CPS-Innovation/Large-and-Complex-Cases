import { useMemo } from "react";
import { LinkButton } from "../govuk";
import styles from "./FolderPath.module.scss";

type FolderPathProps = {
  path: string;
  folderClickHandler: (folderPath: string) => void;
};

type Folder = {
  folderName: string;
  folderPath: string;
};

export const FolderPath: React.FC<FolderPathProps> = ({
  path,
  folderClickHandler,
}) => {
  const folders: Folder[] = useMemo(() => {
    const parts = path.split("/").filter(Boolean);

    const result = parts.map((folderName, index) => ({
      folderName,
      folderPath: parts.slice(0, index + 1).join("/"),
    }));
    return result;
  }, [path]);

  return (
    <div>
      <ol className={styles.orderedList}>
        {folders.map((folder) => {
          return (
            <li className={styles.listItem}>
              <LinkButton onClick={() => folderClickHandler(folder.folderPath)}>
                {folder.folderName}
              </LinkButton>
            </li>
          );
        })}
      </ol>
    </div>
  );
};
