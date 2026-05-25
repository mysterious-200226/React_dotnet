import { LogLevel } from "@azure/msal-browser";

const TENANT_ID = "c45ff7f3-1f43-435b-8689-c79fe17716da";

const SPA_CLIENT_ID = "1201b63f-1b2e-4623-9b81-766ca820b324";

export const API_SCOPE =
  "api://0590f366-c59d-4a56-905a-af66649837e3/access_as_user";

export const msalConfig = {
  auth: {
    clientId: SPA_CLIENT_ID,

    authority: `https://login.microsoftonline.com/${TENANT_ID}`,

    redirectUri:
      process.env.REACT_APP_REDIRECT_URI ||
      window.location.origin,

    navigateToLoginRequestUrl: false, // important to prevent redirect loops
  },

  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: false,
  },

  system: {
    loggerOptions: {
      logLevel: LogLevel.Info,
      loggerCallback: (level, message, containsPii) => {
        if (!containsPii) console.log("MSAL:", message);
      },
    },
  },
};

export const loginRequest = {
  scopes: [
    "openid",
    "profile",
    "email",
    API_SCOPE
  ],

  prompt: "select_account", // prevents forced re-login loop
};