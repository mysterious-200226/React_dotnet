using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PHP.QARAdjustmentTool.API.Services    // ← Services namespace
{
	public class S3HealthCheck : IHealthCheck
	{
		private readonly IAmazonS3 _s3Client;
		private readonly IConfiguration _configuration;

		public S3HealthCheck(IAmazonS3 s3Client, IConfiguration configuration)
		{
			_s3Client = s3Client;
			_configuration = configuration;
		}

		public async Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var bucketName = _configuration["Storage:BucketName"];

				var request = new ListObjectsV2Request
				{
					BucketName = bucketName,
					MaxKeys = 1
				};

				await _s3Client.ListObjectsV2Async(request, cancellationToken);

				return HealthCheckResult.Healthy("S3 bucket is accessible");
			}
			catch (Exception ex)
			{
				return HealthCheckResult.Unhealthy("S3 bucket is NOT accessible", ex);
			}
		}
	}
}