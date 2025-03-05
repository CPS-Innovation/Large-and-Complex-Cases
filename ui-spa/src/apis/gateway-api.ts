import { GATEWAY_BASE_URL } from "../config";

export const getInitialMessage = async () => {
  try {
    const url = `${GATEWAY_BASE_URL}/api/initialmessage`;

    const response = await fetch(url, {
      method: "GET",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    return await response.json();
  } catch (error) {
    console.error("Error:", error);
  }
};

export const getCaseSearchResults = async (searchKey: string) => {
  console.log("calling api....");
  try {
    const url = `${GATEWAY_BASE_URL}/api/cases?search=${searchKey}`;

    const response = await fetch(url, {
      method: "GET",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    return await response.json();
  } catch (error) {
    console.error("Error:", error);
  }
};
