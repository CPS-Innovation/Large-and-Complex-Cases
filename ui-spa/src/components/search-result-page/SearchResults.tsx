import { useContext } from "react";
import type { SearchResultData, SearchResult } from "../../schemas";
import { Table, Tag } from "../govuk";
import { Link, useNavigate } from "react-router";
import { SearchFormData } from "../../common/hooks/useCaseSearchForm";
import { formatDate } from "../../common/utils/formatDate";
import { getUrlSearchParam } from "../../common/utils/getUrlSearchParam";
import { MainStateContext } from "../../providers/MainStateProvider";
import styles from "./SearchResults.module.scss";

type SearchResultsProps = {
  searchQueryString: string;
  searchApiResults: SearchResultData;
  searchType: SearchFormData["searchType"];
};

const SearchResults: React.FC<SearchResultsProps> = ({
  searchQueryString,
  searchApiResults,
}) => {
  const navigate = useNavigate();
  const { dispatch } = useContext(MainStateContext);

  const renderLink = (data: SearchResult, operationName: string) => {
    if (!data.egressWorkspaceId) {
      const egressConnectUrl = `/case/${data.caseId}/egress-connect?${getUrlSearchParam("workspace-name", operationName)}`;
      const handleClick = (
        e: React.MouseEvent<HTMLAnchorElement, MouseEvent>,
      ) => {
        e.preventDefault();
        dispatch({
          type: "SET_EGRESS_CONNECT_PAGE",
          payload: {
            searchQueryString: searchQueryString,
            isNetAppConnected: !!data.netappFolderPath,
          },
        });
        navigate(egressConnectUrl);
      };
      return (
        <Link
          to={egressConnectUrl}
          className={styles.link}
          onClick={handleClick}
        >
          Connect
        </Link>
      );
    }
    if (!data.netappFolderPath) {
      const netAppConnectUrl = `/case/${data.caseId}/netapp-connect?${getUrlSearchParam("operation-name", operationName)}`;
      const handleClick = (
        e: React.MouseEvent<HTMLAnchorElement, MouseEvent>,
      ) => {
        e.preventDefault();
        dispatch({
          type: "SET_SHARED_DRIVE_CONNECT_PAGE",
          payload: {
            searchQueryString: searchQueryString,
            netappRootFolderPath: "",
          },
        });

        navigate(netAppConnectUrl);
      };
      return (
        <Link
          to={netAppConnectUrl}
          className={styles.link}
          onClick={handleClick}
        >
          Connect
        </Link>
      );
    }

    return (
      <Link to={`/case/${data.caseId}/case-management`} className={styles.link}>
        View
      </Link>
    );
  };
  const getTableRowData = () => {
    if (!searchApiResults) return [];
    return searchApiResults.map((data) => {
      const operationName = data.operationName
        ? data.operationName
        : data.leadDefendantName!;
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
                Not connected
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
                Not connected
              </Tag>
            ),
          },
          {
            children: formatDate(data.registrationDate),
          },
          {
            children: renderLink(data, operationName),
          },
        ],
      };
    });
  };
  return (
    <div className={styles.results}>
      {searchApiResults.length > 0 && (
        <Table
          caption="Case search result table"
          captionClassName="govuk-visually-hidden"
          head={[
            {
              children: "Defendant or operation name",
            },
            {
              children: "URN",
            },
            {
              children: "Lead defendants",
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
      {!searchApiResults.length && (
        <div className={styles.noResultsContent}>
          <div>
            <span>You can:</span>
          </div>
          <ul className="govuk-list govuk-list--bullet">
            <li>check for spelling or typing errors</li>
            <li>
              check the case exists and you have access on the Case Management
              System
            </li>
            <li>contact the product team if you need help</li>
          </ul>
        </div>
      )}
    </div>
  );
};

export default SearchResults;
