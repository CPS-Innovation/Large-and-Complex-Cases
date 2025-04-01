import { RawApiResult } from "../../common/types/ApiResult";
import { SearchResultData } from "../../common/types/SearchResultResponse";
import { Table, Tag } from "../govuk";
import { Link } from "react-router";
import { SearchFromData } from "../../common/hooks/useCaseSearchForm";
import { formatDate } from "../../common/utils/formatDate";
import styles from "./searchResults.module.scss";

type SearchResultsProps = {
  searchApiResults: RawApiResult<SearchResultData>;
  searchType: SearchFromData["searchType"];
};
const SearchResults: React.FC<SearchResultsProps> = ({
  searchApiResults,
  searchType,
}) => {
  const getSearchTypeText = () => {
    if (searchType === "defendant name") return "defendant surname";
    return searchType;
  };
  const getTableRowData = () => {
    if (searchApiResults.status !== "succeeded") return [];
    return searchApiResults.data.map((data) => {
      const operationName = data.operationName
        ? data.operationName
        : data.leadDefendantName;
      return {
        cells: [
          {
            children: (
              <Link to="/" className={styles.link}>
                {operationName}
              </Link>
            ),
          },
          {
            children: data.urn,
          },
          {
            children: data.leadDefendantName,
          },
          {
            children:
              data.egressStatus === "connected" ? (
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
            children:
              data.sharedDriveStatus === "connected" ? (
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
                to={`/egress-connect?workspace-name=${operationName}`}
                className={styles.link}
              >
                View{" "}
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
        !!searchApiResults.data.length && (
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
        !searchApiResults.data.length && (
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
