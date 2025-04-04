import { useMemo } from "react";

import { EgressSearchResultData } from "../../common/types/EgressSearchResponse";
import { Button, Table, Tag } from "../govuk";
import { formatDate } from "../../common/utils/formatDate";
import { UseApiResult } from "../../common/hooks/useApiNew";
import styles from "./egressSearchResults.module.scss";

type SearchResultsProps = {
  egressSearchApi: UseApiResult<EgressSearchResultData>;
  handleConnectFolder: (id: string) => void;
};
const EgressSearchResults: React.FC<SearchResultsProps> = ({
  egressSearchApi,
  handleConnectFolder,
}) => {
  console.log("egressSearchApi>>", egressSearchApi);
  const egressSearchResultsData = useMemo(() => {
    if (!egressSearchApi?.data) return [];
    return egressSearchApi.data;
  }, [egressSearchApi]);
  const handleConnect = (id: string) => {
    handleConnectFolder(id);
  };
  const getTableRowData = () => {
    return egressSearchResultsData.map((data) => {
      return {
        cells: [
          {
            children: (
              <div>
                <b>{data.name}</b>
              </div>
            ),
          },

          {
            children: formatDate(data.dateCreated),
          },
          {
            children: (
              <Button
                className="govuk-button--secondary"
                name="secondary"
                onClick={() => handleConnect(data.id)}
              >
                connect folder
              </Button>
            ),
          },
        ],
      };
    });
  };

  if (egressSearchApi.status !== "succeeded") {
    return <></>;
  }

  return (
    <div className={styles.results}>
      <div className={styles.searchResultsCount}>
        There are <b>4 folders</b>matching the case <b>Thunderstruck</b> on
        egress.
      </div>
      {!!egressSearchResultsData.length && (
        <Table
          head={[
            {
              children: (
                <div>
                  <span> Operation or defendant surname</span>
                  <button>sort</button>
                </div>
              ),
            },

            {
              children: "Date created",
            },
            {
              children: "",
            },
          ]}
          rows={getTableRowData()}
        ></Table>
      )}
      {!egressSearchResultsData.length && (
        <div className={styles.noResultsContent}>
          <div>
            <span>You can:</span>
          </div>
          <ul className="govuk-list govuk-list--bullet">
            <li>check for spelling mistakes in the {}.</li>
            <li>
              check the Case Management System to make sure the case exists and
              that you have access.
            </li>
            <li>contact the product team if you need further help.</li>
          </ul>
        </div>
      )}
    </div>
  );
};

export default EgressSearchResults;
