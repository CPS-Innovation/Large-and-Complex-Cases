import type { Metadata } from "next";
import Footer from "../components/footer";
import Header from "../components/header";
import "./globals.scss";

export const metadata: Metadata = {
  title: "GOV.UK - CPS Large and Complex Cases",
  description: "CPS Large and Complex Cases",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="govuk-template">
      <head>
        <meta name="description" content="" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <link rel="icon" sizes="48x48" href="/assets/images/favicon.ico" />
        <link
          rel="icon"
          sizes="any"
          href="/assets/images/favicon.svg"
          type="image/svg+xml"
        />
        <link
          rel="mask-icon"
          href="/assets/images/govuk-icon-mask.svg"
          color="#0b0c0c"
        />
        <link rel="apple-touch-icon" href="/assets/images/govuk-icon-180.png" />
        <link rel="manifest" href="/assets/manifest.json" />
      </head>
      <body>
        <Header />
        {children}
        <Footer />
      </body>
    </html>
  );
}
