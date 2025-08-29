import { Page, expect } from "@playwright/test";
import path from "path";
import { HomePage } from "./HomePage";
import { AUTH_STORAGE_FILE_PATH } from "../playwright.config";

const { MOCK_USERNAME, E2E_AD_USER, E2E_PASSWORD, CMS_LOGIN_PAGE } =
  process.env;

// Not really a single page (we do CMS *and* AD log in). Done like this because we need
//  everything done in one unit so we can save storageState once.
export class LoginPage {
  static readonly route = CMS_LOGIN_PAGE;
  private readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async loginToCms() {
    await this.page.goto(LoginPage.route);
    await this.page.locator('input[name="username"]').fill(MOCK_USERNAME);
    await this.page.locator('input[type="password"]').fill(MOCK_USERNAME);
    await this.page.locator('input[type="submit"]').click();
    await this.page.waitForSelector("strong[data-testid='login-ok']");
  }

  async loginToAd() {
    await this.page.goto(HomePage.route);

    await this.page.waitForSelector('input[type="email"]');
    await this.page.locator('input[type="email"]').fill(E2E_AD_USER);
    await this.page.locator('input[type="submit"]').click();

    await this.page.locator('input[type="password"]').fill(E2E_PASSWORD);
    await this.page.locator('input[type="submit"]').click();

    await this.page.waitForURL(HomePage.route);
    await this.page.waitForSelector("[data-testid=div-ad-username]");
    expect(this.page.getByTestId("div-ad-username")).toHaveText(E2E_AD_USER);
  }

  async persistLoginInfo() {
    // As per https://playwright.dev/docs/auth#basic-shared-account-in-all-tests
    //  all follow on tests will have the cookies/tokens in this saved storage
    await this.page.context().storageState({
      path: path.join(__dirname, "..", AUTH_STORAGE_FILE_PATH),
    });
  }
}
