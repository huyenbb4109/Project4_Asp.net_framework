using FoodOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FoodOnline.Areas.Admin.Controllers
{
    public class LoginAdminController : Controller
    {
        private FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();

        public ActionResult Index()
        {
            if (Session["Admin_Id"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //GET: Register

        public ActionResult Register()
        {
            return View();
        }

        //POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User _user)
        {
            if (ModelState.IsValid)
            {
                var check = db.Users.FirstOrDefault(s => s.Email == _user.Email);
                if (check == null)
                {
                    _user.Password = GetMD5(_user.Password);
                    _user.Role.NameRole = "Admin";
                    _user.Status = true;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    db.Users.Add(_user);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.error = "Email already exists";
                    return View();
                }
            }
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (ModelState.IsValid)
            {

                var f_password = GetMD5(password);
                var data = db.Users.Where(s => s.Email.Equals(email) && s.Password.Equals(f_password)
                && s.Status == true && (s.Role.NameRole == "Admin" || s.Role.NameRole == "SuperAdmin")).ToList();
                var lockData = db.Users.Where(s => s.Email.Equals(email) && s.Password.Equals(f_password)
                && (s.Role.NameRole == "Admin" || s.Role.NameRole == "SuperAdmin") && s.Status == false).ToList();

                if (data.Count() > 0)
                {
                    //add session
                    Session["Name"] = data.FirstOrDefault().NameUser;
                    Session["Email"] = data.FirstOrDefault().Email;
                    Session["Admin_Id"] = data.SingleOrDefault().IdUser;
                    Session["Roles"] = data.FirstOrDefault().Role.NameRole;
                    Session["UserAdmin"] = data.FirstOrDefault();

                    return RedirectToAction("Index", "HomeAdmin");
                }
                if (lockData.Count() > 0)
                {
                    ViewBag.error = "Your account has been lock";
                    return View();
                }
                else
                {
                    ViewBag.error = "Incorrect email or password";
                    return View();
                }
            }
            return View();
        }


        //Logout
        public ActionResult Logout()
        {
            Session.Clear();//remove session
            return RedirectToAction("Login");
        }

        public ActionResult ChangePass()
        {
            //if (Session["Name"] == null)
            //{
            //    return RedirectToAction("Login");
            //}
            //else 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(string Password, string newPassword, string Confirmpwd)
        {
            User user = new User();
            string ad = Session["Name"].ToString();
            int id = int.Parse(Session["Admin_Id"].ToString());
            var login = db.Users.Where(u => u.NameUser.Equals(ad) && u.IdUser.Equals(id)).FirstOrDefault();
            var f_pass = GetMD5(Password);
            if (login.Password == f_pass)
            {
                if (Confirmpwd == newPassword)
                {
                    //login.ConfirmPassword = GetMD5(Confirmpwd);
                    login.Password = GetMD5(newPassword);
                    var str = GetMD5(newPassword);
                    //db.Entry(login).State = EntityState.Modified;
                    db.SaveChanges();
                    ViewBag.Message = "Password has been changed successfully !!!";
                }
                else
                {
                    ViewBag.Message = "New password match !!! Please check";
                }
            }
            else
            {
                ViewBag.Message = "Old password not match !!! Please check entered old password";
            }
            return View();
        }


        //create a string MD5
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
    }
}