import React, { useEffect, useState, useRef } from "react";

import {
  BrowserRouter as Router,
  Routes,
  Route,
  useLocation,
  useNavigate
} from "react-router-dom";

import PdfViewer from "./PdfViewer";

import { useMsal } from "@azure/msal-react";
import { loginRequest } from "./msalConfig";

// =======================================
// SSO COMPLETE PAGE (SAML FLOW)
// =======================================

function SSOComplete() {
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    const token = new URLSearchParams(location.search).get("token");

    if (token) {
      localStorage.setItem("Token", token);
      localStorage.setItem("AuthType", "SAML");

      navigate("/", { replace: true });
    }
  }, [location, navigate]);

  return (
    <div style={{ padding: "40px" }}>
      Processing login...
    </div>
  );
}

// =======================================
// MAIN APPLICATION
// =======================================

function MainApp() {
  const { instance } = useMsal();

  const [authenticated, setAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);

  // 🔥 prevents repeated loginRedirect calls
  const loginTriggered = useRef(false);

  useEffect(() => {
    const checkLogin = async () => {
      try {
        // ==================================
        // 1. COMPLETE MSAL REDIRECT FLOW
        // ==================================
        await instance.handleRedirectPromise();

        // ==================================
        // 2. SAML CHECK (LOCAL TOKEN)
        // ==================================
        const samlToken = localStorage.getItem("Token");
        const authType = localStorage.getItem("AuthType");

        if (samlToken && authType === "SAML") {
          setAuthenticated(true);
          setLoading(false);
          return;
        }

        // ==================================
        // 3. AZURE AD CHECK
        // ==================================
        const accounts = instance.getAllAccounts();

        if (accounts && accounts.length > 0) {
          instance.setActiveAccount(accounts[0]);

          setAuthenticated(true);
          setLoading(false);
          return;
        }

        // ==================================
        // 4. LOGIN REDIRECT (ONLY ONCE)
        // ==================================
        if (!loginTriggered.current) {
          loginTriggered.current = true;

          await instance.loginRedirect(loginRequest);
        }

      } catch (error) {
        console.error("Authentication Error:", error);
        setLoading(false);
      }
    };

    checkLogin();
  }, [instance]);

  // ==================================
  // LOGOUT
  // ==================================
  const logout = async () => {
    localStorage.clear();

    await instance.logoutRedirect({
      postLogoutRedirectUri: window.location.origin
    });
  };

  // ==================================
  // LOADING STATE
  // ==================================
  if (loading) {
    return (
      <div style={{ padding: "40px", fontSize: "18px", fontFamily: "Arial" }}>
        Loading authentication...
      </div>
    );
  }

  // ==================================
  // NOT AUTHENTICATED STATE
  // ==================================
  if (!authenticated) {
    return (
      <div style={{ padding: "40px", fontSize: "18px", fontFamily: "Arial" }}>
        Redirecting to Microsoft Login...
      </div>
    );
  }

  // ==================================
  // AUTHENTICATED UI
  // ==================================
  return (
    <div>
      <div
        style={{
          background: "#0d6efd",
          color: "white",
          padding: "15px 25px",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center"
        }}
      >
        <h2 style={{ margin: 0 }}>PDF Viewer</h2>

        <button
          onClick={logout}
          style={{
            background: "white",
            color: "#0d6efd",
            border: "none",
            padding: "8px 16px",
            borderRadius: "6px",
            cursor: "pointer",
            fontWeight: "bold"
          }}
        >
          Logout
        </button>
      </div>

      <PdfViewer />
    </div>
  );
}

// =======================================
// ROOT APP
// =======================================

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/sso-complete" element={<SSOComplete />} />
        <Route path="*" element={<MainApp />} />
      </Routes>
    </Router>
  );
}

export default App;