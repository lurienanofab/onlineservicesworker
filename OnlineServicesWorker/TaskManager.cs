using LNF;
using LNF.Billing;
using LNF.CommonTools;
using LNF.Models;
using LNF.Models.Billing;
using LNF.Models.Mail;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Scheduler;
using System;
using System.Linq;

namespace OnlineServicesWorker
{
    public class TaskManager
    {
        public IReservationManager ReservationManager => ServiceProvider.Current.Use<IReservationManager>();

        public IApportionmentManager ApportionmentManager => ServiceProvider.Current.Use<IApportionmentManager>();

        public string RunFiveMinuteTask()
        {
            DateTime now = DateTime.Now;

            string message = string.Empty;

            ProcessResult result;

            // every five minutes tasks
            var pastEndableRepairReservations = ReservationManager.SelectPastEndableRepair();
            result = ReservationManager.HandleRepairReservations(pastEndableRepairReservations);
            message += result.LogText;

            var autoEndReservations = ReservationManager.SelectAutoEnd();
            result = ReservationManager.HandleAutoEndReservations(autoEndReservations);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            var pastUnstartedReservations = ReservationManager.SelectPastUnstarted();
            result = ReservationManager.HandleUnstartedReservations(pastUnstartedReservations);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            var sendFiveMinuteTaskEmail = LNF.CommonTools.Utility.GetGlobalSetting("SendFiveMinuteTaskEmail");

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
            result = ResourceClientUtility.CheckExpiringClients(ResourceClientUtility.SelectExpiringClients(), ResourceClientUtility.SelectExpiringEveryone(), noEmail);
            message += result.LogText;

            result = ResourceClientUtility.CheckExpiredClients(ResourceClientUtility.SelectExpiredClients(), ResourceClientUtility.SelectExpiredEveryone(), noEmail);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            // update the DataClean and Data tables
            result = DataTableManager.Update(BillingCategory.Tool | BillingCategory.Room | BillingCategory.Store);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            //2009-08-01 Populate the Billing temp tables
            DateTime yesterday = DateTime.Now.Date.AddDays(-1); //must be yesterday
            DateTime period = yesterday.FirstOfMonth();

            message += $"{Environment.NewLine}{Environment.NewLine}Yesterday: {yesterday:yyyy-MM-dd HH:mm:ss}";
            message += $"{Environment.NewLine}Period: {period:yyyy-MM-dd HH:mm:ss}";

            result = BillingDataProcessStep1.PopulateToolBilling(period, 0, true);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            result = BillingDataProcessStep1.PopulateRoomBilling(period, 0, true);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            result = BillingDataProcessStep1.PopulateStoreBilling(period, true);
            message += Environment.NewLine + Environment.NewLine + result.LogText;

            var sendDailyTaskEmail = LNF.CommonTools.Utility.GetGlobalSetting("SendDailyTaskEmail");

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

            var autoSend = DA.Current.Query<GlobalSettings>().Where(x => new[]
            {
                "ApportionmentReminder_AutoSendMonthlyEmail",
                "FinancialManagerReport_AutoSendMonthlyEmail",
                "ExpiringCardsReminder_AutoSendMonthlyEmail"
            }.Contains(x.SettingName));

            // This sends apportionment emails to clients
            if (autoSend.Any(x => x.SettingName == "ApportionmentReminder_AutoSendMonthlyEmail" && x.SettingValue == "true"))
            {
                result = ApportionmentManager.SendMonthlyApportionmentEmails(period, null, noEmail);
                message += Environment.NewLine + Environment.NewLine + result.LogText;
            }
            else
            {
                message += Environment.NewLine + Environment.NewLine + "ApportionmentReminder_AutoSendMonthlyEmail == false";
            }

            // 2008-04-30
            // Monthly financial report
            if (autoSend.Any(x => x.SettingName == "FinancialManagerReport_AutoSendMonthlyEmail" && x.SettingValue == "true"))
            {
                result = FinancialManagerUtility.SendMonthlyUserUsageEmails(period, 0, 0, new MonthlyEmailOptions() { IncludeManager = !noEmail });
                message += Environment.NewLine + Environment.NewLine + result.LogText;
            }
            else
            {
                message += Environment.NewLine + Environment.NewLine + "FinancialManagerReport_AutoSendMonthlyEmail == false";
            }

            // This sends room expiration emails
            if (autoSend.Any(x => x.SettingName == "ExpiringCardsReminder_AutoSendMonthlyEmail" && x.SettingValue == "true"))
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
            BillingManager bm = new BillingManager(period, 0);
            message += Environment.NewLine + Environment.NewLine + bm.UpdateBilling(BillingCategory.Tool | BillingCategory.Room | BillingCategory.Store);

            var sendMonthlyTaskEmail = LNF.CommonTools.Utility.GetGlobalSetting("SendMonthlyTaskEmail");

            if (!string.IsNullOrEmpty(sendMonthlyTaskEmail))
            {
                // send an email
                SendEmail.SendSystemEmail("OnlineServicesWorker.TaskManager.RunMonthlyTask", $"MonthlyTask result [{now:yyyy-MM-dd HH:mm:ss}]", message, sendMonthlyTaskEmail.Split(','), false);
            }

            return message;
        }
    }
}
