using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Judgmentrac.Models;

namespace Judgmentrac.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private JudgmentDB db = new JudgmentDB();

        // GET: /Payment/Details/5
        public ActionResult Details(int id = 0)
        {
            Payment payment = db.Payments.Find(id);
            if (payment == null)
                return HttpNotFound();
            else
                return View("Details", payment);
        }


        // GET: /Payment/PartialDetails/5
        public ActionResult PartialDetails(int id = 0)
        {
            Dispute dispute = db.Disputes.Find(id);
            // get associated payments and sort
            db.Entry(dispute).Collection(p => p.Payments).Load();
            dispute.Payments = dispute.Payments.AsQueryable().OrderBy(p => p.PayDate).ToList();
            
            ViewBag.DisputeID = id;
            return PartialView("PartialDetails", dispute.Payments);
        }


        // GET: /Payment/Create/3
        public ActionResult Create(int id = 0)
        {
            // get dispute entity and place in viewbag
            Dispute dispute = db.Disputes.Find(id);
            if (dispute == null)
                return HttpNotFound();
            else
            {
                //ViewBag.Dispute = dispute;
                return View();
            }
        }


        // POST: /Payment/Create
        [HttpPost]
        public ActionResult Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                Dispute dispute = db.Disputes.Find(payment.ID);
                if (dispute != null)
                {
                    try
                    {
                        if (dispute.Payments == null) dispute.Payments = new List<Payment>();
                        dispute.Payments.Add(new Payment
                        {
                            PayDate = payment.PayDate,
                            Amount = payment.Amount
                        });
                        db.SaveChanges();
                        return RedirectToAction("Index", "Judgment");
                    }
                    catch
                    {
                        ViewBag.ErrorMsg = "Error saving payment, please try again";
                        return View();
                    }
                }
                else
                    return HttpNotFound();
            }
            else
            {
                ViewBag.ErrorMsg = "Error saving payment, please try again";
                return View(payment);
            }
        }


        // GET: /Payment/Delete/5
        //public ActionResult Delete(int id = 0)
        //{
        //    Payment payment = db.Payments.Find(id);
        //    if (payment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(payment);
        //}


        // POST: /Payment/Delete/5
        [HttpPost]
        public bool Delete(int id)
        {
            try
            {
                Payment payment = db.Payments.Find(id);
                db.Payments.Remove(payment);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}