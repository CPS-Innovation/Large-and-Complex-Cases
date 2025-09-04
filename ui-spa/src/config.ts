export const GATEWAY_BASE_URL = `${import.meta.env.VITE_GATEWAY_BASE_URL}`;
export const GATEWAY_SCOPE = `${import.meta.env.VITE_GATEWAY_SCOPE}`;
export const MOCK_API_SOURCE = `${import.meta.env.VITE_MOCK_API_SOURCE}`;
export const CLIENT_ID = `${import.meta.env.VITE_CLIENT_ID}`;
export const TENANT_ID = `${import.meta.env.VITE_TENANT_ID}`;
export const MOCK_AUTH = `${import.meta.env.VITE_MOCK_AUTH}` === "true";
export const PRIVATE_BETA_USER_GROUP = `${import.meta.env.VITE_PRIVATE_BETA_USER_GROUP}`;
export const PRIVATE_BETA_CONTACT_EMAIL = `${import.meta.env.VITE_PRIVATE_BETA_CONTACT_EMAIL}`;
export const FEATURE_FLAG_CASE_DETAILS =
  `${import.meta.env.VITE_FEATURE_FLAG_CASE_DETAILS}` === "true";
export const PRIVATE_BETA_FEATURE_USER_GROUP2 = `${import.meta.env.VITE_PRIVATE_BETA_FEATURE_USER_GROUP2}`;

console.log(JSON.stringify(import.meta.env));
