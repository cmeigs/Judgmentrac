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

using System.Data.SqlClient;
using System.Data;
using WorkerRole1.Models;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        public string databaseConnectionString = RoleEnvironment.GetConfigurationSettingValue("AzureDatabaseConnectionString");
        
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("WorkerRole1 entry point called", "Information");

            while (true)
            {
                Trace.WriteLine("Running", "Information");

                List<Judgment> judgmentList15 = GetJudgmentList(15);
                foreach (Judgment judgment in judgmentList15)
                    Send15Email(judgment);

                List<Judgment> judgmentList30 = GetJudgmentList(30);
                foreach (Judgment judgment in judgmentList30)
                    Send30Email(judgment);

                List<Judgment> judgmentList45 = GetJudgmentList(45);
                foreach (Judgment judgment in judgmentList45)
                    Send45Email(judgment);

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


        private List<Judgment> GetJudgmentList(int days)
        {
            List<Judgment> judgmentList = null;
            using (SqlConnection conn = new SqlConnection(databaseConnectionString))
            {
                using (SqlCommand command = conn.CreateCommand())
                {
                    try
                    {
                        conn.Open();
                        command.CommandText = "SELECT Name, Principal, Rate, EndDate, UserName " +
                                            "FROM Disputes D " +
                                            "JOIN UserProfile UP on D.UserID = UP.UserID " +
                                            "WHERE EndDate = cast(GETDATE() + " + days + " as date)";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            judgmentList = new List<Judgment>();
                            while (reader.Read())
                            {
                                judgmentList.Add(new Judgment
                                {
                                    JudgmentName = reader["Name"].ToString().Trim(),
                                    UserName = reader["UserName"].ToString().Trim(),
                                    Principal = Convert.ToInt32(reader["Principal"]),
                                    Rate = Convert.ToInt32(reader["Rate"]),
                                    EndDate = Convert.ToDateTime(reader["EndDate"])
                                });
                            }
                            return judgmentList;
                        }
                    }
                    catch
                    {
                        return judgmentList;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }


        private void Send15Email(Judgment judgment)
        {
            MailMessage Email = new MailMessage("cm@roundedco.com", "cm@roundedco.com");
            Email.Subject = "Your Judgment is 15 days to Expiration";
            Email.Body = "Your Judgment is 15 days to Expiration unless you act NOW!";
            
            SmtpClient client = new SmtpClient("smtp.sendgrid.com", 25);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential("cmeigs", "sendgrid08");
            try
            {
                client.Send(Email);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("email failed to send", "Failure");
            }
            finally
            {
                Email.Dispose();
            }
        }


        private void Send30Email(Judgment judgment)
        {
            MailMessage Email = new MailMessage("cm@roundedco.com", "cm@roundedco.com");
            Email.Subject = "Your Judgment is 30 days to Expiration";
            Email.Body = "Your Judgment is 30 days to Expiration unless you act NOW!";

            SmtpClient client = new SmtpClient("smtp.sendgrid.com", 25);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential("cmeigs", "sendgrid08");
            try
            {
                client.Send(Email);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("email failed to send", "Failure");
            }
            finally
            {
                Email.Dispose();
            }
        }


        private void Send45Email(Judgment judgment)
        {
            MailMessage Email = new MailMessage("cm@roundedco.com", "cm@roundedco.com");
            Email.Subject = "Your Judgment is 45 days to Expiration";
            Email.Body = "Your Judgment is 45 days to Expiration unless you act NOW!";

            SmtpClient client = new SmtpClient("smtp.sendgrid.com", 25);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential("cmeigs", "sendgrid08");
            try
            {
                client.Send(Email);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("email failed to send", "Failure");
            }
            finally
            {
                Email.Dispose();
            }
        }

    }
}
