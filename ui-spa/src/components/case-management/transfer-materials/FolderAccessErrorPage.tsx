import { BackLink } from "../../govuk";
import { useLocation } from "react-router-dom";
import styles from "./FolderAccessErrorPage.module.scss";

const FolderAccessErrorPage = () => {
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const folderType = searchParams.get("type");

  return (
    <div className="govuk-width-container">
      <BackLink to={"/"}>Back</BackLink>
      <div className={styles.contentWrapper}>
        <h1 className="govuk-heading-xl">
          Sorry, there was a problem connecting to{" "}
          {folderType === "shareddrive" ? "shared drive" : "egress"}
        </h1>
        <div>
          <span>You can:</span>
        </div>
        <ul className="govuk-list govuk-list--bullet">
          <li>
            check the Case Management System to make sure the case exists and
            that you have access.
          </li>
          <li>contact the product team if you need further help.</li>
        </ul>
      </div>
    </div>
  );
};

export default FolderAccessErrorPage;
