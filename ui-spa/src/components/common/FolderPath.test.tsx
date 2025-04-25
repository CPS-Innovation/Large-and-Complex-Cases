import { render, screen, within, fireEvent } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import FolderPath from "./FolderPath";

describe("FolderPath", () => {
  const folderClickHandlerMock = vi.fn();

  it("It renders the folder path for a given path and fires the clickHandler with correct params", async () => {
    render(
      <FolderPath
        path="home/folder1/folder2"
        folderClickHandler={folderClickHandlerMock}
      />,
    );
    const list = screen.getByRole("list");
    const items = within(list).getAllByRole("listitem");
    expect(items).toHaveLength(3);

    expect(
      within(items[0]).getByRole("button", { name: "home" }),
    ).toBeInTheDocument();
    expect(
      within(items[1]).getByRole("button", { name: "folder1" }),
    ).toBeInTheDocument();
    expect(
      within(items[2]).queryByRole("button", { name: "folder2" }),
    ).not.toBeInTheDocument();
    expect(within(items[2]).getByText("folder2")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "home" }));
    expect(folderClickHandlerMock).toHaveBeenCalledTimes(1);
    expect(folderClickHandlerMock).toHaveBeenCalledWith("home");
    fireEvent.click(screen.getByRole("button", { name: "folder1" }));
    expect(folderClickHandlerMock).toHaveBeenCalledTimes(2);
    expect(folderClickHandlerMock).toHaveBeenCalledWith("home/folder1");
  });

  it("It renders the folder path for a given path ", async () => {
    render(
      <FolderPath path="home" folderClickHandler={folderClickHandlerMock} />,
    );
    const list = screen.getByRole("list");
    const items = within(list).getAllByRole("listitem");
    expect(items).toHaveLength(1);

    expect(
      screen.queryByRole("button", { name: "home" }),
    ).not.toBeInTheDocument();
    expect(within(items[0]).getByText("home")).toBeInTheDocument();
  });

  it("It should render correctly for empty path", async () => {
    render(<FolderPath path="" folderClickHandler={folderClickHandlerMock} />);
    const list = screen.getByRole("list");
    const items = within(list).queryAllByRole("listitem");
    expect(items).toHaveLength(0);
  });
});
