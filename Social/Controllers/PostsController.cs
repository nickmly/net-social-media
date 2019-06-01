using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Social;
using Microsoft.AspNet.Identity;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Social.Models;

namespace Social.Controllers
{
    public static class LinkChecker
    {

        public static string ConvertGifvToMp4(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            Regex regex = new Regex(@"\.(gifv)$");
            Match match = regex.Match(url);
            if (match.Success)
            {
                var newUrl = url.Substring(0, url.Length - 5);
                newUrl += ".mp4";
                return newUrl;
            }

            return url;
        }

        public static string ConvertYoutubeLink(string url)
        {
            string videoID = url.Split(new string[] { "v=" }, StringSplitOptions.None)[1]; // Get ID and variables
            if (videoID == null)
                videoID = url.Split(new string[] { "e/" }, StringSplitOptions.None)[1]; // If there is no v= in the link, just separate with a slash

            int endPoint = videoID.IndexOf("&"); // Find start of variables (time to start video, end video, etc.)
            if (endPoint != -1)
            { // If there are any vars
                videoID = videoID.Substring(0, endPoint); // Get 12 digit video ID and leave out vars
            }
            string newUrl = "https://www.youtube.com/embed/" + videoID;// Youtube by default has links that do not work in an iframe, we have to convert them using /embed/
            return newUrl;
        }

        public static string ConvertGfycatLink(string url)
        {
            string routeData = url.Split(new string[] { ".com/" }, StringSplitOptions.None)[1];
            string newUrl = "https://giant.gfycat.com/" + routeData + ".webm";
            return newUrl;
        }

        public static string GetLinkType(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "Text";

            if (CheckIfImage(url))
                return "Image";
            else if (CheckIfVideo(url))
                return "Video";
            else if (CheckIfYoutube(url))
                return "Youtube";
            else if (CheckIfGfycat(url))
                return "Gfycat";

            return "default";
        }

        private static bool CheckIfImage(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            Regex regex = new Regex(@"\.(jpeg|jpg|gif|png)$");
            Match match = regex.Match(url);
            return match.Success;
        }

        private static bool CheckIfVideo(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            Regex regex = new Regex(@"\.(webm|mp4)$");
            Match match = regex.Match(url);
            return match.Success;
        }

        private static bool CheckIfYoutube(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            Regex regex = new Regex(@"(?:(?:https?:\/\/)(?:www)?\.?(?:youtu\.?be)(?:\.com)?\/(?:.*[=/])*)([^= &?/\r\n]{8,11})");
            Match match = regex.Match(url);
            return match.Success;
        }

        private static bool CheckIfGfycat(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            Regex regex = new Regex(@"(?:https?:\/\/)(?:www)?\.?(?:gfycat)(?:\.com)");
            Match match = regex.Match(url);
            return match.Success;
        }
    }

    public class LikeData
    {
        public string postID { get; set; }
        public string userID { get; set; }
    }

    public class PostsController : Controller
    {

        private List<PostModel> redditPosts = new List<PostModel>();

        public async Task PopulatePosts()
        {
            JObject json = await NetConfig.GetJSONAsync("/r/all/.json");
            if (json["error"] == null)
            {
                int index = 1;
                foreach (var post in json["data"]["children"])
                {
                    var currentPost = post["data"];
                    PostModel newPost = new PostModel
                    {
                        ID = index,
                        Title = currentPost["title"].ToString(),
                        Content = currentPost["selftext"].ToString(),
                        Permalink = currentPost["permalink"].ToString(),
                        Link = currentPost["url"].ToString(),
                        AuthorName = currentPost["author"].ToString(),
                        Likes = Convert.ToInt32(currentPost["ups"])
                    };

                    newPost.LinkType = LinkChecker.GetLinkType(newPost.Link);
                    if (newPost.LinkType == "Youtube")
                        newPost.Link = LinkChecker.ConvertYoutubeLink(newPost.Link);
                    if (newPost.LinkType == "Gfycat")
                    {
                        newPost.Link = LinkChecker.ConvertGfycatLink(newPost.Link);
                        newPost.LinkType = "Video";
                    }

                    if (Convert.ToBoolean(currentPost["is_video"]) == true)
                    {
                        newPost.Link = currentPost["secure_media"]["reddit_video"]["fallback_url"].ToString();
                        newPost.LinkType = "Video";
                    }                    

                    redditPosts.Add(newPost);
                    index++;
                }
            }
            else
            {
                throw new HttpException(400, json["error"].ToString());
            }
        }

        protected async Task<List<CommentModel>> GetPostCommentsAsync(PostModel post)
        {
            List<CommentModel> postComments = new List<CommentModel>();
            JArray json = await NetConfig.GetJSONArrayAsync(post.Permalink + ".json");
            JToken commentsJson = json.ElementAt(1).ElementAt(1).ElementAt(0).ElementAt(2).ElementAt(0);
            for(int i = 0; i < 25; i ++)
            {
                JObject commentJson = (JObject)commentsJson.ElementAt(i);
                var currentComment = commentJson.GetValue("data");
                if (currentComment == null)
                    continue;

                CommentModel newComment = new CommentModel
                {
                    AuthorName = currentComment["author"].ToString(),
                    Content = currentComment["body"].ToString()
                };
                postComments.Add(newComment);
            }
            return postComments;

        }

        // GET: Posts
        public async Task<ActionResult> Index()
        {
            await PopulatePosts();

            var posts = redditPosts.OrderByDescending(p => p.Likes);
            return View(posts);
        }

        // GET: Posts/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // TODO: REMOVE AND FIND WAY TO PERSIST
            await PopulatePosts();

            PostModel post = redditPosts.Find(p => p.ID == id);
            if (post == null)
            {
                return HttpNotFound();
            }            

            ViewBag.AuthorName = post.AuthorName;
            var comments = await GetPostCommentsAsync(post);
            ViewBag.Comments = comments;
            ViewBag.CountComments = comments.Count();
            return View(post);
        }
    }
}
