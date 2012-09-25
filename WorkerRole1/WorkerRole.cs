using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System.Net.Mail;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("WorkerRole1 entry point called", "Information");

            //string dataConn = CloudConfigurationManager.GetSetting("DatabaseConnectionString");
            string dataConn = RoleEnvironment.GetConfigurationSettingValue("AzureDatabaseConnectionString");
            Trace.WriteLine("The setting value is: " + dataConn, "Information");

            while (true)
            {
                Trace.WriteLine("Running", "Information");

                MailMessage Email = new MailMessage("cm@roundedco.com", "cm@roundedco.com");
                Email.Subject = "Text fax send via email";
                Email.Subject = "Subject Of email";
                Email.Body = "Body of email";
                System.Net.Mail.SmtpClient client = new SmtpClient("smtp.sendgrid.com", 25);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential("cmeigs", "sendgrid08");
                try
                {
                    //client.Send(Email);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("email failed to send", "Failure");
                }
                finally
                {
                    Email.Dispose();
                }

                //Thread.Sleep(86400000); //one day
                //Thread.Sleep(3600000);  //one hour
                Thread.Sleep(7200000);  //two hours
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }


        //private List<Dispute> GetJudgments()
        //{
        //    return null;
        //}


    }
}
