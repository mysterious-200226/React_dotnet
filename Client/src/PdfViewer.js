import React, { useEffect, useMemo, useState, useCallback } from "react";

import axios from "axios";

import { useMsal } from "@azure/msal-react";

import { loginRequest } from "./msalConfig";

// ============================================
// API CONFIG
// ============================================

const API_BASE_URL =
  process.env.REACT_APP_API_BASE_URL ||
  "https://api.logicandlaunch.me/api/S3Storage";

// ============================================
// AXIOS INSTANCE
// ============================================

const api = axios.create({
  baseURL: API_BASE_URL,
});

// ============================================
// COMPONENT
// ============================================

function PdfViewer() {
  const { instance, accounts } = useMsal();

  const [files, setFiles] = useState([]);

  const [currentPage, setCurrentPage] = useState(1);

  const [itemsPerPage, setItemsPerPage] = useState(10);

  const [search, setSearch] = useState("");

  const [loading, setLoading] = useState(false);

  const [error, setError] = useState("");

  // ============================================
  // HANDLE UNAUTHORIZED
  // ============================================

  const handleUnauthorized = useCallback(() => {
    localStorage.removeItem("Token");

    localStorage.removeItem("AuthType");

    instance.logoutRedirect();
  }, [instance]);

  // ============================================
  // GET ACCESS TOKEN
  // ============================================

  const getAccessToken = useCallback(async () => {
    try {
      const authType = localStorage.getItem("AuthType");

      console.log("Current Auth Type:", authType);

      // =====================================
      // EXISTING SAML TOKEN
      // =====================================

      if (authType === "SAML") {
        const samlToken = localStorage.getItem("Token");

        console.log("==============================");

        console.log("SAML TOKEN:");

        console.log(samlToken);

        console.log("==============================");

        return samlToken;
      }

      // =====================================
      // AZURE AD TOKEN
      // =====================================

      if (accounts.length === 0) {
        await instance.loginRedirect(loginRequest);

        return null;
      }

      const response = await instance.acquireTokenSilent({
        ...loginRequest,

        account: accounts[0],
      });

      const accessToken = response.accessToken;

      // =====================================
      // PRINT AZURE TOKEN
      // =====================================

      console.log("==============================");

      console.log("AZURE ACCESS TOKEN:");

      console.log(accessToken);

      console.log("==============================");

      localStorage.setItem("Token", accessToken);

      localStorage.setItem("AuthType", "AZURE");

      return accessToken;
    } catch (error) {
      console.error("Token acquisition failed:", error);

      await instance.loginRedirect(loginRequest);

      return null;
    }
  }, [accounts, instance]);

  // ============================================
  // LOAD FILES
  // ============================================

  const loadFiles = useCallback(async () => {
    try {
      setLoading(true);

      setError("");

      const token = await getAccessToken();

      if (!token) {
        return;
      }

      const response = await api.get("/GetAllS3Files", {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      setFiles(response.data || []);
    } catch (error) {
      console.error(error);

      if (error.response?.status === 401) {
        handleUnauthorized();
      } else if (error.response?.status === 403) {
        setError("You do not have permission to access these files.");
      } else {
        setError("Failed to load S3 files.");
      }
    } finally {
      setLoading(false);
    }
  }, [getAccessToken, handleUnauthorized]);

  // ============================================
  // INITIAL LOAD
  // ============================================

  useEffect(() => {
    loadFiles();
  }, [loadFiles]);

  // ============================================
  // VIEW FILE
  // ============================================

  const viewFile = async (s3Key) => {
    try {
      const token = await getAccessToken();

      if (!token) {
        return;
      }

      const response = await api.get(
        `/GetS3File?s3Key=${encodeURIComponent(s3Key)}`,
        {
          responseType: "blob",

          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      const blob = new Blob([response.data], {
        type: "application/pdf",
      });

      const fileUrl = window.URL.createObjectURL(blob);

      window.open(fileUrl, "_blank", "noopener,noreferrer");
    } catch (error) {
      console.error(error);

      if (error.response?.status === 401) {
        handleUnauthorized();
      } else {
        alert("Unable to open file.");
      }
    }
  };

  // ============================================
  // DOWNLOAD FILE
  // ============================================

  const downloadFile = async (s3Key, fileName) => {
    try {
      const token = await getAccessToken();

      if (!token) {
        return;
      }

      const response = await api.get(
        `/GetS3File?s3Key=${encodeURIComponent(s3Key)}`,
        {
          responseType: "blob",

          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      const blob = new Blob([response.data], {
        type: "application/pdf",
      });

      const url = window.URL.createObjectURL(blob);

      const link = document.createElement("a");

      link.href = url;

      link.download = fileName;

      document.body.appendChild(link);

      link.click();

      link.remove();

      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error(error);

      if (error.response?.status === 401) {
        handleUnauthorized();
      } else {
        alert("Unable to download file.");
      }
    }
  };

  // ============================================
  // SEARCH FILTER
  // ============================================

  const filteredFiles = useMemo(() => {
    return files.filter((file) => {
      const fileName = file.fileName?.toLowerCase() || "";

      const s3Key = file.s3Key?.toLowerCase() || "";

      const searchText = search.toLowerCase();

      return fileName.includes(searchText) || s3Key.includes(searchText);
    });
  }, [files, search]);

  // ============================================
  // PAGINATION
  // ============================================

  const totalPages = Math.ceil(filteredFiles.length / itemsPerPage);

  const startIndex = (currentPage - 1) * itemsPerPage;

  const currentFiles = filteredFiles.slice(
    startIndex,
    startIndex + itemsPerPage
  );

  // ============================================
  // RENDER
  // ============================================

  return (
    <div
      style={{
        minHeight: "100vh",
        background:
          "radial-gradient(circle at top left, rgba(255,153,0,0.18), transparent 28%), linear-gradient(135deg, #081220 0%, #0f172a 45%, #12263f 100%)",
        padding: "30px",
        fontFamily: "Segoe UI, Arial, sans-serif",
      }}
    >
      {/* HEADER */}

      <div
        style={{
          background:
            "linear-gradient(135deg, rgba(15,23,42,0.96) 0%, rgba(14,77,110,0.96) 55%, rgba(255,153,0,0.96) 140%)",
          color: "white",
          padding: "25px",
          borderRadius: "12px",
          marginBottom: "25px",
          boxShadow: "0px 18px 50px rgba(0,0,0,0.35)",
          border: "1px solid rgba(255,255,255,0.08)",
        }}
      >
        <h1
          style={{
            margin: 0,
          }}
        >
          AWS S3 PDF Files
        </h1>

        <p
          style={{
            marginTop: 10,
            color: "rgba(255,255,255,0.9)",
          }}
        >
          Secure PDF viewer using AWS IAM, SAML authentication, and S3 storage
        </p>
      </div>

      {/* SEARCH */}

      <div
        style={{
          marginBottom: 20,
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          gap: "20px",
        }}
      >
        <div
          style={{
            fontWeight: "bold",
          }}
        >
          Total Files: {filteredFiles.length}
        </div>

        <input
          type="text"
          placeholder="Search files..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);

            setCurrentPage(1);
          }}
          style={{
            padding: "10px",
            width: "300px",
            borderRadius: "8px",
            border: "1px solid rgba(255,255,255,0.18)",
            background: "rgba(255,255,255,0.95)",
            color: "#0f172a",
          }}
        />
      </div>

      {/* ERROR */}

      {error && (
        <div
          style={{
            background: "#ffe5e5",
            color: "#c40000",
            padding: "15px",
            borderRadius: "8px",
            marginBottom: "20px",
          }}
        >
          {error}
        </div>
      )}

      {/* TABLE */}

      <div
        style={{
          background: "rgba(255,255,255,0.96)",
          borderRadius: "12px",
          overflow: "hidden",
          boxShadow: "0px 18px 40px rgba(0,0,0,0.28)",
          border: "1px solid rgba(15,23,42,0.08)",
        }}
      >
        <table
          style={{
            width: "100%",
            borderCollapse: "collapse",
          }}
        >
          <thead>
            <tr
              style={{
                background:
                  "linear-gradient(90deg, #111827 0%, #0f4c81 50%, #ff9900 130%)",
              }}
            >
              <th style={headerStyle}>#</th>

              <th style={headerStyle}>File Name</th>

              <th style={headerStyle}>S3 Key</th>

              <th style={headerStyle}>Actions</th>
            </tr>
          </thead>

          <tbody>
            {loading ? (
              <tr>
                <td
                  colSpan="4"
                  style={{
                    textAlign: "center",
                    padding: "30px",
                  }}
                >
                  Loading files...
                </td>
              </tr>
            ) : currentFiles.length === 0 ? (
              <tr>
                <td
                  colSpan="4"
                  style={{
                    textAlign: "center",
                    padding: "30px",
                  }}
                >
                  No files found.
                </td>
              </tr>
            ) : (
              currentFiles.map((file, index) => (
                <tr
                  key={`${file.s3Key}-${index}`}
                  style={{
                    borderBottom: "1px solid #eee",
                  }}
                >
                  <td style={cellStyle}>{startIndex + index + 1}</td>

                  <td style={cellStyle}>{file.fileName}</td>

                  <td style={cellStyle}>{file.s3Key}</td>

                  <td style={cellStyle}>
                    <div
                      style={{
                        display: "flex",
                        gap: "10px",
                      }}
                    >
                      <button
                        onClick={() => viewFile(file.s3Key)}
                        style={{
                          background: "#0d6efd",
                          color: "white",
                          border: "none",
                          padding: "8px 14px",
                          borderRadius: "6px",
                          cursor: "pointer",
                        }}
                      >
                        View
                      </button>

                      <button
                        onClick={() =>
                          downloadFile(file.s3Key, file.fileName)
                        }
                        style={{
                          background: "#198754",
                          color: "white",
                          border: "none",
                          padding: "8px 14px",
                          borderRadius: "6px",
                          cursor: "pointer",
                        }}
                      >
                        Download
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* PAGINATION */}

      <div
        style={{
          marginTop: 20,
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <div>
          <button
            disabled={currentPage === 1}
            onClick={() => setCurrentPage(currentPage - 1)}
            style={paginationButton}
          >
            Previous
          </button>

          {[...Array(totalPages)]
            .map((_, i) => i + 1)
            .slice(Math.max(currentPage - 3, 0), currentPage + 2)
            .map((page) => (
              <button
                key={page}
                onClick={() => setCurrentPage(page)}
                style={{
                  ...paginationButton,
                  background: currentPage === page ? "#0d6efd" : "white",
                  color: currentPage === page ? "white" : "black",
                }}
              >
                {page}
              </button>
            ))}

          <button
            disabled={currentPage === totalPages || totalPages === 0}
            onClick={() => setCurrentPage(currentPage + 1)}
            style={paginationButton}
          >
            Next
          </button>
        </div>

        <div>
          <select
            value={itemsPerPage}
            onChange={(e) => {
              setItemsPerPage(Number(e.target.value));

              setCurrentPage(1);
            }}
            style={{
              padding: "8px",
              borderRadius: "6px",
            }}
          >
            <option value={5}>5</option>

            <option value={10}>10</option>

            <option value={20}>20</option>

            <option value={50}>50</option>
          </select>
        </div>
      </div>
    </div>
  );
}

// ============================================
// STYLES
// ============================================

const headerStyle = {
  padding: "15px",
  textAlign: "left",
  color: "#0d47a1",
  fontWeight: "bold",
};

const cellStyle = {
  padding: "15px",
  verticalAlign: "top",
};

const paginationButton = {
  marginRight: "8px",
  padding: "8px 14px",
  border: "1px solid #ccc",
  borderRadius: "6px",
  cursor: "pointer",
  background: "white",
};

export default PdfViewer;
