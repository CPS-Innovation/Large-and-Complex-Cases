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

export const getCaseSearchResults = async (searchParams: string) => {
  console.log("calling api....");
  try {
    const url = `${GATEWAY_BASE_URL}/api/cases?${searchParams}`;

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

export const getCaseDivisionsOrAreas = async () => {
  try {
    const url = `${GATEWAY_BASE_URL}/api/areas`;

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
