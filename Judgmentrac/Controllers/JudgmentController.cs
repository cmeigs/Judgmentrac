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
    public class JudgmentController : Controller
    {
        private JudgmentDB db = new JudgmentDB();


        // GET: /Judgment/
        public ActionResult Index()
        {
            var disputes = from j in db.Disputes
                             where j.UserId == WebSecurity.CurrentUserId
                             select j;

            // create an instance of view model DisputeViewModel so we can take advantage of a computed property (not in DB)
            IEnumerable<Dispute> disputeList = disputes.ToList();
            List<DisputeViewModel> disputeViewList = new List<DisputeViewModel>();
            foreach (Dispute dispute in disputeList)
                disputeViewList.Add(new DisputeViewModel(dispute));

            return View(disputeViewList);
        }

       
        // GET: /Judgment/Create
        public ActionResult Create()
        {
            if (WebSecurity.IsAuthenticated)
            {
                int userID = WebSecurity.CurrentUserId;

                var userProfileJudgment = from upj in db.UserProfileJudgments
                                          where upj.UserId == userID
                                          select upj;

                IEnumerable<UserProfileJudgment> judgmentList = userProfileJudgment.ToList();
                if (judgmentList.Count() > 0)
                {
                    ViewBag.JudgmentCount = judgmentList.Count();
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
            //needed for WebSecurity below
            //WebSecurity.InitializeDatabaseConnection("JudgmentDB", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            try
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
                        UserId = WebSecurity.CurrentUserId
                    };

                    judgment.Disputes.Add(dispute);
                    judgment.SaveChanges();
                }

                return RedirectToAction("Index", new { Message = "Judgment Successfully Created" });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = "Error adding judgment, please try again. " + ex.Message;
                return View();
            }
        }
    }
}
