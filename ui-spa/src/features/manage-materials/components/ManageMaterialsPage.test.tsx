import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, vi, expect, beforeEach } from "vitest";
import { MemoryRouter } from "react-router";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import ManageMaterialsPage from "./ManageMaterialsPage";
import { getNetAppFolders } from "../../../apis/gateway-api";

vi.mock("../../../apis/gateway-api", () => ({
  getNetAppFolders: vi.fn(),
}));

const rootPath = "Thunderstruck/";

const renderPage = () =>
  render(
    <QueryClientProvider client={new QueryClient()}>
      <MemoryRouter>
        <ManageMaterialsPage
          isTabActive={true}
          caseId="12"
          operationName="Thunderstruck"
          netAppPath={rootPath}
        />
      </MemoryRouter>
    </QueryClientProvider>,
  );

const getCheckbox = (name: string) =>
  screen.getByRole("checkbox", { name }) as HTMLInputElement;

describe("ManageMaterialsPage", () => {
  beforeEach(() => {
    vi.mocked(getNetAppFolders).mockImplementation(async (path: string) => {
      if (path === rootPath)
        return {
          folderData: [
            { path: `${rootPath}1. Case management/` },
            { path: `${rootPath}2. Experts/` },
          ],
          fileData: [
            {
              path: `${rootPath}statement.pdf`,
              filesize: 1024,
              lastModified: "2024-08-22T00:00:00Z",
            },
          ],
        };
      return { folderData: [], fileData: [] };
    });
  });

  it("displays each material on its own row with a checkbox, folder icon and name", async () => {
    renderPage();

    expect(
      screen.getByRole("heading", {
        name: "Manage materials on the shared drive",
      }),
    ).toBeInTheDocument();

    const table = await screen.findByTestId("materials-table");
    const rows = within(table).getAllByRole("row").slice(1);
    expect(rows).toHaveLength(3);

    expect(within(rows[0]).getByRole("checkbox")).toBeInTheDocument();
    expect(within(rows[0]).getByTestId("folder-icon")).toBeInTheDocument();
    expect(within(rows[0]).getByText("1. Case management")).toBeInTheDocument();

    expect(within(rows[2]).getByRole("checkbox")).toBeInTheDocument();
    expect(within(rows[2]).queryByTestId("folder-icon")).toBeNull();
    expect(within(rows[2]).getByText("statement.pdf")).toBeInTheDocument();

    expect(
      screen.getByTestId("manage-materials-breadcrumbs"),
    ).toHaveTextContent("Shared Drive: Thunderstruck");
  });

  it("selects single and multiple items, and deselects without affecting the rest", async () => {
    renderPage();
    await screen.findByTestId("materials-table");

    await userEvent.click(getCheckbox("Select folder 1. Case management"));
    expect(getCheckbox("Select folder 1. Case management").checked).toBe(true);
    expect(getCheckbox("Select folder 2. Experts").checked).toBe(false);

    await userEvent.click(getCheckbox("Select file statement.pdf"));
    expect(getCheckbox("Select folder 1. Case management").checked).toBe(true);
    expect(getCheckbox("Select file statement.pdf").checked).toBe(true);

    await userEvent.click(getCheckbox("Select folder 1. Case management"));
    expect(getCheckbox("Select folder 1. Case management").checked).toBe(false);
    expect(getCheckbox("Select file statement.pdf").checked).toBe(true);
  });

  it("selects and deselects all visible items using the header checkbox", async () => {
    renderPage();
    await screen.findByTestId("materials-table");

    await userEvent.click(getCheckbox("Select all files and folders"));
    screen
      .getAllByRole("checkbox")
      .forEach((checkbox) =>
        expect((checkbox as HTMLInputElement).checked).toBe(true),
      );

    await userEvent.click(getCheckbox("Select folder 2. Experts"));
    expect(getCheckbox("Select all files and folders").checked).toBe(false);
    expect(getCheckbox("Select folder 1. Case management").checked).toBe(true);

    await userEvent.click(getCheckbox("Select folder 2. Experts"));
    expect(getCheckbox("Select all files and folders").checked).toBe(true);

    await userEvent.click(getCheckbox("Select all files and folders"));
    screen
      .getAllByRole("checkbox")
      .forEach((checkbox) =>
        expect((checkbox as HTMLInputElement).checked).toBe(false),
      );
  });

  it("opens a folder and clears the selection", async () => {
    renderPage();
    await screen.findByTestId("materials-table");

    await userEvent.click(getCheckbox("Select file statement.pdf"));
    await userEvent.click(
      screen.getByRole("button", { name: "1. Case management" }),
    );

    expect(await screen.findByTestId("no-materials-text")).toBeInTheDocument();
    expect(getNetAppFolders).toHaveBeenCalledWith(
      `${rootPath}1. Case management/`,
    );
    expect(
      screen.getByTestId("manage-materials-breadcrumbs"),
    ).toHaveTextContent("1. Case management");

    await userEvent.click(
      screen.getByRole("button", { name: "Shared Drive: Thunderstruck" }),
    );
    expect(
      (await screen.findByRole("checkbox", {
        name: "Select file statement.pdf",
      })) as HTMLInputElement,
    ).not.toBeChecked();
  });
});
