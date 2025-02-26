import React, { useState } from "react";
import { Button, H1 } from "govuk-react";
import Layout from "./layout";
import { getInitialMessage } from "../apis/gateway-api";
import { Auth, useUserDetails } from "../auth";
import styles from "./app.module.scss";

const Inner: React.FC = () => {
  const { username } = useUserDetails();
  return <div className={styles.username}>{username}</div>;
};

function App() {
  const [message, setMessage] = useState("abc");

  const handleButtonClick = async () => {
    const result = await getInitialMessage();

    if (result) setMessage(result.message);
  };

  return (
    <div>
      <Auth>
        <Layout>
          <div className={`${styles.wrapper} govuk-width-container`}>
            <H1>{`${message}`}</H1>
            <Inner />

            <Button onClick={handleButtonClick}>Click</Button>
          </div>
        </Layout>
      </Auth>
    </div>
  );
}

export default App;
