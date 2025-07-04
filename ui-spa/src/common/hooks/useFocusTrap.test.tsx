import React, { useState } from "react";
import { vi } from "vitest";
import { render, screen, fireEvent, act } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { useFocusTrap } from "./useFocusTrap";

describe("useFocusTrap hook", () => {
  const testSetUp = () => {
    vi.useFakeTimers();
    const TestComponent = () => {
      const [showModal, setShowModal] = useState(false);

      return (
        <div>
          <button onClick={() => setShowModal(true)}>Open Modal</button>
          {showModal && <ModalComponent handleClose={setShowModal} />}
        </div>
      );
    };

    type ModalComponentProps = {
      handleClose: (value: boolean) => void;
    };
    const ModalComponent: React.FC<ModalComponentProps> = ({ handleClose }) => {
      useFocusTrap();
      return (
        <div id="modal">
          <button onClick={() => handleClose(false)}>Close Modal</button>
          <a href="abc">testLink 1</a>
          <a href="abc1">testLink 2</a>
        </div>
      );
    };

    render(<TestComponent />);
  };

  it("Should trap the focus within the Modal for tab and shift+tab keypress", async () => {
    testSetUp();

    const openModalButtonElement = screen.getByText("Open Modal");
    openModalButtonElement.focus();
    expect(openModalButtonElement).toHaveFocus();
    fireEvent.click(openModalButtonElement);
    vi.advanceTimersByTime(10);
    //tab case
    expect(screen.getByText("Close Modal")).toHaveFocus();
    act(() => userEvent.tab());
    await expect(screen.getByText("testLink 1")).toHaveFocus();
    act(() => userEvent.tab());
    expect(screen.getByText("testLink 2")).toHaveFocus();
    act(() => userEvent.tab());
    expect(screen.getByText("Close Modal")).toHaveFocus();
    act(() => userEvent.tab());

    // //shift+tab case
    expect(screen.getByText("testLink 1")).toHaveFocus();
    act(() => userEvent.tab({ shift: true }));
    expect(screen.getByText("Close Modal")).toHaveFocus();
    act(() => userEvent.tab({ shift: true }));
    expect(screen.getByText("testLink 2")).toHaveFocus();
    act(() => userEvent.tab({ shift: true }));
    expect(screen.getByText("testLink 1")).toHaveFocus();
    act(() => userEvent.tab({ shift: true }));
    expect(screen.getByText("Close Modal")).toHaveFocus();
  });
});
