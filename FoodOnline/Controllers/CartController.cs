using FoodOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FoodOnline.Controllers
{
    public class CartController : Controller
    {
        private FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();

        [HttpPost]
        public JsonResult AddItem(int ProductID)
        {
            Product product = db.Products.SingleOrDefault(x => x.IdProduct == ProductID);
            if (Session["Cart"] == null)
            {
                Session["Cart"] = new List<ItemCart>();
            }
            List<ItemCart> itemCarts = Session["Cart"] as List<ItemCart>;
            // Kiểm tra sản phẩm đã tồn tại trong giỏ hàng chưa
            ItemCart check = itemCarts.FirstOrDefault(x => x.ProductID == ProductID);
            // Kiểm tra số lượng tồn
            //if (itemCarts.Count > 0 && check != null && product.Quantity <= check.Quantity)
            //{
            //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            //}
            // Nếu tồn tại thì + số lượng lên 1
            if (check != null)
            {
                for (int i = 0; i < itemCarts.Count; i++)
                {
                    if (itemCarts[i].ProductID == ProductID)
                    {
                        itemCarts[i].Quantity += 1;
                    }
                }
            }
            // Nếu chưa thì thêm mới sản phẩm vào giỏ hàng bằng giá được giảm
            else if (product.StartTimePromotion < DateTime.Now && product.EndTimePromotion > DateTime.Now) 
            {
                //decimal? promotion_test = product.Promotion; //lấy giá trị thực của giá đc giảm
                //decimal promotion_real = promotion_test.Value;
                itemCarts.Add(new ItemCart() { ProductID = product.IdProduct, ProductName = product.NameProduct, ProductPrice = product.Promotion.Value, ProductImage = product.Img, Quantity = 1 });
            }else
            {
                itemCarts.Add(new ItemCart() { ProductID = product.IdProduct, ProductName = product.NameProduct, ProductPrice = product.Price, ProductImage = product.Img, Quantity = 1 });
            }
            Session["Cart"] = itemCarts;
            return Json(new { status = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetTotalCart()
        {
            List<ItemCart> itemCarts = Session["Cart"] as List<ItemCart>;
            return Json(new { TotalPrice = itemCarts.Sum(x => x.ProductPrice * x.Quantity).ToString("#,##"), TotalQuantity = itemCarts.Sum(x => x.Quantity) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateQuantity(int ProductID, int Quantity)
        {
            List<ItemCart> itemCarts = Session["Cart"] as List<ItemCart>;
            
            for (int i = 0; i < itemCarts.Count; i++)
            {
                if (itemCarts[i].ProductID == ProductID)
                {
                    if (Quantity > 0)
                    {
                        itemCarts[i].Quantity = Quantity;
                        break;
                    }
                    else
                    {
                        itemCarts.RemoveAt(i);
                        break;
                    }
                }
            }
            Session["Cart"] = itemCarts;
            if (Quantity > 0)
            {
                return Json(new { update = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { remove = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetSubTotal(int ProductID = 1)
        {
            List<ItemCart> itemCarts = Session["Cart"] as List<ItemCart>;
            return Json(new { SubTotal = itemCarts.Where(x => x.ProductID == ProductID).Sum(x => x.ProductPrice * x.Quantity).ToString("#,##") }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetTotal()
        {
            List<ItemCart> itemCarts = Session["Cart"] as List<ItemCart>;
            return Json(new { Total = itemCarts.Sum(x => x.ProductPrice * x.Quantity).ToString("#,##") }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Checkout()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddOrder(string payment = "")
        {
            User user = Session["User"] as User;
            //Add order
            Order order = new Order();
            order.DateOrder = DateTime.Now;
            order.DateShip = DateTime.Now.AddDays(3);
            order.Status = "Processed";
            order.IdUser = user.IdUser;
            order.IsPaid = false;
            db.Orders.Add(order);
            db.SaveChanges();
            int o = db.Orders.OrderByDescending(p => p.IdOrder).FirstOrDefault().IdOrder;
            Session["OrderId"] = o;
            //Add order detail
            List<ItemCart> listCart = Session["Cart"] as List<ItemCart>;
            foreach (ItemCart item in listCart)
            {
                OrderDetail orderDetail = new OrderDetail();
                orderDetail.IdOrder = order.IdOrder;
                orderDetail.IdProduct = item.ProductID;
                orderDetail.Quantity = item.Quantity;
                orderDetail.Price = item.ProductPrice;
                orderDetail.NameProduct = item.ProductName;
                orderDetail.ImageProduct = item.ProductImage;
                db.OrderDetails.Add(orderDetail);
            }
            db.SaveChanges();
            // Payment
            if (payment == "paypal")
            {
                return RedirectToAction("PaymentWithPaypal", "Payment");
            }
            else if (payment == "momo")
            {
                return RedirectToAction("PaymentWithMomo", "Payment");
            }
            SentMail("Đặt hàng thành công", user.Email, "huynn4109@gmail.com", "google..huynn4109", "<p style=\"font-size:20px\">Cảm ơn bạn đã đặt hàng<br/>Mã đơn hàng của bạn là: " + order.IdOrder);

            Session.Remove("Cart");
            Session.Remove("OrderID");
            return RedirectToAction("Message", new { mess = "Đặt hàng thành công" });
        }
        public void SentMail(string Title, string ToEmail, string FromEmail, string Password, string Content)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(ToEmail);
            mail.From = new MailAddress(ToEmail);
            mail.Subject = Title;
            mail.Body = Content;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(FromEmail, Password);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
        public ActionResult Message(string mess)
        {
            ViewBag.Message = mess;
            return View();
        }

    }
}
