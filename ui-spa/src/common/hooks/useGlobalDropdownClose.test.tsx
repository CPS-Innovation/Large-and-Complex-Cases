/* eslint-disable react-hooks/rules-of-hooks */
import { renderHook, screen, fireEvent } from "@testing-library/react";
import { useRef } from "react";
import { useGlobalDropdownClose } from "./useGlobalDropdownClose";

test("click outside panel closes dropdown panel", () => {
  const setOpen = vi.fn();

  renderHook(() => 0, {
    wrapper: () => {
      const btnRef = useRef(null);
      const panelRef = useRef(null);
      return (
        <>
          <button ref={btnRef} data-testid="btn">
            Btn
          </button>
          <button ref={panelRef} data-testid="panel">
            Panel
          </button>
          {useGlobalDropdownClose(btnRef, panelRef, setOpen, "panel")}
        </>
      );
    },
  });

  fireEvent.click(document.body);
  expect(setOpen).toHaveBeenCalledWith(false);
});

test("should not close the dropdown panel, if clicked outside the panel, but on the dropdown btn", () => {
  const setOpen = vi.fn();

  renderHook(() => 0, {
    wrapper: () => {
      const btnRef = useRef(null);
      const panelRef = useRef(null);
      return (
        <>
          <button ref={btnRef} data-testid="btn">
            Btn
          </button>
          <button ref={panelRef} data-testid="panel">
            Panel
          </button>
          {useGlobalDropdownClose(btnRef, panelRef, setOpen, "panel")}
        </>
      );
    },
  });
  const btn = screen.getByTestId("btn");
  fireEvent.click(btn);
  expect(setOpen).not.toHaveBeenCalled();
});

test("click inside panel does not close the panel", () => {
  const setOpen = vi.fn();

  renderHook(() => 0, {
    wrapper: () => {
      const btnRef = useRef(null);
      const panelRef = useRef(null);
      return (
        <>
          <button ref={btnRef} data-testid="btn">
            Btn
          </button>
          <div ref={panelRef} data-testid="panel">
            Panel
            <button data-testid="panel-child-btn"></button>
          </div>
          {useGlobalDropdownClose(btnRef, panelRef, setOpen, "panel")}
        </>
      );
    },
  });

  const panel = screen.getByTestId("panel");
  const panelChildBtn = screen.getByTestId("panel-child-btn");
  fireEvent.click(panel);
  expect(setOpen).not.toHaveBeenCalled();
  fireEvent.click(panelChildBtn);
  expect(setOpen).not.toHaveBeenCalled();
});

test("Escape key closes and return focus", () => {
  const setOpen = vi.fn();
  const btnFocus = vi.fn();
  vi.spyOn(HTMLElement.prototype, "focus").mockImplementation(btnFocus);

  renderHook(() => 0, {
    wrapper: () => {
      const btnRef = useRef(null);
      const panelRef = useRef(null);
      return (
        <>
          <button ref={btnRef} data-testid="btn">
            Btn
          </button>
          <div ref={panelRef} data-testid="panel">
            Panel
            <button data-testid="panel-child-btn"></button>
          </div>
          {useGlobalDropdownClose(btnRef, panelRef, setOpen, "panel")}
        </>
      );
    },
  });

  fireEvent.keyDown(window, { code: "Escape" });
  expect(setOpen).toHaveBeenCalledWith(false);
  expect(btnFocus).toHaveBeenCalled();
});
