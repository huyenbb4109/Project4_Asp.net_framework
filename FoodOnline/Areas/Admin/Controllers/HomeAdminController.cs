using FoodOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodOnline.Areas.Admin.Controllers
{
    public class HomeAdminController : Controller
    {
        private FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();

        // GET: Admin/Home
        public ActionResult Index()
        {
            ViewBag.TotalOrder = db.Orders.Count(x => x.Status == "Complete");
            ViewBag.TotalMoney = db.Orders.Where(x => x.Status == "Complete").ToList().Sum(x => x.OrderDetails.Sum(n => n.Price * n.Quantity));
            ViewBag.TotalClient = db.Users.Count(x => x.Role.NameRole == "Client");
            ViewBag.TotalProduct = db.Products.Count(x => x.IsDelete == true);
            return View();
        }
    }
}