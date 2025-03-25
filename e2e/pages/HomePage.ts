import { Locator, Page } from "@playwright/test";

export class HomePage {
  static readonly route = "/";
  private readonly page: Page;
  readonly operationName: Locator;
  readonly defendantName: Locator;
  readonly urn: Locator;
  readonly button: Locator;

  constructor(page: Page) {
    this.page = page;
    this.operationName = page.getByTestId("search-operation-name").last();
    this.defendantName = page.getByTestId("search-defendant-name");
    this.urn = page.getByTestId("search-urn");
    this.button = page.locator("button");
  }

  async selectRadio(tag: "urn" | "defendant-name" | "operation-name") {
    await this.page.getByTestId(`radio-search-${tag}`).check();
  }

  async selectArea(tag: "defendant" | "operation", value: string) {
    await this.page.getByTestId(`search-${tag}-area`).selectOption({ value });
  }

  async goto() {
    await this.page.goto(HomePage.route);
    await this.page.waitForSelector("h1");
  }
}
