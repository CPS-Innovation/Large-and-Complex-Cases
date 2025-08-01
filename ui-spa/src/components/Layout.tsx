import Footer from "./Footer";
import Header from "./Header";
import styles from "./Layout.module.scss";

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
