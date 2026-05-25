using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PHP.QARAdjustmentTool.API.Test;
using Azure.Storage.Blobs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;



namespace QARAdjustmentTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QARAdjustmentToolController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly QarContext _dbContext;
        private readonly IMemoryCache _cache;
        private int _fileIdCounter;
        private static string _samlResponse;
        private readonly errMessage _errMessage;

        private readonly ILogger<QARAdjustmentToolController> _logger;

        public QARAdjustmentToolController(QarContext dbContext, IConfiguration configuration,
            ILogger<QARAdjustmentToolController> logger, IMemoryCache cache, IOptions<errMessage> errMessage)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
            _errMessage = errMessage.Value;
        }

        private Dictionary<string, string> ExtractAttributesFromSamlResponse(string samlResponse)
        {
            // Ensure the SAML response contains the required namespaces
            string samlResponseWithNamespace = samlResponse.Replace(
                "<samlp:Response",
                "<samlp:Response xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\""
            );

            // Load the SAML response as XML
            XDocument xmlDoc = XDocument.Parse(samlResponseWithNamespace);

            // Define the SAML namespace
            XNamespace saml = "urn:oasis:names:tc:SAML:2.0:assertion";

            // Extract all attributes by local name
            var attributeElements = xmlDoc.Descendants(saml + "Attribute");

            var attributes = new Dictionary<string, string>();

            foreach (var attr in attributeElements)
            {
                // Get the Name attribute
                var name = attr.Attribute("Name")?.Value;

                // Get the AttributeValue
                var value = attr.Elements(saml + "AttributeValue").FirstOrDefault()?.Value;

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    attributes[name] = value;
                }
            }


            return attributes;
        }

        /// <summary>
        /// Stores SAML response. 
        /// </summary>
        /// <returns>
        /// Stores SAML response.
        /// </returns>
        /// <response code="200">Stores SAML response..</response>
        /// <response code="500">If there is an error retrieving the data</response>
        [HttpPost]
        [Route("ValidateSaml")]
        public IActionResult Post([FromBody] SamlRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.samlResponse))
                {
                    string returnMsg = _configuration["errMessage:SamlError"];
                    return BadRequest(returnMsg);
                }

                _logger.LogInformation("SAML request received", request.samlResponse);

                // Decode the SAML response from Base64
                byte[] samlData = Convert.FromBase64String(request.samlResponse);
                string samlXml = Encoding.UTF8.GetString(samlData);

                // Store the decoded SAML response
                _samlResponse = samlXml;

                _logger.LogInformation("SAML response  decoded successfully. SamlResponse: {SamlResponse}", _samlResponse);

                var attributes = ExtractAttributesFromSamlResponse(_samlResponse);


                // Try to get UserEmail and UserFirstName from attributes
                if (attributes.TryGetValue("UserEmail", out string userEmail))
                {
                    GlobalUserData.UserEmail = userEmail;
                }
                if (attributes.TryGetValue("UserFirstName", out string userFirstName))
                {
                    GlobalUserData.UserFirstName = userFirstName;
                }
                if (attributes.TryGetValue("UserLastName", out string UserLastName))
                {
                    GlobalUserData.UserLastName = UserLastName;
                }
                if (attributes.TryGetValue("OrganizationTaxID", out string OrganizationTaxID))
                {
                    GlobalUserData.OrganizationTaxId = OrganizationTaxID;
                }
                if (attributes.TryGetValue("OrganizationNPI", out string OrganizationNPI))
                {
                    GlobalUserData.OrganizationNpi = OrganizationNPI;
                }
                if (attributes.TryGetValue("Roles", out string Roles))
                {
                    GlobalUserData.roles = Roles;
                }
                if (attributes.TryGetValue("AvailityUserId", out string AvailityUserId))
                {
                    GlobalUserData.availityUserId = AvailityUserId;
                }
                _logger.LogInformation("Step 1. ValidateSaml API started for User: {UserEmail}", userEmail);

                _logger.LogInformation("SAML Attributes extracted. UserEmail: {UserEmail}, AvailityUserId: {AvailityUserId}, OrganizationTaxID:{OrganizationTaxID}", userEmail, AvailityUserId, OrganizationTaxID);

                // Define the output parameter for the UserId
                var newUserIdParam = new SqlParameter
                {
                    ParameterName = "@NewUserId",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                string loadUsersFromQAR = _configuration["Procedures:LoadUsersFromQAR"];

                _logger.LogInformation("Executing stored procedure {Procedure} for user {Email}",
                 loadUsersFromQAR, userEmail);

                // Call the stored procedure
                var result = _dbContext.Database.ExecuteSqlRaw(
                    "EXEC [dbo].[" + loadUsersFromQAR + "] @UserEmail, @UserFirstName, @UserLastName, @OrganizationTaxId, @OrganizationNpi, @Roles, @AvailityUserId, @NewUserId OUTPUT",
                    new SqlParameter("@UserEmail", userEmail),
                    new SqlParameter("@UserFirstName", userFirstName),
                    new SqlParameter("@UserLastName", UserLastName),
                    new SqlParameter("@OrganizationTaxId", OrganizationTaxID),
                   new SqlParameter("@OrganizationNpi", string.IsNullOrEmpty(OrganizationNPI) ? DBNull.Value : OrganizationNPI),
                    new SqlParameter("@Roles", Roles),
                    new SqlParameter("@AvailityUserId", AvailityUserId),
                    newUserIdParam
                );

                // Retrieve the output parameter value
                var newUserId = (int)newUserIdParam.Value;

                _logger.LogInformation("UserId returned from database: {UserId}", newUserId);

                if (newUserId <= 0)
                {
                    string returnMsg = _configuration["errMessage:NoData"];
                    _logger.LogWarning(
                    "User creation/upsert failed. Invalid UserId returned: {UserId}. Email: {Email}, TIN: {TIN}",
                    newUserId,
                    userEmail,
                    OrganizationTaxID
   );
                    return BadRequest(returnMsg);
                }

                var tin = _dbContext.QaradjustmentToolAvailityUsers
                    .Where(user => user.QaradjustmentToolAvailityUsersUserId == newUserId)
                    .Select(user => user.OrganizationTaxId)
                    .FirstOrDefault();

                //check if TaxID matched T drive TIN
                bool exists = _dbContext.QaradjustmentToolProviderGroups.Any(record => record.Tin == tin);

                _logger.LogInformation(
                "TIN access check for UserId: {UserId}, TIN: {TIN}, Exists: {Exists}", newUserId, tin, exists);

                if (exists)
                {

                    SymmetricSecurityKey key;
                    SigningCredentials signIn;
                    JwtSecurityToken token;
                    Claim[] claims;
                    string tokenValue = string.Empty;
                    claims = new[]
                      {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("userID", newUserId.ToString()),
                    new Claim("userFirstName", userFirstName),
                    new Claim("UserLastName", UserLastName),
                    new Claim("Roles", Roles)
                };

                    key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"]);

                    token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                        signingCredentials: signIn
                        );
                    tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
                    _logger.LogInformation("JWT token generated successfully for UserId {UserId}", newUserId);
                    _logger.LogInformation("ValidateSaml API completed successfully for user {UserId}", newUserId);
                    return Ok(new { token = tokenValue });
                }

                else
                {
                    _logger.LogError("JWT token not generated for UserId {UserId}", newUserId);
                    string returnMsg = _configuration["errMessage:Access"];
                    return BadRequest(returnMsg);
                }
            }
            catch (Exception ex)
            {
                // Log the exception using Serilog
                string returnMsg = _configuration["errMessage:stringLogMessageSAML"];
                _logger.LogError(ex, returnMsg);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = _errMessage, details = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetSamplePdf")]
        public async Task<IActionResult> GetSamplePdf()
        {
            try
            {
                var connectionString =
                    _configuration["Storage:ConnectionString"];

                var containerName =
                    _configuration["Storage:ContainerName"];

                var blobPath =
                    "QAR_Drive/" +
                    "ASAS Health - 923717064/" +
                    "1. Training & Reference/" +
                    "sample.pdf";

                var blobServiceClient =
                    new BlobServiceClient(connectionString);

                var containerClient =
                    blobServiceClient.GetBlobContainerClient(containerName);

                var blobClient =
                    containerClient.GetBlobClient(blobPath);

                if (!await blobClient.ExistsAsync())
                    return NotFound("File not found");

                var download =
                    await blobClient.DownloadStreamingAsync();

                return File(
                    download.Value.Content,
                    "application/pdf",
                    "sample.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSamplePdf");

                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetBlobFile")]
        public async Task<IActionResult> GetBlobFile(string blobPath)
        {
            try
            {
                var connectionString =
                    _configuration["Storage:ConnectionString"];

                var containerName =
                    _configuration["Storage:ContainerName"];

                var blobServiceClient =
                    new BlobServiceClient(connectionString);

                var containerClient =
                    blobServiceClient.GetBlobContainerClient(containerName);

                var blobClient =
                    containerClient.GetBlobClient(blobPath);

                if (!await blobClient.ExistsAsync())
                    return NotFound("File not found");

                var download =
                    await blobClient.DownloadStreamingAsync();

                return File(
                    download.Value.Content,
                    "application/pdf",
                    Path.GetFileName(blobPath)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blob file");

                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetAllBlobFiles")]
        public async Task<IActionResult> GetAllBlobFiles()
        {
            try
            {
                var connectionString =
                    _configuration["Storage:ConnectionString"];

                var containerName =
                    _configuration["Storage:ContainerName"];

                var blobServiceClient =
                    new BlobServiceClient(connectionString);

                var containerClient =
                    blobServiceClient.GetBlobContainerClient(containerName);

                var files = new List<BlobFileDto>();

                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    // only pdfs
                    if (blobItem.Name.EndsWith(".pdf"))
                    {
                        files.Add(new BlobFileDto
                        {
                            FileName = Path.GetFileName(blobItem.Name),
                            BlobPath = blobItem.Name
                        });
                    }
                }

                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blob files");

                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("TestDb")]
        public IActionResult TestDb()
        {
            try
            {
                var canConnect = _dbContext.Database.CanConnect();

                return Ok(new
                {
                    Connected = canConnect
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class SamlRequest
    {
        public string samlResponse { get; set; }
    }

    public static class GlobalUserData
    {
        public static string UserEmail { get; set; }
        public static string UserFirstName { get; set; }
        public static string UserLastName { get; set; }
        public static string OrganizationTaxId { get; set; }
        public static string OrganizationNpi { get; set; }
        public static string roles { get; set; }
        public static string availityUserId { get; set; }
    }
    public class BlobFileDto
    {
        public string FileName { get; set; }
        public string BlobPath { get; set; }
    }

    public class errMessage
    {
        public string defaultMessage { get; set; }
    }
}
