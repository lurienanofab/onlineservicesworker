using LNF;
using LNF.Billing;
using LNF.Billing.Process;
using LNF.CommonTools;
using System;
using System.Text;

namespace OnlineServicesWorker
{
    public class BillingManager
    {
        private readonly IProvider _provider;

        public DateTime Period { get; }
        public int ClientID { get; }

        protected IProcessRepository Process => _provider.Billing.Process;

        public BillingManager(IProvider provider, DateTime period, int clientId)
        {
            _provider = provider;

            Period = period;
            ClientID = clientId;
        }

        public string UpdateBilling(BillingCategory types)
        {
            var sb = new StringBuilder();

            // DataClean and Data must be run again to get the last day of the month. The daily task alone will not
            // capture the last day because when it runs on the 1st the new period is used for the import date range.

            // 1) DataClean
            sb.AppendLine(GetLogText(Process.DataClean(GetDataCleanCommand(types))));

            // 2) Data
            sb.AppendLine(GetLogText(Process.Data(GetDataCommand(types))));

            // 3) Step1
            sb.AppendLine(GetLogText(Process.Step1(GetStep1Command(types))));
            
            // 4) Step4
            if (!IsTemp())
                sb.AppendLine(GetLogText(Process.Step4(GetStep4Command())));

            string message = sb.ToString();

            return message;
        }

        public string UpdateToolDataClean() => GetLogText(Process.DataClean(GetDataCleanCommand(BillingCategory.Tool)));

        public string UpdateToolData() => GetLogText(Process.Data(GetDataCommand(BillingCategory.Tool)));

        public string UpdateToolBilling() => GetLogText(Process.Step1(GetStep1Command(BillingCategory.Tool)));

        public string UpdateRoomDataClean() => GetLogText(Process.DataClean(GetDataCleanCommand(BillingCategory.Room)));

        public string UpdateRoomData() => GetLogText(Process.Data(GetDataCommand(BillingCategory.Room)));

        public string UpdateRoomBilling() => GetLogText(Process.Step1(GetStep1Command(BillingCategory.Room)));

        public string UpdateStoreDataClean() => GetLogText(Process.DataClean(GetDataCleanCommand(BillingCategory.Store)));

        public string UpdateStoreData() => GetLogText(Process.Data(GetDataCommand(BillingCategory.Store)));

        public string UpdateStoreBilling() => GetLogText(Process.Step1(GetStep1Command(BillingCategory.Store)));

        public string UpdateSubsidyBilling() => GetLogText(Process.Step4(GetStep4Command()));

        public bool IsTemp() => Period == DateTime.Now.FirstOfMonth();

        private string GetLogText(ProcessResult result)
        {
            return result.LogText;
        }

        private DataCleanCommand GetDataCleanCommand(BillingCategory bc)
        {
            return new DataCleanCommand
            {
                BillingCategory = bc,
                StartDate = Period,
                EndDate = Period.AddMonths(1),
                ClientID = ClientID
            };
        }

        private DataCommand GetDataCommand(BillingCategory bc)
        {
            return new DataCommand
            {
                BillingCategory = bc,
                Period = Period,
                ClientID = ClientID,
                Record = 0
            };
        }

        private Step1Command GetStep1Command(BillingCategory bc)
        {
            return new Step1Command
            {
                BillingCategory = bc,
                Period = Period,
                ClientID = ClientID,
                Record = 0,
                IsTemp = IsTemp(),
                Delete = true
            };
        }

        private Step4Command GetStep4Command()
        {
            return new Step4Command
            {
                Command = "subsidy",
                Period = Period,
                ClientID = ClientID
            };
        }
    }
}
