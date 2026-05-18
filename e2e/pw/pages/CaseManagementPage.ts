import { Page } from "@playwright/test";

export class CaseManagementPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async waitForLoad() {
    await this.page.locator('[role="tablist"]').waitFor({ state: "visible" });
  }

  async switchToTab(tabName: "transfer-materials" | "activity-log") {
    const tabIndex = tabName === "transfer-materials" ? 0 : 1;
    await this.page.getByTestId(`tab-${tabIndex}`).click();
  }

  async getWorkspaceName(): Promise<string> {
    return (await this.page.locator("h1").innerText()).trim();
  }
}
