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

            return null;
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
            return View(post);
        }

        // GET: Posts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Title,Link,Content,LinkType,AuthorID,Likes,Dislikes")] Post post)
        {
            if (ModelState.IsValid)
            {
                post.Likes = 0;
                post.Dislikes = 0;
                post.AuthorID = User.Identity.GetUserId();
                post.Link = LinkChecker.ConvertGifvToMp4(post.Link);
                post.LinkType = LinkChecker.GetLinkType(post.Link);

                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(post);
        }

        // GET: Posts/Edit/5
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
        public ActionResult Edit([Bind(Include = "ID,Title,Link,Content,LinkType,AuthorID,Likes,Dislikes")] Post post)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
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
