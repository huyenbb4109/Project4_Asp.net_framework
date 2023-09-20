using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FoodOnline.Models;

namespace FoodOnline.Areas.ProductFoodOrder.Controllers
{
    public class OrdersController : Controller
    {
        private FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();

        //public bool CheckRole(string type)
        //{
        //    User user = Session["User"] as User;
        //    if (user != null && user.Role.NameRole == type)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        // GET: OrderManage
        public ActionResult Index()
        {
            //if (CheckRole("SuperAdmin"))
            //{

            //}
            //else
            //{
            //    return RedirectToAction("Index", "Orders", new { area = "ProductFoodOrder" });
            //}
            List<Order> orders = db.Orders.ToList();
            return View(orders);
        }
        public ActionResult Details(int ID)
        {
            //if (CheckRole("SuperAdmin"))
            //{

            //}
            //else
            //{
            //    return RedirectToAction("Index", "Orders", new { area = "ProductFoodOrder" });
            //}
            ViewBag.IsProcessed = (db.Orders.Find(ID).Status == "Processed") ? false : true;
            List<OrderDetail> orderDetails = db.OrderDetails.Where(x => x.IdOrder == ID).ToList();
            return View(orderDetails);
        }
        //public ActionResult Processed(int ID)
        //{
        //    Order order = db.Orders.Find(ID);
        //    //order.Status = "Processed";
        //    order.DateShip = DateTime.Now.AddDays(1);
        //    db.SaveChanges();
        //    return RedirectToAction("Index", "Orders", new { area = "ProductFoodOrder" });
        //}
        public ActionResult Delivering(int ID)
        {
            Order order = db.Orders.Find(ID);
            order.Status = "Delivering";
            order.DateShip = DateTime.Now.AddDays(1);
            db.SaveChanges();
            return RedirectToAction("Index", "Orders", new { area = "ProductFoodOrder" });
        }
    }
}
