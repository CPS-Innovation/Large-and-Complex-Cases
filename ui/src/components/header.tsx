import Link from "next/link";
export default function Header() {
  return (
    <header
      className="govuk-header govuk-header--full-width-border"
      role="banner"
      data-module="govuk-header"
    >
      <div className="govuk-header__container govuk-width-container">
        <div className="govuk-header__logo">
          <Link
            href="/"
            className="govuk-header__link govuk-header__link--homepage"
            data-testid="link-homepage"
          >
            <span className="govuk-header__logotype">
              <span className="govuk-header__logotype-text">
                CPS Large and Complex Cases
              </span>
            </span>
          </Link>
        </div>
      </div>
    </header>
  );
}
