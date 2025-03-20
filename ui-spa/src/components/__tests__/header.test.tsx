import { render, screen } from "@testing-library/react";
import Header from "../Header1";

describe("Header Component", () => {
  it("renders a title", () => {
    render(<Header />);
    const titleText = screen.getByText(/CPS Large and Complex Cases/i);
    expect(titleText).toBeInTheDocument();
  });
});
