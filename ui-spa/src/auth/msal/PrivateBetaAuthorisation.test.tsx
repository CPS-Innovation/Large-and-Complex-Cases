import PrivateBetaAuthorisation from "./PrivateBetaAuthorisation";
import * as config from "../../config";
import { AccountInfo, IPublicClientApplication } from "@azure/msal-browser";
import { render, screen } from "@testing-library/react";

const PRIVATE_BETA_USER_GROUP_VALUE = "foo";
const EXPECTED_APP_TEXT = "app_text";

vi.mock("../../config");
vi.mock("react-router-dom", () => ({
  useLocation: () => ({
    pathname: "",
  }),
}));
const mockConfig = config as {
  PRIVATE_BETA_USER_GROUP: string | null;
  PRIVATE_BETA_CONTACT_EMAIL: string | null;
};

let mockAccounts = [] as AccountInfo[];
const mockMsalInstance = {
  getAllAccounts: () => mockAccounts,
} as IPublicClientApplication;

const actFn = () => {
  return render(
    <PrivateBetaAuthorisation msalInstance={mockMsalInstance}>
      <div>{EXPECTED_APP_TEXT}</div>
    </PrivateBetaAuthorisation>,
  );
};

describe("PrivateBetaAuthorisation", () => {
  it("will allow the user to access the app if no private beta group is configured", () => {
    mockConfig.PRIVATE_BETA_USER_GROUP = null;
    actFn();
    expect(screen.queryByText(EXPECTED_APP_TEXT)).toBeInTheDocument();
  });

  it("will not allow the user to access the app if the user has no account", () => {
    mockConfig.PRIVATE_BETA_USER_GROUP = PRIVATE_BETA_USER_GROUP_VALUE;
    actFn();
    expect(screen.queryByText(EXPECTED_APP_TEXT)).not.toBeInTheDocument();
    expect(
      screen.queryByRole("heading", { level: 1, name: /Access Error/i }),
    ).toBeInTheDocument();
    expect(
      screen.queryByText(
        "You cannot access this page. You are not a member of this group.",
      ),
    ).toBeInTheDocument();
  });

  it("will not allow the user to access the app if the user has no groups claims object", () => {
    mockConfig.PRIVATE_BETA_USER_GROUP = PRIVATE_BETA_USER_GROUP_VALUE;
    mockAccounts = [
      {
        idTokenClaims: {},
      } as AccountInfo,
    ];
    actFn();
    expect(screen.queryByText(EXPECTED_APP_TEXT)).not.toBeInTheDocument();
    expect(
      screen.queryByRole("heading", { level: 1, name: /Access Error/i }),
    ).toBeInTheDocument();
    expect(
      screen.queryByText(
        "You cannot access this page. You are not a member of this group.",
      ),
    ).toBeInTheDocument();
  });

  it("will not allow the user to access the app if the user is not in the private beta group", () => {
    mockConfig.PRIVATE_BETA_USER_GROUP = PRIVATE_BETA_USER_GROUP_VALUE;
    mockConfig.PRIVATE_BETA_CONTACT_EMAIL = "abc@email.com";
    mockAccounts = [
      {
        idTokenClaims: {
          groups: ["bar", "baz"],
        } as AccountInfo["idTokenClaims"],
      } as AccountInfo,
    ];

    actFn();
    expect(screen.queryByText(EXPECTED_APP_TEXT)).not.toBeInTheDocument();
    expect(
      screen.queryByRole("heading", { level: 1, name: /Access Error/i }),
    ).toBeInTheDocument();
    expect(
      screen.queryByText(
        "You cannot access this page. You are not a member of this group.",
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /abc@email.com/i }),
    ).toBeInTheDocument();
  });

  it("will allow the user to access the app if the user is in the private beta group", () => {
    mockConfig.PRIVATE_BETA_USER_GROUP = "foo";
    mockAccounts = [
      {
        idTokenClaims: {
          groups: ["bar", "baz", PRIVATE_BETA_USER_GROUP_VALUE],
        } as AccountInfo["idTokenClaims"],
      } as AccountInfo,
    ];
    actFn();
    expect(screen.queryByText(EXPECTED_APP_TEXT)).toBeInTheDocument();
    expect(
      screen.queryByRole("heading", { level: 1, name: /Access Error/i }),
    ).not.toBeInTheDocument();
  });
});
