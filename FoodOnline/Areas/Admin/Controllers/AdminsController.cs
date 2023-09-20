using FoodOnline.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FoodOnline.Areas.Admin.Controllers
{
    public class AdminsController : Controller
    {
        private FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();
        public bool CheckRole(string type)
        {
            Models.User user = Session["UserAdmin"] as Models.User;
            if (user != null && user.Role.NameRole == type)
            {
                return true;
            }
            return false;
        }
        //public ActionResult Index()
        //{
        //    //return View(db.Admins.Where(a => a.IsDelete == 0).ToList());
        //    return View();
        //}
        // GET: user/Users
        public ActionResult Index(string Sort_Order, string Search_Data, int? Page_No)
        {
            if (Session["Roles"] == null)
            {
                return RedirectToAction("Login", "LoginAdmin");
            }
            ModelState.Clear();
            ViewBag.CurrentSort = Sort_Order;
            ViewBag.SortName = String.IsNullOrEmpty(Sort_Order) ? "Name_desc" : "";
            var users = from a in db.Users select a;
            switch (Sort_Order)
            {
                case "Name_desc":
                    users = users.OrderByDescending(f => f.NameUser);
                    break;
                default:
                    users = users.OrderBy(f => f.NameUser);
                    break;
            }
            var ad = users.Where(a => (a.NameUser.Contains(Search_Data) || Search_Data == null)).ToList().ToPagedList(Page_No ?? 1, 5);
            return View(ad);
        }

        // GET: user/Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: user/Users/Create
        public ActionResult Create()
        {
            ViewBag.IdRole = new SelectList(db.Roles, "IdRole", "NameRole");
            return View();
        }

        // POST: user/Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdUser,NameUser,IdRole,Email,Captcha,IsComfirm,Address,Phone,Img,Password,Status,fg_otp")] User user)
        {
            if (ModelState.IsValid)
            {
                var check = db.Users.FirstOrDefault(s => s.Email == user.Email);
                if (check == null)
                {
                    user.Password = GetMD5(user.Password);
                    //user.Role.NameRole = "Admin";
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.error = "Email already exists";
                    return View();
                }
            }

            //ViewBag.IdRole = new SelectList(db.Roles, "IdRole", "NameRole", user.IdRole);
            return View(user);
        }

        // GET: user/Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdRole = new SelectList(db.Roles, "IdRole", "NameRole", user.IdRole);
            return View(user);
        }

        // POST: user/Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdUser,NameUser,IdRole,Email,Captcha,IsComfirm,Address,Phone,Img,Password,Status,fg_otp")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdRole = new SelectList(db.Roles, "IdRole", "NameRole", user.IdRole);
            return View(user);
        }

        // GET: user/Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User admin = db.Users.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            else
            {
                db.Entry(admin).State = EntityState.Modified;
                RedirectToAction("Index");
            }
            db.SaveChanges();
            return View(admin);
        }

        // POST: user/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Lock(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (user.Status == true) user.Status = false;
            else user.Status = true;
            if (user != null)
            {
                /*db.Entry(book).State = EntityState.Modified;*/
                RedirectToAction("Index");
            }
            db.SaveChanges();
            return View(user);
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