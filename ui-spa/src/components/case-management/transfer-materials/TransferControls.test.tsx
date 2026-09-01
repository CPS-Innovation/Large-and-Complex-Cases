import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, vi, expect } from "vitest";
import TransferControls from "./TransferControls";

describe("TransferControls", () => {
  const defaultProps = {
    transferSource: "egress" as const,
    disableControls: false,
    toggleTransferDirection: vi.fn(),
    onCopy: vi.fn(),
  };

  it("renders the Move button when transferSource is egress and onMove is provided", () => {
    const onMove = vi.fn();
    render(<TransferControls {...defaultProps} onMove={onMove} />);

    expect(screen.getByText("Move selected")).toBeInTheDocument();
  });

  it("does not render the Move button when transferSource is egress but onMove is undefined", () => {
    render(<TransferControls {...defaultProps} onMove={undefined} />);

    expect(screen.queryByText("Move selected")).not.toBeInTheDocument();
  });

  it("does not render the Move button when transferSource is netapp even if onMove is provided", () => {
    const onMove = vi.fn();
    render(
      <TransferControls
        {...defaultProps}
        transferSource="netapp"
        onMove={onMove}
      />,
    );

    expect(screen.queryByText("Move selected")).not.toBeInTheDocument();
  });

  it("calls onMove when the Move button is clicked", () => {
    const onMove = vi.fn();
    render(<TransferControls {...defaultProps} onMove={onMove} />);

    userEvent.click(screen.getByText("Move selected"));
    expect(onMove).toHaveBeenCalledTimes(1);
  });

  it("disables the Move button when disableControls is true", () => {
    const onMove = vi.fn();
    render(
      <TransferControls {...defaultProps} disableControls={true} onMove={onMove} />,
    );

    expect(screen.getByText("Move selected")).toBeDisabled();
  });

  it("always renders the Copy button", () => {
    render(<TransferControls {...defaultProps} />);

    expect(screen.getByText("Copy selected")).toBeInTheDocument();
  });

  it("calls onCopy when the Copy button is clicked", () => {
    render(<TransferControls {...defaultProps} />);

    userEvent.click(screen.getByText("Copy selected"));
    expect(defaultProps.onCopy).toHaveBeenCalledTimes(1);
  });

  it("shows View Shared Drive link when transferSource is egress", () => {
    render(<TransferControls {...defaultProps} />);

    expect(screen.getByText("View Shared Drive")).toBeInTheDocument();
  });

  it("shows View Egress link when transferSource is netapp", () => {
    render(<TransferControls {...defaultProps} transferSource="netapp" />);

    expect(screen.getByText("View Egress")).toBeInTheDocument();
  });

  it("calls toggleTransferDirection when the direction link is clicked", () => {
    render(<TransferControls {...defaultProps} />);

    userEvent.click(screen.getByTestId("toggle-transfer-direction"));
    expect(defaultProps.toggleTransferDirection).toHaveBeenCalledTimes(1);
  });
});
