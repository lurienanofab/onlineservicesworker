using LNF;
using LNF.Billing;
using LNF.Billing.Reports;
using LNF.CommonTools;
using LNF.Impl.Billing;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace OnlineServicesWorker
{
    public class TaskManager
    {
        private readonly IProvider _provider;

        public TaskManager(IProvider provider)
        {
            _provider = provider;
        }

        public string RunFiveMinuteTask()
        {
            DateTime now = DateTime.Now;

            string message = string.Empty;

            ProcessResult result;

            // every five minutes tasks
            var pastEndableRepairReservations = _provider.Scheduler.Reservation.SelectPastEndableRepair();
            var util = Reservations.Create(_provider, DateTime.Now);
            result = util.HandleRepairReservations(pastEndableRepairReservations);
            message += result.LogText;

            var autoEndReservations = _provider.Scheduler.Reservation.SelectAutoEnd();
            result = util.HandleAutoEndReservations(autoEndReservations);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            var pastUnstartedReservations = _provider.Scheduler.Reservation.SelectPastUnstarted();
            result = util.HandleUnstartedReservations(pastUnstartedReservations);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            var sendFiveMinuteTaskEmail = GlobalSettings.Current.GetGlobalSetting("SendFiveMinuteTaskEmail");

            if (!string.IsNullOrEmpty(sendFiveMinuteTaskEmail))
            {
                // send an email
                SendEmail.SendSystemEmail("OnlineServicesWorker.TaskManager.RunFiveMinuteTask", $"FiveMinuteTask result [{now:yyyy-MM-dd HH:mm:ss}]", message, sendFiveMinuteTaskEmail.Split(','), false);
            }

            return message;
        }

        public string RunDailyTask(bool noEmail)
        {
            DateTime now = DateTime.Now;

            string message = string.Empty;

            ProcessResult result;

            // Check client tool auths
            result = ResourceClients.CheckExpiringClients(ResourceClients.SelectExpiringClients(), ResourceClients.SelectExpiringEveryone(), noEmail);
            message += result.LogText;

            result = ResourceClients.CheckExpiredClients(ResourceClients.SelectExpiredClients(), ResourceClients.SelectExpiredEveryone(), noEmail);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            // update the DataClean and Data tables
            _provider.Billing.Process.UpdateBilling(new UpdateBillingArgs { BillingCategory = BillingCategory.Tool | BillingCategory.Room | BillingCategory.Store, ClientID = 0, Periods = new DateTime[] { } });
            result = DataTableManager.Create(_provider).Update(BillingCategory.Tool | BillingCategory.Room | BillingCategory.Store);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            //2009-08-01 Populate the Billing temp tables
            DateTime yesterday = DateTime.Now.Date.AddDays(-1); //must be yesterday
            DateTime period = yesterday.FirstOfMonth();

            message += $"{Environment.NewLine}{Environment.NewLine}Yesterday: {yesterday:yyyy-MM-dd HH:mm:ss}";
            message += $"{Environment.NewLine}Period: {period:yyyy-MM-dd HH:mm:ss}";

            using (var conn = NewConnection())
            {
                conn.Open();

                var step1 = new BillingDataProcessStep1(new Step1Config { Connection = conn, Context = "OnlineServicesWorker.TaskManager.RunDailyTask", Period = period, Now = now, ClientID = 0, IsTemp = true });

                result = step1.PopulateToolBilling();
                message += Environment.NewLine + Environment.NewLine + result.LogText;

                result = step1.PopulateRoomBilling();
                message += Environment.NewLine + Environment.NewLine + result.LogText;

                result = step1.PopulateStoreBilling();
                message += Environment.NewLine + Environment.NewLine + result.LogText;

                conn.Close();
            }

            var sendDailyTaskEmail = GlobalSettings.Current.GetGlobalSetting("SendDailyTaskEmail");

            if (!string.IsNullOrEmpty(sendDailyTaskEmail))
            {
                // send an email
                SendEmail.SendSystemEmail("OnlineServicesWorker.TaskManager.RunDailyTask", $"DailyTask result [{now:yyyy-MM-dd HH:mm:ss}]", message, sendDailyTaskEmail.Split(','), false);
            }

            return message;
        }

        public string RunMonthlyTask(bool noEmail)
        {
            DateTime now = DateTime.Now;

            string message = string.Empty;

            ProcessResult result;

            // This is run at midnight on the 1st of the month. So the period should be the 1st of the previous month.
            DateTime period = DateTime.Now.FirstOfMonth().AddMonths(-1);

            message += $"Period: {period:yyyy-MM-dd HH:mm:ss}";

            var autoSend = new Dictionary<string, string>
            {
                ["ApportionmentReminder_AutoSendMonthlyEmail"] = GlobalSettings.Current.GetGlobalSetting("ApportionmentReminder_AutoSendMonthlyEmail"),
                ["FinancialManagerReport_AutoSendMonthlyEmail"] = GlobalSettings.Current.GetGlobalSetting("FinancialManagerReport_AutoSendMonthlyEmail"),
                ["ExpiringCardsReminder_AutoSendMonthlyEmail"] = GlobalSettings.Current.GetGlobalSetting("ExpiringCardsReminder_AutoSendMonthlyEmail")
            };
          

            // This sends apportionment emails to clients
            if (autoSend.Any(x => x.Key == "ApportionmentReminder_AutoSendMonthlyEmail" && x.Value == "true"))
            {
                result = _provider.Billing.Report.SendUserApportionmentReport(new UserApportionmentReportOptions { Period = period, Message = null, NoEmail = noEmail });
                message += Environment.NewLine + Environment.NewLine + result.LogText;
            }
            else
            {
                message += Environment.NewLine + Environment.NewLine + "ApportionmentReminder_AutoSendMonthlyEmail == false";
            }

            // 2008-04-30
            // Monthly financial report
            if (autoSend.Any(x => x.Key == "FinancialManagerReport_AutoSendMonthlyEmail" && x.Value == "true"))
            {
                var fm = new FinancialManagers(_provider.Billing.Report);
                result = fm.SendMonthlyUserUsageEmails(new FinancialManagerReportOptions() { Period = period, ClientID = 0, ManagerOrgID = 0, IncludeManager = !noEmail });
                message += Environment.NewLine + Environment.NewLine + result.LogText;
            }
            else
            {
                message += Environment.NewLine + Environment.NewLine + "FinancialManagerReport_AutoSendMonthlyEmail == false";
            }

            // This sends room expiration emails
            if (autoSend.Any(x => x.Key == "ExpiringCardsReminder_AutoSendMonthlyEmail" && x.Value == "true"))
            {
                RoomAccessExpirationCheck roomAccessExpirationCheck = new RoomAccessExpirationCheck();
                var expirationEmailsSent = roomAccessExpirationCheck.Run();
                message += Environment.NewLine + Environment.NewLine + $"ExpirationEmailsSent: {expirationEmailsSent}";
            }
            else
            {
                message += Environment.NewLine + Environment.NewLine + "ExpiringCardsReminder_AutoSendMonthlyEmail == false";
            }

            // 2009-08-01 Populate the BillingTables
            BillingManager bm = new BillingManager(_provider, period, 0);
            message += Environment.NewLine + Environment.NewLine + bm.UpdateBilling(BillingCategory.Tool | BillingCategory.Room | BillingCategory.Store);

            var sendMonthlyTaskEmail = GlobalSettings.Current.GetGlobalSetting("SendMonthlyTaskEmail");

            if (!string.IsNullOrEmpty(sendMonthlyTaskEmail))
            {
                // send an email
                SendEmail.SendSystemEmail("OnlineServicesWorker.TaskManager.RunMonthlyTask", $"MonthlyTask result [{now:yyyy-MM-dd HH:mm:ss}]", message, sendMonthlyTaskEmail.Split(','), false);
            }

            return message;
        }

        private SqlConnection NewConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);
        }
    }
}
