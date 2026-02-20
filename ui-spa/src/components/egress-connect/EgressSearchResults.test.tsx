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
  const egressSearchApiResults = {
    status: "succeeded" as const,
    data: [
      {
        id: "1",
        dateCreated: "2000-01-25",
        name: "thunderstrike",
        caseId: null,
      },
      {
        id: "2",
        dateCreated: "2000-01-26",
        name: "thunderstrikeab",
        caseId: 123,
      },
      {
        id: "3",
        dateCreated: "2000-01-27",
        name: "thunderstrikeabc",
        caseId: null,
      },
      {
        id: "4",
        dateCreated: "2000-01-28",
        name: "ahunderstrikeabcd",
        caseId: null,
      },
    ],
    refetch: vi.fn(),
  };

  it("It renders the table with given search results", async () => {
    const { rerender } = render(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchApi={egressSearchApiResults}
        handleConnectFolder={handleConnectFolderMock}
      />,
    );
    const table = screen.getByRole("table");
    expect(table).toBeInTheDocument();

    const headers = screen.getAllByRole("columnheader");
    expect(headers).toHaveLength(4);
    expect(headers[0]).toHaveTextContent("Operation or defendant last name");
    expect(headers[1]).toHaveTextContent("Status");
    expect(headers[2]).toHaveTextContent("Date created");
    expect(headers[3]).toHaveTextContent("");
    expect(
      screen.getByRole("button", { name: "Operation or defendant last name" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Status" })).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Date created" }),
    ).toBeInTheDocument();
    const rows = screen.getAllByRole("row");
    const dataRows = rows.slice(1);
    const rowValues = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );
    expect(rowValues).toEqual([
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
      ["thunderstrikeab", "Connected", "26/01/2000", "Connect"],
      ["thunderstrikeabc", "Not connected", "27/01/2000", "Connect"],
      ["ahunderstrikeabcd", "Not connected", "28/01/2000", "Connect"],
    ]);
    expect(screen.queryByTestId("egress-results-count")).toHaveTextContent(
      "There are 4 folders matching thunder.",
    );

    const newResults = {
      ...egressSearchApiResults,
      data: [egressSearchApiResults.data[0]],
    };
    //rerender with single result
    rerender(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchApi={newResults}
        handleConnectFolder={handleConnectFolderMock}
      />,
    );
    expect(screen.queryByTestId("egress-results-count")).toHaveTextContent(
      "There is 1 folder matching thunder.",
    );
    const newRows = screen.getAllByRole("row");
    const newDataRows = newRows.slice(1);
    const newRowValues = newDataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValues).toEqual([
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
    ]);
  });

  it("Should sort the egress search result content based column header Operation or defendant last name", async () => {
    render(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchApi={egressSearchApiResults}
        handleConnectFolder={handleConnectFolderMock}
      />,
    );
    const table = screen.getByRole("table");
    expect(table).toBeInTheDocument();

    const headers = screen.getAllByRole("columnheader");
    expect(headers).toHaveLength(4);
    expect(headers[0]).toHaveTextContent("Operation or defendant last name");
    expect(headers[1]).toHaveTextContent("Status");
    expect(headers[2]).toHaveTextContent("Date created");
    expect(headers[3]).toHaveTextContent("");
    const btnOperationName = screen.getByRole("button", {
      name: "Operation or defendant last name",
    });
    expect(btnOperationName).toBeInTheDocument();

    const rows = screen.getAllByRole("row");
    const dataRows = rows.slice(1);

    const rowValues = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(rowValues).toEqual([
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
      ["thunderstrikeab", "Connected", "26/01/2000", "Connect"],
      ["thunderstrikeabc", "Not connected", "27/01/2000", "Connect"],
      ["ahunderstrikeabcd", "Not connected", "28/01/2000", "Connect"],
    ]);

    fireEvent.click(btnOperationName);

    const newRowValuesAscending = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValuesAscending).toEqual([
      ["ahunderstrikeabcd", "Not connected", "28/01/2000", "Connect"],
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
      ["thunderstrikeab", "Connected", "26/01/2000", "Connect"],
      ["thunderstrikeabc", "Not connected", "27/01/2000", "Connect"],
    ]);

    fireEvent.click(btnOperationName);

    const newRowValuesDescending = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValuesDescending).toEqual([
      ["thunderstrikeabc", "Not connected", "27/01/2000", "Connect"],
      ["thunderstrikeab", "Connected", "26/01/2000", "Connect"],
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
      ["ahunderstrikeabcd", "Not connected", "28/01/2000", "Connect"],
    ]);
  });

  it("Should sort the egress search result content based column header Status", async () => {
    render(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchApi={egressSearchApiResults}
        handleConnectFolder={handleConnectFolderMock}
      />,
    );
    const table = screen.getByRole("table");
    expect(table).toBeInTheDocument();

    const headers = screen.getAllByRole("columnheader");
    expect(headers).toHaveLength(4);
    expect(headers[0]).toHaveTextContent("Operation or defendant last name");
    expect(headers[1]).toHaveTextContent("Status");
    expect(headers[2]).toHaveTextContent("Date created");
    expect(headers[3]).toHaveTextContent("");

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
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
      ["thunderstrikeab", "Connected", "26/01/2000", "Connect"],
      ["thunderstrikeabc", "Not connected", "27/01/2000", "Connect"],
      ["ahunderstrikeabcd", "Not connected", "28/01/2000", "Connect"],
    ]);

    fireEvent.click(btnStatus);

    const newRowValuesAscending = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValuesAscending).toEqual([
      ["thunderstrikeab", "Connected", "26/01/2000", "Connect"],
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
      ["thunderstrikeabc", "Not connected", "27/01/2000", "Connect"],
      ["ahunderstrikeabcd", "Not connected", "28/01/2000", "Connect"],
    ]);

    fireEvent.click(btnStatus);

    const newRowValuesDescending = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValuesDescending).toEqual([
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
      ["thunderstrikeabc", "Not connected", "27/01/2000", "Connect"],
      ["ahunderstrikeabcd", "Not connected", "28/01/2000", "Connect"],
      ["thunderstrikeab", "Connected", "26/01/2000", "Connect"],
    ]);
  });

  it("Should sort the egress search result content based column header Date created", async () => {
    render(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchApi={egressSearchApiResults}
        handleConnectFolder={handleConnectFolderMock}
      />,
    );
    const table = screen.getByRole("table");
    expect(table).toBeInTheDocument();

    const headers = screen.getAllByRole("columnheader");
    expect(headers).toHaveLength(4);
    expect(headers[0]).toHaveTextContent("Operation or defendant last name");
    expect(headers[1]).toHaveTextContent("Status");
    expect(headers[2]).toHaveTextContent("Date created");
    expect(headers[3]).toHaveTextContent("");

    const btnDateCreated = screen.getByRole("button", {
      name: "Date created",
    });

    expect(btnDateCreated).toBeInTheDocument();

    const rows = screen.getAllByRole("row");
    const dataRows = rows.slice(1);

    const rowValues = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(rowValues).toEqual([
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
      ["thunderstrikeab", "Connected", "26/01/2000", "Connect"],
      ["thunderstrikeabc", "Not connected", "27/01/2000", "Connect"],
      ["ahunderstrikeabcd", "Not connected", "28/01/2000", "Connect"],
    ]);

    fireEvent.click(btnDateCreated);

    const newRowValuesAscending = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValuesAscending).toEqual([
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
      ["thunderstrikeab", "Connected", "26/01/2000", "Connect"],
      ["thunderstrikeabc", "Not connected", "27/01/2000", "Connect"],
      ["ahunderstrikeabcd", "Not connected", "28/01/2000", "Connect"],
    ]);

    fireEvent.click(btnDateCreated);

    const newRowValuesDescending = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(newRowValuesDescending).toEqual([
      ["ahunderstrikeabcd", "Not connected", "28/01/2000", "Connect"],
      ["thunderstrikeabc", "Not connected", "27/01/2000", "Connect"],
      ["thunderstrikeab", "Connected", "26/01/2000", "Connect"],
      ["thunderstrike", "Not connected", "25/01/2000", "Connect"],
    ]);
  });

  it("Should disable the Connect button if the folder is already connected to a case", () => {
    render(
      <EgressSearchResults
        workspaceName="thunder"
        egressSearchApi={egressSearchApiResults}
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
      within(rows[2]).getByRole("button", {
        name: /Connect/i,
      }),
    ).toBeDisabled();

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
