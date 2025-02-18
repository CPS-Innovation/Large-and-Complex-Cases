import { render, screen } from "@testing-library/react";
import Header from "../header";

describe("Header Component", () => {
  it("renders a title", () => {
    render(<Header />);
    const titleText = screen.getByText(/CPS Large and Complex Cases/i);
    expect(titleText).toBeInTheDocument();
  });
});
