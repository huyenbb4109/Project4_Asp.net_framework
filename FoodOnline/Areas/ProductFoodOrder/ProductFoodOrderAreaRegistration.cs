using System.Web.Mvc;

namespace FoodOnline.Areas.ProductFoodOrder
{
    public class ProductFoodOrderAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ProductFoodOrder";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ProductFoodOrder_default",
                "ProductFoodOrder/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}