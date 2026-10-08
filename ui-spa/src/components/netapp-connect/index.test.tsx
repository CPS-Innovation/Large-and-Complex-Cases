import { render, screen, waitFor, within } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Routes, Route } from "react-router";
import { type MainState } from "../../reducers/mainStateReducer";

// Mocking the button component as the original is using webpackPrefetch when loading the button
// from govukfrontend, which vitest cannot handle.
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

const mockGetConnectNetAppFolders = vi.fn();
vi.mock("../../apis/gateway-api", () => ({
  getConnectNetAppFolders: (...args: unknown[]) =>
    mockGetConnectNetAppFolders(...args),
}));

import NetAppPage from "./index";
import { MainStateContext } from "../../providers/MainStateProvider";

describe("NetAppPage", () => {
  type ConnectSharedDrivePage = MainState["appData"]["connectSharedDrivePage"];

  const renderPage = (connectSharedDrivePage: ConnectSharedDrivePage) =>
    render(
      <QueryClientProvider
        client={
          new QueryClient({ defaultOptions: { queries: { retry: false } } })
        }
      >
        <MainStateContext.Provider
          value={{
            state: { appData: { connectSharedDrivePage } } as MainState,
            dispatch: vi.fn(),
          }}
        >
          <MemoryRouter
            initialEntries={["/case/12/netapp-connect?operation-name=opName"]}
          >
            <Routes>
              <Route
                path="/case/:caseId/netapp-connect"
                element={<NetAppPage />}
              />
            </Routes>
          </MemoryRouter>
        </MainStateContext.Provider>
      </QueryClientProvider>,
    );

  beforeEach(() => {
    mockGetConnectNetAppFolders.mockReset();
  });

  it("keeps breadcrumbs above the accessible root unclickable when resuming mid-tree", async () => {
    // Returning from the confirmation page remounts the picker deep in the tree, where the API no
    // longer reports the restriction, so the roots must come from state.
    mockGetConnectNetAppFolders.mockResolvedValue({
      rootPath: "RCW/Cardiff/Case123/",
      folders: [],
    });

    renderPage({
      searchQueryString: "",
      netappRootFolderPath: "RCW/Cardiff/Case123/",
      accessibleRoots: ["RCF/", "RCW/Cardiff/"],
    });

    await waitFor(() => expect(mockGetConnectNetAppFolders).toHaveBeenCalled());

    const items = within(screen.getByTestId("folder-path")).getAllByRole(
      "listitem",
    );

    expect(
      within(items[0]).getByRole("button", { name: "Home" }),
    ).toBeInTheDocument();
    expect(
      within(items[1]).queryByRole("button", { name: "RCW" }),
    ).not.toBeInTheDocument();
    expect(within(items[1]).getByText("RCW")).toBeInTheDocument();
    expect(
      within(items[2]).getByRole("button", { name: "Cardiff" }),
    ).toBeInTheDocument();
  });

  it("leaves every breadcrumb clickable when no restriction was persisted", async () => {
    mockGetConnectNetAppFolders.mockResolvedValue({
      rootPath: "RCW/Cardiff/Case123/",
      folders: [],
    });

    renderPage({
      searchQueryString: "",
      netappRootFolderPath: "RCW/Cardiff/Case123/",
      accessibleRoots: null,
    });

    await waitFor(() => expect(mockGetConnectNetAppFolders).toHaveBeenCalled());

    const items = within(screen.getByTestId("folder-path")).getAllByRole(
      "listitem",
    );

    expect(
      within(items[1]).getByRole("button", { name: "RCW" }),
    ).toBeInTheDocument();
    expect(
      within(items[2]).getByRole("button", { name: "Cardiff" }),
    ).toBeInTheDocument();
  });
});
