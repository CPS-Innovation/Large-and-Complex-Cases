import { Page } from "@playwright/test";

export class EgressConnectPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async searchFolder(name: string) {
    await this.page.getByTestId("search-folder-name").fill(name);
    await this.page.getByTestId("search-folder-name").press("Enter");
  }

  async waitForResults() {
    await this.page.waitForSelector("table tbody tr");
  }

  async connectFolder(index: number = 0) {
    const rows = await this.page.locator("table tbody tr").all();
    await rows[index].locator("button").click();
  }
}
