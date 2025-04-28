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

const FolderPath: React.FC<FolderPathProps> = ({
  path,
  folderClickHandler,
}) => {
  const folders: Folder[] = useMemo(() => {
    // const includedHomePath = `Home/${path}`;
    const parts = path.split("/").filter(Boolean);

    const result = parts.map((folderName, index) => ({
      folderName,
      folderPath: `${parts.slice(0, index + 1).join("/")}/`,
    }));
    console.log("result>>>", result);
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
