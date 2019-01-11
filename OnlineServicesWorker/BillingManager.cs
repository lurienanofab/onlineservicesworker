using LNF.CommonTools;
using LNF.Models;
using LNF.Models.Billing;
using System;

namespace OnlineServicesWorker
{
    public class BillingManager
    {
        public DateTime Period { get; }
        public int ClientID { get; }

        private readonly WriteToolDataCleanProcess _writeToolDataCleanProcess;
        private readonly WriteToolDataProcess _writeToolDataProcess;

        private readonly WriteRoomDataCleanProcess _writeRoomDataCleanProcess;
        private readonly WriteRoomDataProcess _writeRoomDataProcess;

        private readonly WriteStoreDataCleanProcess _writeStoreDataCleanProcess;
        private readonly WriteStoreDataProcess _writeStoreDataProcess;

        public BillingManager(DateTime period, int clientId)
        {
            Period = period;
            ClientID = clientId;

            DateTime sd = period;
            DateTime ed = period.AddMonths(1);

            _writeToolDataCleanProcess = new WriteToolDataCleanProcess(sd, ed, ClientID);
            _writeToolDataProcess = new WriteToolDataProcess(Period, ClientID);
            _writeRoomDataCleanProcess = new WriteRoomDataCleanProcess(sd, ed, ClientID);
            _writeRoomDataProcess = new WriteRoomDataProcess(Period, ClientID);
            _writeStoreDataCleanProcess = new WriteStoreDataCleanProcess(sd, ed, ClientID);
            _writeStoreDataProcess = new WriteStoreDataProcess(Period, ClientID);
        }

        public string UpdateBilling(BillingCategory types)
        {
            string message = string.Empty;
            string lineBreak = string.Empty;

            // DataClean and Data must be run again to get the last day of the month. The daily task alone will not
            // capture the last day because when it runs on the 1st the new period is used for the import date range.

            // 1) DataClean
            if ((types & BillingCategory.Tool) > 0)
                message += lineBreak + UpdateToolDataClean();
            if ((types & BillingCategory.Room) > 0)
                message += lineBreak + UpdateRoomDataClean();
            if ((types & BillingCategory.Store) > 0)
                message += lineBreak + UpdateStoreDataClean();

            if (message.Length > 0)
                lineBreak = Environment.NewLine;

            // 2) Data
            if ((types & BillingCategory.Tool) > 0)
                message += lineBreak + UpdateToolData();
            if ((types & BillingCategory.Room) > 0)
                message += lineBreak + UpdateRoomData();
            if ((types & BillingCategory.Store) > 0)
                message += lineBreak + UpdateStoreData();

            if (message.Length > 0)
                lineBreak = Environment.NewLine;

            // 3) Step1
            if ((types & BillingCategory.Tool) > 0)
                message += lineBreak + UpdateToolBilling();
            if ((types & BillingCategory.Room) > 0)
                message += lineBreak + UpdateRoomBilling();
            if ((types & BillingCategory.Store) > 0)
                message += lineBreak + UpdateStoreBilling();

            if (message.Length > 0)
                lineBreak = Environment.NewLine;

            // 4) Step4
            if (!IsTemp())
            {
                message += lineBreak + UpdateSubsidyBilling();
            }

            return message;
        }

        public string UpdateToolDataClean() => GetLogText(_writeToolDataCleanProcess.Start());

        public string UpdateToolData() => GetLogText(_writeToolDataProcess.Start());

        public string UpdateToolBilling() => GetLogText(BillingDataProcessStep1.PopulateToolBilling(Period, ClientID, IsTemp()));

        public string UpdateRoomDataClean() => GetLogText(_writeRoomDataCleanProcess.Start());

        public string UpdateRoomData() => GetLogText(_writeRoomDataProcess.Start());

        public string UpdateRoomBilling() => GetLogText(BillingDataProcessStep1.PopulateRoomBilling(Period, ClientID, IsTemp()));

        public string UpdateStoreDataClean() => GetLogText(_writeStoreDataCleanProcess.Start());

        public string UpdateStoreData() => GetLogText(_writeStoreDataProcess.Start());

        public string UpdateStoreBilling() => GetLogText(BillingDataProcessStep1.PopulateStoreBilling(Period, IsTemp()));

        public string UpdateSubsidyBilling() => GetLogText(BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(Period, ClientID));

        public bool IsTemp() => Period == DateTime.Now.FirstOfMonth();

        private string GetLogText(ProcessResult result)
        {
            return result.LogText;
        }
    }
}
