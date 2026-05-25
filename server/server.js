const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const path = require('path');
const http = require('http');
const https = require('https');
const { Server } = require('socket.io');
const fetch = require('node-fetch');
const fs = require('fs');
const winston = require('winston');
const DailyRotateFile = require('winston-daily-rotate-file');

const serverConfig = require('./server-config');
const apiUrl = `${serverConfig.API_URL}`;

// ✅ Use Linux-compatible log path inside container
const logDir = process.env.LOG_DIR || '/app/logs';

const app = express();

// ✅ Port must match Azure ingress target port (3001)
const PORT = process.env.PORT || 3001;

// ✅ React app URL from environment variable
const REACT_APP_URL = process.env.REACT_APP_URL || 'https://ui.logicandlaunch.me';

console.log('Server file directory:', __dirname);
console.log('React App URL:', REACT_APP_URL);
console.log('API URL:', apiUrl);

app.use(cors());
app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());

// ✅ Create both HTTP and HTTPS agents to handle protocol dynamically
const httpAgent = new http.Agent();
const httpsAgent = new https.Agent({
    rejectUnauthorized: false
});

/* ------------------- LOG DIRECTORY ------------------- */

// ✅ Works on both Windows and Linux
if (!fs.existsSync(logDir)) {
    fs.mkdirSync(logDir, { recursive: true });
}

/* ------------------- LOGGER CONFIG ------------------- */

const logger = winston.createLogger({
    level: 'info',
    format: winston.format.combine(
        winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
        winston.format.printf(({ timestamp, level, message }) => {
            return `${timestamp} [${level.toUpperCase()}] : ${message}`;
        })
    ),
    transports: [
        // ✅ Also log to console so Azure Log Stream can show it
        new winston.transports.Console(),
        new DailyRotateFile({
            dirname: logDir,
            filename: 'QAR_ExpressLogs_%DATE%.txt',
            datePattern: 'YYYY-MM-DD',
            zippedArchive: false,
            maxFiles: '30d'
        })
    ]
});

/* ------------------- SAML FIELD EXTRACTION ------------------- */

function extractSamlFields(xml) {
    const getValue = (attrName) => {
        const regex = new RegExp(
            `<saml:Attribute Name="${attrName}">[\\s\\S]*?<saml:AttributeValue[^>]*>(.*?)</saml:AttributeValue>`
        );
        const match = xml.match(regex);
        return match ? match[1] : "N/A";
    };

    return {
        userId: getValue("AvailityUserId"),
        emailId: getValue("UserEmail"),
        roles: getValue("Roles"),
        organizationTaxId: getValue("OrganizationTaxID")
    };
}

/* ------------------- .NET API CALL ------------------- */

const callDotNetApi = async (samlResponse) => {
    const backendUrl = apiUrl + "/ValidateSaml";

    const response = await fetch(backendUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ samlResponse }),
        // ✅ Dynamically choose the correct agent based on the URL protocol
        agent: backendUrl.startsWith('https') ? httpsAgent : httpAgent
    });

    if (!response.ok) {
        const errorMessage = await response.text();
        logger.error(`API response error: ${errorMessage}`);
        throw new Error(`Failed to fetch token. ${response.status}: ${response.statusText}`);
    }

    return await response.json();
};

/* ------------------- ACS ENDPOINT ------------------- */

app.post('/acs', async (req, res) => {
    logger.info(`\n----- New SSO Request -----`);

    const { SAMLResponse } = req.body;

    if (!SAMLResponse) {
        logger.error('No SAMLResponse in request body');
        return res.status(400).json({ error: 'SAMLResponse is required' });
    }

    const decodedSaml = Buffer.from(SAMLResponse, 'base64').toString('utf8');
    const samlFields = extractSamlFields(decodedSaml);

    logger.info(
        `SAML Login | UserId: ${samlFields.userId} | Email: ${samlFields.emailId} | OrgTaxId: ${samlFields.organizationTaxId} | Roles: ${samlFields.roles}`
    );

    try {
        const backendData = await callDotNetApi(SAMLResponse);
        const token = backendData.token;

        logger.info("JWT token generated successfully");

        // ✅ Redirect to React container URL, not Express host
        const reactAppUrl = `${REACT_APP_URL}/sso-complete?token=${encodeURIComponent(token)}`;

        console.log('Redirecting to React app at:', reactAppUrl);
        logger.info(`Redirecting to React app: ${reactAppUrl}`);

        return res.redirect(302, reactAppUrl);

    } catch (error) {
        logger.error(`Error fetching token: ${error.message}`);
        return res.status(500).json({
            error: 'Failed to retrieve token from .NET API'
        });
    }
});

// ✅ Health check endpoint for Azure / AWS ALB
app.get('/health', (req, res) => {
    res.status(200).json({ status: 'ok', port: PORT });
});

// ✅ Health check endpoint for Azure / AWS ALB
app.get('/acs/health', (req, res) => {
    res.status(200).json({ status: 'ok', port: PORT });
});

// ✅ REMOVED static file serving — Nginx handles React now

/* ------------------- SERVER ------------------- */

const server = http.createServer(app);

const io = new Server(server, {
    cors: { origin: '*' }
});

server.listen(PORT, () => {
    console.log(`Express server running on port ${PORT}`);
    logger.info(`Express server running on port ${PORT}`);
});