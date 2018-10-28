using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Keyzoid.Core.Render.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Keyzoid.Core.Render.Models.Enums;

namespace Keyzoid.Core.Render.Utilities
{
    /// <summary>
    /// Represents a utility class for interacting with DynamoDB.
    /// </summary>
    public class DynamoManager
    {
        #region Fields

        private readonly AmazonDynamoDBClient _client;
        private readonly string _tableName;

        #endregion

        #region Contructors

        public DynamoManager(AmazonDynamoDBClient client, string tableName)
        {
            _client = client;
            _tableName = tableName;
        }

        #endregion

        #region Methods (Public)

        public async Task<SiteContent> GetContent()
        {
            try
            {
                var contentTypes = Enum.GetValues(typeof(ContentType));
                Dictionary<string, AttributeValue> lastKeyEvaluated = null;
                var siteContent = new SiteContent();
                var blogs = new List<Blog>();
                var bookmarks = new List<Bookmark>();
                var tabs = new List<Bookmark>();
                var photoAlbums = new List<PhotoAlbum>();
                var playlists = new List<Playlist>();
                var tags = new List<Models.Tag>();
                var blogViews = new List<Blog>();
                var photoAlbumViews = new List<PhotoAlbum>();
                var playlistViews = new List<Playlist>();
                var statistics = new List<ContentCount>();

                foreach (var contentType in contentTypes)
                {
                    var cType = (ContentType)contentType;

                    do
                    {
                        var request = new QueryRequest
                        {
                            TableName = _tableName,
                            KeyConditionExpression = "ContentType = :ContentType",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                            {
                                {
                                    ":ContentType", new AttributeValue {S = cType.ToString()}
                                }
                            },
                            ScanIndexForward = false
                        };

                        if (lastKeyEvaluated != null && lastKeyEvaluated.Any())
                        {
                            request.ExclusiveStartKey = lastKeyEvaluated;
                        }

                        var response = await _client.QueryAsync(request);

                        lastKeyEvaluated = response.LastEvaluatedKey;

                        if (response?.Items == null || !response.Items.Any())
                        {
                            continue;
                        }

                        var inactiveItems = 0;

                        foreach (var item in response.Items)
                        {
                            switch (cType)
                            {
                                case ContentType.Blog:

                                    var b = MapItemToBlog(item);

                                    if (b.isActive.HasValue && !b.isActive.Value)
                                    {
                                        inactiveItems++;
                                        continue;
                                    }

                                    blogs.Add(b);

                                    break;
                                case ContentType.Bookmark:

                                    var bo = MapItemToBookmark(item);

                                    if (bo.isActive.HasValue && !bo.isActive.Value)
                                    {
                                        inactiveItems++;
                                        continue;
                                    }

                                    bookmarks.Add(bo);

                                    break;
                                case ContentType.Tab:

                                    var t = MapItemToBookmark(item);

                                    if (t.isActive.HasValue && !t.isActive.Value)
                                    {
                                        inactiveItems++;
                                        continue;
                                    }

                                    tabs.Add(t);

                                    break;
                                case ContentType.Message:
                                    break;
                                case ContentType.PhotoAlbum:

                                    var a = MapItemToPhotoAlbum(item);

                                    if (a.isActive.HasValue && !a.isActive.Value)
                                    {
                                        inactiveItems++;
                                        continue;
                                    }

                                    photoAlbums.Add(a);

                                    break;
                                case ContentType.Playlist:

                                    var p = MapItemToPlaylist(item);

                                    if (p.isActive.HasValue && !p.isActive.Value)
                                    {
                                        inactiveItems++;
                                        continue;
                                    }

                                    playlists.Add(p);

                                    break;
                                case ContentType.Tag:

                                    var tag = MapItemToTag(item);
                                    tags.Add(tag);

                                    break;
                                case ContentType.BlogView:

                                    var bv = MapItemToBlog(item);

                                    if (bv.isActive.HasValue && !bv.isActive.Value)
                                    {
                                        inactiveItems++;
                                        continue;
                                    }

                                    blogViews.Add(bv);

                                    break;
                                case ContentType.PhotoAlbumView:

                                    var pav = MapItemToPhotoAlbum(item);

                                    if (pav.isActive.HasValue && !pav.isActive.Value)
                                    {
                                        inactiveItems++;
                                        continue;
                                    }

                                    photoAlbumViews.Add(pav);

                                    break;
                                case ContentType.PlaylistView:

                                    var pv = MapItemToPlaylist(item);

                                    if (pv.isActive.HasValue && !pv.isActive.Value)
                                    {
                                        inactiveItems++;
                                        continue;
                                    }

                                    playlistViews.Add(pv);

                                    break;
                                default:
                                    break;
                            }
                        }

                    } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);
                }

                siteContent.Blogs = blogs.ToArray();
                statistics.Add(new ContentCount { ContentType = ContentType.Blog.ToString(), ItemCount = blogs.Count });

                siteContent.Bookmarks = bookmarks.ToArray();
                statistics.Add(new ContentCount { ContentType = ContentType.Bookmark.ToString(), ItemCount = bookmarks.Count });

                siteContent.PhotoAlbums = photoAlbums.ToArray();
                statistics.Add(new ContentCount { ContentType = ContentType.PhotoAlbum.ToString(), ItemCount = photoAlbums.Count });

                siteContent.Playlists = playlists.ToArray();
                statistics.Add(new ContentCount { ContentType = ContentType.Playlist.ToString(), ItemCount = playlists.Count });

                siteContent.Tabs = tabs.ToArray();
                statistics.Add(new ContentCount { ContentType = ContentType.Tab.ToString(), ItemCount = tabs.Count });

                siteContent.Tags = tags.ToArray();
                statistics.Add(new ContentCount { ContentType = ContentType.Tag.ToString(), ItemCount = tags.Count });

                siteContent.BlogViews = blogViews.ToArray();
                statistics.Add(new ContentCount { ContentType = ContentType.BlogView.ToString(), ItemCount = blogViews.Count });

                siteContent.PhotoAlbumViews = photoAlbumViews.ToArray();
                statistics.Add(new ContentCount { ContentType = ContentType.PhotoAlbumView.ToString(), ItemCount = photoAlbumViews.Count });

                siteContent.PlaylistViews = playlistViews.ToArray();
                statistics.Add(new ContentCount { ContentType = ContentType.PlaylistView.ToString(), ItemCount = playlistViews.Count });

                siteContent.Counts = statistics.ToArray();

                return siteContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Handler exception. Error: {ex.Message}. {ex.StackTrace}.");
                return null;
            }
        }

        public async Task<bool> AddItem(dynamic content, bool isView = false)
        {
            try
            {
                var request = new PutItemRequest
                {
                    TableName = _tableName
                };

                if (content is Blog)
                {
                    request.Item = MapBlogToItem((Blog)content, isView);
                }
                else if (content is Playlist)
                {
                    request.Item = MapPlaylistToItem((Playlist)content, isView);
                }
                else if (content is PhotoAlbum)
                {
                    request.Item = MapPhotoAlbumToItem((PhotoAlbum)content, isView);
                }

                if (request.Item == null)
                {
                    return false;
                }

                var result = await _client.PutItemAsync(request);

                if (result == null || result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Handler exception. Error: {ex.Message}. {ex.StackTrace}.");
                throw;
            }
        }

        /// <summary>
        /// Generates blog view JSON files from Dynamo blogs.
        /// </summary>
        public async Task<List<Blog>> GenerateBlogViews(bool updateDynamo, bool newOnly = true)
        {
            try
            {
                var contentTypes = new ContentType[] { ContentType.Blog, ContentType.BlogView };
                Dictionary<string, AttributeValue> lastKeyEvaluated = null;
                var siteContent = new SiteContent();
                var blogs = new List<Blog>();
                var blogViews = new List<Blog>();

                foreach (var contentType in contentTypes)
                {
                    var cType = (ContentType)contentType;
                    do
                    {
                        var request = new QueryRequest
                        {
                            TableName = _tableName,
                            KeyConditionExpression = "ContentType = :ContentType",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                            {
                                {
                                    ":ContentType", new AttributeValue {S = cType.ToString()}
                                }
                            },
                            ScanIndexForward = false
                        };

                        if (lastKeyEvaluated != null && lastKeyEvaluated.Any())
                        {
                            request.ExclusiveStartKey = lastKeyEvaluated;
                        }

                        var response = await _client.QueryAsync(request);

                        lastKeyEvaluated = response.LastEvaluatedKey;

                        if (response?.Items == null || !response.Items.Any())
                        {
                            continue;
                        }

                        var inactiveItems = 0;

                        foreach (var item in response.Items)
                        {
                            switch (cType)
                            {
                                case ContentType.Blog:

                                    var a = MapItemToBlog(item);

                                    if (a.isActive.HasValue && !a.isActive.Value)
                                    {
                                        inactiveItems++;
                                        continue;
                                    }

                                    blogs.Add(a);

                                    break;
                                case ContentType.BlogView:

                                    var b = MapItemToBlog(item);

                                    if (b.isActive.HasValue && !b.isActive.Value)
                                    {
                                        inactiveItems++;
                                        continue;
                                    }

                                    blogViews.Add(b);

                                    break;
                                default:
                                    break;
                            }
                        }

                    } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);
                }

                foreach (var blog in blogs.OrderBy(a => a.createdOn).ToList())
                {
                    var view = blogViews.FirstOrDefault(p => p.uniqueName == blog.uniqueName);

                    if (view != null && newOnly)
                        continue;

                    view = new Blog
                    {
                        createdOn = blog.createdOn,
                        title = blog.title,
                        uniqueName = blog.uniqueName,
                        image = blog.image,
                        tags = blog.tags,
                        isActive = blog.isActive
                    };

                    var body = JsonConvert.DeserializeObject<Paragraph[]>(blog.body);
                    var content = string.Empty;
                    var take = 0;
                    for (int i = 0; i < body.Length; i++)
                    {
                        take++;

                        if (!string.IsNullOrEmpty(body[i].style) &&
                            body[i].style == "text")
                        {
                            content += body[i].text;
                        }
                        if (content.Length >= 150)
                        {
                            break;
                        }
                    }

                    view.body = JsonConvert.SerializeObject(body.Take(take));
                    blogViews.Add(view);

                    if (updateDynamo)
                    {
                        var success = await AddItem(view, true);
                    }
                }

                return blogViews;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
                throw;
            }
        }

        #endregion

        #region Methods (Private)

        private Blog MapItemToBlog(Dictionary<string, AttributeValue> item)
        {
            try
            {
                var blog = new Blog();

                if (item.ContainsKey(TableContent.Name.ToString()))
                {
                    blog.uniqueName = item[TableContent.Name.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Title.ToString()))
                {
                    blog.title = item[TableContent.Title.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Content.ToString()))
                {
                    blog.body = item[TableContent.Content.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Author.ToString()))
                {
                    blog.author = item[TableContent.Author.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Feature.ToString()))
                {
                    blog.image = item[TableContent.Feature.ToString()].S;
                }

                if (item.ContainsKey(TableContent.CreatedOn.ToString()))
                {
                    var success = int.TryParse(item[TableContent.CreatedOn.ToString()].N, out int date);

                    if (success)
                    {
                        blog.createdOn = date;
                    }
                }

                if (item.ContainsKey(TableContent.ModifiedOn.ToString()))
                {
                    var success = int.TryParse(item[TableContent.ModifiedOn.ToString()].S, out int date);

                    if (success)
                    {
                        blog.modifiedOn = date;
                    }
                }

                if (item.ContainsKey(TableContent.Tags.ToString()) && !string.IsNullOrEmpty(item[TableContent.Tags.ToString()].S))
                {
                    blog.tags = item[TableContent.Tags.ToString()].S.Split(',');
                }

                if (item.ContainsKey(TableContent.IsActive.ToString()))
                {
                    blog.isActive = item[TableContent.IsActive.ToString()].BOOL;
                }

                return blog;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: MapItemToBlog problem. Error: {ex.Message}. {ex.StackTrace}.");
                return null;
            }
        }

        private Playlist MapItemToPlaylist(Dictionary<string, AttributeValue> item)
        {
            try
            {
                var playlist = new Playlist();

                if (item.ContainsKey(TableContent.Name.ToString()))
                {
                    playlist.uniqueName = item[TableContent.Name.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Title.ToString()))
                {
                    playlist.title = item[TableContent.Title.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Content.ToString()))
                {
                    playlist.tracks = JsonConvert.DeserializeObject<Track[]>(item[TableContent.Content.ToString()].S);
                }

                if (item.ContainsKey(TableContent.Feature.ToString()))
                {
                    playlist.image = item[TableContent.Feature.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Description.ToString()))
                {
                    playlist.description = item[TableContent.Description.ToString()].S;
                }

                if (item.ContainsKey(TableContent.CreatedOn.ToString()))
                {
                    var success = int.TryParse(item[TableContent.CreatedOn.ToString()].N, out int date);

                    if (success)
                    {
                        playlist.createdOn = date;
                    }
                }

                if (item.ContainsKey(TableContent.ModifiedOn.ToString()))
                {
                    var success = int.TryParse(item[TableContent.ModifiedOn.ToString()].N, out int date);

                    if (success)
                    {
                        playlist.modifiedOn = date;
                    }
                }

                if (item.ContainsKey(TableContent.Tags.ToString()) && !string.IsNullOrEmpty(item[TableContent.Tags.ToString()].S))
                {
                    playlist.tags = item[TableContent.Tags.ToString()].S.Split(',');
                }

                if (item.ContainsKey(TableContent.IsActive.ToString()))
                {
                    playlist.isActive = item[TableContent.IsActive.ToString()].BOOL;

                }

                return playlist;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: MapItemToPlaylist problem. Error: {ex.Message}. {ex.StackTrace}.");
                return null;
            }
        }

        private PhotoAlbum MapItemToPhotoAlbum(Dictionary<string, AttributeValue> item)
        {
            try
            {
                var photoAlbum = new PhotoAlbum();

                if (item.ContainsKey(TableContent.Name.ToString()))
                {
                    photoAlbum.uniqueName = item[TableContent.Name.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Title.ToString()))
                {
                    photoAlbum.name = item[TableContent.Title.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Description.ToString()))
                {
                    photoAlbum.description = item[TableContent.Description.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Content.ToString()))
                {
                    photoAlbum.pictures = JsonConvert.DeserializeObject<Picture[]>(item[TableContent.Content.ToString()].S);
                }

                if (item.ContainsKey(TableContent.Author.ToString()))
                {
                    photoAlbum.author = item[TableContent.Author.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Feature.ToString()))
                {
                    photoAlbum.feature = item[TableContent.Feature.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Thumbnail.ToString()))
                {
                    photoAlbum.thumb = item[TableContent.Thumbnail.ToString()].S;
                }

                if (item.ContainsKey(TableContent.CreatedOn.ToString()))
                {
                    var success = int.TryParse(item[TableContent.CreatedOn.ToString()].N, out int date);

                    if (success)
                    {
                        photoAlbum.createdOn = date;
                    }
                }

                if (item.ContainsKey(TableContent.ModifiedOn.ToString()))
                {
                    var success = int.TryParse(item[TableContent.ModifiedOn.ToString()].N, out int date);

                    if (success)
                    {
                        photoAlbum.modifiedOn = date;
                    }
                }

                if (item.ContainsKey(TableContent.Tags.ToString()) && !string.IsNullOrEmpty(item[TableContent.Tags.ToString()].S))
                {
                    photoAlbum.tags = item[TableContent.Tags.ToString()].S.Split(',');
                }

                if (item.ContainsKey(TableContent.IsActive.ToString()))
                {
                    photoAlbum.isActive = item[TableContent.IsActive.ToString()].BOOL;
                }

                return photoAlbum;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: MapItemToPhotoAlbum problem. Error: {ex.Message}. {ex.StackTrace}.");
                return null;
            }
        }

        private Bookmark MapItemToBookmark(Dictionary<string, AttributeValue> item)
        {
            try
            {
                var bookmark = new Bookmark();

                if (item.ContainsKey(TableContent.ContentType.ToString()))
                {
                    bookmark.isTab = item[TableContent.ContentType.ToString()].S != ContentType.Bookmark.ToString();
                }

                if (item.ContainsKey(TableContent.Title.ToString()))
                {
                    bookmark.name = item[TableContent.Title.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Content.ToString()))
                {
                    bookmark.url = item[TableContent.Content.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Author.ToString()))
                {
                    bookmark.category = item[TableContent.Author.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Description.ToString()))
                {
                    bookmark.description = item[TableContent.Description.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Video.ToString()))
                {
                    bookmark.video = JsonConvert.DeserializeObject<Video>(item[TableContent.Video.ToString()].S);
                }

                if (item.ContainsKey(TableContent.CreatedOn.ToString()))
                {
                    var success = int.TryParse(item[TableContent.CreatedOn.ToString()].N, out int date);

                    if (success)
                    {
                        bookmark.createdOn = date;
                    }
                }

                if (item.ContainsKey(TableContent.ModifiedOn.ToString()))
                {
                    var success = int.TryParse(item[TableContent.ModifiedOn.ToString()].N, out int date);

                    if (success)
                    {
                        bookmark.modifiedOn = date;
                    }
                }

                if (item.ContainsKey(TableContent.Tags.ToString()) && !string.IsNullOrEmpty(item[TableContent.Tags.ToString()].S))
                {
                    bookmark.tags = item[TableContent.Tags.ToString()].S.Split(',');
                }

                return bookmark;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: MapItemToBookmark problem. Error: {ex.Message}. {ex.StackTrace}.");
                return null;
            }
        }

        private Models.Tag MapItemToTag(Dictionary<string, AttributeValue> item)
        {
            try
            {
                var tag = new Models.Tag();

                if (item.ContainsKey(TableContent.Title.ToString()))
                {
                    tag.title = item[TableContent.Title.ToString()].S;
                }

                if (item.ContainsKey(TableContent.Name.ToString()))
                {
                    tag.uniqueName = item[TableContent.Name.ToString()].S;
                }

                if (item.ContainsKey(TableContent.CreatedOn.ToString()))
                {
                    var success = int.TryParse(item[TableContent.CreatedOn.ToString()].N, out int date);

                    if (success)
                    {
                        tag.createdOn = date;
                    }
                }

                return tag;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: MapItemToTag problem. Error: {ex.Message}. {ex.StackTrace}.");
                return null;
            }
        }

        private Dictionary<string, AttributeValue> MapBlogToItem(Blog blog, bool isView = false)
        {
            try
            {
                var item = new Dictionary<string, AttributeValue>();

                item[TableContent.ContentType.ToString()] = new AttributeValue
                {
                    S = isView ? ContentType.BlogView.ToString() : ContentType.Blog.ToString()
                };

                if (!string.IsNullOrEmpty(blog.uniqueName))
                {
                    item[TableContent.Name.ToString()] = new AttributeValue
                    {
                        S = blog.uniqueName
                    };
                }

                if (!string.IsNullOrEmpty(blog.title))
                {
                    item[TableContent.Title.ToString()] = new AttributeValue
                    {
                        S = blog.title
                    };
                }

                if (!string.IsNullOrEmpty(blog.body))
                {
                    item[TableContent.Content.ToString()] = new AttributeValue
                    {
                        S = blog.body
                    };
                }

                if (!string.IsNullOrEmpty(blog.image))
                {
                    item[TableContent.Feature.ToString()] = new AttributeValue
                    {
                        S = blog.image
                    };
                }

                item[TableContent.CreatedOn.ToString()] = new AttributeValue
                {
                    N = blog.createdOn.ToString()
                };

                if (blog.modifiedOn.HasValue)
                {
                    item[TableContent.ModifiedOn.ToString()] = new AttributeValue
                    {
                        N = blog.modifiedOn.Value.ToString()
                    };
                }

                if (blog.tags != null && blog.tags.Any())
                {
                    item[TableContent.Tags.ToString()] = new AttributeValue
                    {
                        S = string.Join(',', blog.tags)
                    };
                }

                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: MapBlogToItem problem. Error: {ex.Message}. {ex.StackTrace}.");
                return null;
            }
        }

        private Dictionary<string, AttributeValue> MapPlaylistToItem(Playlist playlist, bool isView = false)
        {
            try
            {
                var item = new Dictionary<string, AttributeValue>();

                item[TableContent.ContentType.ToString()] = new AttributeValue
                {
                    S = isView ? ContentType.PlaylistView.ToString() : ContentType.Playlist.ToString()
                };

                if (!string.IsNullOrEmpty(playlist.uniqueName))
                {
                    item[TableContent.Name.ToString()] = new AttributeValue
                    {
                        S = playlist.uniqueName
                    };
                }

                if (!string.IsNullOrEmpty(playlist.title))
                {
                    item[TableContent.Title.ToString()] = new AttributeValue
                    {
                        S = playlist.title
                    };
                }

                if (!string.IsNullOrEmpty(playlist.description))
                {
                    item[TableContent.Description.ToString()] = new AttributeValue
                    {
                        S = playlist.description
                    };
                }

                if (playlist.tracks != null)
                {
                    item[TableContent.Content.ToString()] = new AttributeValue
                    {
                        S = JsonConvert.SerializeObject(playlist.tracks, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                    };
                }

                if (!string.IsNullOrEmpty(playlist.author))
                {
                    item[TableContent.Author.ToString()] = new AttributeValue
                    {
                        S = playlist.author
                    };
                }

                if (!string.IsNullOrEmpty(playlist.image))
                {
                    item[TableContent.Feature.ToString()] = new AttributeValue
                    {
                        S = playlist.image
                    };
                }

                item[TableContent.CreatedOn.ToString()] = new AttributeValue
                {
                    N = playlist.createdOn.ToString()
                };

                if (playlist.modifiedOn.HasValue)
                {
                    item[TableContent.ModifiedOn.ToString()] = new AttributeValue
                    {
                        N = playlist.modifiedOn.Value.ToString()
                    };
                }

                if (playlist.tags != null && playlist.tags.Any())
                {
                    item[TableContent.Tags.ToString()] = new AttributeValue
                    {
                        S = JsonConvert.SerializeObject(playlist.tags, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                    };
                }

                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: MapPlaylistToItem problem. Error: {ex.Message}. {ex.StackTrace}.");
                return null;
            }
        }

        private Dictionary<string, AttributeValue> MapPhotoAlbumToItem(PhotoAlbum album, bool isView = false)
        {
            try
            {
                var item = new Dictionary<string, AttributeValue>();

                item[TableContent.ContentType.ToString()] = new AttributeValue
                {
                    S = isView ? ContentType.PhotoAlbumView.ToString() : ContentType.PhotoAlbum.ToString()
                };

                if (!string.IsNullOrEmpty(album.uniqueName))
                {
                    item[TableContent.Name.ToString()] = new AttributeValue
                    {
                        S = album.uniqueName
                    };
                }

                if (!string.IsNullOrEmpty(album.name))
                {
                    item[TableContent.Title.ToString()] = new AttributeValue
                    {
                        S = album.name
                    };
                }

                if (!string.IsNullOrEmpty(album.description))
                {
                    item[TableContent.Description.ToString()] = new AttributeValue
                    {
                        S = album.description
                    };
                }

                if (album.pictures != null)
                {
                    item[TableContent.Content.ToString()] = new AttributeValue
                    {
                        S = JsonConvert.SerializeObject(album.pictures, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                    };
                }

                if (!string.IsNullOrEmpty(album.author))
                {
                    item[TableContent.Author.ToString()] = new AttributeValue
                    {
                        S = album.author
                    };
                }

                if (!string.IsNullOrEmpty(album.feature))
                {
                    item[TableContent.Feature.ToString()] = new AttributeValue
                    {
                        S = album.feature
                    };
                }

                if (!string.IsNullOrEmpty(album.thumb))
                {
                    item[TableContent.Feature.ToString()] = new AttributeValue
                    {
                        S = album.thumb
                    };
                }

                item[TableContent.CreatedOn.ToString()] = new AttributeValue
                {
                    N = album.createdOn.ToString()
                };

                if (album.modifiedOn.HasValue)
                {
                    item[TableContent.ModifiedOn.ToString()] = new AttributeValue
                    {
                        N = album.modifiedOn.Value.ToString()
                    };
                }

                if (album.tags != null && album.tags.Any())
                {
                    item[TableContent.Tags.ToString()] = new AttributeValue
                    {
                        S = JsonConvert.SerializeObject(album.tags, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                    };
                }

                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: MapPhotoAlbumToItem problem. Error: {ex.Message}. {ex.StackTrace}.");
                return null;
            }
        }

        #endregion
    }
}
