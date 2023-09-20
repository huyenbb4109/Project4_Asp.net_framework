using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.UI.WebControls;
using FoodOnline.Models;
using FoodOnline.PostedFileBase;
using PagedList;
using PayPal.Api;

namespace FoodOnline.Areas.ProductFoodOrder.Controllers
{
    public class ProductsController : Controller
    {
        private FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();

        // GET: ProductFoodOrder/Products
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page, int? size)
        {
            //phan trang theo dropdown
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "5", Value = "5" });
            items.Add(new SelectListItem { Text = "10", Value = "10" });
            items.Add(new SelectListItem { Text = "20", Value = "20" });
            items.Add(new SelectListItem { Text = "25", Value = "25" });
            items.Add(new SelectListItem { Text = "50", Value = "50" });
            items.Add(new SelectListItem { Text = "100", Value = "100" });
            items.Add(new SelectListItem { Text = "200", Value = "200" });

            // 1.1. Giữ trạng thái kích thước trang được chọn trên DropDownList
            foreach (var item in items)
            {
                if (item.Value == size.ToString()) item.Selected = true;
            }

            // 1.2. Tạo các biến ViewBag
            ViewBag.size = items; // ViewBag DropDownList
            ViewBag.currentSize = size; // tạo biến kích thước trang hiện tại


            //=====================sắp xếp============================================
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.Price = sortOrder == "price" ? "price_desc" : "price";
            //ViewBag.FullName = sortOrder == "fullname" ? "fullname_desc" : "fullname";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var ps = from s in db.Products where s.IsDelete == true select s;//o product ton tai
            if (!String.IsNullOrEmpty(searchString))
            {
                ps = ps.Where(s => s.NameProduct.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    ps = ps.OrderByDescending(s => s.NameProduct);
                    break;
                case "price":
                    ps = ps.OrderBy(s => s.Price);
                    break;
                case "price_desc":
                    ps = ps.OrderByDescending(s => s.Price);
                    break;
                default:  // Name ascending 
                    ps = ps.OrderBy(s => s.NameProduct);
                    break;
            }


            //int pageSize = 10;
            int pageSize = (size ?? 5);
            int pageNumber = (page ?? 1);
            return View(ps.ToPagedList(pageNumber, pageSize));
            //var products = db.Products.Include(p => p.Category);
            //return View(products.ToList());
        }

        // GET: ProductFoodOrder/Products/Details/5
        public ActionResult Details(int? id)
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

        // GET: ProductFoodOrder/Products/Create
        public ActionResult Create()
        {
            ViewBag.IdCategory = new SelectList(db.Categories, "IdCategory", "NameCategory");
            return View();
        }

        // POST: ProductFoodOrder/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdProduct,NameProduct,IdCategory,Img,Price,Describe,Promotion,StartTimePromotion,EndTimePromotion,IsDelete")] Product product,
            HttpPostedFileBase image)
        {

            if (ModelState.IsValid)
            {
                if (HttpPostedFileBaseExtensions.IsImage(image) == false)
                {
                    TempData["ErrorImg"] = "Create food fail, image not correct!";
                    return RedirectToAction("Create");
                }
                else
                {
                    //if (image != null && image.ContentLength > 0 )
                    //{
                    string path = Server.MapPath("~/Areas/UploadedFiles");
                    string fileName = Path.GetFileName(image.FileName);
                    string fullPath = Path.Combine(path, fileName);

                    image.SaveAs(fullPath);
                    product.Img = image.FileName;


                }


                product.NameProduct = product.NameProduct.ToString().Trim();
                if (product.Describe != null)
                {
                    product.Describe = product.Describe.ToString().Trim();
                }

                product.IsDelete = true;

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdCategory = new SelectList(db.Categories, "IdCategory", "NameCategory", product.IdCategory);

            return View(product);
        }

        // GET: ProductFoodOrder/Products/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.IdCategory = new SelectList(db.Categories, "IdCategory", "NameCategory", product.IdCategory);
            return View(product);
        }

        // POST: ProductFoodOrder/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdProduct,NameProduct,IdCategory,Img,Price,Describe,Promotion,StartTimePromotion,EndTimePromotion,IsDelete")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdCategory = new SelectList(db.Categories, "IdCategory", "NameCategory", product.IdCategory);
            return View(product);
        }

        // GET: ProductFoodOrder/Products/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: ProductFoodOrder/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            product.IsDelete = false;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
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
