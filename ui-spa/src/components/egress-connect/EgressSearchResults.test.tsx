import { render, screen, within, fireEvent } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import EgressSearchResults from "./EgressSearchResults";

//mocking the button component as the original compoenent is using webpackPrefetch  when trying to load the button from govukfrontend which which vitest cant handle and throws an unhandled error while running the test.
// So far this prefetch code is seen only in button component, hence mocking only that component in the test. To see the actual error command the mock code and run test.
vi.mock("../govuk", async () => {
  const actual = await vi.importActual("../govuk");
  return {
    ...actual,
    Button: ({
      children,
      disabled,
    }: {
      children: React.ReactNode;
      disabled: boolean;
    }) => <button disabled={disabled}>{children}</button>,
  };
});

describe("EgressSearchResults", () => {
  const handleConnectFolderMock = vi.fn();
  const egressSearchResults = [
    {
      id: "1",
      dateCreated: "2000-01-25",
      name: "thunderstrike",
      caseId: null,
      connectedCaseUrn: null,
      connectedCaseName: null,
    },
    {
      id: "2",
      dateCreated: "2000-01-26",
      name: "thunderstrikeab",
      caseId: 123,
      connectedCaseUrn: "16XL2628726",
      connectedCaseName: "Thunderstrike1 Case",
    },
    {
      id: "3",
      dateCreated: "2000-01-27",
      name: "thunderstrikeabc",
      caseId: null,
      connectedCaseUrn: null,
      connectedCaseName: null,
    },
    {
      id: "4",
      dateCreated: "2000-01-28",
      name: "ahunderstrikeabcd",
      caseId: null,
      connectedCaseUrn: null,
      connectedCaseName: null,
    },
  ];

  it("It renders the table with given search results", async () => {
    const { rerender } = render(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchResults={egressSearchResults}
        handleConnectFolder={handleConnectFolderMock}
      />,
    );
    const table = screen.getByRole("table");
    expect(table).toBeInTheDocument();

    const headers = screen.getAllByRole("columnheader");
    expect(headers).toHaveLength(3);
    expect(headers[0]).toHaveTextContent("");
    expect(headers[1]).toHaveTextContent("Name");
    expect(headers[2]).toHaveTextContent("Status");

    expect(screen.getByRole("button", { name: "Name" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Status" })).toBeInTheDocument();

    const rows = screen.getAllByRole("row");
    const dataRows = rows.slice(1);
    const rowValues = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );
    expect(rowValues).toEqual([
      ["Connect", "thunderstrike", "Available to connect"],
      ["", "thunderstrikeab", "Connected to Thunderstrike1 Case"],
      ["Connect", "thunderstrikeabc", "Available to connect"],
      ["Connect", "ahunderstrikeabcd", "Available to connect"],
    ]);
    expect(screen.queryByTestId("egress-results-count")).toHaveTextContent(
      "4 cases found for thunder",
    );

    const newResults = [egressSearchResults[0]];
    //rerender with single result
    rerender(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchResults={newResults}
        handleConnectFolder={handleConnectFolderMock}
      />,
    );
    expect(screen.queryByTestId("egress-results-count")).toHaveTextContent(
      "1 case found for thunder",
    );
    const newRows = screen.getAllByRole("row");
    const newDataRows = newRows.slice(1);
    const newRowValues = newDataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValues).toEqual([
      ["Connect", "thunderstrike", "Available to connect"],
    ]);
  });

  it("Should sort the egress search result content based column header Name", async () => {
    render(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchResults={egressSearchResults}
        handleConnectFolder={handleConnectFolderMock}
      />,
    );
    const table = screen.getByRole("table");
    expect(table).toBeInTheDocument();

    const headers = screen.getAllByRole("columnheader");
    expect(headers).toHaveLength(3);
    expect(headers[0]).toHaveTextContent("");
    expect(headers[1]).toHaveTextContent("Name");
    expect(headers[2]).toHaveTextContent("Status");

    expect(screen.getByRole("button", { name: "Name" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Status" })).toBeInTheDocument();

    const btnName = screen.getByRole("button", {
      name: "Name",
    });
    expect(btnName).toBeInTheDocument();

    const rows = screen.getAllByRole("row");
    const dataRows = rows.slice(1);

    const rowValues = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(rowValues).toEqual([
      ["Connect", "thunderstrike", "Available to connect"],
      ["", "thunderstrikeab", "Connected to Thunderstrike1 Case"],
      ["Connect", "thunderstrikeabc", "Available to connect"],
      ["Connect", "ahunderstrikeabcd", "Available to connect"],
    ]);

    fireEvent.click(btnName);

    const newRowValuesAscending = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValuesAscending).toEqual([
      ["Connect", "ahunderstrikeabcd", "Available to connect"],
      ["Connect", "thunderstrike", "Available to connect"],
      ["", "thunderstrikeab", "Connected to Thunderstrike1 Case"],
      ["Connect", "thunderstrikeabc", "Available to connect"],
    ]);

    fireEvent.click(btnName);

    const newRowValuesDescending = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValuesDescending).toEqual([
      ["Connect", "thunderstrikeabc", "Available to connect"],
      ["", "thunderstrikeab", "Connected to Thunderstrike1 Case"],
      ["Connect", "thunderstrike", "Available to connect"],
      ["Connect", "ahunderstrikeabcd", "Available to connect"],
    ]);
  });

  it("Should sort the egress search result content based column header Status", async () => {
    render(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchResults={egressSearchResults}
        handleConnectFolder={handleConnectFolderMock}
      />,
    );
    const table = screen.getByRole("table");
    expect(table).toBeInTheDocument();

    const headers = screen.getAllByRole("columnheader");
    expect(headers).toHaveLength(3);
    expect(headers[0]).toHaveTextContent("");
    expect(headers[1]).toHaveTextContent("Name");
    expect(headers[2]).toHaveTextContent("Status");

    expect(screen.getByRole("button", { name: "Name" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Status" })).toBeInTheDocument();

    const btnStatus = screen.getByRole("button", {
      name: "Status",
    });

    expect(btnStatus).toBeInTheDocument();

    const rows = screen.getAllByRole("row");
    const dataRows = rows.slice(1);

    const rowValues = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(rowValues).toEqual([
      ["Connect", "thunderstrike", "Available to connect"],
      ["", "thunderstrikeab", "Connected to Thunderstrike1 Case"],
      ["Connect", "thunderstrikeabc", "Available to connect"],
      ["Connect", "ahunderstrikeabcd", "Available to connect"],
    ]);

    fireEvent.click(btnStatus);

    const newRowValuesAscending = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValuesAscending).toEqual([
      ["", "thunderstrikeab", "Connected to Thunderstrike1 Case"],
      ["Connect", "thunderstrike", "Available to connect"],
      ["Connect", "thunderstrikeabc", "Available to connect"],
      ["Connect", "ahunderstrikeabcd", "Available to connect"],
    ]);

    fireEvent.click(btnStatus);

    const newRowValuesDescending = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValuesDescending).toEqual([
      ["Connect", "thunderstrike", "Available to connect"],
      ["Connect", "thunderstrikeabc", "Available to connect"],
      ["Connect", "ahunderstrikeabcd", "Available to connect"],
      ["", "thunderstrikeab", "Connected to Thunderstrike1 Case"],
    ]);
  });

  it("Should not show the Connect button if the folder is already connected to a case", () => {
    render(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchResults={egressSearchResults}
        handleConnectFolder={handleConnectFolderMock}
      />,
    );
    const rows = screen.getAllByRole("row");

    expect(
      within(rows[1]).getByRole("button", {
        name: /Connect/i,
      }),
    ).not.toBeDisabled();

    expect(
      within(rows[2]).queryByRole("button", {
        name: /Connect/i,
      }),
    ).not.toBeInTheDocument();

    expect(
      within(rows[3]).getByRole("button", {
        name: /Connect/i,
      }),
    ).not.toBeDisabled();
    expect(
      within(rows[4]).getByRole("button", {
        name: /Connect/i,
      }),
    ).not.toBeDisabled();
  });
});
