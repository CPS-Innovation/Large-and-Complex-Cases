"use client";
import { useState, useEffect } from "react";
import { Input, Button,Label, LabelText } from "govuk-react";
import { FEATURE_FLAG_FIND_A_CASE } from "../config";
import styles from "./page.module.scss";
import { CLIENT_STATIC_FILES_RUNTIME_POLYFILLS } from "next/dist/shared/lib/constants";

export const Home = () => {
  console.log("FEATURE_FLAG_FIND_A_CASE", FEATURE_FLAG_FIND_A_CASE);
  const [isClient, setIsClient] = useState(false);
  useEffect(() => {
    setIsClient(true);
  }, []);
  return (
    <div className="govuk-width-container">
      <div
        className={
          isClient
            ? `${styles.wrapper} ${styles.visible}`
            : `${styles.wrapper} ${styles.hidden}`
        }
      >
        <h1 className="govuk-heading-xl govuk-!-margin-bottom-4">
          Find a case
        </h1>

        {FEATURE_FLAG_FIND_A_CASE && 
        <div className={styles.inputWrapper}>
          <Label>
            <LabelText>
            Search by Operation name
            </LabelText>
        <Input className="govuk-input--width-20"/>
        </Label>
        <Button >Search</Button>
        </div>}
      </div>
    </div>
  );
};

export default Home;
