using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyEcommerceAdmin.Models;
using System.Data;

namespace MyEcommerceAdmin.Controllers
{
    public class CheckOutController : Controller
    {
        MyEcommerceDbContext db = new MyEcommerceDbContext();

        public ActionResult Index()
        {
            ViewBag.PayMethod = new SelectList(db.PaymentTypes, "PayTypeID", "TypeName");
            var data = this.GetDefaultData();

            if (TempShpData.items != null && TempShpData.items.Any())
            {
                decimal subtotal = TempShpData.items.Sum(item => item.TotalAmount ?? 0);
                decimal discount = 0; // Asume que no hay descuento por defecto
                decimal iva = Math.Round(subtotal * 0.13m, 2); // 13% de IVA, redondeado a 2 decimales
                decimal totalAmount = subtotal + iva - discount;

                ViewBag.SubTotal = subtotal.ToString("C");
                ViewBag.Discount = discount.ToString("C");
                ViewBag.Taxes = iva.ToString("C");
                ViewBag.TotalAmount = totalAmount.ToString("C");
            }
            else
            {
                ViewBag.SubTotal = "$0.00";
                ViewBag.Discount = "$0.00";
                ViewBag.Taxes = "$0.00";
                ViewBag.TotalAmount = "$0.00";
            }

            return View(data);
        }

        //PLACE ORDER--LAST STEP
        public ActionResult PlaceOrder(FormCollection getCheckoutDetails)
        {
            int shpID = db.ShippingDetails.Any() ? db.ShippingDetails.Max(x => x.ShippingID) + 1 : 1;
            int payID = db.Payments.Any() ? db.Payments.Max(x => x.PaymentID) + 1 : 1;
            int orderID = db.Orders.Any() ? db.Orders.Max(x => x.OrderID) + 1 : 1;

            ShippingDetail shpDetails = new ShippingDetail
            {
                ShippingID = shpID,
                FirstName = getCheckoutDetails["FirstName"],
                LastName = getCheckoutDetails["LastName"],
                Email = getCheckoutDetails["Email"],
                Mobile = getCheckoutDetails["Mobile"],
                Address = getCheckoutDetails["Address"],
                City = getCheckoutDetails["City"],
                PostCode = getCheckoutDetails["PostCode"]
            };
            db.ShippingDetails.Add(shpDetails);
            db.SaveChanges();

            Payment pay = new Payment
            {
                PaymentID = payID,
                Type = Convert.ToInt32(getCheckoutDetails["PayMethod"])
            };
            db.Payments.Add(pay);
            db.SaveChanges();

            // Cálculo del subtotal, IVA y total
            decimal subtotal = (decimal)TempShpData.items.Sum(item => item.TotalAmount);
            decimal discount = 0;//Convert.ToDecimal(getCheckoutDetails["discount"]);
            decimal iva = Math.Round(subtotal * 0.13m, 2); // 13% de IVA, redondeado a 2 decimales
            decimal totalAmount = subtotal + iva - discount;

            Order o = new Order
            {
                OrderID = orderID,
                CustomerID = TempShpData.UserID,
                PaymentID = payID,
                ShippingID = shpID,
                Discount = Convert.ToInt32(discount),
                Taxes = Convert.ToInt32(iva),
                TotalAmount = Convert.ToInt32(totalAmount),
                isCompleted = true,
                OrderDate = DateTime.Now
            };
            db.Orders.Add(o);
            db.SaveChanges();

            foreach (var OD in TempShpData.items)
            {
                OD.OrderID = orderID;
                OD.Order = db.Orders.Find(orderID);
                OD.Product = db.Products.Find(OD.ProductID);
                db.OrderDetails.Add(OD);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "ThankYou");
        }
    }
}