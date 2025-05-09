import { render, screen, within, fireEvent } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import FolderPath from "./FolderPath";

describe("FolderPath", () => {
  const handleFolderPathClickMock = vi.fn();

  it("It renders the folder path for given folders and fires the clickHandler with correct params", async () => {
    render(
      <FolderPath
        disabled={false}
        folders={[
          { folderName: "Home", folderPath: "", folderId: "1" },
          { folderName: "folder1", folderPath: "folder1/", folderId: "2" },
          {
            folderName: "folder2",
            folderPath: "folder1/folder2",
            folderId: "3",
          },
        ]}
        handleFolderPathClick={handleFolderPathClickMock}
      />,
    );
    const list = screen.getByRole("list");
    const items = within(list).getAllByRole("listitem");
    expect(items).toHaveLength(3);

    expect(
      within(items[0]).getByRole("button", { name: "Home" }),
    ).toBeInTheDocument();
    expect(
      within(items[1]).getByRole("button", { name: "folder1" }),
    ).toBeInTheDocument();
    expect(
      within(items[2]).queryByRole("button", { name: "folder2" }),
    ).not.toBeInTheDocument();
    expect(within(items[2]).getByText("folder2")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Home" }));
    expect(handleFolderPathClickMock).toHaveBeenCalledTimes(1);
    expect(handleFolderPathClickMock).toHaveBeenCalledWith("");
    fireEvent.click(screen.getByRole("button", { name: "folder1" }));
    expect(handleFolderPathClickMock).toHaveBeenCalledTimes(2);
    expect(handleFolderPathClickMock).toHaveBeenCalledWith("folder1/");
  });

  it("It renders the folder path for a given folders", async () => {
    render(
      <FolderPath
        disabled={false}
        folders={[
          { folderName: "Home", folderPath: "", folderId: "1" },
          { folderName: "folder1", folderPath: "folder1/", folderId: "2" },
        ]}
        handleFolderPathClick={handleFolderPathClickMock}
      />,
    );
    const list = screen.getByRole("list");
    const items = within(list).getAllByRole("listitem");
    expect(items).toHaveLength(2);
    expect(
      within(items[0]).getByRole("button", { name: "Home" }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole("button", { name: "folder1" }),
    ).not.toBeInTheDocument();
    expect(within(items[1]).getByText("folder1")).toBeInTheDocument();
  });

  it("It should not render any folder path items for empty folders list", async () => {
    render(
      <FolderPath
        disabled={false}
        folders={[]}
        handleFolderPathClick={handleFolderPathClickMock}
      />,
    );
    const list = screen.getByRole("list");
    const items = within(list).queryAllByRole("listitem");
    expect(items).toHaveLength(0);
  });
});
