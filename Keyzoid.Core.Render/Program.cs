using Amazon;
using Amazon.DynamoDBv2;
using Keyzoid.Core.Render.Models;
using Keyzoid.Core.Render.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Amazon.S3;

namespace Keyzoid.Core.Render
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Load configuration
                IConfiguration config = new ConfigurationBuilder()
                  .AddJsonFile("appsettings.json", true, true)
                  .Build();

                var sitemapBaseUrl = config["sitemapBaseUrl"];
                var sitemapOutputPath = config["sitemapOutputPath"];
                var renderUrl = config["renderUrl"];
                var awsKey = config["awsKey"];
                var awsSecret = config["awsSecret"];
                var awsRegion = config["awsRegion"];
                var uploadToAwsRaw = config["uploadToAws"];
                var success = bool.TryParse(uploadToAwsRaw, out var uploadToAws);
                var chromeExePath = config["chromeExePath"];
                var localOutputPathHtml = $"{config["localHtmlOutputPath"]}{DateTime.Now.ToString("yyyyMMdd")}";
                var dynamoTableName = config["dynamoTableName"];

                Utilities.Render.BaseUrl = renderUrl;
                Utilities.Render.ChromeExePath = chromeExePath;
                Route.LocalPath = localOutputPathHtml;

                var dynamoManager = new DynamoManager(new AmazonDynamoDBClient(awsKey,
                    awsSecret, RegionEndpoint.GetBySystemName(awsRegion)), dynamoTableName);
                var s3Client = new AmazonS3Client(awsKey, awsSecret, RegionEndpoint.GetBySystemName(awsRegion));

                // Get all site content from DynamoDB
                var siteContent = Task.Run(async () => await dynamoManager.GetContent()).Result;

                // Generate the site map
                var siteMapPath = MapSite(siteContent, sitemapOutputPath, sitemapBaseUrl);

                // Render the site
                var renderedContent = Task.Run(async () => await Utilities.Render.Execute(siteMapPath, true, success && uploadToAws, s3Client)).Result;

                if (!success || !uploadToAws)
                {
                    return;
                }

                // Upload the rendered content to S3
                foreach (var route in renderedContent)
                {
                    if (route.Uri != null && route.Query != "//" &&
                        !route.Query.Contains("sitemap") && !string.IsNullOrEmpty(route.Content))
                    {
                        Console.WriteLine($"Uploading page content: {route.Query}");
                        Task.Run(async () => await FileManager.Upload(route, sitemapBaseUrl, s3Client)).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while rendering. {ex.Message} {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Generates an XML site map.
        /// </summary>
        /// <param name="siteContent">The site content with pre-rendered HTML.</param>
        /// <param name="outputPath">The local output path to write the site map.</param>
        /// <param name="baseUrl">The base Url to use in generating the site map.</param>
        /// <returns></returns>
        private static string MapSite(SiteContent siteContent, string outputPath, string baseUrl)
        {
            try
            {
                var siteMapGenerator = new SitemapGenerator();
                var siteMap1 = siteMapGenerator.Generate(baseUrl, siteContent);
                var siteMapPath = $"{outputPath}\\sitemap.xml";
                using (var writer = new StreamWriter(siteMapPath))
                {
                    writer.WriteLine(siteMap1);
                }
                return siteMapPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while mapping site: {ex.Message} - {ex.StackTrace}");
                throw;
            }
        }
    }
}
