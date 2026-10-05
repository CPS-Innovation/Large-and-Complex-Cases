import { render, screen, within } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { MemoryRouter } from "react-router";
import NetAppFolderResultsPage from "./NetAppFolderResultsPage";
import { type ConnectNetAppFolderData } from "../../schemas";

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

describe("NetAppFolderResultsPage", () => {
  const handleConnectFolderMock = vi.fn();
  const handleGetFolderContentMock = vi.fn();

  const renderPage = ({
    rootFolderPath = "",
    netAppFolderResults,
    accessibleRoots,
  }: {
    rootFolderPath?: string;
    netAppFolderResults: ConnectNetAppFolderData;
    accessibleRoots?: string[] | null;
  }) =>
    render(
      <MemoryRouter>
        <NetAppFolderResultsPage
          backLinkUrl="/search-results"
          rootFolderPath={rootFolderPath}
          netAppFolderResults={netAppFolderResults}
          isNetAppFolderResultsLoading={false}
          accessibleRoots={accessibleRoots}
          handleConnectFolder={handleConnectFolderMock}
          handleGetFolderContent={handleGetFolderContentMock}
        />
      </MemoryRouter>,
    );

  it("shows the standard permissions guidance when the user can list the bucket root", () => {
    renderPage({
      netAppFolderResults: {
        rootPath: "",
        folders: [{ folderPath: "thunderstrike/", caseId: null }],
      },
    });

    expect(
      screen.queryByTestId("restricted-root-text"),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText(/If the folder you need is not listed/),
    ).toBeInTheDocument();
  });

  it("shows the restricted access message when only entry prefixes are returned", () => {
    renderPage({
      netAppFolderResults: {
        rootPath: "",
        folders: [
          { folderPath: "RCF/", caseId: null },
          { folderPath: "RCW/Cardiff/", caseId: null },
        ],
        isRestrictedRoot: true,
        accessibleRoots: ["RCF/", "RCW/Cardiff/"],
      },
      accessibleRoots: ["RCF/", "RCW/Cardiff/"],
    });

    expect(screen.getByTestId("restricted-root-text")).toBeInTheDocument();
    expect(
      screen.queryByText(/If the folder you need is not listed/),
    ).not.toBeInTheDocument();
  });

  it("makes every breadcrumb above the current folder clickable when the user is unrestricted", () => {
    renderPage({
      rootFolderPath: "RCW/Cardiff/Case123/",
      netAppFolderResults: { rootPath: "RCW/Cardiff/Case123/", folders: [] },
    });

    const items = within(screen.getByTestId("folder-path")).getAllByRole(
      "listitem",
    );

    expect(
      within(items[0]).getByRole("button", { name: "Home" }),
    ).toBeInTheDocument();
    expect(
      within(items[1]).getByRole("button", { name: "RCW" }),
    ).toBeInTheDocument();
    expect(
      within(items[2]).getByRole("button", { name: "Cardiff" }),
    ).toBeInTheDocument();
    expect(
      within(items[3]).queryByRole("button", { name: "Case123" }),
    ).not.toBeInTheDocument();
  });

  it("does not let a restricted user navigate above their accessible root", () => {
    renderPage({
      rootFolderPath: "RCW/Cardiff/Case123/",
      netAppFolderResults: { rootPath: "RCW/Cardiff/Case123/", folders: [] },
      accessibleRoots: ["RCF/", "RCW/Cardiff/"],
    });

    const items = within(screen.getByTestId("folder-path")).getAllByRole(
      "listitem",
    );

    // Home re-lists the root, which returns the entry prefixes again, so it stays clickable.
    expect(
      within(items[0]).getByRole("button", { name: "Home" }),
    ).toBeInTheDocument();
    // RCW/ sits above the accessible root, so it is shown for context but is not clickable.
    expect(
      within(items[1]).queryByRole("button", { name: "RCW" }),
    ).not.toBeInTheDocument();
    expect(within(items[1]).getByText("RCW")).toBeInTheDocument();
    expect(
      within(items[2]).getByRole("button", { name: "Cardiff" }),
    ).toBeInTheDocument();
  });
});
