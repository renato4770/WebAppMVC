using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyEcommerceAdmin.Models;
namespace MyEcommerceAdmin.Controllers
{
    public class HomeController : Controller
    {
        MyEcommerceDbContext db = new MyEcommerceDbContext();

        // GET: Home
        public ActionResult Index()
        {
            ViewBag.MenProduct = db.Products.Where(x => x.Category.Name.Equals("Hombres")).ToList();
            ViewBag.WomenProduct = db.Products.Where(x => x.Category.Name.Equals("Mujer")).ToList();
            ViewBag.AccessoriesProduct = db.Products.Where(x => x.Category.Name.Equals("Accesorios Electronicos")).ToList();
            ViewBag.ElectronicsProduct = db.Products.Where(x => x.Category.Name.Equals("Deportivo y Campo")).ToList();
            ViewBag.Slider = db.genMainSliders.ToList();
            ViewBag.PromoRight = db.genPromoRights.ToList();

            this.GetDefaultData();

            return View();
        }
    }
}