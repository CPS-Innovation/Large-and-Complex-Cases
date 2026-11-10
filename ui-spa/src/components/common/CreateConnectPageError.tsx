import { useContext } from "react";
import { Button } from "../govuk";
import { useNavigate } from "react-router";
import { PageContentWrapper } from "../govuk/PageContentWrapper";
import { MainStateContext } from "../../providers/MainStateProvider";

type CreateConnectErrorPageProps = {
  type:
    | "create-egress-case-error"
    | "connect-egress-case-error"
    | "create-shared-drive-folder-error"
    | "connect-shared-drive-folder-error";
};

const CreateConnectErrorPage: React.FC<CreateConnectErrorPageProps> = ({
  type,
}) => {
  const { state } = useContext(MainStateContext);

  const getPageDetails = () => {
    if (
      type === "create-egress-case-error" ||
      type === "connect-egress-case-error"
    ) {
      return {
        backLinkUrl: state.appData.egressCreateConnectFailurePage.backLinkUrl,
        title:
          type === "create-egress-case-error"
            ? "There is a problem creating a case on Egress"
            : "There is a problem connecting to Egress",
        description1:
          type === "create-egress-case-error"
            ? "return to the previous screen and try again"
            : "return to the previous screen and try to connect again",
        description2: "contact the product team if you need help",
      };
    }

    return {
      backLinkUrl:
        state.appData.sharedDriveCreateConnectFailurePage.backLinkUrl,
      title:
        type === "create-shared-drive-folder-error"
          ? "There is a problem creating folders on the Shared Drive"
          : "There is a problem connecting to the Shared Drive",
      description1:
        type === "create-shared-drive-folder-error"
          ? "return to the previous screen and try again"
          : "return to the previous screen and try to connect again",
      description2: "contact the product team if you need help",
    };
  };

  const { backLinkUrl, title, description1, description2 } = getPageDetails();

  const navigate = useNavigate();

  const handleTryAgain = () => {
    void navigate(backLinkUrl);
  };

  return (
    <div>
      <PageContentWrapper>
        <div className="govuk-main-wrapper">
          <h1 className="govuk-heading-xl">{title}</h1>
          <p>You can:</p>
          <ul className="govuk-list govuk-list--bullet">
            <li>{description1}</li>
            <li>{description2}</li>
          </ul>

          <Button onClick={handleTryAgain}> Try again</Button>
        </div>
      </PageContentWrapper>
    </div>
  );
};

export default CreateConnectErrorPage;
