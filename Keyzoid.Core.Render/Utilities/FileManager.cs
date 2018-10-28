using Amazon.S3;
using Amazon.S3.Model;
using Keyzoid.Core.Render.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Keyzoid.Core.Render.Utilities
{
    /// <summary>
    /// Utility class for interacting with files.
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// Exports a route to a local file.
        /// </summary>
        /// <param name="route">The route to export.</param>
        /// <returns>An async Task.</returns>
        public static async Task Export(Route route)
        {
            try
            {
                if (!Directory.Exists(Route.LocalPath))
                    Directory.CreateDirectory(Route.LocalPath);

                var fi = new FileInfo(route.File);
                if (!fi.Directory.Exists)
                {
                    Directory.CreateDirectory(fi.DirectoryName);
                }

                using (var writer = new StreamWriter(route.File))
                {
                    await writer.WriteLineAsync(route.Content);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Uploads route content to S3.
        /// </summary>
        /// <param name="route">The route with content to upload.</param>
        /// <param name="s3Bucket">The S3 bucket to which to upload.</param>
        /// <param name="s3Client">The S3 client to use in upload operations.</param>
        /// <returns>An async task.</returns>
        /// <remarks>Note we are uploading HTML content without a file extension.</remarks>
        public static async Task Upload(Route route, string s3Bucket, AmazonS3Client s3Client)
        {
            try
            {
                if (s3Client == null)
                {
                    s3Client = new AmazonS3Client();
                }

                var s3Key = string.Join(@"/", route.Uri.Segments);

                if (string.IsNullOrEmpty(route.Query) || route.Query == "/")
                {
                    return;
                }

                s3Key = s3Key.Replace("//", "/").TrimStart('/');

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(route.Content)))
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = s3Bucket,
                        ContentType = "text/html",
                        InputStream = stream,
                        Key = s3Key
                    };

                    var result = await s3Client.PutObjectAsync(request);

                    if (result.HttpStatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(result.ETag))
                    {
                        Console.WriteLine($"Error while uploading file to key: {s3Key}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while uploading file: {ex.Message} - {ex.StackTrace}");
                throw;
            }
        }
    }
}
