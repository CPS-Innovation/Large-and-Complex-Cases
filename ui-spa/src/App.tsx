import React, { useState } from "react";
import { Button, H1 } from "govuk-react";
import { getInitialMessage } from "./apis/gateway-api";
import "./App.css";
import { Auth, useUserDetails } from "./auth";

const Inner: React.FC = () => {
  const { username } = useUserDetails();
  return <>{username}</>;
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
        <H1>{`${message}`}</H1>
        <div>
          <Inner />
        </div>
        <Button onClick={handleButtonClick}>Click</Button>
      </Auth>
    </div>
  );
}

export default App;
