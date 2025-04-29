import { render, screen, within, fireEvent } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import FolderPath from "./FolderPath";

describe("FolderPath", () => {
  const folderClickHandlerMock = vi.fn();

  it("It renders the folder path for a given path adding Home folder with empty pathby default and fires the clickHandler with correct params", async () => {
    render(
      <FolderPath
        disabled={false}
        path="folder1/folder2"
        folderClickHandler={folderClickHandlerMock}
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
    expect(folderClickHandlerMock).toHaveBeenCalledTimes(1);
    expect(folderClickHandlerMock).toHaveBeenCalledWith("");
    fireEvent.click(screen.getByRole("button", { name: "folder1" }));
    expect(folderClickHandlerMock).toHaveBeenCalledTimes(2);
    expect(folderClickHandlerMock).toHaveBeenCalledWith("folder1/");
  });

  it("It renders the folder path for a given folder path", async () => {
    render(
      <FolderPath
        disabled={false}
        path="folder1/"
        folderClickHandler={folderClickHandlerMock}
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

  it("It should render correctly for empty path, Home folder should be added by default", async () => {
    render(
      <FolderPath
        disabled={false}
        path=""
        folderClickHandler={folderClickHandlerMock}
      />,
    );
    const list = screen.getByRole("list");
    const items = within(list).queryAllByRole("listitem");
    expect(items).toHaveLength(1);
    expect(
      screen.queryByRole("button", { name: "Home" }),
    ).not.toBeInTheDocument();
    expect(within(items[0]).getByText("Home")).toBeInTheDocument();
  });
});
