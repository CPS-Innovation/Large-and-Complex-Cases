import { useMemo, useState } from "react";

import { EgressSearchResultData } from "../../common/types/EgressSearchResponse";
import { Button, SortableTable } from "../govuk";
import { formatDate } from "../../common/utils/formatDate";
import { UseApiResult } from "../../common/hooks/useApiNew";
import {
  sortByStringProperty,
  sortByDateProperty,
} from "../../common/utils/sortUtils";
import styles from "./egressSearchResults.module.scss";

type SearchResultsProps = {
  egressSearchApi: UseApiResult<EgressSearchResultData>;
  handleConnectFolder: (id: string) => void;
};
const EgressSearchResults: React.FC<SearchResultsProps> = ({
  egressSearchApi,
  handleConnectFolder,
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();
  const egressSearchResultsData = useMemo(() => {
    if (!egressSearchApi?.data) return [];
    if (sortValues?.name === "workspace-name")
      return sortByStringProperty(
        egressSearchApi.data,
        "name",
        sortValues.type,
      );
    if (sortValues?.name === "date-created")
      return sortByDateProperty(
        egressSearchApi.data,
        "dateCreated",
        sortValues.type,
      );
    return egressSearchApi.data;
  }, [egressSearchApi, sortValues]);
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

  const handleTableSort = (
    sortName: string,
    sortType: "ascending" | "descending",
  ) => {
    setSortValues({ name: sortName, type: sortType });
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
        <SortableTable
          head={[
            {
              children: "Operation or defendant surname",
              sortable: true,
              sortName: "workspace-name",
            },

            {
              children: "Date created",
              sortable: true,
              sortName: "date-created",
            },
            {
              children: "",
              sortable: false,
            },
          ]}
          rows={getTableRowData()}
          handleTableSort={handleTableSort}
        />
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
