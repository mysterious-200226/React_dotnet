using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PHP.QARAdjustmentTool.API.Services;
using PHP.QARAdjustmentTool.API.Test;

[Route("api/[controller]")]
[ApiController]
public class S3StorageController : ControllerBase
{
	private readonly IConfiguration _configuration;
	private readonly QarContext _dbContext;
	private readonly IMemoryCache _cache;
	private readonly S3StorageService _s3Service;
	private readonly ILogger<S3StorageController> _logger;

	public S3StorageController(
		QarContext dbContext,
		IConfiguration configuration,
		ILogger<S3StorageController> logger,
		IMemoryCache cache,
		S3StorageService s3Service)  // ← inject S3 service
	{
		_dbContext = dbContext;
		_configuration = configuration;
		_logger = logger;
		_cache = cache;
		_s3Service = s3Service;
	}

	// ============================================
	// REPLACES GetSamplePdf
	// ============================================
	[HttpGet]
	[Route("GetSamplePdf")]
	public async Task<IActionResult> GetSamplePdf()
	{
		try
		{
			var s3Key =
				"QAR_Drive/" +
				"ASAS Health - 923717064/" +
				"1. Training & Reference/" +
				"sample.pdf";

			var stream = await _s3Service.GetFileAsync(s3Key);

			if (stream == null)
				return NotFound("File not found");

			return File(stream, "application/pdf", "sample.pdf");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in GetSamplePdf");
			return StatusCode(500, ex.Message);
		}
	}

	// ============================================
	// REPLACES GetBlobFile
	// ============================================
	[HttpGet]
	[Route("GetS3File")]
	public async Task<IActionResult> GetS3File(string s3Key)
	{
		try
		{
			var stream = await _s3Service.GetFileAsync(s3Key);

			if (stream == null)
				return NotFound("File not found");

			return File(stream, "application/pdf", Path.GetFileName(s3Key));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting S3 file");
			return StatusCode(500, ex.Message);
		}
	}

	// ============================================
	// REPLACES GetAllBlobFiles
	// ============================================
	[HttpGet]
	[Route("GetAllS3Files")]
	public async Task<IActionResult> GetAllS3Files()
	{
		try
		{
			var files = await _s3Service.GetAllPdfFilesAsync();
			return Ok(files);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting S3 files");
			return StatusCode(500, ex.Message);
		}
	}
}