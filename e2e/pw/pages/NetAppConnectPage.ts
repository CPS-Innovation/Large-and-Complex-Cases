import { Page } from "@playwright/test";
import { REGISTER_CASE_NETAPP_FOLDER } from "../helpers/constants";

export class NetAppConnectPage {
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async waitForFolders() {
    await this.page.waitForSelector("table tbody tr");
  }

  /** Matched by name: row order is alphabetical, and no other folder
   * substitutes (README "NetApp source pre-condition"). */
  async connectFolder(): Promise<void> {
    const row = this.page
      .locator("table tbody tr")
      .filter({
        has: this.page.getByRole("button", {
          name: REGISTER_CASE_NETAPP_FOLDER,
          exact: true,
        }),
      })
      .first();

    if ((await row.count()) === 0) {
      throw new Error(
        `Shared-drive folder "${REGISTER_CASE_NETAPP_FOLDER}" is not listed. ` +
          `Check the E2E user can see it, or that it still exists.`,
      );
    }

    const connect = row.locator('button[name="secondary"]');
    if (!(await connect.isEnabled())) {
      throw new Error(
        `Shared-drive folder "${REGISTER_CASE_NETAPP_FOLDER}" is linked to ` +
          `another case (Connect disabled). Free it via DELETE ` +
          `/api/v1/netapp/connections?case-id=<id>, which 409s until that ` +
          `case's active transfer is cleared.`,
      );
    }

    await connect.click();
  }
}
