using Amazon.S3;
using Amazon.S3.Model;

namespace PHP.QARAdjustmentTool.API.Services
{
	public class S3StorageService
	{
		private readonly IAmazonS3 _s3Client;
		private readonly string _bucketName;

		public S3StorageService(IConfiguration configuration, IAmazonS3 s3Client)
		{
			_s3Client = s3Client;
			_bucketName = configuration["Storage:BucketName"];
		}

		// ============================================
		// GET FILE
		// ============================================
		public async Task<Stream?> GetFileAsync(string s3Key)
		{
			try
			{
				var request = new GetObjectRequest
				{
					BucketName = _bucketName,
					Key = s3Key
				};

				var response = await _s3Client.GetObjectAsync(request);
				return response.ResponseStream;
			}
			catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null;
			}
		}

		// ============================================
		// LIST ALL PDF FILES
		// ============================================
		public async Task<List<S3FileDto>> GetAllPdfFilesAsync()
		{
			var files = new List<S3FileDto>();
			string? continuationToken = null;

			do
			{
				var request = new ListObjectsV2Request
				{
					BucketName = _bucketName,
					ContinuationToken = continuationToken
				};

				var response = await _s3Client.ListObjectsV2Async(request);

				foreach (var obj in response.S3Objects)
				{
					if (obj.Key.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
					{
						files.Add(new S3FileDto
						{
							FileName = Path.GetFileName(obj.Key),
							S3Key = obj.Key
						});
					}
				}

				continuationToken = response.IsTruncated == true
					? response.NextContinuationToken
					: null;

			} while (continuationToken != null);

			return files;
		}

		// ============================================
		// CHECK FILE EXISTS
		// ============================================
		public async Task<bool> FileExistsAsync(string s3Key)
		{
			try
			{
				var request = new GetObjectMetadataRequest
				{
					BucketName = _bucketName,
					Key = s3Key
				};

				await _s3Client.GetObjectMetadataAsync(request);
				return true;
			}
			catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return false;
			}
		}
	}

	public class S3FileDto
	{
		public string FileName { get; set; }
		public string S3Key { get; set; }
	}
}