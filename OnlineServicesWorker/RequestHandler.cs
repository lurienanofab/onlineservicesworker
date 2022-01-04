using LNF;
using LNF.Billing;
using LNF.CommonTools;
using LNF.Worker;
using System;

namespace OnlineServicesWorker
{
    public class RequestHandler
    {
        private readonly IProvider _provider;

        public RequestHandler(IProvider provider)
        {
            _provider = provider;
        }

        public string Start(WorkerRequest req)
        {
            string message;

            switch (req.Command)
            {
                case "UpdateBilling":
                    message = RunUpdateBilling(req.Args);
                    break;
                case "UpdateToolDataClean":
                    message = RunUpdateToolDataClean(req.Args);
                    break;
                case "UpdateToolData":
                    message = RunUpdateToolData(req.Args);
                    break;
                case "UpdateToolBilling":
                    message = RunUpdateToolBilling(req.Args);
                    break;
                case "UpdateRoomDataClean":
                    message = RunUpdateRoomDataClean(req.Args);
                    break;
                case "UpdateRoomData":
                    message = RunUpdateRoomData(req.Args);
                    break;
                case "UpdateRoomBilling":
                    message = RunUpdateRoomBilling(req.Args);
                    break;
                case "UpdateStoreDataClean":
                    message = RunUpdateStoreDataClean(req.Args);
                    break;
                case "UpdateStoreData":
                    message = RunUpdateStoreData(req.Args);
                    break;
                case "UpdateStoreBilling":
                    message = RunUpdateStoreBilling(req.Args);
                    break;
                case "UpdateSubsidyBilling":
                    message = RunUpdateSubsidyBilling(req.Args);
                    break;
                case "RunTask":
                    message = RunTask(req.Args);
                    break;
                case "SetWagoState":
                    message = RunSetWagoState(req.Args);
                    break;
                case "GetWagoState":
                    message = RunGetWagoState(req.Args);
                    break;
                case "SendEmail":
                    message = SendEmail(req.Args);
                    break;
                case "Poke":
                    message = Poke();
                    break;
                default:
                    throw new Exception($"Unknown command: {req.Command}");
            }

            return message;
        }

        public string RunUpdateBilling(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId, out string billingCategories);

            var bc = (BillingCategory)Enum.Parse(typeof(BillingCategory), billingCategories, true);

            if (bc == 0)
                throw new Exception("At least one billing category is required.");

            var mgr = new BillingManager(_provider, period, clientId);
            return mgr.UpdateBilling(bc);
        }

        public string RunUpdateToolDataClean(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            var mgr = new BillingManager(_provider, period, clientId);
            return mgr.UpdateToolDataClean();
        }

        public string RunUpdateToolData(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            var mgr = new BillingManager(_provider, period, clientId);
            return mgr.UpdateToolData();
        }

        public string RunUpdateToolBilling(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            var mgr = new BillingManager(_provider, period, clientId);
            return mgr.UpdateToolBilling();
        }

        public string RunUpdateRoomDataClean(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            var mgr = new BillingManager(_provider, period, clientId);
            return mgr.UpdateRoomDataClean();
        }

        public string RunUpdateRoomData(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            var mgr = new BillingManager(_provider, period, clientId);
            return mgr.UpdateRoomData();
        }

        public string RunUpdateRoomBilling(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            var mgr = new BillingManager(_provider, period, clientId);
            return mgr.UpdateRoomBilling();
        }

        public string RunUpdateStoreDataClean(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            var mgr = new BillingManager(_provider, period, clientId);
            return mgr.UpdateStoreDataClean();
        }

        public string RunUpdateStoreData(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            var mgr = new BillingManager(_provider, period, clientId);
            return mgr.UpdateStoreData();
        }

        public string RunUpdateStoreBilling(string[] args)
        {
            ParseArgs(args, out DateTime period);
            var mgr = new BillingManager(_provider, period, 0);
            return mgr.UpdateStoreBilling();
        }

        public string RunUpdateSubsidyBilling(string[] args)
        {
            ParseArgs(args, out DateTime period, out int clientId);
            var mgr = new BillingManager(_provider, period, clientId);
            return mgr.UpdateSubsidyBilling();
        }

        public string RunTask(string[] args)
        {
            if (args.Length > 0)
            {
                string task = args[0];

                var mgr = new TaskManager(_provider);

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
            else
                throw new Exception($"Wrong number of required args for this command. [Expected: 1, Actual: {args.Length}]");
        }

        public string RunSetWagoState(string[] args)
        {
            ParseArgs(args, out int resourceId, out bool state);
            WagoInterlock.ToggleInterlock(resourceId, state, 0);
            return $"Wago point for {resourceId} set to {(state ? "ON" : "OFF")}.";
        }

        public string RunGetWagoState(string[] args)
        {
            ParseArgs(args, out int resourceId);
            var state = WagoInterlock.GetPointState(resourceId);
            return $"Wago point for {resourceId} is {(state ? "ON" : "OFF")}.";
        }

        public string SendEmail(string[] args)
        {
            if (args.Length == 7)
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
