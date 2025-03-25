import { Locator, Page } from "@playwright/test";

export class HomePage {
  static readonly route = "/";

  private readonly page: Page;
  //readonly operationNameRadio: Locator;
  readonly operationName: Locator;
  //readonly defendantNameRadio: Locator;
  readonly defendantName: Locator;
  //  readonly urnRadio: Locator;
  readonly urn: Locator;
  readonly button: Locator;

  constructor(page: Page) {
    this.page = page;
    // this.operationNameRadio = page
    //   .getByTestId("radio-search-operation-name")
    //   .first();
    this.operationName = page.getByTestId("search-operation-name").last();

    // this.defendantNameRadio = page
    //   .getByTestId("radio-search-defendant-name")
    //   .first();
    this.defendantName = page.getByTestId("search-defendant-name");

    //this.urnRadio = page.getByTestId("radio-search-urn").first();
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
