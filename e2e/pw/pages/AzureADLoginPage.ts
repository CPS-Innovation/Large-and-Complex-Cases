import { Locator, Page } from "@playwright/test";

export class AzureADLoginPage {
  private readonly page: Page;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly submitButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.emailInput = page.locator('input[type="email"]');
    this.passwordInput = page.locator('input[type="password"]');
    this.submitButton = page.locator('input[type="submit"]');
  }

  async login(email: string, password: string) {
    // Step 1: Enter email
    await this.page.waitForSelector('input[type="email"]');
    await this.emailInput.fill(email);
    await this.submitButton.click();

    // Step 2: Wait for password field (AD transitions between screens)
    await this.page.waitForSelector('input[type="password"]', {
      state: "visible",
    });
    await this.passwordInput.fill(password);
    await this.submitButton.click();

    // Step 3: Handle "Stay signed in?" prompt if it appears
    try {
      const staySignedIn = this.page.locator(
        'input[type="submit"][value="Yes"], input[type="button"][value="Yes"], #idSIButton9'
      );
      await staySignedIn.click({ timeout: 5000 });
    } catch {
      // No "Stay signed in?" prompt — continue
    }
  }
}
