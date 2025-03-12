import Footer from "../components/footer";
import Header from "../components/header";
import styles from "./layout.module.scss";

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  console.log("hii layout>>");

  return (
    <>
      <Header />
      <div className={styles.mainContent}>{children}</div>
      <Footer />
    </>
  );
}
