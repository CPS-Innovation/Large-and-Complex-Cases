import { render, screen } from "@testing-library/react";

import App from "./App1";

describe("App", () => {
  it("renders the App component", () => {
    render(<App />);
    screen.debug();
  });
});
