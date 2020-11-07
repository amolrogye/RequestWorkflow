using RequestWorkflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RequestWorkflow.Controllers
{
    public class UserController : Controller
    {
        DBEntities db = null;
        public UserController()
        {
            db = new DBEntities();
        }
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserMaster userMaster)
        {
            try
            {
                var user = db.UserMasters.ToList()?.FirstOrDefault(x => x.UserName.ToLower() == userMaster.UserName.ToLower() && x.Password == userMaster.Password);

                if (user != null)
                {
                    Session["LoginId"] = user.Id;
                    Session["UserName"] = user.UserName;
                    return Json(new { login = true, msg = "Login Succeed" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { login = false, msg = "Invalid UserName or Passsword" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { login = false, msg = "Something Went wrong" }, JsonRequestBehavior.AllowGet);

            }
        }
    }
}