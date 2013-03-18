using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using WebMatrix.WebData;
using Judgmentrac.Models;

namespace Judgmentrac.Controllers
{
    public class CommerceController : Controller
    {

        #region Helper Methods
        private string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }

        private bool DeleteInvoice(int invoiceID)
        {
            try
            {
                JudgmentDB db = new JudgmentDB();
                UserProfileJudgment invoice = db.UserProfileJudgments.Find(invoiceID);
                db.UserProfileJudgments.Remove(invoice);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool UpdateInvoice(int invoiceID, int numJudgments)
        {
            try
            {
                JudgmentDB db = new JudgmentDB();
                UserProfileJudgment invoice = db.UserProfileJudgments.Find(invoiceID);
                invoice.JudgmentCount = numJudgments;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        // GET: /Commerce/
        [Authorize]
        public ActionResult Index()
        {
            string amount = ConfigurationManager.AppSettings["JudgmentAmount"];
            ViewBag.TotalPrice = amount;

            return View();
        }


        // GET: /Commerce/Purchase
        [Authorize]
        [HttpPost]
        public ActionResult Purchase(FormCollection postData)
        {
            string apiLoginID = ConfigurationManager.AppSettings["AuthorizeAPILoginID"];
            
            // get amount of purchase
            string amount = postData["x_amount"];
            ViewBag.Amount = amount;
            ViewBag.NumJudgment = postData["num_judgment"];

            // Invoice: create a new UserProfileJudgment record that will become the invoice (after we hear back from Authorize.NET)
            int invoice = 0;
            using (var judgment = new JudgmentDB())
            {
                UserProfileJudgment userProfileJudgment = new UserProfileJudgment
                {
                    UserId = WebSecurity.CurrentUserId,
                    JudgmentCount = 0
                };
                judgment.UserProfileJudgments.Add(userProfileJudgment);
                if (judgment.SaveChanges() == 1)
                    invoice = userProfileJudgment.invoice;
            }
            ViewBag.Invoice = invoice.ToString();

            // generate timestamp for Authorize.NET
            //string totalSeconds = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds.ToString();
            //string secondsRounded = totalSeconds.Substring(0, totalSeconds.IndexOf("."));
            //ViewBag.Seconds = secondsRounded;
            ViewBag.Seconds = AuthorizeNet.Crypto.GenerateTimestamp();

            /* Authorize.NET SDK methods to help with the direct post method (but not much documentation)
            AuthorizeNet.Crypto.GenerateSequence();
            AuthorizeNet.Crypto.GenerateFingerprint();
            */

            string hashSeed = apiLoginID + "^" + invoice + "^" + ViewBag.Seconds + "^" + amount + "^";
            string hashKey = ConfigurationManager.AppSettings["AuthorizeTransactionKey"];

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(hashKey);
            HMACMD5 hmacmd5 = new HMACMD5(keyByte);
            byte[] messageBytes = encoding.GetBytes(hashSeed);
            byte[] hashmessage = hmacmd5.ComputeHash(messageBytes);
            ViewBag.MD5Hash = ByteToString(hashmessage);

            return View();
        }


        [HttpPost]
        public ActionResult DPM(FormCollection post)
        {
            var response = new AuthorizeNet.SIMResponse(post);

            //validate that it was Auth.net that posted this using the first 20 characters of the "MD5 hash" string specifiec in the settins admin pannel
            bool isValid = response.Validate(ConfigurationManager.AppSettings["AuthorizeMerchantHash"], ConfigurationManager.AppSettings["AuthorizeAPILoginID"]);

            //foreach (string key in post.Keys)
            //    ViewBag.Message += "Key: " + key + "<br>";

            //ViewBag.Message = "Is Response Valid? " + isValid.ToString() +
            //    ", Message: " + response.Message +
            //    ", Invoice: " + response.InvoiceNumber +
            //    ", ResponseCode: " + response.ResponseCode +
            //    ", MD5Hash: " + response.MD5Hash +
            //    ", MD5HashTruncated: " + response.MD5Hash.Substring(0, 20) +
            //    ", AuthCode: " + response.AuthorizationCode +
            //    ", Approved: " + response.Approved +
            //    ", TransactionID: " + response.TransactionID +
            //    ", CreditCard: " + response.CardNumber +
            //    ", User ID: " + post.Get("x_cust_id") +
            //    ", Num_Judgment(s): " + post.Get("num_judgment");
            //return View();

            //if it's not valid - just send them to the home page. Don't throw - that's how hackers figure out what's wrong :)
            if (!isValid)
                return Redirect("/");
            else
            {
                string returnUrl = "";
                if (response.Approved)
                {
                    //get previously created invoice record and update with the correct judgments purchased
                    //JudgmentDB db = new JudgmentDB();
                    //int userID = Convert.ToInt32(post.Get("x_cust_id"));
                    //var invoice = (
                    //    from i in db.UserProfileJudgments
                    //    where i.UserId == userID && i.JudgmentCount == 0
                    //    select i).Single();
                    //invoice.JudgmentCount = Convert.ToInt32(post.Get("num_judgment"));
                    //db.SaveChanges();

                    returnUrl = ConfigurationManager.AppSettings["AuthorizeReturnURL"] + "Commerce/Success?n=" + post.Get("num_judgment") + "&i=" + post.Get("x_cust_id") + "&m=" + response.Message;
                }
                else
                {
                    returnUrl = ConfigurationManager.AppSettings["AuthorizeReturnURL"] + "Commerce/Failure?m=" + response.Message;
                }
                
                //the URL to redirect to- this MUST be absolute
                return Content(AuthorizeNet.Helpers.CheckoutFormBuilders.Redirecter(returnUrl));
            }
        }


        public ActionResult Success()
        {
            try
            {
                ViewBag.Message = Request.QueryString["m"];
                int userID = Convert.ToInt32(Request.QueryString["i"]);
                int numJudgments = Convert.ToInt32(Request.QueryString["n"]);

                JudgmentDB db = new JudgmentDB();
                var invoice = (
                    from i in db.UserProfileJudgments
                    where i.UserId == userID && i.JudgmentCount == 0
                    select i);
                List<UserProfileJudgment> userProfileJudgmentList = invoice.ToList();

                int index = 0;
                int count = userProfileJudgmentList.Count();
                foreach (UserProfileJudgment upg in userProfileJudgmentList)
                {
                    if (index + 1 < count)
                        DeleteInvoice(upg.invoice);
                    else
                        UpdateInvoice(upg.invoice, numJudgments);
                    index++;
                }
                return View();
            }
            catch
            {
                ViewBag.Message = "Error saving success data";
                return View();
            }
        }


        public ActionResult Failure()
        {
            ViewBag.Message = Request.QueryString["m"];
            return View();
        }



    }
}
