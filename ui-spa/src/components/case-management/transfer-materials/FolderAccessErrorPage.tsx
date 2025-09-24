import { BackLink } from "../../govuk";
import { useLocation } from "react-router-dom";
import styles from "./FolderAccessErrorPage.module.scss";

const FolderAccessErrorPage = () => {
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const folderType = searchParams.get("type");

  return (
    <div>
      <BackLink to={"/"}>Back</BackLink>
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">
          Sorry, there was a problem connecting to{" "}
          {folderType === "shareddrive" ? "Shared Drive" : "Egress"}
        </h1>
        <div>
          <span>You can:</span>
        </div>
        <ul className="govuk-list govuk-list--bullet">
          <li>
            check the case exists and you have access on the Case Management
            System
          </li>
          <li>contact the product team if you need help</li>
        </ul>
      </div>
    </div>
  );
};

export default FolderAccessErrorPage;
