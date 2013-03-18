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
using System.IO;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        public string databaseConnectionString = RoleEnvironment.GetConfigurationSettingValue("AzureDatabaseConnectionString");
        
        public override void Run()
        {
            // not called for some reason
            //Trace.WriteLine("WorkerRole1 entry point called", "Information");

            while (true)
            {
                //Trace.WriteLine("Running", "Information");

                try
                {
                    SendTestEmail();

                    List<Judgment> judgmentList15 = GetJudgmentList(15);
                    foreach (Judgment judgment in judgmentList15)
                        Send15Email(judgment);

                    List<Judgment> judgmentList30 = GetJudgmentList(30);
                    foreach (Judgment judgment in judgmentList30)
                        Send30Email(judgment);

                    List<Judgment> judgmentList45 = GetJudgmentList(45);
                    foreach (Judgment judgment in judgmentList45)
                        Send45Email(judgment);

                    //Trace.WriteLine("Sleep my pretty...");

                    Thread.Sleep(86400000); //one day
                    //Thread.Sleep(43200000); //six hours
                    //Thread.Sleep(7200000);  //two hours
                    //Thread.Sleep(3600000);  //one hour
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error in Run(): " + ex.Message, "Exception");
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // add listener instance if it doesn't exist, will create the Table WADLogsTable in Azure Storage
            bool haveListener = false;
            foreach (object listener in Trace.Listeners)
            {
                DiagnosticMonitorTraceListener dmtListener = listener as DiagnosticMonitorTraceListener;
                if (dmtListener != null)
                {
                    haveListener = true;
                    break;
                }
            }
            if (!haveListener)
            {
                Trace.Listeners.Add(new DiagnosticMonitorTraceListener());
                Trace.AutoFlush = true;
            }
            // not called for some reason
            //Trace.WriteLine("we have a valid trace listener");

            // Get the default initial configuration for DiagnosticMonitor.
            DiagnosticMonitorConfiguration diagnosticConfiguration = DiagnosticMonitor.GetDefaultInitialConfiguration();

            // Filter the logs so that ALL log messages are transferred to persistent storage.
            diagnosticConfiguration.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;

            // Schedule a transfer period of 30 minutes.
            diagnosticConfiguration.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(3.0);

            // Specify a buffer quota of 1GB.
            diagnosticConfiguration.Logs.BufferQuotaInMB = 1024;

            // Start the DiagnosticMonitor using the diagnosticConfig and our connection string.
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", diagnosticConfiguration);

            return base.OnStart();
        }


        private List<Judgment> GetJudgmentList(int days)
        {
            //Trace.WriteLine("GetJudgmentList Metod Called", "Information");

            List<Judgment> judgmentList = null;
            using (SqlConnection conn = new SqlConnection(databaseConnectionString))
            {
                using (SqlCommand command = conn.CreateCommand())
                {
                    try
                    {
                        conn.Open();
                        command.CommandText = "SELECT Name, Principal, Rate, EndDate, Name " +
                                            "FROM Disputes D " +
                                            "JOIN UserProfile UP on D.UserID = UP.UserID " +
                                            "WHERE IsActive = '1' " + 
                                            "AND EndDate = cast(GETDATE() + " + days + " as date)";
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
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Error in GetJudgmentList: " + ex.Message, "Exception");
                        return judgmentList; 
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }


        private void SendTestEmail()
        {
            Trace.WriteLine("Sending Test Email", "Information");

            MailMessage Email = new MailMessage("cm@roundedco.com", "cm@roundedco.com");
            Email.Subject = "Test Email from JudgmenTrac";
            Email.Body = "This is a test email from JudgmenTrac";
            Email.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.sendgrid.com", 25);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential("cmeigs", "sendgrid08");
            try
            {
                client.Send(Email);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Email failed to send: " + ex.Message, "Exception");
            }
            finally
            {
                Email.Dispose();
            }
        }


        private void Send15Email(Judgment judgment)
        {
            Trace.WriteLine("Sending 15 day email", "Information");

            MailMessage Email = new MailMessage("cm@roundedco.com", "cm@roundedco.com");
            Email.Subject = "Your Judgment is 15 days to Expiration";
            Email.Body = "Your Judgment is 15 days to Expiration unless you act NOW!";
            Email.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.sendgrid.com", 25);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential("cmeigs", "sendgrid08");
            try
            {
                //using (StreamReader reader = new StreamReader(@"../../../../../../WorkerRole1/EmailTemplate.html"))
                //{
                //    string output = "";
                //    String line = String.Empty;
                //    while ((line = reader.ReadLine()) != null)
                //    {
                //        output += line;
                //    }
                //    Email.Body = output;
                //}

                
                client.Send(Email);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Email failed to send: " + ex.Message, "Exception");
            }
            finally
            {
                Email.Dispose();
            }
        }


        private void Send30Email(Judgment judgment)
        {
            Trace.WriteLine("Sending 30 day email", "Information");
            
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
                Trace.WriteLine("Email failed to send: " + ex.Message, "Exception");
            }
            finally
            {
                Email.Dispose();
            }
        }


        private void Send45Email(Judgment judgment)
        {
            Trace.WriteLine("Sending 45 day email", "Information");
            
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
                Trace.WriteLine("Email failed to send: " + ex.Message, "Exception");
            }
            finally
            {
                Email.Dispose();
            }
        }





        private void ShowConfig(DiagnosticMonitorConfiguration config)
        {
            try
            {
                if (null == config)
                {
                    Trace.WriteLine("Null configuration passed to ShowConfig");
                    return;
                }

                // Display the general settings of the configuration
                Trace.WriteLine("*** General configuration settings ***");
                Trace.WriteLine("Config change poll interval: " + config.ConfigurationChangePollInterval.ToString());
                Trace.WriteLine("Overall quota in MB: " + config.OverallQuotaInMB);

                // Display the diagnostic infrastructure logs
                Trace.WriteLine("*** Diagnostic infrastructure settings ***");
                Trace.WriteLine("DiagnosticInfrastructureLogs buffer quota in MB: " + config.DiagnosticInfrastructureLogs.BufferQuotaInMB);
                Trace.WriteLine("DiagnosticInfrastructureLogs scheduled transfer log filter: " + config.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter);
                Trace.WriteLine("DiagnosticInfrastructureLogs transfer period: " + config.DiagnosticInfrastructureLogs.ScheduledTransferPeriod.ToString());

                // List the Logs info
                Trace.WriteLine("*** Logs configuration settings ***");
                Trace.WriteLine("Logs buffer quota in MB: " + config.Logs.BufferQuotaInMB);
                Trace.WriteLine("Logs scheduled transfer log level filter: " + config.Logs.ScheduledTransferLogLevelFilter);
                Trace.WriteLine("Logs transfer period: " + config.Logs.ScheduledTransferPeriod.ToString());

                // List the Directories info
                Trace.WriteLine("*** Directories configuration settings ***");
                Trace.WriteLine("Directories buffer quota in MB: " + config.Directories.BufferQuotaInMB);
                Trace.WriteLine("Directories scheduled transfer period: " + config.Directories.ScheduledTransferPeriod.ToString());
                int count = config.Directories.DataSources.Count, index;
                if (0 == count)
                {
                    Trace.WriteLine("No data sources for Directories");
                }
                else
                {
                    for (index = 0; index < count; index++)
                    {
                        Trace.WriteLine("Directories configuration data source:");
                        Trace.WriteLine("\tContainer: " + config.Directories.DataSources[index].Container);
                        Trace.WriteLine("\tDirectory quota in MB: " + config.Directories.DataSources[index].DirectoryQuotaInMB);
                        Trace.WriteLine("\tPath: " + config.Directories.DataSources[index].Path);
                        Trace.WriteLine("");
                    }
                }

                // List the event log info
                Trace.WriteLine("*** Event log configuration settings ***");
                Trace.WriteLine("Event log buffer quota in MB: " + config.WindowsEventLog.BufferQuotaInMB);
                count = config.WindowsEventLog.DataSources.Count;
                if (0 == count)
                {
                    Trace.WriteLine("No data sources for event log");
                }
                else
                {
                    for (index = 0; index < count; index++)
                    {
                        Trace.WriteLine("Event log configuration data source:" + config.WindowsEventLog.DataSources[index]);
                    }
                }
                Trace.WriteLine("Event log scheduled transfer log level filter: " + config.WindowsEventLog.ScheduledTransferLogLevelFilter);
                Trace.WriteLine("Event log scheduled transfer period: " + config.WindowsEventLog.ScheduledTransferPeriod.ToString());

                // List the performance counter info
                Trace.WriteLine("*** Performance counter configuration settings ***");
                Trace.WriteLine("Performance counter buffer quota in MB: " + config.PerformanceCounters.BufferQuotaInMB);
                Trace.WriteLine("Performance counter scheduled transfer period: " + config.PerformanceCounters.ScheduledTransferPeriod.ToString());
                count = config.PerformanceCounters.DataSources.Count;
                if (0 == count)
                {
                    Trace.WriteLine("No data sources for PerformanceCounters");
                }
                else
                {
                    for (index = 0; index < count; index++)
                    {
                        Trace.WriteLine("PerformanceCounters configuration data source:");
                        Trace.WriteLine("\tCounterSpecifier: " + config.PerformanceCounters.DataSources[index].CounterSpecifier);
                        Trace.WriteLine("\tSampleRate: " + config.PerformanceCounters.DataSources[index].SampleRate.ToString());
                        Trace.WriteLine("");
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception during ShowConfig: " + e.Message, "Exception");
                // Take other action as needed.
            }
        }
    





    }
}
