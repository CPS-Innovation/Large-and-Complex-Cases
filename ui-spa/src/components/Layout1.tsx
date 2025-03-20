import Footer from "./Footer1";
import Header from "./Header1";
import styles from "./layout.module.scss";

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <>
      <Header />
      <div className={styles.mainContent}>{children}</div>
      <Footer />
    </>
  );
}
