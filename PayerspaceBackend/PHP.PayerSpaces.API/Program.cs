using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PHP.QARAdjustmentTool.API.Models;
using PHP.QARAdjustmentTool.API.Services;
using PHP.QARAdjustmentTool.API.Test;
using Serilog;
using System.Reflection;
using System.Text;
using Microsoft.Identity.Web;
using Amazon;
using Amazon.Extensions.NETCore.Setup;

var builder = WebApplication.CreateBuilder(args);


// ======================================================
// LOAD AZURE KEY VAULT
// ======================================================

/*var keyVaultUri =
    builder.Configuration["KeyVault:VaultUri"];

var managedIdentityClientId =
    builder.Configuration["KeyVault:ManagedIdentityClientId"];

if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    var credential =
        new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId =
                    managedIdentityClientId
            });

    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        credential);
}
*/

// ======================================================
// READ CONFIGURATION
// ======================================================

var jwtKey =
	builder.Configuration["Jwt:Key"];

var connectionString =
	builder.Configuration.GetConnectionString("DefaultConnection");

var storageConnectionString =
	builder.Configuration["Storage:ConnectionString"];

var storageBucketName =
	builder.Configuration["Storage:BucketName"]; // Changed to S3 Bucket

var corsOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();


// ======================================================
// VALIDATE REQUIRED CONFIGURATION
// ======================================================

if (string.IsNullOrWhiteSpace(jwtKey))
{
	throw new Exception("JWT Key is missing from configuration.");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
	throw new Exception("Database connection string is missing.");
}

if (string.IsNullOrWhiteSpace(storageBucketName))
{
	throw new Exception("AWS S3 Bucket Name is missing.");
}



// ======================================================
// SERILOG CONFIGURATION
// ======================================================

Log.Logger =
    new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console()
        .CreateLogger();

builder.Host.UseSerilog();


// ======================================================
// SERVICES
// ======================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient();

builder.Services.AddAuthorization();

// ADDED: Configure AWS options (region) and register services
var awsRegion = builder.Configuration["Storage:Region"] ?? "us-east-1";
var awsOptions = new AWSOptions
{
    Region = RegionEndpoint.GetBySystemName(awsRegion)
};

builder.Services.AddDefaultAWSOptions(awsOptions);

// Register Health Checks for AWS ELB and S3 health probe
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<S3HealthCheck>();
builder.Services.AddHealthChecks().AddCheck<S3HealthCheck>("s3_health");

builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();
builder.Services.AddScoped<S3StorageService>();



// ======================================================
// DATABASE CONFIGURATION
// ======================================================

builder.Services.AddDbContext<QarContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});


// ======================================================
// STORAGE SETTINGS
// ======================================================

builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("Storage"));


// ======================================================
// CORS CONFIGURATION
// ======================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ======================================================
// AUTHENTICATION
// ======================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })

    // EXISTING JWT

    .AddJwtBearer("CustomJwt", options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });
    // AZURE AD

   




// ======================================================
// SWAGGER CONFIGURATION
// ======================================================

builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(
        Path.Combine(
            AppContext.BaseDirectory,
            $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter: Bearer {token}"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});


// ======================================================
// BUILD APPLICATION
// ======================================================

var app = builder.Build();


// ======================================================
// MIDDLEWARE PIPELINE
// ======================================================

app.UseSwagger();

app.UseSwaggerUI();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("ok");
    }
});

app.Run();