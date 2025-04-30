import { Tabs, TabsProps } from "./Tabs";
import { render, screen, fireEvent } from "@testing-library/react";
import { useState } from "react";

vi.mock("../../../../common/hooks/useAppInsightsTracks", () => ({
  useAppInsightsTrackEvent: () => vi.fn(),
}));

describe("Tabs", () => {
  const scrollIntoView = Element.prototype.scrollIntoView;

  beforeAll(() => {
    Element.prototype.scrollIntoView = vi.fn();
  });

  afterAll(() => {
    Element.prototype.scrollIntoView = scrollIntoView;
  });

  it("can render empty tabs", async () => {
    const props: TabsProps = {
      title: "Tabs-title",
      items: [],
      activeTabId: "",

      handleTabSelection: () => {},
    };

    render(<Tabs {...props} />);
    await screen.findByTestId("tabs");
    expect(screen.queryAllByRole("tab")).toHaveLength(0);
  });

  it("can render tabs", async () => {
    const props: TabsProps = {
      title: "Tabs-title",
      activeTabId: "",
      items: [
        {
          id: "t1",

          label: "tab-1",
          panel: <></>,
        },
        {
          id: "t2",

          label: "tab-2",
          panel: <></>,
        },
        {
          id: "t3",

          label: "tab-3",
          panel: <></>,
        },
      ],

      handleTabSelection: () => {},
    };

    render(<Tabs {...props} />);
    await screen.findByTestId("tabs");
    expect(screen.queryAllByRole("tab")).toHaveLength(3);
  });

  it("can highlight the active tab", async () => {
    const props: TabsProps = {
      title: "Tabs-title",
      activeTabId: "",
      items: [
        {
          id: "t1",

          label: "tab-1",
          panel: <>content-1</>,
        },
        {
          id: "t2",

          label: "tab-2",
          panel: <>content-2</>,
        },
        {
          id: "t3",

          label: "tab-3",
          panel: <>content-3</>,
        },
      ],

      handleTabSelection: () => {},
    };

    const { rerender } = render(<Tabs {...props} />);
    await screen.findByTestId("tabs");
    // first tab is active if no hash passed
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
    expect(screen.getByTestId("tab-content-t1")).not.toHaveClass(
      "govuk-tabs__panel--hidden",
    );
    expect(screen.getByTestId("tab-content-t2")).toHaveClass(
      "govuk-tabs__panel--hidden",
    );
    rerender(<Tabs {...props} activeTabId="t2" />);

    // otherwise active tab driven by hash
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-2");
    expect(screen.getByTestId("tab-content-t1")).toHaveClass(
      "govuk-tabs__panel--hidden",
    );
    expect(screen.getByTestId("tab-content-t2")).not.toHaveClass(
      "govuk-tabs__panel--hidden",
    );
  });

  it("can navigate using keyboard", async () => {
    const TestComponent = () => {
      const [activeTabId, setActiveTabId] = useState("");
      const props: TabsProps = {
        activeTabId,
        title: "Tabs-title",
        items: [
          {
            id: "t1",

            label: "tab-1",
            panel: <></>,
          },
          {
            id: "t2",

            label: "tab-2",
            panel: <></>,
          },
          {
            id: "t3",

            label: "tab-3",
            panel: <></>,
          },
        ],

        handleTabSelection: (id: string) => {
          setActiveTabId(id);
        },
      };
      return (
        <div>
          <Tabs {...props} />
        </div>
      );
    };

    render(<TestComponent />);
    await screen.findByTestId("tabs");
    // make sure we have landed on the expected first tab
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
    // right goes to next tab
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowRight",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-2");
    // down goes to next tab
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowRight",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-3");
    // stays on right-most tab on attempted navigate
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowRight",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-3");
    // left goes to previous tab
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowLeft",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-2");
    // up goes to previous tab
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowLeft",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
    // stays on left-most tab on attempted navigate
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowLeft",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
  });

  // // Not strictly a testable feature as the tab code just renders what it gets given in terms of items.
  // //  However, (one of) the reasons we writing our own tabs is that adding new tabs dynamically
  // //  breaks standard GDS tabs (the keyboard navigation doesn't work for the newly added tabs).
  // //  So lets just check that we are achieving our goal.
  it("can add a tab", async () => {
    const props: TabsProps = {
      title: "Tabs-title",
      activeTabId: "",
      handleTabSelection: () => {},
      items: [],
    };

    const { rerender } = render(<Tabs {...props} />);
    await screen.findByTestId("tabs");
    expect(screen.queryAllByRole("tab")).toHaveLength(0);
    rerender(
      <Tabs
        {...props}
        items={[
          {
            id: "t1",

            label: "tab-1",
            panel: <></>,
          },
        ]}
      />,
    );
    expect(screen.queryAllByRole("tab")).toHaveLength(1);
    // going from no tabs to one tab we expect the new tab to get focus
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
    rerender(
      <Tabs
        {...props}
        items={[
          {
            id: "t1",
            label: "tab-1",
            panel: <></>,
          },
          {
            id: "t2",
            label: "tab-2",
            panel: <></>,
          },
        ]}
      />,
    );
    expect(screen.queryAllByRole("tab")).toHaveLength(2);
    // going from some tabs to one more tab we expect the new tab NOT to get focus
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
  });
});
