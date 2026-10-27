import { useMemo, useState } from "react";
import { type EgressSearchResultData } from "../../../schemas";
import { Button, SortableTable, Tag } from "../../govuk";
import {
  sortByStringProperty,
  sortByDateProperty,
} from "../../../common/utils/sortUtils";
import styles from "./EgressSearchResultsV2.module.scss";

type EgressSearchResultsProps = {
  workspaceName: string;
  egressSearchResults: EgressSearchResultData;
  disableBtns: boolean;
  handleConnectFolder: (id: string) => void;
};
const EgressSearchResultsV2: React.FC<EgressSearchResultsProps> = ({
  workspaceName,
  egressSearchResults,
  disableBtns,
  handleConnectFolder,
}) => {
  const [sortValues, setSortValues] = useState<{
    name: string;
    type: "ascending" | "descending";
  }>();

  const sortByStatus = (
    data: EgressSearchResultData,
    order: "ascending" | "descending",
  ): EgressSearchResultData => {
    const included = data.filter((item) => item.caseId);
    const excluded = data.filter((item) => !item.caseId);
    if (order === "ascending") return [...included, ...excluded];
    return [...excluded, ...included];
  };

  const egressSearchResultsData = useMemo(() => {
    if (!egressSearchResults) return [];
    if (sortValues?.name === "workspace-name")
      return sortByStringProperty(egressSearchResults, "name", sortValues.type);
    if (sortValues?.name === "date-created")
      return sortByDateProperty(
        egressSearchResults,
        "dateCreated",
        sortValues.type,
      );
    if (sortValues?.name === "status")
      return sortByStatus(egressSearchResults, sortValues.type);
    return egressSearchResults;
  }, [egressSearchResults, sortValues]);

  const handleConnect = (id: string) => {
    handleConnectFolder(id);
  };

  const getTableRowData = () => {
    return egressSearchResultsData.map((data) => {
      return {
        cells: [
          {
            children: !data.caseId && (
              <Button
                className="govuk-button--secondary"
                name="secondary"
                onClick={() => handleConnect(data.id)}
                disabled={disableBtns}
              >
                Connect
              </Button>
            ),
          },
          {
            children: <div>{data.name}</div>,
          },
          {
            children: data.caseId ? (
              <Tag gdsTagColour="grey" className={styles.statusTag}>
                Connected to {data.connectedCaseName}
              </Tag>
            ) : (
              <Tag gdsTagColour="green" className={styles.statusTag}>
                Available to connect
              </Tag>
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

  return (
    <div className={styles.results}>
      {!!egressSearchResultsData.length && (
        <>
          <div
            className={styles.searchResultsCount}
            data-testid="egress-results-count"
          >
            {egressSearchResultsData.length}{" "}
            {egressSearchResultsData.length > 1 ? "cases" : "case"} found for{" "}
            <b>{workspaceName}</b>
          </div>
          <div className={styles.tableWrapper}>
            <SortableTable
              caption="egress cases table, column headers with buttons are sortable"
              captionClassName="govuk-visually-hidden"
              head={[
                {
                  children: "",
                  sortable: false,
                },
                {
                  children: "Name",
                  sortable: true,
                  sortName: "workspace-name",
                },
                {
                  children: "Status",
                  sortable: true,
                  sortName: "status",
                },
              ]}
              rows={getTableRowData()}
              handleTableSort={handleTableSort}
            />
          </div>
        </>
      )}
      {!egressSearchResultsData.length && (
        <>
          <div className={styles.searchResultsCount}>
            There are <b>no cases </b>matching <b>{workspaceName}</b>.
          </div>
          <div className={styles.noResultsContent}>
            <div>
              <span>You can:</span>
            </div>
            <ul className="govuk-list govuk-list--bullet">
              <li>check for spelling or typing errors</li>
              <li>
                check the case exists on Egress and you have the correct
                permissions
              </li>
              <li>contact the product team if you need help</li>
            </ul>
          </div>
        </>
      )}
    </div>
  );
};

export default EgressSearchResultsV2;
