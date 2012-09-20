using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

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
        #endregion

        // GET: /Commerce/
        [Authorize]
        public ActionResult Index()
        {
            string apiLoginID = ConfigurationManager.AppSettings["AuthorizeAPILoginID"];

            /* Authorize.NET SDK methods to help with the direct post method (but not much documentation)
            AuthorizeNet.Crypto.GenerateSequence();
            AuthorizeNet.Crypto.GenerateFingerprint();
            */

            // will eventually be identity
            Random random = new Random();
            string invoice = random.Next(1, 100).ToString();
            ViewBag.Invoice = invoice;     
            
            //string totalSeconds = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds.ToString();
            //string secondsRounded = totalSeconds.Substring(0, totalSeconds.IndexOf("."));
            //ViewBag.Seconds = secondsRounded;
            ViewBag.Seconds = AuthorizeNet.Crypto.GenerateTimestamp();

            string amount = ConfigurationManager.AppSettings["JudgmentAmount"];
            ViewBag.TotalPrice = amount;
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

            ViewBag.Message = "Is Response Valid? " + isValid.ToString() +
                ", Message: " + response.Message +
                ", Invoice: " + response.InvoiceNumber +
                ", ResponseCode: " + response.ResponseCode +
                ", MD5Hash: " + response.MD5Hash +
                ", MD5HashTruncated: " + response.MD5Hash.Substring(0, 20) +
                ", AuthCode: " + response.AuthorizationCode +
                ", Approved: " + response.Approved +
                ", TransactionID: " + response.TransactionID +
                ", CreditCard: " + response.CardNumber +
                ", User ID: " + post.Get("x_cust_id") +
                ", Num_Judgment(s): " + post.Get("num_judgment");
            return View();

            //if it's not valid - just send them to the home page. Don't throw - that's how hackers figure out what's wrong :)
            if (!isValid)
                return Redirect("/");
            else
            {
                string returnUrl = "";
                if (!response.Approved)
                {
                    // get judgments purchased and add to database
                    string numJudgmentPurchased = post.Get("num_judment");

                    //does user have any existing 
                    //return RedirectToAction("Failure", "Commerce");
                    returnUrl = "http://judgment.azurewebsites.net/Commerce/Failure?m=" + response.Message;
                }
                else
                {
                    //return RedirectToAction("Success", "Commerce");
                    returnUrl = "http://judgment.azurewebsites.net/Commerce/Success?m=" + response.Message;
                }
                //the URL to redirect to- this MUST be absolute
                return Content(AuthorizeNet.Helpers.CheckoutFormBuilders.Redirecter(returnUrl));
            }
        }


        public ActionResult Success()
        {
            ViewBag.Message = Request.QueryString["m"];
            return View();
        }


        public ActionResult Failure()
        {
            ViewBag.Message = Request.QueryString["m"];
            return View();
        }



    }
}
