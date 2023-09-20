using FoodOnline.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace FoodOnline.Controllers
{
    public class UsersController : Controller
    {
        FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();
        // GET: Users
        public ActionResult Index()
        {
            return View();
        }
        public bool CheckRole(string type)
        {
            User user = Session["User"] as User;
            if (user.Role.NameRole == type)
            {
                return true;
            }
            return false;
        }
        public void SentMail(string title, string ToEmail, string FromEmail, string password, string content)
        {
            //string passHash = GetMD5(password);
            MailMessage mail = new MailMessage();
            mail.To.Add(ToEmail);
            mail.From = new MailAddress(ToEmail);
            mail.Subject = title;
            mail.Body = content;
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(FromEmail, password);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/Users/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("duykhanh18102002@gmail.com", "Dotnet Awesome");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "ytlipmoseyimohec"; // Replace with actual password

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "Your account is successfully created!";
                body = "<br/><br/>We are excited to tell you that your Dotnet Awesome account is" +
                    " successfully created. Please click on the below link to verify your account" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";

            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Hi,<br/>br/>We got request for reset your account password. Please click on the below link to reset your password" +
                    "<br/><br/><a href=" + link + ">Reset Password link</a>";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        [HttpGet]
        public ActionResult ConfirmEmail(int ID)
        {
            User user = db.Users.SingleOrDefault(u => u.IdUser == ID);
            if (user.IsComfirm.Value)
            {
                ViewBag.Message = "Email Confirmed";
                return View();
            }
            string urlBase = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~");
            ViewBag.Email = "Access to Email to verify account: " + user.Email;
            SentMail("Mã xác minh tài khoản", user.Email, "duykhanh18102002@gmail.com", "ytlipmoseyimohec", "Xác minh nhanh bằng cách click vào link: "
                + urlBase + "Users/ConfirmEmailLink/" + ID + "?Captcha=" + user.Captcha + "</div>");
            return View();
        }
        [HttpGet]
        public ActionResult ConfirmEmailLink(int ID, string captcha)
        {
            User user = db.Users.SingleOrDefault(u => u.IdUser == ID && u.Captcha == captcha);
            if (user != null)
            {
                user.IsComfirm = true;
                db.SaveChanges();
                ViewBag.Message = "Account Verification Successful";
                return View();
            }
            ViewBag.Message = "\r\nAccount Verification Failed";
            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string EmailID)
        {
            //Verify Email ID
            //Generate Reset password link 
            //Send Email 
            string message = "";

            using (FoodOrdersOnlineEntities dc = new FoodOrdersOnlineEntities())
            {
                var account = dc.Users.Where(a => a.Email == EmailID).FirstOrDefault();
                if (account != null)
                {
                    //Send email for reset password
                    string resetCode = new Random().Next(100000, 999999).ToString();
                    SendVerificationLinkEmail(account.Email, resetCode, "ResetPassword");
                    account.fg_otp = resetCode;
                    //This line I have added here to avoid confirm password not match issue , as we had added a confirm password property 
                    //in our model class in part 1
                    dc.Configuration.ValidateOnSaveEnabled = false;
                    dc.SaveChanges();
                    message = "Reset password link has been sent to your email id.";
                }
                else
                {
                    message = "Account not found";
                }
            }
            ViewBag.Message = message;
            return View();
        }
        public ActionResult ResetPassword(string id)
        {
            //Verify the reset password link
            //Find account associated with this link
            //redirect to reset password page
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            using (FoodOrdersOnlineEntities dc = new FoodOrdersOnlineEntities())
            {
                var user = dc.Users.Where(a => a.fg_otp == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (FoodOrdersOnlineEntities dc = new FoodOrdersOnlineEntities())
                {
                    var user = dc.Users.Where(a => a.fg_otp == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = GetMD5(model.NewPassword);
                        user.fg_otp = "";
                        dc.Configuration.ValidateOnSaveEnabled = false;
                        dc.SaveChanges();
                        message = "New password updated successfully";
                    }
                }
            }
            else
            {
                message = "Something invalid";
            }
            ViewBag.Message = message;
            return View(model);
        }


        [HttpGet]
        public ActionResult ProfileUser(int? userID)
        {
            if (userID == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else
            {
                var user = db.Users.SingleOrDefault(x => x.IdUser == userID);
                return View(user);
            }
        }

        [HttpPost]
        public ActionResult ProfileUser(User user, FormCollection form)
        {
            int userID = (int)user.IdUser;
            var updateUser = db.Users.SingleOrDefault(x => x.IdUser == userID);
            //var passHash = GetMD5(user.Password);
            //var updatePass = GetMD5(updateUser.Password);
            updateUser.NameUser = user.NameUser;
            updateUser.Address = user.Address;
            updateUser.Phone = user.Phone;
            //updateUser.Password = passHash;
            db.SaveChanges();
            return RedirectToAction("ProfileUser", new { userID = updateUser.IdUser });
        }

        [HttpPost]
        public ActionResult UploadAvatar(HttpPostedFileBase file, int IdUser)
        {
            int userID = (int)IdUser;
            try
            {
                if (file.ContentLength > 0)
                {
                    string _fileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/Images"), _fileName);
                    file.SaveAs(_path);
                    var user = db.Users.SingleOrDefault(x => x.IdUser == userID);
                    if (user != null)
                    {
                        string filename = (string)file.FileName;
                        user.Img = filename;
                        db.SaveChanges();
                    }
                }
                ViewBag.Message = "Upload Success";
                return RedirectToAction("ProfileUser", new { userID = IdUser });
            }
            catch
            {
                ViewBag.Message = "Upload Fail";
                return RedirectToAction("ProfileUser", new { userID = IdUser });
            }
        }

        public ActionResult ChangePass()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(string Password, string newPassword, string Confirmpwd)
        {
            User objadmin = new User();
            string ad = Session["Name"].ToString();
            int id = int.Parse(Session["User_Id"].ToString());
            var login = db.Users.Where(u => u.NameUser.Equals(ad) && u.IdUser.Equals(id)).FirstOrDefault();
            var f_pass = GetMD5(Password);
            if (login.Password == f_pass)
            {
                if (Confirmpwd == newPassword)
                {
                    login.Password = GetMD5(newPassword);
                    var str = GetMD5(newPassword);
                    db.SaveChanges();
                    ViewBag.Message = "Password has been changed successfully !!!";
                }
                else
                {
                    ViewBag.Message = "Confirm password not match !!! Please check !!!";
                }
            }
            else
            {
                ViewBag.Message = "Old password not match !!! Please check !!!";
            }
            return View();
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
    }
}