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
    }

    public class LikeData
    {
        public string postID { get; set; }
        public string userID { get; set; }
    }

    public class PostsController : Controller
    {
        private Entities db = new Entities();

        // GET: Posts
        public ActionResult Index()
        {
            return View(db.Posts.ToList());
        }

        // GET: Posts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorName = db.AspNetUsers.Where(u => (u.Id == post.AuthorID)).First().UserName;
            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "ID,Title,Link,Content,LinkType,AuthorID,AuthorName,Likes,Dislikes")] Post post)
        {
            if (ModelState.IsValid)
            {
                post.Likes = 0;
                post.Dislikes = 0;
                // Get User ID and name and assign it to the post for reference later
                post.AuthorID = User.Identity.GetUserId();
                post.AuthorName = User.Identity.GetUserName();

                // Convert the link to an mp4 if it is a gifv
                post.Link = LinkChecker.ConvertGifvToMp4(post.Link);
                // Get link type (Video, Image, Youtube)
                post.LinkType = LinkChecker.GetLinkType(post.Link);

                if (post.LinkType == "Youtube")
                    post.Link = LinkChecker.ConvertYoutubeLink(post.Link);

                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "ID,Title,Link,Content,LinkType,AuthorID,AuthorName,Likes,Dislikes")] Post post)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }

        [HttpPost]
        [Authorize]
        public string Like(LikeData likeData)
        {
            int id = Convert.ToInt32(likeData.postID);
            Post post = db.Posts.FirstOrDefault(p => p.ID == id);

            UserLikedPost userLikedPost = new UserLikedPost();
            userLikedPost.PostID = post.ID;
            userLikedPost.UserID = likeData.userID;
            userLikedPost.PostTitle = post.Title;
        
            // See if post has already been liked/disliked by this user
            var foundLikedPost = db.UserLikedPosts.FirstOrDefault(p => (p.PostID == post.ID && p.UserID == likeData.userID));
            var foundDislikedPost = db.UserDislikedPosts.FirstOrDefault(p => (p.PostID == post.ID && p.UserID == likeData.userID));

            // If user has disliked it, then remove the dislike
            if(foundDislikedPost != null)
            {
                db.UserDislikedPosts.Remove(foundDislikedPost);
                post.Dislikes--;
            }

            // If so, remove it from list and remove a like
            if (foundLikedPost != null)
            { 
               db.UserLikedPosts.Remove(foundLikedPost);
               post.Likes--;
            }
            // Otherwise, like it and add to user's liked posts
            else
            {
                db.UserLikedPosts.Add(userLikedPost);
                post.Likes++;
            }
  
       
            db.Entry(post).State = EntityState.Modified;
            db.SaveChanges();

            return $"{post.Likes},{post.Dislikes}";
        }

        [HttpPost]
        [Authorize]
        public string Dislike(LikeData likeData)
        {
            int id = Convert.ToInt32(likeData.postID);
            Post post = db.Posts.FirstOrDefault(p => p.ID == id);

            UserDislikedPost userDislikedPost = new UserDislikedPost();
            userDislikedPost.PostID = post.ID;
            userDislikedPost.UserID = likeData.userID;
            userDislikedPost.PostTitle = post.Title;

            // See if post has already been disliked/liked by this user
            var foundDislikedPost = db.UserDislikedPosts.FirstOrDefault(p => (p.PostID == post.ID && p.UserID == likeData.userID));
            var foundLikedPost = db.UserLikedPosts.FirstOrDefault(p => (p.PostID == post.ID && p.UserID == likeData.userID));


            // If user has liked it, then remove the like
            if (foundLikedPost != null)
            {
                db.UserLikedPosts.Remove(foundLikedPost);
                post.Likes--;
            }

            // If so, remove it from list and remove a dislike
            if (foundDislikedPost != null)
            {
                db.UserDislikedPosts.Remove(foundDislikedPost);
                post.Dislikes--;
            }
            // Otherwise, dislike it and add to user's disliked posts
            else
            {
                db.UserDislikedPosts.Add(userDislikedPost);
                post.Dislikes++;
            }


            db.Entry(post).State = EntityState.Modified;
            db.SaveChanges();

            return $"{post.Likes},{post.Dislikes}";
        }

        // GET: Posts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
