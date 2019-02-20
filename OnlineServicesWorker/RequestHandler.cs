using LNF;
using LNF.CommonTools;
using LNF.Models.Billing;
using LNF.Models.Mail;
using LNF.Models.Worker;
using LNF.Repository;
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

        public string Start()
        {
            string message = string.Empty;

            switch (Request.Command)
            {
                case "UpdateBilling":
                    message = RunUpdateBilling(Request.Args);
                    break;
                case "UpdateToolDataClean":
                    message = RunUpdateToolDataClean(Request.Args);
                    break;
                case "UpdateToolData":
                    message = RunUpdateToolData(Request.Args);
                    break;
                case "UpdateToolBilling":
                    message = RunUpdateToolBilling(Request.Args);
                    break;
                case "UpdateRoomDataClean":
                    message = RunUpdateRoomDataClean(Request.Args);
                    break;
                case "UpdateRoomData":
                    message = RunUpdateRoomData(Request.Args);
                    break;
                case "UpdateRoomBilling":
                    message = RunUpdateRoomBilling(Request.Args);
                    break;
                case "UpdateStoreDataClean":
                    message = RunUpdateStoreDataClean(Request.Args);
                    break;
                case "UpdateStoreData":
                    message = RunUpdateStoreData(Request.Args);
                    break;
                case "UpdateStoreBilling":
                    message = RunUpdateStoreBilling(Request.Args);
                    break;
                case "UpdateSubsidyBilling":
                    message = RunUpdateSubsidyBilling(Request.Args);
                    break;
                case "RunTask":
                    message = RunTask(Request.Args);
                    break;
                case "SetWagoState":
                    message = RunSetWagoState(Request.Args);
                    break;
                case "GetWagoState":
                    message = RunGetWagoState(Request.Args);
                    break;
                case "SendEmail":
                    message = SendEmail(Request.Args);
                    break;
                case "Poke":
                    message = Poke();
                    break;
                default:
                    throw new Exception($"Unknown command: {Request.Command}");
            }

            return message;
        }

        public string RunUpdateBilling(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId, out string billingCategories);

            var bc = (BillingCategory)Enum.Parse(typeof(BillingCategory), billingCategories, true);

            if (bc == 0)
                throw new Exception("At least one billing category is required.");

            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, clientId);
                return mgr.UpdateBilling(bc);
            }
        }

        public string RunUpdateToolDataClean(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, clientId);
                return mgr.UpdateToolDataClean();
            }
        }

        public string RunUpdateToolData(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, clientId);
                return mgr.UpdateToolData();
            }
        }

        public string RunUpdateToolBilling(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, clientId);
                return mgr.UpdateToolBilling();
            }
        }

        public string RunUpdateRoomDataClean(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, clientId);
                return mgr.UpdateRoomDataClean();
            }
        }

        public string RunUpdateRoomData(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, clientId);
                return mgr.UpdateRoomData();
            }
        }

        public string RunUpdateRoomBilling(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, clientId);
                return mgr.UpdateRoomBilling();
            }
        }

        public string RunUpdateStoreDataClean(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, clientId);
                return mgr.UpdateStoreDataClean();
            }
        }

        public string RunUpdateStoreData(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, clientId);
                return mgr.UpdateStoreData();
            }
        }

        public string RunUpdateStoreBilling(string[] args)
        {
            ParseArgs(args, out DateTime period);
            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, 0);
                return mgr.UpdateStoreBilling();
            }
        }

        public string RunUpdateSubsidyBilling(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            using (DA.StartUnitOfWork())
            {
                var mgr = new BillingManager(period, clientId);
                return mgr.UpdateSubsidyBilling();
            }
        }

        public string RunTask(string[] args)
        {
            if (args.Length > 0)
            {
                using (DA.StartUnitOfWork())
                {
                    string task = args[0];

                    var mgr = new TaskManager();

                    // optional noEmail parameter
                    bool noEmail = false;
                    if (args.Length > 1)
                        noEmail = bool.Parse(args[1]);

                    if (task == "5min")
                        return mgr.RunFiveMinuteTask();
                    else if (task == "daily")
                        return mgr.RunDailyTask(noEmail);
                    else if (task == "monthly")
                        return mgr.RunMonthlyTask(noEmail);
                    else
                        throw new Exception($"Unknown task: {task}");
                }
            }
            else
                throw new Exception($"Wrong number of required args for this command. [Expected: 1, Actual: {args.Length}]");
        }

        public string RunSetWagoState(string[] args)
        {
            ParseArgs(args, out int resourceId, out bool state);
            using (DA.StartUnitOfWork())
            {
                WagoInterlock.ToggleInterlock(resourceId, state, 0);
                return $"Wago point for {resourceId} set to {(state ? "ON" : "OFF")}.";
            }
        }

        public string RunGetWagoState(string[] args)
        {
            ParseArgs(args, out int resourceId);
            using (DA.StartUnitOfWork())
            {
                var state = WagoInterlock.GetPointState(resourceId);
                return $"Wago point for {resourceId} is {(state ? "ON" : "OFF")}.";
            }
        }

        public string SendEmail(string[] args)
        {
            if (args.Length == 7)
            {
                using (DA.StartUnitOfWork())
                {
                    string from = args[0];
                    string[] to = args[1].Split(',');
                    string[] cc = args[2].Split(',');
                    string[] bcc = args[3].Split(',');
                    string subj = args[4];
                    string body = args[5];
                    bool isHtml = bool.Parse(args[6]);

                    LNF.CommonTools.SendEmail.Send(0, "OnlineServicesWorker.RequestHandler.SendEmail", subj, body, from, to, cc, bcc, isHtml);

                    return "Email sent OK!" + Environment.NewLine;
                }
            }
            else
                throw new Exception($"Wrong number of required args for this command. [Expected: 7, Actual: {args.Length}]");
        }

        public string Poke()
        {
            return "ouch!";
        }

        private void ParseArgs<T1>(string[] args, out T1 v1)
        {
            if (args.Length > 0)
                v1 = (T1)Convert.ChangeType(args[0], typeof(T1));
            else
                throw new Exception($"Wrong number of required args for this command. [Expected: 1, Actual: {args.Length}]");
        }

        private void ParseArgs<T1, T2>(string[] args, out T1 v1, out T2 v2)
        {
            if (args.Length > 1)
            {
                v1 = (T1)Convert.ChangeType(args[0], typeof(T1));
                v2 = (T2)Convert.ChangeType(args[1], typeof(T2));
            }
            else
                throw new Exception($"Wrong number of required args for this command. [Expected: 2, Actual: {args.Length}]");
        }

        private void ParseArgs<T1, T2, T3>(string[] args, out T1 v1, out T2 v2, out T3 v3)
        {
            if (args.Length > 2)
            {
                v1 = (T1)Convert.ChangeType(args[0], typeof(T1));
                v2 = (T2)Convert.ChangeType(args[1], typeof(T2));
                v3 = (T3)Convert.ChangeType(args[2], typeof(T3));
            }
            else
                throw new Exception($"Wrong number of required args for this command. [Expected: 3, Actual: {args.Length}]");
        }
    }
}
