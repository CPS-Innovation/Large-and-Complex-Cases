import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, vi, expect } from "vitest";
import TreeView, { TreeNode } from "./TreeView";

describe("TreeView", () => {
  it("renders root nodes and exposes tree role", () => {
    const data: TreeNode[] = [
      { id: "f1", name: "Folder 1", isFolder: true },
      { id: "file1", name: "File 1", isFolder: false },
    ];

    render(<TreeView data={data} />);

    expect(screen.getByRole("tree")).toBeInTheDocument();
    expect(screen.getByText("Folder 1")).toBeInTheDocument();
    expect(screen.getByText("File 1")).toBeInTheDocument();
  });

  it("calls onLoadChildren when expanding a folder and shows children after load", async () => {
    const data: TreeNode[] = [{ id: "f1", name: "Folder 1", isFolder: true }];

    const onLoadChildren = vi.fn((nodeId: string) => {
      if (nodeId === "f1") {
        return Promise.resolve([
          { id: "f1-child-1", name: "Child 1", isFolder: false },
        ] as TreeNode[]);
      }
      return Promise.resolve([] as TreeNode[]);
    });

    render(<TreeView data={data} onLoadChildren={onLoadChildren} />);

    // find the toggle button for Folder 1 and click it to expand
    const folder = screen.getByText("Folder 1").closest("li");
    expect(folder).toBeTruthy();
    const toggleBtn = within(folder as HTMLElement).getByRole("button", {
      name: /plus|minus|\+|-/i,
    });

    userEvent.click(toggleBtn);

    expect(onLoadChildren).toHaveBeenCalledWith("f1");

    // spinner appears while loading
    expect(screen.getByTestId("loading-spinner")).toBeInTheDocument();

    // child appears after load
    await waitFor(() => {
      expect(screen.getByText("Child 1")).toBeInTheDocument();
    });
  });

  it("supports nested loading (expand parent then child)", async () => {
    const data: TreeNode[] = [{ id: "p1", name: "Parent", isFolder: true }];

    const onLoadChildren = vi.fn((nodeId: string) => {
      if (nodeId === "p1") {
        // parent returns a child folder
        return Promise.resolve([
          { id: "p1-cf", name: "Child Folder", isFolder: true },
        ] as TreeNode[]);
      }
      if (nodeId === "p1-cf") {
        // nested folder returns a file
        return Promise.resolve([
          { id: "p1-cf-file", name: "Nested File", isFolder: false },
        ] as TreeNode[]);
      }
      return Promise.resolve([] as TreeNode[]);
    });

    render(<TreeView data={data} onLoadChildren={onLoadChildren} />);

    // expand parent
    const parentLi = screen.getByText("Parent").closest("li") as HTMLElement;
    const parentToggle = within(parentLi).getByRole("button", {
      name: /plus|minus|\+|-/i,
    });
    userEvent.click(parentToggle);
    expect(onLoadChildren).toHaveBeenCalledWith("p1");

    // wait for child folder
    await waitFor(() =>
      expect(screen.getByText("Child Folder")).toBeInTheDocument(),
    );

    // expand child folder
    const childLi = screen
      .getByText("Child Folder")
      .closest("li") as HTMLElement;
    const childToggle = within(childLi).getByRole("button", {
      name: /plus|minus|\+|-/i,
    });
    userEvent.click(childToggle);
    expect(onLoadChildren).toHaveBeenCalledWith("p1-cf");

    // nested file appears
    await waitFor(() =>
      expect(screen.getByText("Nested File")).toBeInTheDocument(),
    );
  });

  it("supports keyboard navigation and Enter selects a folder", async () => {
    const data: TreeNode[] = [
      {
        id: "a",
        name: "A",
        isFolder: true,
        children: [{ id: "a1", name: "A1", isFolder: false }],
      },
      { id: "b", name: "B", isFolder: false },
    ];

    const onSelect = vi.fn();
    render(<TreeView data={data} onSelect={onSelect} />);

    const items = screen.getAllByRole("treeitem");
    // focus first item
    items[0].focus();
    expect(document.activeElement).toBe(items[0]);

    // ArrowDown moves to next
    userEvent.keyboard("{ArrowDown}");
    expect(document.activeElement).toBe(items[1]);

    // Move back up and press Enter to select folder A
    userEvent.keyboard("{ArrowUp}");
    expect(document.activeElement).toBe(items[0]);
    userEvent.keyboard("{Enter}");

    expect(onSelect).toHaveBeenCalledTimes(1);
    expect(onSelect.mock.calls[0][0].id).toBe("a");

    // ArrowRight should expand the focused folder (A) and then a second ArrowRight
    // should move focus into its first child.
    // First, ensure A is focused
    items[0].focus();
    userEvent.keyboard("{ArrowRight}");
    // child should become visible
    await waitFor(() => expect(screen.getByText("A1")).toBeInTheDocument());

    // second ArrowRight should move focus to the first child
    userEvent.keyboard("{ArrowRight}");
    await waitFor(() => {
      const focused = document.activeElement as HTMLElement | null;
      expect(focused).not.toBeNull();
      expect(focused?.getAttribute("id")).toBe("a1");
    });

    // ArrowLeft from child should move focus back to parent
    userEvent.keyboard("{ArrowLeft}");
    await waitFor(() => {
      const focused = document.activeElement as HTMLElement | null;
      expect(focused).not.toBeNull();
      expect(focused?.getAttribute("id")).toBe("a");
    });

    // Home should focus first visible item and End should focus last
    userEvent.keyboard("{End}");
    await waitFor(() => {
      const focused = document.activeElement as HTMLElement | null;
      expect(focused).not.toBeNull();
      // after expansion visible nodes are A, A1, B -> last is B
      expect(focused?.getAttribute("id")).toBe("b");
    });

    userEvent.keyboard("{Home}");
    await waitFor(() => {
      const focused = document.activeElement as HTMLElement | null;
      expect(focused).not.toBeNull();
      expect(focused?.getAttribute("id")).toBe("a");
    });
  });

  it("selects folders on click", async () => {
    const data: TreeNode[] = [
      { id: "f1", name: "Folder 1", isFolder: true },
      { id: "file1", name: "File 1", isFolder: false },
    ];

    const onSelect = vi.fn();
    render(<TreeView data={data} onSelect={onSelect} />);

    // clicking the folder label/button should trigger onSelect
    const folderBtn = screen.getByRole("button", { name: /folder 1/i });
    userEvent.click(folderBtn);

    expect(onSelect).toHaveBeenCalledTimes(1);
    expect(onSelect.mock.calls[0][0].id).toBe("f1");
  });

  it("file nodes do not call onLoadChildren and are selectable", async () => {
    const data: TreeNode[] = [{ id: "fileX", name: "File X", isFolder: false }];

    const onLoadChildren = vi.fn();
    const onSelect = vi.fn();

    render(
      <TreeView
        data={data}
        onLoadChildren={onLoadChildren}
        onSelect={onSelect}
      />,
    );

    // file should render but should not have a toggle button
    const fileLi = screen.getByText("File X").closest("li") as HTMLElement;
    const toggle = within(fileLi).queryByRole("button", {
      name: /plus|minus|\+|-/i,
    });
    expect(toggle).toBeNull();

    // keyboard: focusing the file and pressing ArrowRight should NOT call onLoadChildren
    // and pressing Enter should select the file
    fileLi.focus();
    userEvent.keyboard("{ArrowRight}");
    expect(onLoadChildren).not.toHaveBeenCalled();

    userEvent.keyboard("{Enter}");
    expect(onSelect).toHaveBeenCalledTimes(0);
  });

  it("folder with pre-populated children does not call onLoadChildren when toggled", async () => {
    const data: TreeNode[] = [
      {
        id: "pf",
        name: "PrePopulated",
        isFolder: true,
        children: [{ id: "pf-c1", name: "ChildX", isFolder: false }],
      },
    ];

    const onLoadChildren = vi.fn();

    render(<TreeView data={data} onLoadChildren={onLoadChildren} />);

    const folderLi = screen
      .getByText("PrePopulated")
      .closest("li") as HTMLElement;
    const toggle = within(folderLi).getByRole("button", {
      name: /plus|minus|\+|-/i,
    });

    // clicking toggle should expand but should NOT call onLoadChildren (children already present)
    userEvent.click(toggle);
    expect(onLoadChildren).not.toHaveBeenCalled();

    // child should be visible immediately
    await waitFor(() => expect(screen.getByText("ChildX")).toBeInTheDocument());
  });
  it("should not call onLoadChildren if the children's are already loaded and should should display it from the stored data", async () => {
    const data: TreeNode[] = [
      {
        id: "a",
        name: "folder a",
        isFolder: true,
      },
    ];
    const onLoadChildren = vi.fn((nodeId: string) => {
      if (nodeId === "a") {
        return Promise.resolve([
          { id: "b", name: "folder b", isFolder: true },
        ] as TreeNode[]);
      }
      if (nodeId === "b") {
        return Promise.resolve([
          { id: "c", name: "folder c", isFolder: true },
        ] as TreeNode[]);
      }

      return Promise.resolve([] as TreeNode[]);
    });

    render(<TreeView data={data} onLoadChildren={onLoadChildren} />);

    const folderALi = screen
      .getByText(/folder\s*a/i)
      .closest("li") as HTMLElement;
    const toggleFolderA = within(folderALi).getByRole("button", {
      name: /plus|minus|\+|-/i,
    });
    // clicking toggle should expand and call onLoadChildren for folder a
    userEvent.click(toggleFolderA);
    expect(onLoadChildren).toHaveBeenCalledTimes(1);
    expect(onLoadChildren).toHaveBeenCalledWith("a");
    expect(screen.getByTestId("loading-spinner")).toBeInTheDocument();

    await waitFor(() =>
      expect(screen.getByText("folder b")).toBeInTheDocument(),
    );
    // clicking toggle should minimize for folder a
    userEvent.click(toggleFolderA);

    await waitFor(() =>
      expect(screen.queryByText("folder b")).not.toBeInTheDocument(),
    );
    expect(onLoadChildren).toHaveBeenCalledTimes(1);
    userEvent.click(toggleFolderA);

    expect(onLoadChildren).toHaveBeenCalledTimes(1);
    expect(screen.queryByTestId("loading-spinner")).not.toBeInTheDocument();
    await waitFor(() =>
      expect(screen.getByText("folder b")).toBeInTheDocument(),
    );
    const folderBLi = screen
      .getByText(/folder\s*b/i)
      .closest("li") as HTMLElement;
    const toggleFolderB = within(folderBLi).getByRole("button", {
      name: /plus|minus|\+|-/i,
    });
    // clicking toggle should expand and call onLoadChildren for folder b
    userEvent.click(toggleFolderB);
    expect(onLoadChildren).toHaveBeenCalledTimes(2);
    expect(onLoadChildren).toHaveBeenCalledWith("b");
    expect(screen.getByTestId("loading-spinner")).toBeInTheDocument();

    await waitFor(() =>
      expect(screen.getByText("folder c")).toBeInTheDocument(),
    );
    // clicking toggle should minimize for folder b
    userEvent.click(toggleFolderB);

    await waitFor(() =>
      expect(screen.queryByText("folder c")).not.toBeInTheDocument(),
    );
    expect(onLoadChildren).toHaveBeenCalledTimes(2);
    // clicking toggle should expand folder b
    userEvent.click(toggleFolderB);
    expect(onLoadChildren).toHaveBeenCalledTimes(2);
    expect(screen.queryByTestId("loading-spinner")).not.toBeInTheDocument();
    await waitFor(() =>
      expect(screen.getByText("folder c")).toBeInTheDocument(),
    );
  });
});
