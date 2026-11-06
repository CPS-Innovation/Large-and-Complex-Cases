import { Panel } from "../../govuk";
import { PageContentWrapper } from "../../govuk/PageContentWrapper";
import styles from "./index.module.scss";
const CaseRegistrationFinalConfirmationPage = () => {
  return (
    <PageContentWrapper>
      <div className={styles.caseRegistrationFinalConfirmationPage}>
        <Panel titleChildren="Case registered "></Panel>
        <h2>Next steps</h2>
        <p>Use the URN to find the case.</p>
      </div>
    </PageContentWrapper>
  );
};
export default CaseRegistrationFinalConfirmationPage;
