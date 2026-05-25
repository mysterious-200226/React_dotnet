import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";

import {
  PublicClientApplication,
  EventType
} from "@azure/msal-browser";

import {
  MsalProvider
} from "@azure/msal-react";

import {
  msalConfig
} from "./msalConfig";

// =======================================
// CREATE MSAL INSTANCE
// =======================================

const msalInstance =
  new PublicClientApplication(msalConfig);

// =======================================
// MSAL EVENT CALLBACKS (DEBUGGING)
// =======================================

msalInstance.addEventCallback((event) => {
  if (event.eventType === EventType.LOGIN_SUCCESS) {
    console.log("✅ LOGIN SUCCESS:", event);
  }

  if (event.eventType === EventType.LOGIN_FAILURE) {
    console.error("❌ LOGIN FAILURE:", event.error);
  }

  if (event.eventType === EventType.HANDLE_REDIRECT_START) {
    console.log("➡️ Redirect handling started");
  }

  if (event.eventType === EventType.HANDLE_REDIRECT_END) {
    console.log("⬅️ Redirect handling finished");
  }
});

// =======================================
// INITIALIZE APPLICATION
// =======================================

async function startApplication() {
  try {
    // IMPORTANT: initialize MSAL
    await msalInstance.initialize();
    console.log("MSAL initialized");

    // IMPORTANT: handle redirect BEFORE render
    await msalInstance.handleRedirectPromise();
    console.log("MSAL redirect processed");

    // Render React App
    const root = ReactDOM.createRoot(
      document.getElementById("root")
    );

    root.render(
      <React.StrictMode>
        <MsalProvider instance={msalInstance}>
          <App />
        </MsalProvider>
      </React.StrictMode>
    );

  } catch (error) {
    console.error("MSAL initialization error:", error);

    // Still render app to avoid blank screen
    const root = ReactDOM.createRoot(
      document.getElementById("root")
    );

    root.render(
      <MsalProvider instance={msalInstance}>
        <App />
      </MsalProvider>
    );
  }
}

// Start app
startApplication();