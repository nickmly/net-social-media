using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Social;

namespace Social.Controllers
{
    public class UsersController : Controller
    {
        private Entities db = new Entities();


        // GET: Users/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            var likedPosts = db.UserLikedPosts.Where(p => p.UserID == aspNetUser.Id);
            ViewBag.LikedPosts = likedPosts;
            ViewBag.CountLikedPosts = likedPosts.Count();

            var dislikedPosts = db.UserDislikedPosts.Where(p => p.UserID == aspNetUser.Id);
            ViewBag.DislikedPosts = dislikedPosts;
            ViewBag.CountDislikedPosts = dislikedPosts.Count();

            var comments = db.Comments.Where(c => c.AuthorID == aspNetUser.Id);
            ViewBag.Comments = comments;
            ViewBag.CountComments = comments.Count();

            return View(aspNetUser);
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
