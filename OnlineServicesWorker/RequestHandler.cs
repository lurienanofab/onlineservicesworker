using LNF;
using LNF.CommonTools;
using LNF.Service;
using System;

namespace OnlineServicesWorker
{
    public class RequestHandler
    {
        public WorkerRequest Request { get; }

        public RequestHandler(WorkerRequest req)
        {
            Request = req;
        }

        public void Start()
        {
            using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
            {
                switch (Request.Command)
                {
                    case "UpdateBilling":
                        UpdateBilling(Request.Args);
                        break;
                    default:
                        throw new Exception($"Unknown command: {Request.Command}");
                }
            }
        }

        public void UpdateBilling(string[] args)
        {
            if (args.Length == 4)
            {
                DateTime sd = DateTime.Parse(args[0]);
                DateTime ed = DateTime.Parse(args[1]);
                int clientId = int.Parse(args[2]);
                string[] billingCategories = args[3].Split(',');

                var writeToolDataMgr = WriteToolDataManager.Create(sd, ed, clientId);
                var writeRoomDataMgr = WriteRoomDataManager.Create(sd, ed, clientId);
                var writeStoreDataMgr = WriteStoreDataManager.Create(sd, ed, clientId);

                // 1) DataClean
                foreach (var bc in billingCategories)
                {
                    if (bc == "tool")
                        writeToolDataMgr.WriteToolDataClean();
                    else if (bc == "room")
                        writeRoomDataMgr.WriteRoomDataClean();
                    else if (bc == "store")
                        writeStoreDataMgr.WriteStoreDataClean();
                }

                // 1) Data
                foreach (var bc in billingCategories)
                {
                    if (bc == "tool")
                        writeToolDataMgr.WriteToolData();
                    else if (bc == "room")
                        writeRoomDataMgr.WriteRoomData();
                    else if (bc == "store")
                        writeStoreDataMgr.WriteStoreData();
                }

                // Step1
                DateTime p = sd;
                while (p < ed)
                {
                    bool temp = p == DateTime.Now.FirstOfMonth();

                    foreach (var bc in billingCategories)
                    {
                        if (bc == "tool")
                            BillingDataProcessStep1.PopulateToolBilling(p, clientId, temp);
                        else if (bc == "room")
                            BillingDataProcessStep1.PopulateRoomBilling(p, clientId, temp);
                        else if (bc == "store")
                            BillingDataProcessStep1.PopulateStoreBilling(p, temp);
                    }

                    if (!temp)
                    {
                        BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(p, clientId);
                    }

                    p = p.AddMonths(1);
                }
            }
            else
                throw new Exception($"Wrong number of args for this command. [Expected: 4, Actual: {args.Length}]");
        }
    }
}
