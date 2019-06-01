using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Social;
using Social.Models;

namespace Social.Controllers
{
    public class UsersController : Controller
    {

        // GET: Users/Name
        public ActionResult Details(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            UserModel user = new UserModel
            {
                username = name
            };

            return View(user);
        }
    }
}
