import { test as setup } from "@playwright/test";
import { LoginPage } from "../pages/LoginPage";

setup("authenticate", async ({ page }) => {
  const login = new LoginPage(page);
  await login.loginToCms();
  await login.loginToAd();
  await login.persistLoginInfo();
});
