import { UseApiResult } from "../../common/hooks/useApi";
import {
  SearchResultData,
  SearchResult,
} from "../../common/types/SearchResultResponse";
import { Table, Tag } from "../govuk";
import { Link } from "react-router";
import { SearchFromData } from "../../common/hooks/useCaseSearchForm";
import { formatDate } from "../../common/utils/formatDate";
import styles from "./searchResults.module.scss";

type SearchResultsProps = {
  searchQueryString: string;
  searchApiResults: UseApiResult<SearchResultData>;
  searchType: SearchFromData["searchType"];
};
const SearchResults: React.FC<SearchResultsProps> = ({
  searchQueryString,
  searchApiResults,
  searchType,
}) => {
  const getSearchTypeText = () => {
    if (searchType === "defendant name") return "defendant surname";
    return searchType;
  };
  const getConnectOrViewUrl = (data: SearchResult, operationName: string) => {
    if (!data.egressWorkspaceId)
      return `/case/${data.caseId}/egress-connect?workspace-name=${operationName}`;
    if (!data.netappFolderPath)
      return `/case/${data.caseId}/netapp-connect?operation-name=${operationName}`;
    return `/case/${data.caseId}/case-management`;
  };
  const getTableRowData = () => {
    if (!searchApiResults.data) return [];
    return searchApiResults.data.map((data) => {
      const operationName = data.operationName
        ? data.operationName
        : data.leadDefendantName;
      return {
        cells: [
          {
            children: <span>{operationName}</span>,
          },
          {
            children: data.urn,
          },
          {
            children: data.leadDefendantName,
          },
          {
            children: data.egressWorkspaceId ? (
              <Tag gdsTagColour="green" className={styles.statusTag}>
                Connected
              </Tag>
            ) : (
              <Tag gdsTagColour="grey" className={styles.statusTag}>
                Inactive
              </Tag>
            ),
          },
          {
            children: data.netappFolderPath ? (
              <Tag gdsTagColour="green" className={styles.statusTag}>
                Connected
              </Tag>
            ) : (
              <Tag gdsTagColour="grey" className={styles.statusTag}>
                Inactive
              </Tag>
            ),
          },
          {
            children: formatDate(data.registrationDate),
          },
          {
            children: (
              <Link
                to={getConnectOrViewUrl(data, operationName)}
                state={{
                  searchQueryString: searchQueryString,
                  netappFolderPath: data.netappFolderPath,
                }}
                className={styles.link}
              >
                {!data.egressWorkspaceId || !data.netappFolderPath
                  ? "Connect"
                  : "View"}
              </Link>
            ),
          },
        ],
      };
    });
  };
  return (
    <div className={styles.results}>
      {searchApiResults.status === "succeeded" &&
        !!searchApiResults.data?.length && (
          <Table
            head={[
              {
                children: "Defendant or Operation name",
              },
              {
                children: "URN",
              },
              {
                children: "Lead defendant",
              },
              {
                children: "Egress",
              },
              {
                children: "Shared Drive",
              },
              {
                children: "Case created",
              },
              {
                children: "",
              },
            ]}
            rows={getTableRowData()}
          ></Table>
        )}
      {searchApiResults.status === "succeeded" &&
        !searchApiResults.data?.length && (
          <div className={styles.noResultsContent}>
            <div>
              <span>You can:</span>
            </div>
            <ul className="govuk-list govuk-list--bullet">
              <li>check for spelling mistakes in the {getSearchTypeText()}.</li>
              <li>
                check the Case Management System to make sure the case exists
                and that you have access.
              </li>
              <li>contact the product team if you need further help.</li>
            </ul>
          </div>
        )}
    </div>
  );
};

export default SearchResults;
