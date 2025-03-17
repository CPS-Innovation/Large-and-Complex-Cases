export const GATEWAY_BASE_URL = `${import.meta.env.VITE_GATEWAY_BASE_URL}`;
export const MOCK_API_SOURCE = `${import.meta.env.VITE_MOCK_API_SOURCE}`;
export const CLIENT_ID = `${import.meta.env.VITE_CLIENT_ID}`;
export const TENANT_ID = `${import.meta.env.VITE_TENANT_ID}`;
export const MOCK_AUTH = `${import.meta.env.VITE_MOCK_AUTH}` === "true";

console.log(JSON.stringify(import.meta.env));
