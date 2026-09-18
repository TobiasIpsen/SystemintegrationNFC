using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public FileController(IAmazonS3 s3Client, IConfiguration config)
        {
            _s3Client = s3Client;
            _bucketName = config["SeaweedFS:BucketName"] ?? "app-files";
        }

        [HttpPost("generate-upload-url")]
        public IActionResult GetPresignedUploadUrl(/*[FromBody] UploadRequest request*/)
        {
            //Console.WriteLine($"-----Request Filename: {request.FileName} \n-----Request ContentType: {request.ContentType}");
            Guid imageGuid = Guid.NewGuid();
            var objectKey = $"user-upload/{imageGuid}";

            var presignedRequest = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(15),
                //ContentType = request.ContentType
            };

            string uploadUrl = _s3Client.GetPreSignedURL(presignedRequest);

            if (uploadUrl.StartsWith("https://"))
            {
                uploadUrl = "http://" + uploadUrl.Substring(8);
            }

            Console.WriteLine(new
            {
                UploadUrl = uploadUrl,
                ObjectKey = objectKey
            });

            return Ok(new
            {
                UploadUrl = uploadUrl,
                ObjectKey = objectKey,
                ImageGuid = imageGuid
            });
        }
    }

    public record UploadRequest(string FileName, string ContentType);
}
