using FoodOnline.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FoodOnline.Controllers
{
    public class HomeController : Controller
    {
        private FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(User user)
        {
            User check = db.Users.SingleOrDefault(u => u.Email == user.Email);
            if (check != null)
            {
                ViewBag.Message = "Email already exists";
                return View();
            }

            User userAdd = new User();
            string pass = GetMD5(user.Password);
            try
            {
                user.Captcha = new Random().Next(100000, 999999).ToString();
                user.IsComfirm = false;
                user.IdRole = 3;
                user.Status = true;
                user.Img = "pr.jpg";
                user.Password = pass;
                userAdd = db.Users.Add(user);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Sign up failed " + ex.Message;
                return View();
            }
            return RedirectToAction("ConfirmEmail", "Users", new { ID = userAdd.IdUser });
        }
        public ActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignIn(string email, string password)
        {
            string passHash = GetMD5(password);
            User check = db.Users.FirstOrDefault(u => u.Email == email && u.Password == passHash);
            User wrongEmail = db.Users.FirstOrDefault(u => u.Email != email);
            User wrongPass = db.Users.FirstOrDefault(u => u.Email == email && u.Password != passHash);
            //var check2 = from u in db.Users
            //            join r in db.Roles on u.IdRole equals r.IdRole
            //            where (u.Email == email && u.Password == passHash)
            //            select new
            //            {
            //                EmailUser = u.Email,
            //                IDUser = u.IdUser,
            //                IDRole = u.IdRole,
            //                Name = u.NameUser,
            //                namerole = r.NameRole
            //            };
            if (check != null)
            {
                Session["User"] = check;
                Session["Roles"] = check;
                Session["User_Id"] = check.IdUser;
                Session["Name"] = check.NameUser;
                return RedirectToAction("Index", "Home");
            }
            else if (wrongEmail != null)
            {
                ViewBag.Message = "Email does not exist";
                return View();
            }
            else if (wrongPass != null)
            {
                ViewBag.Message = "Wrong password";
                return View();
            }
            return View();
        }
        public ActionResult SignOut()
        {
            Session.Remove("User");
            return RedirectToAction("Index", "Home");
        }

       
        public static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");

            }
            return byte2String;
        }
        public ActionResult Menu()
        {
            ViewBag.Message = "Your menu page.";

            return View(db.Products.ToList());
        }
        public ActionResult Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
    }
}