using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Keyzoid.Core.Render.Models
{
    public class Route
    {
        public string Path { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public static string LocalPath { get; set; }
        private Uri _uri;
#if DEBUG
        private static string _home = "http://localhost:11586";
#else
        private static string _home = "http://www.YOUR-URL-HERE.com";
#endif

        public Uri Uri
        {
            get
            {
                if (_uri == null)
                    _uri = new Uri(Path);

                return _uri;
            }
        }

        public string Query
        {
            get
            {
                return Uri.PathAndQuery;
            }
        }

        public bool IsIndex
        {
            get
            {
                return ContentType == "home" || ContentType == Query.Substring(1) || Query == "/";
            }
        }

        public string Folder
        {
            get
            {
                var folder = Query.Replace("/", "\\");
                return folder;
            }
        }

        public string File
        {
            get
            {
                return IsIndex ? $"{LocalPath}\\index.html" : $"{LocalPath}{Folder}.html";
            }
        }

        public static string Home => _home;

        /// <summary>
        /// Gets the site routes from the site content.
        /// </summary>
        /// <param name="content">The website content.</param>
        /// <returns>A collection of routes.</returns>
        /// <remarks>Note this method is very customized, creating parent and child routes as needed for the website.</remarks>
        public static List<Route> GetRoutes(SiteContent content)
        {
            try
            {
                var routes = new List<Route>();

                routes.Add(new Route
                {
                    Path = Home,
                    ContentType = "home"
                });
                routes.Add(new Route
                {
                    Path = $"{Home}/albums",
                    ContentType = "albums"
                });

                if (content.PhotoAlbums != null)
                {
                    foreach (var album in content.PhotoAlbums)
                    {
                        var url = $"{Home}/albums/{album.uniqueName}";
                        routes.Add(new Route
                        {
                            Path = url,
                            ContentType = "albums"
                        });
                    }
                }

                routes.Add(new Route
                {
                    Path = $"{Home}/music",
                    ContentType = "music"
                });

                routes.Add(new Route
                {
                    Path = $"{Home}/music/playlists",
                    ContentType = "playlists"
                });

                if (content.Playlists != null)
                {
                    foreach (var playlist in content.Playlists)
                    {
                        var url = $"{Home}/music/playlists/{playlist.uniqueName}";
                        routes.Add(new Route
                        {
                            Path = url,
                            ContentType = "playlists"
                        });
                    }
                }

                routes.Add(new Route
                {
                    Path = $"{Home}/music/search",
                    ContentType = "search"
                });

                routes.Add(new Route
                {
                    Path = $"{Home}/music/tabs",
                    ContentType = "tabs"
                });

                if (content.Tabs != null)
                {
                    foreach (var tab in content.Tabs)
                    {
                        var url = $"{Home}/music/tabs/{tab.createdOn}";
                        routes.Add(new Route
                        {
                            Path = url,
                            ContentType = "tabs"
                        });
                    }
                }

                routes.Add(new Route
                {
                    Path = $"{Home}/projects",
                    ContentType = "projects"
                });

                routes.Add(new Route
                {
                    Path = $"{Home}/art",
                    ContentType = "art"
                });

                routes.Add(new Route
                {
                    Path = $"{Home}/contact",
                    ContentType = "contact"
                });
                
                routes.Add(new Route
                {
                    Path = $"{Home}/code",
                    ContentType = "code"
                });

                routes.Add(new Route
                {
                    Path = $"{Home}/blog",
                    ContentType = "blog"
                });

                if (content.Blogs != null)
                {
                    foreach (var blog in content.Blogs)
                    {
                        var url = $"{Home}/blogs/{blog.uniqueName}";
                        routes.Add(new Route
                        {
                            Path = url,
                            ContentType = "blog"
                        });
                    }
                }

                routes.Add(new Route
                {
                    Path = $"{Home}/blog/search",
                    ContentType = "search"
                });

                routes.Add(new Route
                {
                    Path = $"{Home}/bookmarks",
                    ContentType = "bookmarks"
                });

                routes.Add(new Route
                {
                    Path = $"{Home}/bookmarks/search",
                    ContentType = "search"
                });

                if (content.Bookmarks != null)
                {
                    foreach (var bookmark in content.Bookmarks)
                    {
                        var url = $"{Home}/bookmarks/{bookmark.createdOn}";
                        routes.Add(new Route
                        {
                            Path = url,
                            ContentType = "bookmarks"
                        });
                    }
                }

                routes.Add(new Route
                {
                    Path = $"{Home}/tags",
                    ContentType = "tags"
                });

                if (content.Tags != null)
                {
                    foreach (var tag in content.Tags)
                    {
                        var url = $"{Home}/tags/{tag.uniqueName}";
                        routes.Add(new Route
                        {
                            Path = url,
                            ContentType = "tags"
                        });
                    }
                }

                routes.Add(new Route
                {
                    Path = $"{Home}/store",
                    ContentType = "store"
                });

                routes.Add(new Route
                {
                    Path = $"{Home}/error",
                    ContentType = "error"
                });

                return routes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<Route> GetRoutes(string siteMapPath)
        {
            try
            {
                var routes = new List<Route>();

                using (var reader = new StreamReader(siteMapPath))
                {
                    var xml = reader.ReadToEnd();
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);

                    XmlNodeList xnList = xmlDoc.GetElementsByTagName("url");

                    foreach (XmlNode node in xnList)
                    {
                        routes.Add(new Route
                        {
                            Path = node["loc"].InnerText
                        });
                    }
                }

                return routes;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
