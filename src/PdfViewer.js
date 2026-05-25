import React, {
  useEffect,
  useState
} from "react";

import axios from "axios";

function PdfViewer() {

  const [files, setFiles] = useState([]);

  const [currentPage, setCurrentPage] =
    useState(1);

  const [itemsPerPage, setItemsPerPage] =
    useState(10);

  const [search, setSearch] =
    useState("");

  const [loading, setLoading] =
    useState(false);

  useEffect(() => {
    loadFiles();
  }, []);

  const loadFiles = async () => {

    try {

      setLoading(true);

      const response = await axios.get(
        "http://php-poc-alb-1558296517.us-east-1.elb.amazonaws.com/api/S3Storage/GetAllS3Files"
      );

      setFiles(response.data);

    } catch (error) {

      console.error(error);

    } finally {

      setLoading(false);
    }
  };

  // ============================================
  // VIEW FILE
  // ============================================

  const viewFile = async (s3Key) => {

    try {

      const response = await axios.get(
        `http://php-poc-alb-1558296517.us-east-1.elb.amazonaws.com/api/S3Storage/GetS3File?s3Key=${encodeURIComponent(s3Key)}`,
        {
          responseType: "blob"
        }
      );

      const file = new Blob(
        [response.data],
        {
          type: "application/pdf"
        }
      );

      const fileURL =
        URL.createObjectURL(file);

      window.open(fileURL);

    } catch (error) {

      console.error(error);
    }
  };

  // ============================================
  // DOWNLOAD FILE
  // ============================================

  const downloadFile = async (
    s3Key,
    fileName
  ) => {

    try {

      const response = await axios.get(
        `http://php-poc-alb-1558296517.us-east-1.elb.amazonaws.com/api/S3Storage/GetS3File?s3Key=${encodeURIComponent(s3Key)}`,
        {
          responseType: "blob"
        }
      );

      const file = new Blob(
        [response.data],
        {
          type: "application/pdf"
        }
      );

      const url =
        window.URL.createObjectURL(file);

      const link =
        document.createElement("a");

      link.href = url;

      link.download = fileName;

      document.body.appendChild(link);

      link.click();

      link.remove();

    } catch (error) {

      console.error(error);
    }
  };

  // ============================================
  // SEARCH
  // ============================================

  const filteredFiles =
    files.filter(file =>
      file.fileName
        .toLowerCase()
        .includes(search.toLowerCase())
      ||
      file.s3Key
        .toLowerCase()
        .includes(search.toLowerCase())
    );

  // ============================================
  // PAGINATION
  // ============================================

  const totalPages =
    Math.ceil(
      filteredFiles.length /
      itemsPerPage
    );

  const startIndex =
    (currentPage - 1) * itemsPerPage;

  const currentFiles =
    filteredFiles.slice(
      startIndex,
      startIndex + itemsPerPage
    );

  return (

    <div
      style={{
        minHeight: "100vh",
        background:
          "linear-gradient(to right, #f4f7fb, #e8eef7)",
        padding: "30px",
        fontFamily: "Arial"
      }}
    >

      {/* HEADER */}

      <div
        style={{
          background:
            "linear-gradient(to right, #005bea, #3f8efc)",
          color: "white",
          padding: "25px",
          borderRadius: "12px",
          marginBottom: "25px",
          boxShadow:
            "0px 4px 12px rgba(0,0,0,0.2)"
        }}
      >

        <h1
          style={{
            margin: 0
          }}
        >
          AWS S3 PDF Files
        </h1>

        <p
          style={{
            marginTop: 10
          }}
        >
          View and download PDF
          documents from AWS S3 Storage
        </p>

      </div>

      {/* SEARCH */}

      <div
        style={{
          marginBottom: 20,
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center"
        }}
      >

        <div
          style={{
            fontWeight: "bold"
          }}
        >
          Total Files:
          {" "}
          {filteredFiles.length}
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
            border: "1px solid #ccc"
          }}
        />

      </div>

      {/* TABLE */}

      <div
        style={{
          background: "white",
          borderRadius: "12px",
          overflow: "hidden",
          boxShadow:
            "0px 2px 10px rgba(0,0,0,0.1)"
        }}
      >

        <table
          style={{
            width: "100%",
            borderCollapse: "collapse"
          }}
        >

          <thead>

            <tr
              style={{
                background: "#edf3ff"
              }}
            >

              <th style={headerStyle}>
                #
              </th>

              <th style={headerStyle}>
                File Name
              </th>

              <th style={headerStyle}>
                S3 Key
              </th>

              <th style={headerStyle}>
                Actions
              </th>

            </tr>

          </thead>

          <tbody>

            {loading ? (

              <tr>
                <td
                  colSpan="4"
                  style={{
                    textAlign: "center",
                    padding: "30px"
                  }}
                >
                  Loading...
                </td>
              </tr>

            ) : (

              currentFiles.map((file, index) => (

                <tr
                  key={index}
                  style={{
                    borderBottom:
                      "1px solid #eee"
                  }}
                >

                  <td style={cellStyle}>
                    {startIndex + index + 1}
                  </td>

                  <td style={cellStyle}>
                    {file.fileName}
                  </td>

                  <td style={cellStyle}>
                    {file.s3Key}
                  </td>

                  <td style={cellStyle}>

                    <div
                      style={{
                        display: "flex",
                        gap: "10px"
                      }}
                    >

                      <button
                        onClick={() =>
                          viewFile(
                            file.s3Key
                          )
                        }
                        style={{
                          background: "#0d6efd",
                          color: "white",
                          border: "none",
                          padding:
                            "8px 14px",
                          borderRadius: "6px",
                          cursor: "pointer"
                        }}
                      >
                        View
                      </button>

                      <button
                        onClick={() =>
                          downloadFile(
                            file.s3Key,
                            file.fileName
                          )
                        }
                        style={{
                          background: "#198754",
                          color: "white",
                          border: "none",
                          padding:
                            "8px 14px",
                          borderRadius: "6px",
                          cursor: "pointer"
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
          justifyContent:
            "space-between",
          alignItems: "center"
        }}
      >

        <div>

          <button
            disabled={currentPage === 1}
            onClick={() =>
              setCurrentPage(
                currentPage - 1
              )
            }
            style={paginationButton}
          >
            Previous
          </button>

          {[...Array(totalPages)]
            .map((_, i) => i + 1)
            .slice(
              Math.max(currentPage - 3, 0),
              currentPage + 2
            )
            .map(page => (

              <button
                key={page}
                onClick={() =>
                  setCurrentPage(page)
                }
                style={{
                  ...paginationButton,
                  background:
                    currentPage === page
                      ? "#0d6efd"
                      : "white",
                  color:
                    currentPage === page
                      ? "white"
                      : "black"
                }}
              >
                {page}
              </button>

            ))}

          <button
            disabled={
              currentPage === totalPages || totalPages === 0
            }
            onClick={() =>
              setCurrentPage(
                currentPage + 1
              )
            }
            style={paginationButton}
          >
            Next
          </button>

        </div>

        {/* PAGE SIZE */}

        <div>

          <select
            value={itemsPerPage}
            onChange={(e) => {
              setItemsPerPage(
                Number(e.target.value)
              );
              setCurrentPage(1);
            }}
            style={{
              padding: "8px",
              borderRadius: "6px"
            }}
          >

            <option value={5}>
              5
            </option>

            <option value={10}>
              10
            </option>

            <option value={20}>
              20
            </option>

            <option value={50}>
              50
            </option>

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
  fontWeight: "bold"
};

const cellStyle = {
  padding: "15px",
  verticalAlign: "top",
  wordBreak: "break-all" 
};

const paginationButton = {
  marginRight: "8px",
  padding: "8px 14px",
  border: "1px solid #ccc",
  borderRadius: "6px",
  cursor: "pointer",
  background: "white"
};

export default PdfViewer;