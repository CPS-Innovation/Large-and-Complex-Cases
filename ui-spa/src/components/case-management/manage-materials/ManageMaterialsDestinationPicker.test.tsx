import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, vi, expect, beforeEach } from "vitest";
import ManageMaterialsDestinationPicker from "./ManageMaterialsDestinationPicker";
import { getNetAppFolders } from "../../../apis/gateway-api";

vi.mock("../../../apis/gateway-api", () => ({
  getNetAppFolders: vi.fn(),
}));

const mockGetNetAppFolders = vi.mocked(getNetAppFolders);

const netAppPath = "/case/";

const renderPicker = (
  lockedPaths: string[],
  conflictError: string | null = null,
) =>
  render(
    <ManageMaterialsDestinationPicker
      netAppPath={netAppPath}
      operationName="Operation Name"
      action="copy"
      selectedCount={2}
      conflictError={conflictError}
      lockedPaths={lockedPaths}
      onConfirm={vi.fn()}
      onCancel={vi.fn()}
    />,
  );

const lockedBannerText =
  /a copy or move operation is in progress for this case\. some folders are locked/i;

const expandRoot = async () => {
  const rootLi = screen
    .getByText(/Shared Drive: Operation Name/i)
    .closest("li") as HTMLElement;
  const toggle = within(rootLi).getByRole("button", {
    name: /plus|minus|\+|-/i,
  });
  userEvent.click(toggle);
  await waitFor(() =>
    expect(screen.getByText("Interviews")).toBeInTheDocument(),
  );
};

describe("ManageMaterialsDestinationPicker", () => {
  beforeEach(() => {
    mockGetNetAppFolders.mockReset();
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    mockGetNetAppFolders.mockResolvedValue({
      folderData: [
        { path: "/case/Interviews/" },
        { path: "/case/Exhibits/" },
      ],
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);
  });

  it("disables selection of folders that overlap a locked path", async () => {
    renderPicker(["/case/Interviews/"]);
    await expandRoot();

    const lockedBtn = screen.getByRole("button", { name: /interviews/i });
    expect(lockedBtn).toHaveAttribute("aria-disabled", "true");

    const selectableBtn = screen.getByRole("button", { name: /exhibits/i });
    expect(selectableBtn).not.toHaveAttribute("aria-disabled");
  });

  it("does not enable the confirm button when a locked folder is clicked", async () => {
    renderPicker(["/case/Interviews/"]);
    await expandRoot();

    userEvent.click(screen.getByRole("button", { name: /interviews/i }));

    const confirmBtn = screen.getByRole("button", { name: /^copy$/i });
    expect(confirmBtn).toBeDisabled();
  });

  it("enables the confirm button when a selectable folder is clicked", async () => {
    renderPicker(["/case/Interviews/"]);
    await expandRoot();

    userEvent.click(screen.getByRole("button", { name: /exhibits/i }));

    await waitFor(() =>
      expect(
        screen.getByRole("button", { name: /copy to exhibits/i }),
      ).toBeEnabled(),
    );
  });

  it("shows the locked-operation banner when folders are locked", () => {
    renderPicker(["/case/Interviews/"]);
    expect(screen.getByText(lockedBannerText)).toBeInTheDocument();
  });

  it("does not show the locked-operation banner when nothing is locked", () => {
    renderPicker([]);
    expect(screen.queryByText(lockedBannerText)).not.toBeInTheDocument();
  });

  it("hides the locked-operation banner while a conflict error is shown", () => {
    renderPicker(["/case/Interviews/"], "A conflicting operation is in progress.");
    expect(screen.queryByText(lockedBannerText)).not.toBeInTheDocument();
    expect(
      screen.getByText(/a conflicting operation is in progress/i),
    ).toBeInTheDocument();
  });
});
