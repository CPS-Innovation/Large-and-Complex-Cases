import { render, screen, within, fireEvent } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { SortableTable } from "./SortableTable";

describe("Sortable Table Component", () => {
  const handleTableSortMock = vi.fn();
  beforeEach(() => {
    const rowData = [
      {
        cells: [
          { children: <span>alex</span> },
          { children: <span>active</span> },
          { children: <span>1234</span> },
        ],
      },
      {
        cells: [
          { children: <span>bob</span> },
          { children: <span>inactive</span> },
          { children: <span>879</span> },
        ],
      },
      {
        cells: [
          { children: <span>eric</span> },
          { children: <span>inactive</span> },
          { children: <span>567</span> },
        ],
      },
      {
        cells: [
          { children: <span>supra</span> },
          { children: <span>active</span> },
          { children: <span>467</span> },
        ],
      },
    ];

    render(
      <SortableTable
        caption="Case search result table"
        captionClassName="govuk-visually-hidden"
        head={[
          {
            children: "Name",
            sortable: true,
            sortName: "name",
          },
          {
            children: "Status",
            sortable: false,
          },

          {
            children: "Id",
            sortable: true,
            sortName: "id",
          },
        ]}
        rows={rowData}
        handleTableSort={handleTableSortMock}
      />,
    );
  });
  afterEach(() => {
    vi.resetAllMocks();
  });
  it("renders a table with given data", async () => {
    const table = screen.getByRole("table");
    expect(table).toBeInTheDocument();

    const headers = screen.getAllByRole("columnheader");
    expect(headers).toHaveLength(3);
    expect(headers[0]).toHaveTextContent("Name");
    expect(headers[1]).toHaveTextContent("Status");
    expect(headers[2]).toHaveTextContent("Id");
    const rows = screen.getAllByRole("row");
    const dataRows = rows.slice(1);

    const rowValues = dataRows.map((row) =>
      within(row)
        .getAllByRole("cell")
        .map((cell) => cell.textContent),
    );

    expect(screen.getByRole("button", { name: "Name" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Id" })).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Status" }),
    ).not.toBeInTheDocument();

    expect(rowValues).toEqual([
      ["alex", "active", "1234"],
      ["bob", "inactive", "879"],
      ["eric", "inactive", "567"],
      ["supra", "active", "467"],
    ]);
  });
  it("Should show the sort icons correctly and buttons correctly", () => {
    const btnName = screen.getByRole("button", { name: "Name" });
    const btnId = screen.getByRole("button", { name: "Id" });
    expect(screen.getByRole("button", { name: "Name" })).toBeInTheDocument();
    expect(within(btnName).getByTestId("sort-icon")).toBeInTheDocument();
    expect(
      within(btnName).queryByTestId("arrow-up-icon"),
    ).not.toBeInTheDocument();
    expect(
      within(btnName).queryByTestId("arrow-down-icon"),
    ).not.toBeInTheDocument();
    expect(btnId).toBeInTheDocument();
    expect(within(btnId).getByTestId("sort-icon")).toBeInTheDocument();
    expect(
      within(btnId).queryByTestId("arrow-up-icon"),
    ).not.toBeInTheDocument();
    expect(
      within(btnId).queryByTestId("arrow-down-icon"),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Status" }),
    ).not.toBeInTheDocument();
  });

  it("should show the sort icons correctly and call the sort handler function with correct params", () => {
    const btnName = screen.getByRole("button", { name: "Name" });
    const btnId = screen.getByRole("button", { name: "Id" });
    fireEvent.click(btnName);
    expect(handleTableSortMock).toHaveBeenCalledTimes(1);
    expect(handleTableSortMock).toHaveBeenCalledWith("name", "ascending");
    expect(within(btnName).getByTestId("arrow-up-icon")).toBeInTheDocument();
    expect(within(btnName).queryByTestId("sort-icon")).not.toBeInTheDocument();
    fireEvent.click(btnName);
    expect(handleTableSortMock).toHaveBeenCalledTimes(2);
    expect(handleTableSortMock).toHaveBeenCalledWith("name", "descending");
    expect(within(btnName).getByTestId("arrow-down-icon")).toBeInTheDocument();

    fireEvent.click(btnId);
    expect(handleTableSortMock).toHaveBeenCalledTimes(3);
    expect(handleTableSortMock).toHaveBeenCalledWith("id", "ascending");
    expect(within(btnId).getByTestId("arrow-up-icon")).toBeInTheDocument();
    expect(
      within(btnName).queryByTestId("arrow-down-icon"),
    ).not.toBeInTheDocument();
    expect(within(btnName).getByTestId("sort-icon")).toBeInTheDocument();
    fireEvent.click(btnId);
    expect(handleTableSortMock).toHaveBeenCalledTimes(4);
    expect(handleTableSortMock).toHaveBeenCalledWith("id", "descending");
    expect(within(btnId).getByTestId("arrow-down-icon")).toBeInTheDocument();
    fireEvent.click(btnId);
    expect(handleTableSortMock).toHaveBeenCalledTimes(5);
    expect(handleTableSortMock).toHaveBeenCalledWith("id", "ascending");
    expect(within(btnId).getByTestId("arrow-up-icon")).toBeInTheDocument();
    fireEvent.click(btnName);
    expect(handleTableSortMock).toHaveBeenCalledTimes(6);
    expect(handleTableSortMock).toHaveBeenCalledWith("name", "ascending");
    expect(within(btnId).getByTestId("sort-icon")).toBeInTheDocument();
    expect(within(btnName).getByTestId("arrow-up-icon")).toBeInTheDocument();
    fireEvent.click(btnId);
    expect(handleTableSortMock).toHaveBeenCalledTimes(7);
    expect(handleTableSortMock).toHaveBeenCalledWith("id", "ascending");
    expect(within(btnId).getByTestId("arrow-up-icon")).toBeInTheDocument();
    expect(
      within(btnName).queryByTestId("arrow-up-icon"),
    ).not.toBeInTheDocument();
    expect(within(btnName).getByTestId("sort-icon")).toBeInTheDocument();
  });
});
