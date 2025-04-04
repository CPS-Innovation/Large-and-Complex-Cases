import { useEffect, useState } from "react";
import { Button, Input, InsetText, ErrorSummary, BackLink } from "../govuk";
import EgressSearchResults from "./EgressSearchResults";
import { UseApiResult } from "../../common/hooks/useApiNew";
import { useNavigate, useSearchParams } from "react-router-dom";
import { EgressSearchResultData } from "../../common/types/EgressSearchResponse";
import styles from "./EgressSearchPage.module.scss";

type EgressSearchPageProps = {
  searchValue: string;
  egressSearchApi: UseApiResult<EgressSearchResultData>;
  handleFormChange: (value: string) => void;
  handleSearch: () => void;
  handleConnectFolder: (id: string) => void;
};
const EgressSearchPage: React.FC<EgressSearchPageProps> = ({
  searchValue,
  egressSearchApi,
  handleFormChange,
  handleSearch,
  handleConnectFolder,
}) => {
  const navigate = useNavigate();

  //   const handleFormChange = (value: string) => {
  //     setFormValue(value);
  //   };

  //   const handleConnectFolder = (id: string) => {
  //     setSelectedFolder(id);
  //     // navigate(`/egress-connect?id=${id}`);
  //   };

  const handleFormSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    handleSearch();
  };
  if (egressSearchApi.status === "loading")
    return <div className="govuk-width-container">Loading...</div>;

  return (
    <div className="govuk-width-container">
      <BackLink to="/">Back</BackLink>

      <div>
        <h1>Select an Egress folder to link to the case</h1>
        <InsetText>
          <p>Select a folder from the list to link it to this case.</p>
          <p>
            If the folder you need is not listed, check that you have the
            correct permissions in Egress or contact the product team for
            support.
          </p>
        </InsetText>
      </div>
      <form onSubmit={handleFormSubmit}>
        <div className={styles.inputWrapper}>
          <Input
            id="search-urn"
            data-testid="search-urn"
            className="govuk-input--width-20"
            label={{
              children: "Egress folder name",
            }}
            // errorMessage={
            //   //   formDataErrors[SearchFormField.urn]
            //   //     ? {
            //   //         children:
            //   //           formDataErrors[SearchFormField.urn].inputErrorText ??
            //   //           formDataErrors[SearchFormField.urn].errorSummaryText,
            //   //       }
            //   //     : undefined
            // }
            name="Egress folder name"
            type="text"
            value={searchValue}
            onChange={(value: string) => {
              handleFormChange(value);
            }}
            disabled={false}
          />
          <div className={styles.btnWrapper}>
            <Button type="submit">Search</Button>
          </div>
        </div>
      </form>
      <div className={styles.searchResultsCount}>
        There are <b>4 folders</b>matching the case <b>Thunderstruck</b> on
        egress.
      </div>
      <EgressSearchResults
        egressSearchApi={egressSearchApi}
        handleConnectFolder={handleConnectFolder}
      />
    </div>
  );
};

export default EgressSearchPage;
