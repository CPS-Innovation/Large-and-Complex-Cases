import { Page } from "@playwright/test";

export class NetAppConnectPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async waitForFolders() {
    await this.page.waitForSelector("table tbody tr");
  }

  async connectFolder(index: number = 0) {
    const rows = await this.page.locator("table tbody tr").all();
    await rows[index].locator('button[name="secondary"]').click();
  }
}
