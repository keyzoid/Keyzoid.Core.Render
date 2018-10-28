using Amazon.S3;
using HtmlAgilityPack;
using Keyzoid.Core.Render.Models;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Keyzoid.Core.Render.Utilities
{
    /// <summary>
    /// Utility class for pre-rendering a website.
    /// </summary>
    public class Render
    {
        public static string BaseUrl { get; set; }
        public static string ChromeExePath { get; set; }

        public static async Task<Dictionary<string, string>> Execute(SiteContent content, bool export = false)
        {
            try
            {
                var result = new Dictionary<string, string>();
                var routes = Route.GetRoutes(content);
                await GetContent(routes, export);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static async Task<List<Route>> Execute(string siteMapPath, bool export = false,
            bool upload = false, AmazonS3Client s3Client = null)
        {
            try
            {
                var routes = Route.GetRoutes(siteMapPath);
                var routesToLoad = new List<Route>();

                // Custom logic to exclude some nested routes from rendering
                foreach (var route in routes)
                {
                    if (route.Path.Contains("/albums"))
                    {
                        var uri = new Uri(route.Path);
                        if (uri.Segments.Length > 4)
                        {
                            continue;
                        }
                        routesToLoad.Add(route);
                    }
                    else
                    {
                        routesToLoad.Add(route);
                    }
                }

                var result = await GetContent(routesToLoad, export, s3Client);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static async Task<List<Route>> GetContent(List<Route> routes, bool exportHtml = false, AmazonS3Client s3Client = null)
        {
            try
            {
                var result = new List<Route>();
                var options = new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = ChromeExePath
                };
               
                using (var browser = await Puppeteer.LaunchAsync(options))
                {
                    foreach (var route in routes)
                    {
                        Console.WriteLine($"Loading page: {route.Path}");

                        using (var page = await browser.NewPageAsync())
                        {
                            var response = await page.GoToAsync(route.Path, new NavigationOptions
                            {
                                WaitUntil = new WaitUntilNavigation[]
                                {
                                    WaitUntilNavigation.Networkidle0
                                }
                            });

                            route.Content = await page.GetContentAsync();
                            
                            if (!string.IsNullOrEmpty(route.Content))
                            {
                                route.Content = Format(route.Content);
                            }

                            result.Add(route);

                            if (exportHtml && !string.IsNullOrEmpty(route.Content) && 
                                route.Uri != null && route.Query != "//" && !route.Query.Contains("sitemap"))
                            {
                                Console.WriteLine($"Exporting page: {route.Path}");
                                await FileManager.Export(route);
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Render error: {ex.Message}. {ex.StackTrace}.");
                return null;
            }
        }

        /// <summary>
        /// Custom formatting to clean up HTML output.
        /// </summary>
        /// <param name="content">The HTML content to format.</param>
        /// <returns>Formatted HTML.</returns>
        private static string Format(string content)
        {
            try
            {
                // Remove comments and AngularJS placeholders
                content = Regex.Replace(content, "<!--(.|\\s)*?-->", string.Empty);

                var html = new HtmlDocument();
                html.LoadHtml(content);

                // images: convert relative to full paths
                foreach (HtmlNode node in html.DocumentNode
                   .SelectNodes("//img[@src]"))
                {
                    var src = node.Attributes["src"].Value;

                    if (src.StartsWith('/'))
                        node.SetAttributeValue("src", $"{BaseUrl}{src}");
                    else
                        node.SetAttributeValue("src", $"{BaseUrl}/{src}");
                }

                // features: take first item from carousel slides
                var firstSlide = html.DocumentNode.SelectSingleNode("//div[contains(@class,'carousel-inner')]//a");

                if (firstSlide != null)
                {
                    var carousel = html.DocumentNode
                        .SelectSingleNode("//div[contains(@class,'carousel-inner')]");

                    carousel.InnerHtml = firstSlide.ParentNode.InnerHtml;
                }

                using (var stream = new MemoryStream())
                {
                    html.Save(stream);
                    stream.Position = 0;
                    var streamReader = new StreamReader(stream);
                    content = streamReader.ReadToEnd();
                }

                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Render.Format error: {ex.Message}. {ex.StackTrace}.");
                throw;
            }
        }
    }
}
