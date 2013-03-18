using Judgmentrac.Models;
using Judgmentrac.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;


namespace Judgmentrac.Controllers
{
    [Authorize]        
    public class JudgmentController : SuperController
    {
        private JudgmentDB db = new JudgmentDB();


        // GET: /Judgment/
        public ActionResult Index(string sortOrder)
        {
            // do we have an error in the query string?
            //string error = this.Request.QueryString["err"];
            //if (error != "") ViewBag.ErrorMessage = error;

            // viewbag sort variables
            ViewBag.NameSortParm = sortOrder == "Name" ? "Name desc" : "Name";
            ViewBag.StartSortParm = sortOrder == "Start" ? "Start desc" : "Start";
            ViewBag.EndSortParm = sortOrder == "End" ? "End desc" : "End";

            IEnumerable<Dispute> disputeList = GetJudgmentsByUserId(WebSecurity.CurrentUserId);
            
            // create an instance of view model DisputeViewModel so we can take advantage of computed properties (not stored in DB)
            List<DisputeViewModel> disputeViewList = new List<DisputeViewModel>();
            foreach (Dispute dispute in disputeList)
                disputeViewList.Add(new DisputeViewModel(dispute));

            // sort
            switch (sortOrder)
            {
                case "Name":
                    disputeViewList = disputeViewList.OrderBy(s => s.Dispute.Name).ToList();
                    break;
                case "Name desc":
                    disputeViewList = disputeViewList.OrderByDescending(s => s.Dispute.Name).ToList();
                    break;
                case "Start":
                    disputeViewList = disputeViewList.OrderBy(s => s.Dispute.StartDate).ToList();
                    break;
                case "Start desc":
                    disputeViewList = disputeViewList.OrderByDescending(s => s.Dispute.StartDate).ToList();
                    break;
                case "End":
                    disputeViewList = disputeViewList.OrderBy(s => s.Dispute.EndDate).ToList();
                    break;
                case "End desc":
                    disputeViewList = disputeViewList.OrderByDescending(s => s.Dispute.EndDate).ToList();
                    break;
            }

            ViewBag.JudgmentCount = GetJudgmentCount();
            return View(disputeViewList);
        }

       
        // GET: /Judgment/Create
        public ActionResult Create()
        {
            if (WebSecurity.IsAuthenticated)
            {
                int judgmentCount = GetJudgmentCount();
                if (judgmentCount > 0)
                {
                    ViewBag.JudgmentCount = judgmentCount;
                    return View("Create");
                }
                else
                    return View("NoJudgment");
            }
            else
            {
                return View("NoJudgment");
            }
        }


        // POST: /Judgement/Create
        [HttpPost]
        public ActionResult Create(Dispute disputeInput)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var judgment = new JudgmentDB())
                    {
                        var dispute = new Dispute
                        {
                            Name = disputeInput.Name,
                            Principal = disputeInput.Principal,
                            Rate = disputeInput.Rate,
                            StartDate = disputeInput.StartDate,
                            EndDate = disputeInput.EndDate,
                            UserId = WebSecurity.CurrentUserId,
                            IsActive = true
                        };

                        judgment.Disputes.Add(dispute);
                        judgment.SaveChanges();
                    }

                    return RedirectToAction("Index", new { Message = "Judgment Successfully Created" });
                }
                else
                {
                    ViewBag.ErrorMsg = "Error on form, please fix and resubmit";
                    return View();
                }
            }
            catch
            {
                ViewBag.ErrorMsg = "Error adding judgment, please try again.";
                return View();
            }
        }


        // POST: /Judgment/Delete/5
        [HttpPost]
        public bool Delete(int id)
        {
            try
            {
                Dispute dispute = db.Disputes.Find(id);
                dispute.IsActive = false;
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
